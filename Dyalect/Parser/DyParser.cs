using Dyalect.Parser.Model;

namespace Dyalect.Parser
{
    public static class DyParser
    {
        private const string MEMORY = "<stdin>";

        public static Result<DyCodeModel> Parse(SourceBuffer buffer)
        {
            var fileName = buffer.FileName ?? MEMORY;
            var ip = new InternalParser(fileName, new(buffer));
            ip.Parse();
            var cd = new DyCodeModel(ip.Root, ip.Imports.ToArray(), fileName);
            return ip.Errors.Count == 0
                ? Result.Create(cd)
                : Result.Create(cd, ip.Errors);
        }
    }
}
