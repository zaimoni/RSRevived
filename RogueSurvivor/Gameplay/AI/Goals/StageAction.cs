using System;
using djack.RogueSurvivor.Data;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Gameplay.AI.Goals
{
    [Serializable]
    internal class StageAction : Objective, IJsonStaged<StageAction>
    {
      public readonly ActorAction Intent;

      #region JSON save/load

      private StageAction(Actor who, JsonElement src, JsonSerializerOptions options) : base(who, src)

      {

        Intent = src.Require("Intent").Deserialize<ActorAction>(options) ?? throw new JsonException("null Intent");

      }


      static StageAction IJsonStaged<StageAction>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new StageAction(JsonActor(src), src, options));


      public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)

      {

        toJsonBase(writer);

        writer.WritePropertyName("Intent");

        JsonSerializer.Serialize(writer, Intent, options);

      }

      #endregion


      public StageAction(int t0, Actor who, ActorAction free_act) : base(t0,who)
      {
#if DEBUG
        if (!(who.Controller is ObjectiveAI)) throw new ArgumentOutOfRangeException(nameof(who), who, "not intelligent enough to plan ahead");
#endif
        Intent = free_act;
      }

      // always execute.  Expire on execution
      public override bool UrgentAction_readonly(out ActorAction ret)
      {
        ret = null;
        if (Intent.IsLegal() && !Intent.Abort()) return false;   // would be staged: keep.  Legal, not performable: a save is not now
        _isExpired = true;
        return true;
      }

      public override bool UrgentAction(out ActorAction ret)
      {
        ret = null;
        if (Intent.IsPerformable() && !Intent.Abort()) {
          // XXX need some sense of what a combat action is
          var ai = m_Actor.Controller;
          if (null != ai.enemies_in_FOV) return false;

          var oai = (ai as ObjectiveAI)!;
          if (!oai.VetoAction(Intent)) oai.Stage(Intent);
        }
       _isExpired = true;
        return true;
      }
    }
}
