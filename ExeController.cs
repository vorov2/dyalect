﻿using Dyalect;
using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
namespace DyalectService;

[ApiController]
[Route("[controller]")]
public class ExeController : ControllerBase
{
    private readonly IWebHostEnvironment webHostEnvironment;

    public ExeController(IWebHostEnvironment webHostEnvironment) => this.webHostEnvironment = webHostEnvironment;

    [HttpGet]
    public JsonResult Get()
    {
        var obj = new {
            version = typeof(Exe).Assembly.GetType("Dyalect.Meta").GetField("Version").GetValue(null)?.ToString()
        };
        return new JsonResult(obj);
    }

    [HttpPost]
    public JsonResult Post([FromForm] string source)
    {
        var sw = new StringWriter();
        Console.SetOut(sw);

        var t = Type.GetType("Dyalect.Meta");

        var result = Task.Run(() =>
        {
            try
            {
                var phys = Path.Combine(webHostEnvironment.ContentRootPath, "dya");
                var lookup = FileLookup.Create(BuilderOptions.Default(), phys);
                var linker = new DyLinker(lookup, BuilderOptions.Default());
                var res = linker.Make(SourceBuffer.FromString(source));

                if (!res.Success)
                    return new JsonResult(new
                    {
                        success = false,
                        cout = sw.ToString(),
                        messages = res.Messages.Select(m => m.ToString()).ToArray()
                    });

                var ctx = DyMachine.CreateExecutionContext(res.Value);
                var ret = DyMachine.Execute(ctx);
                return new JsonResult(new { success = true, value = MakeString(ret), cout = sw.ToString() });
            }
            catch (DyBuildException ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    cout = sw.ToString(),
                    messages = ex.Messages.Select(m => m.ToString()).ToArray()
                });
            }
            catch (DyCodeException ex)
            {
                var xs = new List<string>
                {
                    ex.Message,
                    "Stack trace:"
                };
                var trace = ex.CallTrace.Select(t => HttpUtility.HtmlEncode(t.ToString()));
                xs.AddRange(trace);
                return new JsonResult(new
                {
                    success = false,
                    cout = sw.ToString(),
                    messages = xs.ToArray()
                });
            }
            catch (DyException ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    cout = sw.ToString(),
                    messages = new string[] { ex.ToString() }
                });
            }
        });

        if (!result.Wait(1000))
            return new JsonResult(new { success = false, cout = sw.ToString(), messages = new string[] { "Execution timed out." } });

        return result.Result;
    }

    private static string MakeString(ExecutionResult ret)
    {
        try
        {
            if (ret.Value is DyNil)
                return string.Empty;

            return ret.Value.ToLiteral(ret.Context) + " :: " + ret.Value.TypeName;
        }
        catch
        {
            return ret.Value?.ToString();
        }
    }
}