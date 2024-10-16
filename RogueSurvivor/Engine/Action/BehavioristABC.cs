using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace djack.RogueSurvivor.Engine._Action
{
    // monolith code...not ideal, but trying to manage save/load issues
    // tentatively assuming not Resolvable because the resolving function may return null
    // competes with class ActionSequence
    [Serializable]
    internal class BehavioristABC : ActorAction
    {
        private readonly ReflexCode _reflexCode;
        [NonSerialized] private bool _resolved = false;
        [NonSerialized] private ActorAction? _action = null;

        public BehavioristABC(Actor whom, ReflexCode code) : base(whom) {
            _reflexCode = code;
        }
        public override bool IsLegal() { return true; }
        public override bool IsPerformable() {
            resolve();
            return null != _action;
        }
        public override void Perform() { _action?.Perform(); }

        private void resolve() {
            if (!_resolved) {
                _resolved = true;
                switch (_reflexCode) {
                case ReflexCode.RecruitedLOS:
                    _action = (m_Actor.Controller as Gameplay.AI.ObjectiveAI)?.RecruitedLOS();
                    return;
                }
            }
        }

        public enum ReflexCode {
            RecruitedLOS = 1
        }
    }
}
