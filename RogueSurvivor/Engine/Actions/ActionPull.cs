﻿using System;
using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    internal interface ObjectOrigin
    {
        Location obj_origin { get; }  // of m_Actor
    }

    [Serializable]
    class ActionPull : ActorAction, ActorDest, ObjectOrigin
    {
        #region Fields
        readonly MapObject m_Object;
        readonly Location m_MoveActorTo;
        #endregion

        public Location dest { get { return m_MoveActorTo; } }
        public Location obj_origin { get { return m_Object.Location; } }

        #region Init
        public ActionPull(Actor actor, MapObject pullObj, Direction moveActorDir) : base(actor)
        {
            m_Object = pullObj
#if DEBUG
                ?? throw new ArgumentNullException(nameof(pullObj))
#endif
            ;
            m_MoveActorTo = m_Actor.Location + moveActorDir;
            if (!Map.Canonical(ref m_MoveActorTo)) throw new ArgumentNullException(nameof(m_MoveActorTo));
        }

        public ActionPull(Actor actor, MapObject pullObj, Location moveActorTo) : base(actor)
        {
            if (!Map.Canonical(ref moveActorTo)) throw new ArgumentNullException(nameof(moveActorTo));
            m_Object = pullObj
#if DEBUG
                ?? throw new ArgumentNullException(nameof(pullObj))
#endif
            ;
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
            RogueGame.Game.DoPull(m_Actor, m_Object, in m_MoveActorTo);
        }
        #endregion

    }
}
