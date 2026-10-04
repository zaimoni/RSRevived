using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Zaimoni.JSON;

#nullable enable

namespace djack.RogueSurvivor.Engine._Action
{
    // monolith code...not ideal, but trying to manage save/load issues
    // competes with class ActionSequence
    [Serializable]
    internal class BehavioristABC : ActorAction, Actions.Resolvable, IJsonStaged<BehavioristABC>
    {
        private readonly ReflexCode _reflexCode;
        [NonSerialized] private bool _resolved = false;
        [NonSerialized] private ActorAction? _action = null;

        #region JSON save/load
        private BehavioristABC(Actor actor, JsonElement src) : base(actor)
        {
            _reflexCode = src.RequireEnum<ReflexCode>("Reflex");
        }

        static BehavioristABC IJsonStaged<BehavioristABC>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new BehavioristABC(JsonActor(src), src));

        public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            toJsonActor(writer);
            writer.WriteString("Reflex", _reflexCode.ToString());
        }
        #endregion

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
