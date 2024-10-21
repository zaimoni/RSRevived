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
    // competes with class ActionSequence
    [Serializable]
    internal class BehavioristABC : ActorAction, Actions.Resolvable
    {
        private readonly ReflexCode _reflexCode;
        [NonSerialized] private bool _resolved = false;
        [NonSerialized] private ActorAction? _action = null;

        public BehavioristABC(Actor whom, ReflexCode code) : base(whom) {
            _reflexCode = code;
        }
        public override bool IsLegal() {
            resolve();
            if (!_resolved) return true;
            if (null == _action) return false;
            return _action.IsLegal();
        }
        public override bool IsPerformable() {
            resolve();
            if (!_resolved) return true;
            if (null == _action) return false;
            return _action.IsPerformable();
        }
        public override void Perform() { _action?.Perform(); }

        public ActorAction ConcreteAction { get {
          resolve();
          return _action;
        } }

        private void resolve() {
            if (!_resolved && m_Actor.Location.Map.IsMyTurn(m_Actor)) {
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
