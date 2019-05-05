using Dyalect.Parser;
using Dyalect.Parser.Model;
using System.Collections.Generic;

namespace Dyalect.Compiler
{
    //Этот кусок отвечает за добавление и поиск переменных
    partial class Builder
    {
        //Индексаторы переменных
        private Stack<int> counters; //Стек индексов лексического скоупа
        private int currentCounter; //Глобальный индексер

        private Dictionary<string, ImportedName> imports = new Dictionary<string, ImportedName>(); //Все глобальные импорты имён

        //Стандартная процедура добавления переменных. Используется, когда нужна безыменная переменная
        //private каких-нибудь тёмных внутренних дел
        private int AddVariable()
        {
            var ret = 0 | currentCounter << 8;
            currentCounter++;
            return ret;
        }

        private int AddSystemVariable(string name)
        {
            return AddVariable(name, null, -1);
        }

        public int AddVariable(string name, DNode node, int data) =>
            AddVariable(name, node != null ? node.Location : default, data);

        //Добавляем нормальные именованные переменные
        private int AddVariable(string name, Location loc, int data)
        {
            //Смотрим, а не добавили ли мы уже такую в локальный скоуп
            if (currentScope.Locals.ContainsKey(name))
            {
                AddError(CompilerError.VariableAlreadyDeclared, loc, name);
                return -1;
            }

            currentScope.Locals.Add(name, new ScopeVar(currentCounter, data));

            //Дополнительная отладочная информация генерируется только в дебуг режиме
            if (isDebug && !loc.IsEmpty)
            {
                AddVarPragma(name, currentCounter, cw.Offset, data);
                AddLinePragma(loc);
                cw.Nop();
            }

            var retval = AddVariable();

            if ((data & VarFlags.Exported) == VarFlags.Exported)
                unit.ExportList.Add(new PublishedName(name, new ScopeVar(retval, data)));

            return retval;
        }

        //Ищем переменную по имени в глобальном скоупе
        private ScopeVar GetParentVariable(string name, DNode node)
        {
            return GetParentVariable(name, node.Location);
        }

        private ScopeVar GetParentVariable(string name, Location loc)
        {
            var cur = currentScope;

            //Backtracks the scopes to find parent
            while (cur.Parent != null)
            {
                if (cur.Function)
                {
                    cur = cur.Parent;
                    break;
                }

                cur = cur.Parent;
            }

            if (cur.Locals.TryGetValue(name, out var var))
                return new ScopeVar(1 | var.Address << 8, var.Data);

            AddError(CompilerError.UndefinedBaseVariable, loc, name);
            return ScopeVar.Empty;
        }

        //Ищем переменную по имени, начиная с указанного скоупа
        private ScopeVar GetVariable(string name, DNode node, bool err = true)
        {
            return GetVariable(name, currentScope, node.Location, err);
        }

        private ScopeVar GetVariable(string name, Location loc, bool err = true)
        {
            return GetVariable(name, currentScope, loc, err);
        }

        private ScopeVar GetVariable(string name, Scope startScope, Location loc, bool err)
        {
            var cur = startScope;
            var shift = 0;
            var var = ScopeVar.Empty;

            //Рекурсивно проходим по всем скоупам
            do
            {
                if (cur.Locals.TryGetValue(name, out var))
                    return new ScopeVar(shift | var.Address << 8, var.Data);

                if (cur.Function)
                    shift++;

                var = ScopeVar.Empty;
                cur = cur.Parent;
            }
            while (cur != null);

            //Не нашли. Смотрим, нет ли такого импортированного имени
            if (imports.TryGetValue(name, out ImportedName imp))
            {
                return new ScopeVar(imp.ModuleHandle | (imp.PublishedName.Data.Address >> 8) << 8,
                    imp.PublishedName.Data.Data | VarFlags.External);
            }

            if (err)
                AddError(CompilerError.UndefinedVariable, loc, name);

            return ScopeVar.Empty;
        }

        //Ищем скоуп, где объявлена переменная. Возвращаем первый же, который подходит
        private Scope GetScope(string name)
        {
            var cur = currentScope;

            do
            {
                if (cur.Locals.ContainsKey(name))
                    return cur;

                cur = cur.Parent;
            }
            while (cur != null);

            return null;
        }
    }
}
