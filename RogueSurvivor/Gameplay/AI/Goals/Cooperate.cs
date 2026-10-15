using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;
using System;
using System.Collections.Generic;
using Zaimoni.Data;
using System.Text.Json;
using Zaimoni.JSON;

#nullable enable

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    // need this level of indirection so that all actors know about the plan's progress
    [Serializable]
    public sealed class SharedPlan : IJsonStaged<SharedPlan>
    {
        private WorldUpdate m_Plan;
        private Condition<Actor> m_Target;
        private bool m_Expired = false;

        public bool IsExpired { get { return m_Expired; } }

#region JSON save/load
        // Shared between the cooperating actors: has an $id (Sealed<SharedPlan>).  Condition<Actor> has one implementor, keyed by its name.
        private SharedPlan(JsonElement src, JsonSerializerOptions options)
        {
            m_Plan = src.Require("Plan").Deserialize<WorldUpdate>(options) ?? throw new JsonException("null Plan");
            var target = src.Require("Target");
            if (target.TryGetProperty("CoverTrap", out var x)) m_Target = Engine._Goal.CoverTrap.fromJson(x, options);
            else throw new JsonException("unknown plan target: " + target.GetRawText());
            if (src.TryGetProperty("Expired", out x)) m_Expired = x.GetBoolean();
        }

        static SharedPlan IJsonStaged<SharedPlan>.fromJson(JsonElement src, JsonSerializerOptions options)
        {
            var ret = new SharedPlan(src, options);
            src.RecordRef(ret);
            return ret;
        }

        public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            writer.WritePropertyName("Plan");
            JsonSerializer.Serialize(writer, m_Plan, options);
            writer.WriteStartObject("Target");
            if (m_Target is Engine._Goal.CoverTrap trap) {
                writer.WritePropertyName("CoverTrap");
                trap.toJson(writer, options);
            } else throw new JsonException("plan target of " + m_Target.GetType().FullName + " is not saved");
            writer.WriteEndObject();
            if (m_Expired) writer.WriteBoolean("Expired", true);
        }
#endregion

        public SharedPlan(WorldUpdate src, Condition<Actor> done) {
#if DEBUG
            if (!src.IsLegal()) throw new InvalidOperationException("illegal objective");
#endif
            m_Plan = src;
            m_Target = done;
        }

        public bool IsLegal() { return !m_Target.IsDone() && m_Plan.IsLegal(); }

        // we ask the plan, because combat plans would not be suppressed just by enemies in sight
        public bool IsSuppressed(Actor a) => m_Plan.IsSuppressed(a);
        public bool IsDone(Actor a) => m_Target.IsDone(a);

        public bool IsRelevant(Location loc) {
#if DEBUG
            if (!m_Plan.IsLegal()) throw new InvalidOperationException("illegal objective");
#else
            if (!m_Plan.IsLegal()) {
                m_Expired = true;
                return false;
            }
#endif
            return m_Plan.IsRelevant(loc);
        }

        public ActorAction? Bind(Actor who) {
            if (m_Plan.IsRelevant(who.Location)) {
                var test = m_Plan;
                while(ForceRelevant(who.Location, ref test));
                var act = test.Bind(who);
                if (null != act) {
                    if (test is Engine.Op.Join join && join.Sequel.IsLegal()) m_Plan = join.Sequel;
                    else m_Expired = true;
                    return act;
                }
            }
            return null;
        }

        public HashSet<Location>? Goals(Actor actor) {
#if DEBUG
            if (!m_Plan.IsLegal()) throw new InvalidOperationException("illegal objective");
#else
            if (!m_Plan.IsLegal()) return null;
#endif
            var ret = new HashSet<Location>();
            m_Plan.Goals(ret);
            m_Plan.Blacklist(ret);
            ret.RemoveWhere(loc => !actor.MayEnter(loc)); // matters for burning cars
            // goal locations should make actions relevant.  If we are *at* a goal location already,
            // bail so we don't self-path.
            if (ret.Contains(actor.Location)) return null;
            return 0 < ret.Count ? ret : null;
        }

        static bool ForceRelevant(Location loc, ref WorldUpdate src) {
            if (src is Engine.Op.Fork fork) return fork.ForceRelevant(loc, ref src);
            if (src is Engine.Op.Join join) return join.ForceRelevant(loc, ref src);
            return false;
        }
    }

    [Serializable]
    class Cooperate : Objective, IJsonStaged<Cooperate>
    {
        private SharedPlan m_Plan;

        #region JSON save/load

        private Cooperate(Actor who, JsonElement src, JsonSerializerOptions options) : base(who, src)

        {

            m_Plan = src.Require("Plan").Deserialize<SharedPlan>(options) ?? throw new JsonException("null Plan");

        }


        static Cooperate IJsonStaged<Cooperate>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new Cooperate(JsonActor(src), src, options));


        public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)

        {

            toJsonBase(writer);

            writer.WritePropertyName("Plan");

            JsonSerializer.Serialize(writer, m_Plan, options);

        }

        #endregion


        public Cooperate(Actor who, SharedPlan plan) : base(who.Location.Map.LocalTime.TurnCounter, who) {
#if DEBUG
            if (!plan.IsLegal()) throw new InvalidOperationException("tried to cooperate with illegal plan");
#endif
            m_Plan = plan;
        }

        public override bool UrgentAction_readonly(out ActorAction? ret)

        {

          ret = null;

          if (m_Plan.IsExpired) return true; // expired

          if (!m_Plan.IsLegal() || m_Plan.IsDone(m_Actor)) {

            _isExpired = true;

            return true;

          }

          return false;

        }


        public override bool UrgentAction(out ActorAction? ret)
        {
            ret = null;
            if (m_Plan.IsExpired) return true; // expired
            if (!m_Plan.IsLegal()) {
                _isExpired = true;
                return true;
            }
            if (m_Plan.IsDone(m_Actor)) {
                _isExpired = true;
                return true;
            }

            if (m_Plan.IsRelevant(m_Actor.Location)) {
                ret = m_Plan.Bind(m_Actor);
                if (null != ret) {
                    if (m_Plan.IsExpired) _isExpired = true;
                    return true;
                }
                ret = new ActionWait(m_Actor); // not necessarily (crowd control issues)
                return true;
            }
            var goals = m_Plan.Goals(m_Actor);
            if (null != goals) {
                ret = (m_Actor.Controller as ObjectiveAI).BehaviorPathTo(goals);
                return null != ret;
            }
            return false;
        }
    }
}
