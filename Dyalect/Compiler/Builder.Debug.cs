using Dyalect.Debug;
using Dyalect.Parser;
using Dyalect.Parser.Model;
using System;

namespace Dyalect.Compiler
{
    //В этом кусочке билдера содержится всё, связанное с отладочной информацией
    partial class Builder
    {
        //Последний локейшин, который был передан в метод AddLineSym.
        //Используется методом EndScope, чтобы добавить локейшин завершения скоупа.
        private Location lastLocation;

        private DebugWriter pdb; //Писатель символов
        private bool isDebug; //Показывает, производим ли мы компиляцию в дебаг-режиме

        //Вызывается, чтобы создать первую часть символа FunSym.
        private void StartFun(string name, Par[] pars, int parCount)
        {
            cw.StartFrame();
            pdb.StartFunction(name, cw.Offset, pars);
        }

        //Вызывается, когда компиляция функции завершается, чтобы создать
        //оставшуюся часть символа FunSym
        private int EndFun(int handle)
        {
            pdb.EndFunction(handle, cw.Offset);
            return cw.FinishFrame();
        }

        //Генерирует данные о лексических скоупах. Вызывается, когда лексический
        //скоуп открывается.
        private void StartScope(bool fun, Location loc)
        {
            currentScope = new Scope(fun, currentScope);

            if (isDebug)
                pdb.StartScope(cw.Offset, loc.Line, loc.Column);
        }

        //Вызывается по завершении лексического скоупа
        private void EndScope()
        {
            currentScope = currentScope.Parent != null ? currentScope.Parent : null;

            if (isDebug)
                pdb.EndScope(cw.Offset, lastLocation.Line, lastLocation.Column);
        }

        //Вызывается совместно со StartScope, когда лексический скоуп является
        //реальным и присутствует в рантайме (как, например, лексический скоуп функции).
        private void StartSection()
        {
            counters.Push(currentCounter);
            currentCounter = 0;
        }

        //Вызывается по завершении реального лексического скоупа.
        private void EndSection()
        {
            currentCounter = counters.Pop();
        }

        //Генерирует прагму для строки и запоминает локейшин с помощью полей
        //lastLine и lastColumn
        private void AddLinePragma(DNode node)
        {
            lastLocation = node.Location;
            pdb.AddLineSym(cw.Offset, lastLocation.Line, lastLocation.Column);
        }

        private void AddLinePragma(Location loc)
        {
            lastLocation = loc;
            pdb.AddLineSym(cw.Offset, loc.Line, loc.Column);
        }

        //Используется только при генерации расширенной отладочной информации
        private void AddVarPragma(string name, int address, int offset, int data)
        {
            if (isDebug)
                pdb.AddVarSym(name, address, offset, 0, data);
        }
    }
}
