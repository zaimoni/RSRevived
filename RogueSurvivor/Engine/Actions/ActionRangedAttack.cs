// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionRangedAttack
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Gameplay.AI;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Zaimoni.JSON;

using Point = Zaimoni.Data.Vector2D<short>;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  // VAPORWARE need to include burst fire as well (both aimed and snap/hipfire)
  // * convert army rifle to burst-competent, scale army rifle clip accordingly; leave sniper rifle alone
  // * machine pistol: not in gun stores; allow for survivalist caches and SWAT police (leaders?).
  [Serializable]
  public enum FireMode
  {
    AIMED = 0,  // single, aimed shot; keyword default
    RAPID       // single snap shot
  }

  [Serializable]
  public sealed class ActionRangedAttack : ActorAction, CombatAction, IJsonStaged<ActionRangedAttack>
  {
    private readonly List<Point> m_LoF = new List<Point>();
    private readonly Actor m_Target;
    public readonly FireMode FMode;
    [NonSerialized] ObjectiveAI oai;

    #region JSON save/load
    private ActionRangedAttack(Actor actor, JsonElement src, JsonSerializerOptions options) : base(actor)
    {
      m_Target = src.Require("Target").RequireRef<Actor>();
      if (src.TryGetProperty("LoF", out var x)) foreach (var pt in x.EnumerateArray()) m_LoF.Add(pt.ReadPoint());
      if (src.TryGetProperty("FireMode", out x)) FMode = JSON_ext.ParseEnum<FireMode>(x.GetString());
      OnDeserialized();
    }

    static ActionRangedAttack IJsonStaged<ActionRangedAttack>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new ActionRangedAttack(JsonActor(src), src, options));

    public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      toJsonActor(writer);
      writer.WriteRef("Target", m_Target);
      if (0 < m_LoF.Count) {
        writer.WritePropertyName("LoF");
        writer.WriteRawValue(JSON_ext.RenderPoints(m_LoF));
      }
      if (default != FMode) writer.WriteString("FireMode", FMode.ToString());
    }
    #endregion

    public ActionRangedAttack(Actor actor, Actor target, FireMode mode=default) : base(actor)
    {
      m_Target = target;
      FMode = mode;
      OnDeserialized();
    }

    public ActionRangedAttack(Actor actor, Actor target, List<Point> lof, FireMode mode) : base(actor)
    {
      m_Target = target;
      m_LoF = lof;
      FMode = mode;
      OnDeserialized();
    }

    public Actor target { get { return m_Target; } }

    private void OnDeserialized()
    {
      if (m_Actor.Controller is ObjectiveAI ai) oai = ai;
      else throw new ArgumentNullException(nameof(oai));
    }

    public override bool IsLegal()
    {
      m_FailReason = ReasonIllegal(m_Target) ?? "";
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override bool IsPerformable()
    {
      m_FailReason = ReasonIllegal(m_Target) ?? ReasonCant(m_Actor, m_Target, m_LoF) ?? "";
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override void Perform()
    {
      m_Actor.Aggress(m_Target);
      oai.RecordLoF(m_LoF);
      switch(FMode) {
        case FireMode.AIMED:
          m_Actor.SpendActionPoints();
          RogueGame.Game.DoSingleRangedAttack(m_Actor, m_Target, m_LoF, 0);
          break;
        case FireMode.RAPID:
          m_Actor.SpendActionPoints(Actor.BASE_ACTION_COST/2);
          RogueGame.Game.DoSingleRangedAttack(m_Actor, m_Target, m_LoF, oai.Recoil+1);
          break;
        default:
          throw new ArgumentOutOfRangeException("unhandled mode");
      }
      if (!m_Target.IsDead) oai.RecruitHelp(m_Target);
      m_Target.InferEnemy(m_Actor);
    }

        private static string? ReasonIllegal(Actor target)
        {
            if (target.IsDead) return "already dead!";
            return null;
        }

        private static string? ReasonCant(Actor aggressor, Actor target, List<Point>? LoF)
        {
            LoF?.Clear();
            if (!(aggressor.GetEquippedWeapon() is ItemRangedWeapon itemRangedWeapon)) return "no ranged weapon equipped";
            int dist = Rules.InteractionDistance(aggressor.Location, target.Location);
            var range = aggressor.CurrentRangedAttack.Range;
            if (range < dist) return "out of range";
            if (itemRangedWeapon.Ammo <= 0) return "no ammo left";
            if (1 < dist && !LOS.CanTraceFireLine(aggressor.Location, target.Location, range, LoF)) return "no line of fire";
            if (target.IsDead) return "already dead!";
            return null;
        }

        private static string ReasonCannot(Actor aggressor, Actor target, List<Point>? LoF = null)
        {
            return ReasonIllegal(target) ?? ReasonCant(aggressor, target, LoF) ?? "";
        }

        public static bool Can(Actor aggressor, Actor target, out string reason, List<Point>? LoF = null)
        {
            reason = ReasonCannot(aggressor, target, LoF);
            return string.IsNullOrEmpty(reason);
        }

        public static bool Can(Actor aggressor, Actor target, List<Point>? LoF = null)
        {
            return string.IsNullOrEmpty(ReasonCannot(aggressor, target, LoF));
        }
    }

    // This is not covering fire.  More like a pop out and snipe at something
    [Serializable]
    internal class OpportunityTarget : WorldUpdate, IJsonStaged<OpportunityTarget>
    {
        public OpportunityTarget() { }

        // JSON: no state
        static OpportunityTarget IJsonStaged<OpportunityTarget>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new OpportunityTarget());
        public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options) { }

        public override bool IsLegal() => true;
        public override bool IsRelevant() => true;
        public override bool IsRelevant(Location loc) => true;
        public override bool IsSuppressed(Actor a) => false;

        /// <returns>null, or a Performable action</returns>
        public override ActorAction? Bind(Actor src) {
            var ordai = src.Controller as OrderableAI;
            if (null == ordai) return null;
            return ordai.OpportunityFire();
        }
        public override KeyValuePair<ActorAction, WorldUpdate?>? BindReduce(Actor src) => null;
        public override void Blacklist(HashSet<Location> goals) { }
        public override void Goals(HashSet<Location> goals) { }
    }

}
