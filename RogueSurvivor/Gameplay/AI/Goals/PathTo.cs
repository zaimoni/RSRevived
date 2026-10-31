using System;
using System.Collections.Generic;
using System.Linq;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Actions;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    // Ancestor class Goal_PathTo
    [Serializable]
    internal class PathTo : Objective, LatePathable, IJsonStaged<PathTo>
    {
      private readonly HashSet<Location> _locs;
      private readonly bool walking;
      private int turns;

      #region JSON save/load

      private PathTo(Actor who, JsonElement src, JsonSerializerOptions options) : base(who, src)

      {

        _locs = src.Require("Locations").Deserialize<HashSet<Location>>(options) ?? throw new JsonException("null Locations");

        if (src.TryGetProperty("Walking", out var x)) walking = x.GetBoolean();

        turns = src.TryGetProperty("Turns", out x) ? x.GetInt32() : int.MaxValue;

      }


      static PathTo IJsonStaged<PathTo>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new PathTo(JsonActor(src), src, options));


      public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)

      {

        toJsonBase(writer);

        writer.WriteArray("Locations", _locs, options);

        if (walking) writer.WriteBoolean("Walking", true);

        if (int.MaxValue != turns) writer.WriteNumber("Turns", turns);

      }

      #endregion


      public PathTo(Actor who, in Location loc, bool walk=false,int n=int.MaxValue)
      : base(who.Location.Map.LocalTime.TurnCounter, who)
      {
#if DEBUG
        if (!who.Model.CanEnter(loc)) throw who.Spotlight(new InvalidOperationException("must be able to exist at all destinations"));
#endif
        _locs = new HashSet<Location>{loc};
        walking = walk;
        turns = n;
      }

      public PathTo(Actor who, IEnumerable<Location> locs, bool walk = false, int n = int.MaxValue)
      : base(who.Location.Map.LocalTime.TurnCounter, who)
      {
#if DEBUG
        foreach(var loc in locs) if (!who.Model.CanEnter(loc)) throw who.Spotlight(new InvalidOperationException("must be able to exist at all destinations"));
#endif
        _locs = new(locs);
        walking = walk;
        turns = n;
      }

      public override bool UrgentAction(out ActorAction? ret)
      {
        ret = null;
        if (_locs.Contains(m_Actor.Location) || 0 >= turns--) {
          _isExpired = true;
          return true;
        }

        if (Controller.enemies_in_FOV is not null) {
          _isExpired = true;    // cancel: something urgent
          return true;
        }
        return false; // defer actual pathing until later
      }

      public ActorAction? Pathing()
      {
        var dests = _locs.Where(loc => loc != m_Actor.Location && m_Actor.MayEnter(loc));
        if (!dests.Any()) return null;

        var oai = Controller;
        var ret = oai.BehaviorPathTo([.. dests]);
        if (ret is null || !ret.IsPerformable()) {
          _isExpired = true;    // cancel: buggy
          return null;
        }

        while(ret is Resolvable res) ret = res.ConcreteAction;
        if (ret is ActionMoveStep step) {
          bool want_to_walk = true;
          if (m_Actor.CanRun()) {
            if (walking) {
              if (m_Actor.MaxSTA <= m_Actor.StaminaPoints) want_to_walk = false;
              else if (!m_Actor.RunIsFreeMove && !m_Actor.WillTireAfter(oai.STA_delta(0, 1, 0, 0) + m_Actor.RunningStaminaCost(step.dest)))
                want_to_walk = false;
            } else {
              if (!m_Actor.WillTireAfter(oai.STA_delta(0, 1, 0, 0) + m_Actor.RunningStaminaCost(step.dest)))
                want_to_walk = false;
            }
          }
          if (want_to_walk) m_Actor.Walk();
          else m_Actor.Run();
        } else m_Actor.Walk();

        return ret;
      }
    }
}
