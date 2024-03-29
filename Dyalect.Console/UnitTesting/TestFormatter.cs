using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dyalect.UnitTesting;

public static class TestFormatter
{
    private const string Header = "Test session from {0:dd/MM/yyyy HH:mm}";
    private const string FileHeader1 = "{0} test file(s):";
    private const string FileHeader2 = "Test file(s):";
    private const string Warnings = "Warnings:";
    private const string Report = "Report:";
    private const string SummaryHeader = "Summary:";
    private const string Summary = "{0} passed, {1} failed in {2} file(s)";

    public static string Format(TestReport report, TestFormatFlags flags)
    {
        var builder = new StringBuilder();

        if ((flags & TestFormatFlags.Markdown) == TestFormatFlags.Markdown)
            FormatMd(builder, report, flags);
        else
            FormatText(builder, report, flags);

        return builder.ToString();
    }

    private static void FormatMd(StringBuilder sb, TestReport report, TestFormatFlags flags)
    {
        sb.AppendLine("# " + string.Format(Header, DateTime.Now));
        sb.AppendLine();

        sb.AppendLine("## " + SummaryHeader);
        sb.AppendLine(string.Format(Summary,
            report.Results.Count(r => r.Error is null),
            report.Results.Count(r => r.Error is not null).ToString() + (report.FailedFiles.Any() ? "1+" : ""),
            report.TestFiles.Length
        ));
        sb.AppendLine();

        sb.AppendLine("## " + FileHeader2);
        sb.AppendLine(string.Join(", ", report.TestFiles
            .Select(f => "[" + Path.GetFileName(f) + "](" + Path.GetFullPath(f) + ")")));
        sb.AppendLine();

        if (report.BuildWarnings != null && report.BuildWarnings.Count > 0)
        {
            sb.AppendLine("## " + Warnings);

            foreach (var w in report.BuildWarnings)
                sb.Append("* " + w);

            sb.AppendLine();
        }

        IEnumerable<TestResult> results;
        var onlyFailed = (flags & TestFormatFlags.OnlyFailed) == TestFormatFlags.OnlyFailed;

        if (onlyFailed)
            results = report.Results.Where(r => r.Error is not null);
        else
            results = report.Results;

        if (!onlyFailed || results.Any())
        {
            sb.AppendLine("## " + Report);
            sb.AppendLine();

            foreach (var group in results.GroupBy(r => r.FileName))
            {
                sb.AppendLine("### " + GetShortFileName(group.Key));

                foreach (var f in group)
                {
                    if (f.Error is null)
                        sb.AppendLine("* &#9745; **" + f.Name + "**");
                    else if (f.Name is not null)
                        sb.AppendLine("* &#9746; **" + f.Name + "**: " + f.Error);
                    else
                        sb.AppendLine("* &#9746; " + f.Error);
                }

                sb.AppendLine();
            }
        }
    }

    private static void FormatText(StringBuilder sb, TestReport report, TestFormatFlags flags)
    {
        sb.AppendLine(string.Format(Header, DateTime.Now));
        sb.AppendLine();
        sb.AppendLine(string.Format(FileHeader1, report.TestFiles.Length));
        sb.AppendLine(string.Join(", ", report.TestFiles.Select(Path.GetFileName)));
        sb.AppendLine();

        if (report.BuildWarnings != null && report.BuildWarnings.Count > 0)
        {
            sb.AppendLine(Warnings);

            foreach (var w in report.BuildWarnings)
                sb.Append(w);

            sb.AppendLine();
        }

        IEnumerable<TestResult> results;
        var onlyFailed = (flags & TestFormatFlags.OnlyFailed) == TestFormatFlags.OnlyFailed;

        if (onlyFailed)
            results = report.Results.Where(r => r.Error is not null);
        else
            results = report.Results;

        if (!onlyFailed || results.Any())
        {
            sb.AppendLine(Report);
            sb.AppendLine();

            foreach (var group in results.GroupBy(r => r.FileName))
            {
                sb.AppendLine(GetShortFileName(group.Key));

                foreach (var f in group)
                {
                    if (f.Error is null)
                        sb.AppendLine("[+] \"" + f.Name + "\"");
                    else if (f.Name is not null)
                        sb.AppendLine("[ ] \"" + f.Name + "\": " + f.Error);
                    else
                        sb.AppendLine("[ ] " + f.Error);
                }

                sb.AppendLine();
            }
        }

        sb.AppendLine(SummaryHeader);
        sb.AppendLine(string.Format(Summary,
            report.Results.Count(r => r.Error is null),
            report.Results.Count(r => r.Error is not null).ToString() + (report.FailedFiles.Any() ? "1+" : ""),
            report.TestFiles.Length
        ));
    }

    private static string GetShortFileName(string fileName)
    {
        var fi = new FileInfo(fileName);
        return $"{fi.Directory?.Name}/{fi.Name}:";
    }
}
