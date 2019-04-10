using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Command
{
    public sealed class OptionBag : IOptionBag
    {
        [Binding("debug", "Производить компиляцию в режиме отладки.")]
        public bool Debug { get; set; }

        [Binding("nolang", "Отключить неявный импорт модуля lang, содержащего базовые функции и примитивы.")]
        public bool NoLang { get; set; }

        [Binding("path", "Путь, по которому линковщик будет искать подключаемые модули. Допускается указание этого ключа несколько раз.")]
        public string[] Paths { get; set; }

        public string StartupPath { get; set; }

        public string DefaultArgument { get; set; }
    }
}
