using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Zaimoni.Lazy
{
    public interface hook
    {
        bool signal();
        bool isExpired();
    }

    class LogicalAND : hook
    {
        long latch = 0;
        Func<bool> op;
        List<hook>? notify = null;

        public bool signal() {
            if (0 < --latch) return false;
            if (!op()) return false;
            if (null != notify) {
                var ub = notify.Count;
                while (0 <= --ub) {
                    if (!notify[ub].isExpired()) notify[ub].signal();
                    notify[ub] = null;
                }
                notify = null;
            }
            return true;
        }

        public bool isExpired() => 0 >= latch;
        public void Targetted() => latch++;

        public void AddListener(hook src) {
            (notify ??= new()).Add(src);
        }
        public void RemoveListener(hook src)
        {
            if (null != notify && notify.Remove(src) && 0 >= notify.Count) notify = null;
        }
    }

    class LogicalOR : hook
    {
        bool _expired = false;
        Func<bool> op;
        List<hook>? notify = null;

        public bool signal()
        {
            if (_expired) return true;
            if (!op()) return false;
            _expired = true;
            if (null != notify) {
                var ub = notify.Count;
                while (0 <= --ub) {
                    if (!notify[ub].isExpired()) notify[ub].signal();
                    notify[ub] = null;
                }
                notify = null;
            }
            return true;
        }

        public bool isExpired() => _expired;

        public void AddListener(hook src)
        {
            (notify ??= new()).Add(src);
        }
        public void RemoveListener(hook src)
        {
            if (null != notify && notify.Remove(src) && 0 >= notify.Count) notify = null;
        }
    }
}
