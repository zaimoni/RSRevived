// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorDirective
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  // This is set *only* by the player; a total replacement with the objectives system is anticipated.
  [Serializable]
  public class ActorDirective
  {
    public const bool CanThrowGrenades_default = true;
    public const bool CanSleep_default = true;
    public const bool CanTrade_default = true;
    public const ActorCourage Courage_default = ActorCourage.CAUTIOUS;

    public bool CanThrowGrenades = CanThrowGrenades_default;
    public bool CanSleep = CanSleep_default;    // may want this indefinitely (useful to disable when appointing guards)
    public bool CanTrade = CanTrade_default;
    public ActorCourage Courage = Courage_default;
  }
}

namespace Zaimoni.JsonConvert
{
    using System.Text.Json;
    using Zaimoni.JSON;
    using Directive = djack.RogueSurvivor.Data.ActorDirective;

    public sealed class ActorDirective : System.Text.Json.Serialization.JsonConverter<Directive>
    {
        public override Directive Read(ref Utf8JsonReader reader, Type src, JsonSerializerOptions options)
        {
            var stage = JsonElement.ParseValue(ref reader);
            Directive ret = new();
            if (stage.TryGetProperty("CanThrowGrenades", out var x)) ret.CanThrowGrenades = x.GetBoolean();
            if (stage.TryGetProperty("CanSleep", out x)) ret.CanSleep = x.GetBoolean();
            if (stage.TryGetProperty("CanTrade", out x)) ret.CanTrade = x.GetBoolean();
            if (stage.TryGetProperty("Courage", out x)) ret.Courage = JSON_ext.ParseEnum<djack.RogueSurvivor.Data.ActorCourage>(x.GetString());
            return ret;
        }

        public override void Write(Utf8JsonWriter writer, Directive src, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            if (Directive.CanThrowGrenades_default != src.CanThrowGrenades) writer.WriteBoolean("CanThrowGrenades", src.CanThrowGrenades);
            if (Directive.CanSleep_default != src.CanSleep) writer.WriteBoolean("CanSleep", src.CanSleep);
            if (Directive.CanTrade_default != src.CanTrade) writer.WriteBoolean("CanTrade", src.CanTrade);
            if (Directive.Courage_default != src.Courage) writer.WriteString("Courage", src.Courage.ToString());
            writer.WriteEndObject();
        }
    }
}
