using System;
using djack.RogueSurvivor.Data;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Engine.Actions
{
    [Serializable]
    class ActionPull : ActorAction, ActorDest
    {
        #region Fields
        readonly MapObject m_Object;
        readonly Location m_MoveActorTo;
        #endregion

        // this can be null during pathfinding
        public Location dest { get { return m_MoveActorTo; } }

        #region Init
        public ActionPull(Actor actor, MapObject pullObj, Direction moveActorDir) : base(actor)
        {
#if DEBUG
            if (pullObj == null) throw new ArgumentNullException(nameof(pullObj));
#endif
            m_Object = pullObj;
            m_MoveActorTo = m_Actor.Location + moveActorDir;
            if (!Map.Canonical(ref m_MoveActorTo)) throw new ArgumentNullException(nameof(m_MoveActorTo));
        }

        public ActionPull(Actor actor, MapObject pullObj, Location moveActorTo) : base(actor)
        {
#if DEBUG
            if (pullObj == null) throw new ArgumentNullException(nameof(pullObj));
#endif
            if (!Map.Canonical(ref moveActorTo)) throw new ArgumentNullException(nameof(moveActorTo));
            m_Object = pullObj;
            m_MoveActorTo = moveActorTo;
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            return m_Actor.CanPull(m_Object, in m_MoveActorTo, out m_FailReason);
        }

        public override bool IsPerformable()
        {
            if (!base.IsPerformable()) return false;
            return  1==Rules.GridDistance(m_Actor.Location, m_Object.Location)   // no pull/push through vertical exits
                 && 1==Rules.GridDistance(m_Actor.Location, m_MoveActorTo);
        }

        public override void Perform()
        {
            RogueForm.Game.DoPull(m_Actor, m_Object, in m_MoveActorTo);
        }
        #endregion

    }
}
