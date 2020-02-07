using System;

namespace Zaimoni.Data
{
    public ref struct Stack<T>
    {
        private Span<T> _x;
        private int ub;

        public Stack(Span<T> src) { _x = src; ub = 0; }
        public void push(ref T src) { _x[ub++] = src;  }
        public void push(T src) { _x[ub++] = src;  }
        public int Count { get { return ub; } }
        public void Clear() { ub = 0; }
        public T this[int n] { get { return _x[n]; } }
    }
}
