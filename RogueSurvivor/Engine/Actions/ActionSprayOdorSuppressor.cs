using System;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Items;

namespace djack.RogueSurvivor.Engine.Actions
{
    // alpha10
    class ActionSprayOdorSuppressor : ActorAction,TargetActor,Use<ItemSprayScent>
    {
        #region Fields
        readonly ItemSprayScent m_Spray;
        readonly Actor m_SprayOn;
        #endregion

        #region Init
        public ActionSprayOdorSuppressor(Actor actor, ItemSprayScent spray, Actor sprayOn) : base(actor)
        {
            m_Spray = spray;
            m_SprayOn = sprayOn
#if DEBUG
                ?? throw new ArgumentNullException(nameof(sprayOn))
#endif
            ;
        }
        #endregion

        public Actor Whom { get { return m_SprayOn; } }
        public ItemSprayScent Use { get { return m_Spray; } }

        #region ActorAction
        public override bool IsLegal()
        {
            return m_Actor.CanSprayOdorSuppressor(m_Spray, m_SprayOn, out m_FailReason);
        }

        public override void Perform()
        {
            RogueForm.Game.DoSprayOdorSuppressor(m_Actor, m_Spray, m_SprayOn);
        }
        #endregion
    }
}
