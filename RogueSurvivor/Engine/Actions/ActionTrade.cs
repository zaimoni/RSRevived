// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionTrade
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Gameplay.AI;
using System;
using System.Text.Json;
using Zaimoni.JSON;

namespace djack.RogueSurvivor.Engine.Actions
{
  [Serializable]
  internal class ActionTrade : ActorAction, IJsonStaged<ActionTrade>
  {
    private readonly LivingActor m_Target;
#if PROTOTYPE_TRADE_RAM_FOR_CPU_OR_IL_SIZE
    [NonSerialized] OrderableAI actor_ordai;
    [NonSerialized] OrderableAI target_ordai;
#endif

    #region JSON save/load
    private new readonly LivingActor m_Actor;   // the same object as the base field

    private ActionTrade(LivingActor actor, JsonElement src) : base(actor)
    {
      m_Actor = actor;
      m_Target = src.Require("Target").RequireRef<LivingActor>();
      // Perform needs OrderableAI controllers on both sides: a trade is between NPCs.  The controllers are shells by now (two-phase load).
      if (actor.Controller is not OrderableAI) throw new JsonException("ActionTrade: " + actor.UnmodifiedName + " is not run by an OrderableAI");
      if (m_Target.Controller is not OrderableAI) throw new JsonException("ActionTrade: " + m_Target.UnmodifiedName + " is not run by an OrderableAI");
    }

    static ActionTrade IJsonStaged<ActionTrade>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new ActionTrade(JsonLivingActor(src), src));

    public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      toJsonActor(writer);
      writer.WriteRef("Target", m_Target);
    }
    #endregion

    public ActionTrade(LivingActor actor, LivingActor target) : base(actor)
    {
      m_Actor = actor;
      m_Target = target
#if DEBUG
        ?? throw new ArgumentNullException(nameof(target))
#endif
      ;
#if DEBUG
      if (!(actor.Controller is OrderableAI)) throw new InvalidOperationException("tracing");
      if (!(target.Controller is OrderableAI)) throw new InvalidOperationException("tracing");
#endif
    }

     public override bool IsLegal()
    {
      m_FailReason = ReasonIllegal(m_Actor, m_Target) ?? "";
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override bool IsPerformable()
    {
      m_FailReason = ReasonIllegal(m_Actor, m_Target) ?? ReasonCant(m_Actor, m_Target) ?? "";
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override void Perform()
    {
      var speaker_c = (m_Actor.Controller as OrderableAI)!;
      var target_c = (m_Target.Controller as OrderableAI)!;
      RogueGame.Game.DoTrade(speaker_c, RogueGame.PickItemsToTrade(speaker_c, m_Target), target_c, false);
    }

        private static string? ReasonIllegal(LivingActor src, LivingActor target)
        {
            if (!src.IsPlayer && target.IsPlayer) return "target is player";
            if (!src.Model.Abilities.CanTrade && target.Leader != src) return "can't trade";
            if (!target.Model.Abilities.CanTrade && src.Leader != target) return "target can't trade";
            if (src.IsEnemyOf(target)) return "is an enemy";
            if (target.IsSleeping) return "is sleeping";

            // RS Revived: no trading with differing treacherous factions; should be "by AI controller" but player controller would need to simulate underlying target
            if (target.Faction != src.Faction) {
                switch (src.Faction.ID) {
                    case GameFactions.IDs.TheCHARCorporation:
                    case GameFactions.IDs.TheBikers:
                    case GameFactions.IDs.TheGangstas:
                        return "untrustworthy";
                }

                switch (target.Faction.ID) {
                    case GameFactions.IDs.TheCHARCorporation:
                    case GameFactions.IDs.TheBikers:
                    case GameFactions.IDs.TheGangstas:
                        return "anticipates treachery";
                }
            }

            return null;
        }

        private static string? ReasonCant(LivingActor src, LivingActor target)
        {
            if (src.Inventory.IsEmpty) return "nothing to offer";
            if (target.Inventory.IsEmpty) return "has nothing to trade";
            // alpha10 dont bother someone who is fighting or fleeing
            if (   target.Activity == Activity.CHASING
                || target.Activity == Activity.FIGHTING
                || target.Activity == Activity.FLEEING
                || target.Activity == Activity.FLEEING_FROM_EXPLOSIVE)
            {
                if (!target.IsPlayer) return "in combat";
            }

            if (src.Controller is OrderableAI speaker_c) {
                if (target.Controller is OrderableAI target_c) {
                    var theirs = target_c.GetRationalTradeableItems(speaker_c);
                    if (theirs is null) return "target unwilling to trade";
                    var mine = speaker_c.GetRationalTradeableItems(target_c);
                    if (mine is null) return "unwilling to trade";
                    bool ok = false;
                    foreach (var want in theirs) {
                        foreach (var have in mine) {
                            if (!ObjectiveAI.TradeVeto(have, want) && !ObjectiveAI.TradeVeto(want, have)) {
                                ok = true;
                                break;
                            }
                        }
                    }
                    if (!ok) return "no mutually acceptable trade";
                }
            }
            return null;
        }

        // For historical reasons, even though using a PC would hard-error Perform()
        // the Can predicate allows PCs
        private static string ReasonCannot(LivingActor src, LivingActor target)
        {
            return ReasonIllegal(src, target) ?? ReasonCant(src, target) ?? "";
        }

        public static bool Can(LivingActor src, LivingActor target, out string reason)
        {
            reason = ReasonCannot(src, target);
            return string.IsNullOrEmpty(reason);
        }

        public static bool Can(LivingActor src, LivingActor target)
        {
            return string.IsNullOrEmpty(ReasonCannot(src, target));
        }
    }
}
