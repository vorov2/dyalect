using Dyalect.Parser.Model;

namespace Dyalect.Parser
{
    public static class DyParser
    {
        private const string MEMORY = "<memory>";

        public static Result<DyCodeModel> Parse(SourceBuffer buffer)
        {
            var ip = new InternalParser(new(buffer));
            ip.Parse();
            var cd = new DyCodeModel(ip.Root, ip.Imports.ToArray(), buffer.FileName ?? MEMORY);
            return ip.Errors.Count == 0
                ? Result.Create(cd)
                : Result.Create(cd, ip.Errors);
        }
    }
}
