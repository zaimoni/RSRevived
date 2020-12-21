using System;
using System.Collections.Generic;
using System.Linq;

namespace Zaimoni.Data
{
    public ref struct Stack<T>  // note namespace collision w/C# standard library
    {
        private readonly Span<T> _x;
        private int ub;

        public Stack(Span<T> src) { _x = src; ub = 0; }
        public void push(ref T src) { _x[ub++] = src; }
        public void push(T src) { _x[ub++] = src; }
        public int Count { get { return ub; } }
        public void Clear() { ub = 0; }
        public T this[int n] { get { return _x[n]; } }

        public Value Min<Value>(Func<T, Value> xform) where Value:IComparable<Value>
        {
            Value ret = (Value)typeof(Value).GetField("MaxValue").GetValue(default(Value));
            int i = ub;
            Value test;
            while (0 <= --i) {
                if (0 < ret.CompareTo(test = xform(_x[i]))) ret = test;
            }
            return ret;
        }

        public Value Max<Value>(Func<T, Value> xform) where Value : IComparable<Value>
        {
            Value ret = (Value)typeof(Value).GetField("MinValue").GetValue(default(Value));
            int i = ub;
            Value test;
            while (0 <= --i) {
                if (0 > ret.CompareTo(test = xform(_x[i]))) ret = test;
            }
            return ret;
        }

        public void SelfFilter(Predicate<T> test)
        {
            if (0 < ub) {
                int origin = 0;
                int i = 0;
                do if (test(_x[i])) {
                        if (origin < i) _x[origin++] = _x[i];
                        else origin++;
                   }
                while(++i < ub);
                ub = origin;
            }
        }
    }

    static internal class Stack_ext
    {
        static public bool Contains<T>(this ref Zaimoni.Data.Stack<T> src, T x) where T : IComparable
        {
            int i = src.Count;
            while (0 < i--) if (0==x.CompareTo(src[i])) return true;
            return false;
        }

        static public Stack<T> ToZStack<T>(this IEnumerable<T> src, Predicate<T> test)
        {
          int ub;
          if (null == src || 0 >= (ub = src.Count())) return default;
          var ret = new Stack<T>(new T[ub]);
          foreach (var x in src) if (test(x)) ret.push(x);
          return ret;
        }
    }
}