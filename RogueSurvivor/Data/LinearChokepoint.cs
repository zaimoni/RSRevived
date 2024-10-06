using System;
using djack.RogueSurvivor.Engine;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Data
{
    // usage is as follows:
    // a non-PC ObjectiveAI that wishes to use a chokepoint must declare an ideal route no later than on entry.
    // this allows allied/friendly ais to read plans and de-conflict pathing by using acceptable pushes or pulls

    [Serializable]
    public sealed class LinearChokepoint  // must be a class due to wanting to reuse object instances in data structures
    {
        public readonly Location[] Entrance;    // adjacent to Chokepoint[0]
        public readonly Location[] Chokepoint;
        public readonly Location[] Exit;        // adjacent to other end of Chokepoint

        public LinearChokepoint(Location[] enter, Location[] choke, Location[] exit)
        {
#if DEBUG
            if (null == enter) throw new ArgumentNullException(nameof(enter));
            if (null == choke) throw new ArgumentNullException(nameof(choke));
            if (null == exit) throw new ArgumentNullException(nameof(exit));

            int end = choke.Length - 1;
            foreach (var x in enter) if (1 != Rules.InteractionDistance(in x, in choke[0])) throw new InvalidOperationException("topology failed");
            foreach (var x in exit) if (1 != Rules.InteractionDistance(in x, in choke[end])) throw new InvalidOperationException("topology failed");
            foreach (var x in enter) foreach(var y in exit) if (1 == Rules.InteractionDistance(in x, in y)) throw new InvalidOperationException("topology failed");
#endif
            Entrance = enter;
            Chokepoint = choke;
            Exit = exit;
        }

      public int Contains(Location loc)
      {
        if (0<=Array.IndexOf(Entrance,loc)) return 1;
        if (0<=Array.IndexOf(Chokepoint,loc)) return 2;
        if (0<=Array.IndexOf(Exit,loc)) return 3;
        return 0;
      }

      public override string ToString()
      {
        return "enter: "+Entrance.to_s()+"; choke: "+Chokepoint.to_s()+"; exit: "+Exit.to_s();
      }
    }

    struct UsingLinearChokepoint
    {
        readonly LinearChokepoint Src;
        readonly Location Start;
        readonly Location Final;
        readonly bool parity;    // true for start-in-start, false for start-in-final
        readonly bool pass_through;

        UsingLinearChokepoint(LinearChokepoint src, Location start, Location final)
        {
            Src = src;
            Start = start;
            Final = final;
            int start_entrance = Array.IndexOf(src.Entrance, start);
            int start_exit = Array.IndexOf(src.Exit, start);
            int final_entrance = Array.IndexOf(src.Entrance, final);
            int final_exit = Array.IndexOf(src.Exit, final);
#if DEBUG
            if (0 > start_entrance && 0 > start_exit) throw new InvalidOperationException("improper use of linear chokepoint");
            if (0 > final_entrance && 0 > final_exit) throw new InvalidOperationException("improper use of linear chokepoint");
#endif
            parity = 0 <= start_entrance;
            pass_through = (0 <= start_entrance)==(0 <= start_exit);
        }
    }
}
