using System;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    [Serializable]
    class ActionPull : ActorAction, ActorDest
    {
        #region Fields
        readonly MapObject m_Object;
        readonly Direction m_MoveActorDir;
        readonly Point m_MoveActorTo;
        #endregion

        #region Properties
        public Direction MoveActorDirection { get { return m_MoveActorDir; } }  // \todo savefile break: convert this to a calculated field
        public Point MoveActorTo { get { return m_MoveActorTo; } }
        public Location dest { get { return new Location(m_Object.Location.Map, m_MoveActorTo); } }
        #endregion

        #region Init
        public ActionPull(Actor actor, MapObject pullObj, Direction moveActorDir)
            : base(actor)
        {
#if DEBUG
            if (pullObj == null) throw new ArgumentNullException(nameof(pullObj));
#endif

            m_Object = pullObj;
            m_MoveActorDir = moveActorDir;
            m_MoveActorTo = m_Actor.Location.Position + moveActorDir;
        }

        public ActionPull(Actor actor, MapObject pullObj, Point moveActorTo)
            : base(actor)
        {
#if DEBUG
            if (pullObj == null) throw new ArgumentNullException(nameof(pullObj));
#endif

            m_Object = pullObj;
            m_MoveActorDir = Direction.FromVector(moveActorTo.X-actor.Location.Position.X, moveActorTo.Y - actor.Location.Position.Y);
#if FALSE_POSITIVE
            if (null == m_MoveActorDir) throw new ArgumentNullException(nameof(m_MoveActorDir));    // pathfinder can trigger this
#endif
            m_MoveActorTo = moveActorTo;
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            return m_Actor.CanPull(m_Object, m_MoveActorTo, out m_FailReason);
        }

        public override bool IsPerformable()
        {
            if (!base.IsPerformable()) return false;
            return  1==Rules.GridDistance(m_Actor.Location, m_Object.Location)   // no pull/push through vertical exits
                 && 1==Rules.GridDistance(m_Actor.Location, new Location(m_Object.Location.Map, m_MoveActorTo));
        }

        public override void Perform()
        {
            RogueForm.Game.DoPull(m_Actor, m_Object, m_MoveActorTo);
        }
        #endregion

    }
}
