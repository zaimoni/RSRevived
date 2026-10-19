using System;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Gameplay.AI;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Engine.Goal
{
    [Serializable]
    internal class NextAction : Objective, IJsonStaged<NextAction>
    {
        private readonly ActorAction? Intent_engaged;
        private readonly ActorAction? Intent_disengaged;
        private readonly WorldUpdate? Plan_engaged;
        private readonly WorldUpdate? Plan_disengaged;

        #region JSON save/load

        private NextAction(Actor who, JsonElement src, JsonSerializerOptions options) : base(who, src)

        {

            if (src.TryGetProperty("Engaged", out var x)) Intent_engaged = x.Deserialize<ActorAction>(options);

            if (src.TryGetProperty("Disengaged", out x)) Intent_disengaged = x.Deserialize<ActorAction>(options);

            if (src.TryGetProperty("PlanEngaged", out x)) Plan_engaged = x.Deserialize<WorldUpdate>(options);

            if (src.TryGetProperty("PlanDisengaged", out x)) Plan_disengaged = x.Deserialize<WorldUpdate>(options);

            if (Intent_engaged is null && Intent_disengaged is null && Plan_engaged is null && Plan_disengaged is null) throw new JsonException("next action with no intent");

        }


        static NextAction IJsonStaged<NextAction>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new NextAction(JsonActor(src), src, options));


        public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)

        {

            toJsonBase(writer);

            void slot<T>(string name, T? value) where T : class {

                if (value is null) return;

                writer.WritePropertyName(name);

                JsonSerializer.Serialize(writer, value, options);

            }

            slot("Engaged", Intent_engaged);

            slot("Disengaged", Intent_disengaged);

            slot("PlanEngaged", Plan_engaged);

            slot("PlanDisengaged", Plan_disengaged);

        }

        #endregion


        public NextAction(Actor who, ActorAction engaged) : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
#if DEBUG
            if (engaged is null) throw new ArgumentNullException(nameof(engaged));
#endif
            Intent_engaged = engaged;
        }

        public NextAction(Actor who, ActorAction? engaged, ActorAction? disengaged)
        : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
#if DEBUG
            if (engaged is null && disengaged is null) throw new ArgumentNullException(nameof(engaged) + "; " + nameof(disengaged));
#endif
            Intent_engaged = engaged;
            Intent_disengaged = disengaged;
        }

        public NextAction(Actor who, ActorAction? engaged, WorldUpdate? disengaged)
        : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
#if DEBUG
            if (engaged is null && disengaged is null) throw new ArgumentNullException(nameof(engaged) + "; " + nameof(disengaged));
#endif
            Intent_engaged = engaged;
            Plan_disengaged = disengaged;
        }

        public NextAction(Actor who, WorldUpdate engaged) : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
#if DEBUG
            if (engaged is null) throw new ArgumentNullException(nameof(engaged));
#endif
            Plan_engaged = engaged;
        }

        public NextAction(Actor who, WorldUpdate? engaged, ActorAction? disengaged)
        : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
#if DEBUG
            if (engaged is null && disengaged is null) throw new ArgumentNullException(nameof(engaged) + "; " + nameof(disengaged));
#endif
            Plan_engaged = engaged;
            Intent_disengaged = disengaged;
        }

        public NextAction(Actor who, WorldUpdate? engaged, WorldUpdate? disengaged)
        : base(who.Location.Map.LocalTime.TurnCounter, who)
        {
#if DEBUG
            if (engaged is null && disengaged is null) throw new ArgumentNullException(nameof(engaged) + "; " + nameof(disengaged));
#endif
            Plan_engaged = engaged;
            Plan_disengaged = disengaged;
        }

        // always execute.  Expire on execution
        public override bool UrgentAction_readonly(out ActorAction? ret)
        {
          ret = null;   // one-shot, and binding a plan is an act: never pruned at a save
          if (   (Intent_engaged?.IsLegal() ?? false)
              || (Intent_disengaged?.IsLegal() ?? false)
              || (Plan_engaged?.IsLegal() ?? false)
              || (Plan_disengaged?.IsLegal() ?? false)
              ) return false;   // legal, not performable: a save is not now
          return true;
        }

        public override bool UrgentAction(out ActorAction? ret)
        {
            ret = null;
            _isExpired = true;
            if (m_Actor.Controller.enemies_in_FOV is null) {
                ret = _intentDisengaged();
            } else {
                ret = _intentEngaged();
            }
            return true;
        }

        private ActorAction? _intentEngaged() {
            if (Intent_engaged is not null && Intent_engaged.IsPerformable()) return Intent_engaged;
            if (Plan_engaged is not null) return Plan_engaged.Bind(m_Actor);
            return null;
        }

        private ActorAction? _intentDisengaged()
        {
            if (Intent_disengaged is not null && Intent_disengaged.IsPerformable()) return Intent_disengaged;
            if (Plan_disengaged is not null) return Plan_disengaged.Bind(m_Actor);
            return null;
        }
    }
}
