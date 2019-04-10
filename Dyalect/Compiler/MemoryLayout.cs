namespace Dyalect.Compiler
{
    //Эта структура описывает разбивку памяти для реального лексического скоупа,
    //который живёт в рантайме (например, глобальный или у функции). Используется
    //для адресации
    public sealed class MemoryLayout
    {
        internal MemoryLayout(int size, int stackSize, int address)
        {
            Size = size;
            StackSize = stackSize;
            Address = address;
        }

        //Размер операционного стека
        public int StackSize { get; }

        //Количество локальных переменных
        public int Size { get; }

        //Адрес (смещение по ASM-коду)
        public int Address { get; internal set; }
    }
}