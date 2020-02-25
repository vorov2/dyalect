using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Compiler
{
    internal struct MatchEntryKey
    {
        public const int FixedArray = 0x01;
        public const int Array = 0x02;
        public const int Integer = 0x04;
        public const int Float = 0x08;
        public const int String = 0x10;
    }
}
