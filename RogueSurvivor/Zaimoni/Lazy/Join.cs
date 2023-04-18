using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zaimoni.Lazy
{
    public class Join<T> where T : class
    {
        private readonly T relay;
        private readonly Action<T> onDone;
        private long latch = 0;

        public Join(T src, Action<T> handler)
        {
            relay = src;
            onDone = handler;
        }

        public void Schedule() => latch++;

        public bool signal()
        {
            if (0 < --latch) return false;
            onDone(relay);
            return true;
        }

        public bool isDone() {
            if (0 < latch) return false;
            onDone(relay);
            return true;
        }
    }
}
