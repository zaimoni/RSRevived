using System;
using System.Collections.Generic;
using System.Linq;

namespace Zaimoni.Data
{
    internal static partial class Virtual
    {
        // Some library classes have less than useful ToString() overrides.
        // We go with Ruby syntax x.to_s() rather than Python syntax str(x)
        public static string to_s<T>(this HashSet<T> x)
        {
            if (null == x) return "null";
            var ub = x.Count;
            if (0 >= ub) return "{}";
            var tmp = new List<string>(ub);
            foreach (T iter in x)
            {
                tmp.Add(iter.to_s());
            }
            tmp[0] = "{" + tmp[0];
            ub = tmp.Count;
            tmp[ub - 1] += "} (" + ub.ToString() + ")";
            return string.Join(",\n", tmp);
        }

        public static string to_s<T>(this List<List<T>> x)
        {  // would be redundant in C++
            if (null == x) return "null";
#if DEBUG
            throw new InvalidOperationException("eliminate this?");
#endif
            var ub = x.Count;
            if (0 >= ub) return "[]";
            var tmp = new List<string>(ub);
            foreach (var iter in x)
            {
                tmp.Add(iter.to_s());
            }
            tmp[0] = "[" + tmp[0];
            ub = tmp.Count;
            tmp[ub - 1] += "] (" + ub.ToString() + ")";
            return string.Join(",\n", tmp);
        }

        public static string to_s<T>(this List<T[]> x)
        {
            if (null == x) return "null";
            var ub = x.Count;
            if (0 >= ub) return "[]";
            var tmp = new List<string>(ub);
            foreach (var iter in x)
            {
                tmp.Add(iter.to_s());
            }
            tmp[0] = "[" + tmp[0];
            ub = tmp.Count;
            tmp[ub - 1] += "] (" + ub.ToString() + ")";
            return string.Join(",\n", tmp);
        }

        public static string to_s<T>(this T[] x)
        {
            if (null == x) return "null";
            var ub = x.Length;
            if (0 >= ub) return "[]";
            var tmp = new List<string>(ub);
            foreach (var iter in x)
            {
                tmp.Add(iter.to_s());
            }
            tmp[0] = "[" + tmp[0];
            ub = tmp.Count;
            tmp[ub - 1] += "] (" + ub.ToString() + ")";
            return string.Join(",\n", tmp);
        }

        public static string to_s<T>(this List<T> x)
        {
            if (null == x) return "null";
            var ub = x.Count;
            if (0 >= ub) return "[]";
            var tmp = new List<string>(ub);
            foreach (var iter in x)
            {
                tmp.Add(iter.to_s());
            }
            tmp[0] = "[" + tmp[0];
            ub = tmp.Count;
            tmp[ub - 1] += "] (" + ub.ToString() + ")";
            return string.Join(",\n", tmp);
        }

        public static string to_s<T, U>(this Dictionary<T, List<U>> x)
        {
            if (null == x) return "null";
#if DEBUG
            throw new InvalidOperationException("eliminate this?");
#endif
            var ub = x.Count;
            if (0 >= ub) return "{}";
            var tmp = new List<string>(ub);
            foreach (var iter in x)
            {
                tmp.Add(iter.Key.to_s() + ":" + iter.Value.to_s());
            }
            tmp[0] = "{" + tmp[0];
            ub = tmp.Count;
            tmp[ub - 1] += "} (" + ub.ToString() + ")";
            return string.Join(",\n", tmp);
        }

        public static string to_s<T, U>(this Dictionary<T, U> x)
        {
            if (null == x) return "null";
            var ub = x.Count;
            if (0 >= ub) return "{}";
            var tmp = new List<string>(ub);
            foreach (var iter in x)
            {
                tmp.Add(iter.Key.to_s() + ":" + iter.Value.to_s());
            }
            tmp[0] = "{" + tmp[0];
            ub = tmp.Count;
            tmp[ub - 1] += "} (" + ub.ToString() + ")";
            return string.Join(",\n", tmp);
        }

        public static string to_s<T>(this IEnumerable<T> x)
        {
            if (null == x) return "null";
            var ub = x.Count();
            if (0 >= ub) return "[]";
            var tmp = new List<string>(ub);
            foreach (T iter in x)
            {
                tmp.Add(iter.to_s());
            }
            tmp[0] = "[" + tmp[0];
            ub = tmp.Count;
            tmp[ub - 1] += "] (" + ub.ToString() + ")";
            return string.Join(",\n", tmp);
        }

        // ultimate fallback
        public static string to_s<T>(this T x)
        {
            if (null == x) return "null";
            var t_info = typeof(T);

            // Try to simulate at runtime what C++ does at compile time
            // want to catch instance members T::to_s()
            var to_s_candidate = t_info.GetMethod("to_s", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (null != to_s_candidate) {
                return (string)to_s_candidate.Invoke(x, null);
            }

            if (t_info.IsGenericType) { // will not handle arrays
                var t_name = t_info.FullName;
                var method_name = string.Empty;
                var t_args = t_info.GetGenericArguments();
                if (1 == t_args.Length) {
                    t_name = t_name.Substring(0, t_name.IndexOf('['));
                    method_name = "System.String to_s[T]("+t_name+"[T])";
                }
                var method_candidates = typeof(Virtual).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public
                            | System.Reflection.BindingFlags.NonPublic);
                if (method_candidates.Any(x => x.Name != "to_s")) method_candidates = method_candidates.Where(x => x.Name == "to_s").ToArray();
                if (!string.IsNullOrEmpty(method_name) && method_candidates.Any(x => x.ToString() == method_name)) {
                    method_candidates = method_candidates.Where(x => x.ToString() == method_name).ToArray();
                    var exec = method_candidates[0].MakeGenericMethod(t_args);
                    return (string)exec.Invoke(null, new object[] { x });
                }
#if DEBUG
                throw new InvalidOperationException("test case: "+t_name+", "+t_args.Length.ToString()+", "+method_candidates.Length.ToString());
#endif
            }
            return x.ToString();
        }
    }
}
