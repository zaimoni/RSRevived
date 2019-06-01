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
        readonly Point m_MoveActorTo;
        #endregion

        #region Properties
        // this can be null during pathfinding
        public Direction MoveActorDirection { get { return Direction.FromVector(m_MoveActorTo.X - m_Actor.Location.Position.X, m_MoveActorTo.Y - m_Actor.Location.Position.Y); } }
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
            m_MoveActorTo = m_Actor.Location.Position + moveActorDir;
        }

        public ActionPull(Actor actor, MapObject pullObj, Point moveActorTo)
            : base(actor)
        {
#if DEBUG
            if (pullObj == null) throw new ArgumentNullException(nameof(pullObj));
#endif

            m_Object = pullObj;
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
