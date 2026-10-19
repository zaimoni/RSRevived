using System;
using System.Collections.Generic;
using System.Linq;

namespace Zaimoni.Lazy
{
    // use case: triggering cache variable update when data completely loads.

    public interface hook
    {
        // Kleene's strong 3-valued logic: true, false, null
        bool? code { get; }
        bool signal();
        void update(hook h);
    }

    class Relay : hook {
        bool? _code = null;
        private List<hook>? exec;
        public void AddListener(hook src) => (exec ??= new()).Add(src);

        public Relay() { }
        public bool? code { get { return _code; } }
        public bool signal() => _code is not null;
        public void update(hook h) { }


        public void fire() {
            _code = true;
            _notify();
        }

        public void fail() {
            _code = false;
            _notify();
        }

        private void _notify() {
            if (exec is not null) {
                var ub = exec.Count;
                while (0 <= --ub) {
                    exec[ub].signal();
                    exec[ub] = null; // enable garbage collector early
                }
                exec = null;
            }
        }
    }

    class Notice : hook
    {
        private bool? _code = null;
        private Func<bool?>? op = null;

        public Notice(Func<bool?>? op)
        {
            this.op = op;
        }

        public bool? code { get { return _code; } }
        public void update(hook h) { }

        public bool signal() {
            if (_code is null && op is not null) {
                _code = op();
                if (_code is not null) op = null;
            }
            return _code is not null;
        }

    }

    class ExecOnDemand : hook
    {
        private bool? _code = null;
        private List<hook>? exec = null;
        public void AddListener(hook src) => (exec ??= new()).Add(src);

        public ExecOnDemand() {}
        public bool? code { get { return _code; } }
        public void update(hook h) { }

        public bool signal() {
            if (exec is null) {
                _code = true;
                return true;
            }

            exec.AND_hooks(ref _code);
            if (0 >= exec.Count) {
                exec = null;
                if (_code is null) {
                    _code = true;
                    return true;
                }
            }
            return _code ?? false;
        }

    }

    class LogicalAND : hook
    {
        private bool? _code = null;
        private List<hook>? guard = null;
        private List<hook>? exec = null;
        public void AddListener(hook src) => (exec ??= new()).Add(src);
        public void AddGuard(hook src) => (guard ??= new()).Add(src);

        public LogicalAND() { }
        public bool? code { get { return _code; } }
        public void update(hook h) {
            if (h.code is null) return;
            if (guard is null) return;
            bool test = h.code.Value;
            var ub = guard.Count;
            while (0 <= --ub) {
                if (h == guard[ub]) {
                    if (h.code.Value) {
                        guard.RemoveAt(ub);
                        if (0 >= guard.Count) guard = null;
                        return;
                    }
                    _code = false;
                    guard = null;
                    exec = null;
                    return;
                }
            }
        }

        public bool signal() {
            if (_code is not null) return _code.Value;
            if (guard is not null) {
                guard.AND_hooks(ref _code);
                if (0 >= guard.Count) guard = null;
                if (_code is not null) return _code.Value;
            }

            if (exec is null) {
                _code = true;
                return true;
            }
            exec.AND_hooks(ref _code);
            if (0 >= exec.Count) {
                exec = null;
                if (_code is null) {
                    _code = true;
                    return true;
                }
            }
            return _code ?? false;
        }
    }

    class LogicalOR : hook
    {
        private bool? _code = null;
        private List<hook>? guard = null;
        private List<hook>? exec = null;
        public void AddListener(hook src) => (exec ??= new()).Add(src);
        public void AddGuard(hook src) => (guard ??= new()).Add(src);

        public LogicalOR() { }
        public bool? code { get { return _code; } }
        public void update(hook h) {
            if (h.code is null) return;
            if (guard is null) return;
            bool test = h.code.Value;
            var ub = guard.Count;
            while (0 <= --ub) {
                if (h == guard[ub]) {
                    if (!h.code.Value) {
                        guard.RemoveAt(ub);
                        if (0 >= guard.Count) guard = null;
                        return;
                    }
                    guard = null;
                    return;
                }
            }
        }

        public bool signal()
        {
            if (_code is not null) return true;
            if (guard is not null) {
                bool? ret = guard.OR_hooks();
                if (ret is not null) {
                    guard = null;
                    if (!ret.Value) {
                        _code = false;
                        return true;
                    }
                }
            }

            if (exec is null) {
                _code = true;
                return true;
            }
            exec.AND_hooks(ref _code);
            if (0 >= exec.Count) {
                exec = null;
                if (_code is null) {
                    _code = true;
                    return true;
                }
            }
            return _code ?? false;
        }
    }

    static public class Lazy_ext
    {
        static public void AND_hooks(this List<hook> exec, ref bool? code) {
            var ub = exec.Count;
            while (0 <= --ub) {
                var test = exec[ub].code;
                if (test is not null) {
                    if (false == test.Value) code = false;
                    exec.RemoveAt(ub);
                    continue;
                }
                if (exec[ub].signal()) {
                    var res = exec[ub].code.Value;
                    if (!res) code = false;
                    exec.RemoveAt(ub);
                    continue;
                }
            }
        }

        static public bool? OR_hooks(this List<hook> exec)
        {
            var ub = exec.Count;
            while (0 <= --ub) {
                var test = exec[ub].code;
                if (test is not null) {
                    if (test.Value) return true;
                    exec.RemoveAt(ub);
                    continue;
                }
                if (exec[ub].signal()) {
                    var res = exec[ub].code.Value;
                    if (res) return true;
                    exec.RemoveAt(ub);
                    continue;
                }
            }
            return (0 >= exec.Count) ? false : null;
        }
    }
}
