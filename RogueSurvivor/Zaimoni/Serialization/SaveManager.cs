using System.Collections.Generic;

#nullable enable

namespace Zaimoni.Serialization
{
    // these won't be completely reliable with structs
    interface IOnSerializing
    {
        void OnSerializing(in StreamingContext context);
    }

    interface IOnSerialized
    {
        void OnSerialized(in StreamingContext context);
    }

    /// <summary>
    /// Part of cloning the System.Runtime.Serialization specification, which is deprecated and cannot be assumed to be available indefinitely.
    /// This corresponds to SerializationObjectManager
    /// </summary>
    public sealed class SaveManager
    {
        private readonly StreamingContext _context;
        private readonly List<IOnSerialized?> _onSerializedTargets = new List<IOnSerialized?>(); // could use event syntax for this

        public SaveManager(StreamingContext context) {
            _context = context;
        }

        public void Register(object obj)
        {
            if (obj is IOnSerializing x) x.OnSerializing(in _context);
            if (obj is IOnSerialized y)
            {
                lock (_onSerializedTargets)
                {
                    if (!_onSerializedTargets.Contains(y)) _onSerializedTargets.Add(y);
                }
            }
        }

        public void Register<T>(T obj) where T:class {
            if (obj is IOnSerializing x) x.OnSerializing(in _context);
            if (obj is IOnSerialized y) {
                lock (_onSerializedTargets) {
                    if (!_onSerializedTargets.Contains(y)) _onSerializedTargets.Add(y);
                }
            }
        }

        public void RaiseOnSerializedEvent() {
            lock (_onSerializedTargets) {
                var ub = _onSerializedTargets.Count;
                // \todo what we would like to do is "move" the backing array without copying,
                // do the countdown on the backing array, then let it scope out
                while (0 < --ub) {
                    _onSerializedTargets[ub]!.OnSerialized(in _context);
                    _onSerializedTargets[ub] = null; // trigger GC if we had boxed a struct
                }
                _onSerializedTargets.Clear();
            }
        }
    }
}
