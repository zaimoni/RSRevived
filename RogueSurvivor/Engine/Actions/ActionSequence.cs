using System;
using System.Collections.Generic;
using System.Linq;
using djack.RogueSurvivor.Data;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Engine.Actions
{
    [Serializable]
    internal class ActionSequence : ActorAction, IJsonStaged<ActionSequence>
    {
        private readonly int _AP;
        private readonly int _turn;
        private readonly List<ZeroAryBehaviors> _handler_codes;
        [NonSerialized] private ActorAction _result;

        #region JSON save/load
        // the live constructor reads the actor's action points and clock: these are the saved ones
        private ActionSequence(Actor actor, JsonElement src, JsonSerializerOptions options) : base(actor)
        {
            _AP = src.Require("AP").GetInt32();
            _turn = src.Require("Turn").GetInt32();
            _handler_codes = new();
            foreach (var x in src.Require("Behaviors").EnumerateArray()) _handler_codes.Add(JSON_ext.ParseEnum<ZeroAryBehaviors>(x.GetString()));
        }

        static ActionSequence IJsonStaged<ActionSequence>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new ActionSequence(JsonActor(src), src, options));

        public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            toJsonActor(writer);
            writer.WriteNumber("AP", _AP);
            writer.WriteNumber("Turn", _turn);
            writer.WriteStartArray("Behaviors");
            foreach (var x in _handler_codes) writer.WriteStringValue(x.ToString());
            writer.WriteEndArray();
        }
        #endregion

        public ActionSequence(Actor actor, IEnumerable<ZeroAryBehaviors> codes)
        : base(actor)
        {
#if DEBUG
            if (null == codes || !codes.Any()) throw new ArgumentNullException(nameof(codes));
#endif
            _AP = m_Actor.ActionPoints;
            _turn = m_Actor.Location.Map.LocalTime.TurnCounter;
            _handler_codes = codes.ToList();
        }

        public override bool IsLegal()
        {
            if (_AP <= m_Actor.ActionPoints && _turn >= m_Actor.Location.Map.LocalTime.TurnCounter) return true;
            if (0 < _handler_codes.Count && !m_Actor.Controller.IsMyTurn()) return true;    // XXX i.e., this ActionSequence class will not work for PlayerController
            return (_result ??= _resolve())?.IsPerformable() ?? false;
        }

        public override void Perform()
        {
            (_result ??= _resolve())?.Perform();
            _result = null;
        }

        private ActorAction? _resolve()
        {
            while(0 < _handler_codes.Count) {
                var working = m_Actor.Controller.ExecAryZeroBehavior(_handler_codes[0]);
                if (null != working && working.IsPerformable()) return working;
                // \todo we're using a list so we can append further handlers
                _handler_codes.RemoveAt(0);
            }
            return null;
        }
    }
}
