using System;
using System.Collections.Generic;
using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Gameplay.AI
{
    [Serializable]
    internal abstract class Objective
    {
      protected int turn;   // turn count of WorldTime .. will need a more complex representation at some point.
      protected readonly Actor m_Actor;   // owning actor is likely important
      protected bool _isExpired;

      public int TurnCounter { get { return turn; } }
      public bool IsExpired { get { return _isExpired; } }
      public Actor Actor { get { return m_Actor; } }

      protected Objective(int t0, Actor who)
      {
#if DEBUG
         if (null == who) throw new ArgumentNullException(nameof(who));
#endif
         turn = t0;
         m_Actor = who;
      }

      /// <param name="ret">null triggers deletion.  non-null ret.IsPerformable() must be true</param>
      /// <returns>true to take action</returns>
      public abstract bool UrgentAction(out ActorAction ret);

      public virtual List<Objective> Subobjectives() { return null; }
    }

    internal interface Pathable
    {
        ActorAction Pathing();
    }
}
