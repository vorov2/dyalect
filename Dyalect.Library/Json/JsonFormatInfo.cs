namespace Dyalect.Library.Json
{
    public sealed class JsonFormatInfo
    {
        public enum Eol
        {
            Auto,
            Cr,
            Lf,
            CrLf
        }

        public static readonly JsonFormatInfo Default = new JsonFormatInfo
        { 
            IndentSize = 2,
            CompactList = true
        };
        
        public static readonly JsonFormatInfo Compact = new JsonFormatInfo
        {
            CompactAll = true,
            CompactList = true
        };

        public int IndentSize { get; set; }

        public bool IndentWithTabs { get; set; }
        
        public bool CompactAll { get; set; }
        
        public bool CompactList { get; set; }

        public Eol LineEndings { get; set; }
    }
}