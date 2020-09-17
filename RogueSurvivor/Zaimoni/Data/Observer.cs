using System;
using System.Collections.Generic;

namespace Zaimoni.Data
{
    // observer pattern support
    public interface Observer<in T>
    {
        /// <returns>true if and only if should be de-registered</returns>
        bool update(T src);
    }

    [Serializable]
    public class Observed<T>
    {
        private List<Observer<T>> m_Watchers = new List<Observer<T>>();

        public void update(T src) {
            var ub = m_Watchers.Count;
            while (0 <= --ub) {
                if (m_Watchers[ub].update(src)) m_Watchers.RemoveAt(ub);
            }
        }

        public void Add(Observer<T> src) { if (!m_Watchers.Contains(src)) m_Watchers.Add(src); }
    }
}
