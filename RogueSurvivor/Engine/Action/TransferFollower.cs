using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace djack.RogueSurvivor.Engine._Action
{
    internal class TransferFollower : ActorAction
    {
        private Actor whom;
        private Actor dest;
        private Action<Actor, Actor, Actor>? messaging; // rules out saving to hard drive

        public TransferFollower(Actor authority, Actor who, Actor leads, Action<Actor, Actor, Actor>? msg = null) : base(authority) {
#if DEBUG
            var prior = who.LiveLeader;
            if (null != prior && authority != prior) throw new InvalidOperationException("invalid transfer of follower");
            if (null == prior && authority != who && authority != leads) throw new InvalidOperationException("invalid transfer of follower");
#endif
            whom = who;
            dest = leads;
            messaging = msg;
        }

        static public string ReasonIllegalToBeLed(Actor who)
        {
            if (who.Model.Abilities.IsUndead) return "undead";
            return string.Empty;
        }
        static public string ReasonCannotBeLed(Actor who)
        {
          if (who.IsSleeping) return "sleeping";
//        if (HasLeader) return "already has a leader";
          if (0 < who.CountFollowers) return "is a leader";  // XXX organized force would have a chain of command
          if (who.Controller.InCombat && !who.IsPlayer) return who.Name+" in combat";
          return string.Empty;
        }

        static public string ReasonIllegalToLead(Actor who)
        {
            int num = who.MaxFollowers;
            if (num == 0) return "can't lead";
            if (who.CountFollowers >= num) return "too many followers";
            return string.Empty;
        }
        static public string ReasonCannotLead(Actor who)
        {
            if (!who.IsPlayer && who.Controller.InCombat) return "in combat";
            return string.Empty;
        }
        public override bool IsLegal()
        {
            m_FailReason = ReasonIllegalToBeLed(whom);
            if (!string.IsNullOrEmpty(m_FailReason)) return false;
            m_FailReason = ReasonIllegalToLead(dest);
            if (!string.IsNullOrEmpty(m_FailReason)) return false;
            return true;
        }
        public override bool IsPerformable()
        {
            if (!IsLegal()) return false;
            m_FailReason = ReasonCannotBeLed(whom);
            if (!string.IsNullOrEmpty(m_FailReason)) return false;
            m_FailReason = ReasonCannotLead(dest);
            if (!string.IsNullOrEmpty(m_FailReason)) return false;
            return true;
        }
        public override void Perform()
        {
            m_Actor.SpendActionPoints();
            var old_leader = whom.LiveLeader;
            if (null != old_leader) {
                old_leader.RemoveFollower(whom);
                whom.SetTrustIn(old_leader, whom.TrustInLeader);
            }
            if (whom.IsPlayer && !dest.IsPlayer) dest.MakePC();
            dest.AddFollower(whom);
            int trustIn = whom.GetTrustIn(dest);
            whom.TrustInLeader = trustIn;
            if (null != messaging) messaging(m_Actor, whom, dest);
        }

    }
}
