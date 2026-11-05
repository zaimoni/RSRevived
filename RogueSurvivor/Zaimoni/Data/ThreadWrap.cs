using System;
using System.Collections.Generic;
using System.Threading;

namespace Zaimoni.Data
{
    public class ThreadWrap
    {
        private static readonly object m_SimStateLock = new object();
        private static CancellationTokenSource? m_CancelSource = null;
        private static List<ThreadWrap> m_Registry = new();

        private Thread? m_SimThread = null;
        public readonly string Name;

        public ThreadWrap(string name) {
            Name = name;
        }

        public bool IsAlive => m_SimThread?.IsAlive ?? false;
        public bool IsMe => Name == Thread.CurrentThread.Name;

        public static bool IsCancelled { get {
            lock (m_SimStateLock) {
                return m_CancelSource?.IsCancellationRequested ?? true;
            }
        } }

        static public bool IsSimulating { get {
            var i_am = Thread.CurrentThread.Name;
            foreach (var t in m_Registry) if (t.Name == i_am) return true;
            return false;
        } }

        public void Start(Action handler) {
            if (m_SimThread is null) {
                m_SimThread = new Thread(new ThreadStart(handler)) {
                    Name = Name,
                    IsBackground = true
                };
            }
            lock (m_SimStateLock) {
                if (m_CancelSource is null) m_CancelSource = new();
                m_Registry.Add(this);
            }
            m_SimThread.Start();
        }

        public void Stop() {
            if (m_SimThread is null) return;

            // .NET 5.0: Thread::Abort() becomes unsupported
            lock (m_SimStateLock) {
                m_CancelSource.Cancel();
                m_Registry.Remove(this);
                if (0 >= m_Registry.Count) {
                    m_CancelSource.Dispose();
                    m_CancelSource = null;
                }
            }

        retry:
            bool _stopped = m_SimThread.Join(300); // outlast the Sleep call
            if (!_stopped && m_SimThread.IsAlive) goto retry;
            m_SimThread = null;
        }
    }
}
