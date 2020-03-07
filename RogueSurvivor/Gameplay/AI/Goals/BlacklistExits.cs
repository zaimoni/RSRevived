using System;
using System.Collections.Generic;

using djack.RogueSurvivor.Data;
using Zaimoni.Data;

// work required to make Exit class suitable for C# hashing deemed excessive (Actor was problematic)
// but likely would address CPU issues (current implementation is quadratic)
namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    [Serializable]
    class BlacklistExits : Objective
    {
        private readonly Dictionary<int, List<Exit>> _blacklist = new Dictionary<int, List<Exit>>();

        public BlacklistExits(int t0, Actor who, Exit e)
        : base(t0, who)
        {
            Blacklist(e);
        }

        // this is an influence on behavior; never generates an action
        public override bool UrgentAction(out ActorAction ret)
        {
            ret = null;
            var turn = Engine.Session.Get.WorldTime.TurnCounter;
            var d_size = Engine.RogueGame.Options.DistrictSize;
            _blacklist.OnlyIf(time => turn <= time+d_size);
            if (0 >= _blacklist.Count) _isExpired = true;
            return false;
        }

        public void Blacklist(Exit e) {
            var del = new Zaimoni.Data.Stack<int>(stackalloc int[_blacklist.Count]);
            var turn = Engine.Session.Get.WorldTime.TurnCounter;
            bool landed = false;
            foreach (var x in _blacklist) {
                if (x.Key == turn) {
                    if (!x.Value.Contains(e)) x.Value.Add(e);
                    landed = true;
                    continue;
                }
                if (x.Value.Contains(e)) {
                    x.Value.Remove(e);
                    if (0 >= x.Value.Count) del.push(x.Key);
                }
            }
            if (!landed) _blacklist.Add(turn, new List<Exit> { e });
            int i = del.Count;
            while (0 <= --i) _blacklist.Remove(del[i]);
        }

        public bool Veto(Exit e) {
            foreach (var x in _blacklist) if (x.Value.Contains(e)) return true;
            return false;
        }
    }
}
