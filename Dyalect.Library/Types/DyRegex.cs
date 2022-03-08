//using Dyalect.Compiler;
//using Dyalect.Debug;
//using Dyalect.Runtime;
//using Dyalect.Runtime.Types;
//using System.Text.RegularExpressions;

//namespace Dyalect.Library.Types
//{
//    public sealed class DyRegex : DyForeignObject<DyRegexTypeInfo>
//    {
//        internal readonly Regex Regex;

//        public DyRegex(RuntimeContext rtx, Unit unit, string regex) : base(rtx, unit)
//        {
//            this.Regex = new Regex(regex, RegexOptions.Compiled);
//        }

//        public override object ToObject() => Regex;

//        public override DyObject Clone() => this;
//    }

//    //public sealed class DyRegexMatch : DyObject
//    //{
//    //    internal readonly Match Match;

//    //    public DyRegexMatch(int typeCode, Match match) : base(typeCode)
//    //    {
//    //        this.Match = match;
//    //    }

//    //    protected override bool GetBool() => Match.Success;

//    //    public override object ToObject() => Match;

//    //    public override DyObject Clone() => this;
//    //}

//    public sealed class DyRegexTypeInfo : ForeignTypeInfo
//    {
//        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
//        {
//            return new DyString(((DyRegex)arg).Regex.ToString());
//        }

//        public override string TypeName => "Regex";

//        protected override SupportedOperations GetSupportedOperations() =>
//            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

//        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
//        {
//            return left is DyRegex a && right is DyRegex b && a.Regex.ToString() == b.Regex.ToString()
//                ? DyBool.True : DyBool.False;
//        }

//        private DyObject Match(ExecutionContext ctx, DyObject self, DyObject input, DyObject start, DyObject len)
//        {
//            if (input.TypeId != DyType.String)
//                return ctx.InvalidType(input);

//            var str = (string)input.ToObject();
//            var istart = (int)(long)start.ToObject();
//            var ilen = len.TypeId == DyType.Nil ? str.Length : (int)(long)len.ToObject();
//            var rx = ((DyRegex)self).Regex;

//            if (istart + ilen > str.Length)
//                return ctx.IndexOutOfRange();

//            var m = rx.Match(str, istart, ilen);
//            //return new DyRegexMatch(m);
//            return null!;
//        }

//        //private DyObject New(ExecutionContext ctx, DyObject arg)
//        //{
//        //    if (arg.TypeId != DyType.String)
//        //        return ctx.InvalidType(arg);

//        //    return new DyRegex((string)arg.ToObject());
//        //}

//        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx)
//        {
//            //if (name == "Regex")
//            //    return Func.Static(name, New, -1, new Par("pattern"));

//            return base.InitializeStaticMember(name, ctx);
//        }
//    }
//}
