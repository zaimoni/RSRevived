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
    private readonly Actor m_Target;
#if PROTOTYPE_TRADE_RAM_FOR_CPU_OR_IL_SIZE
    [NonSerialized] OrderableAI actor_ordai;
    [NonSerialized] OrderableAI target_ordai;
#endif

    #region JSON save/load
    private ActionTrade(Actor actor, JsonElement src) : base(actor)
    {
      m_Target = src.Require("Target").RequireRef<Actor>();
    }

    static ActionTrade IJsonStaged<ActionTrade>.fromJson(JsonElement src, JsonSerializerOptions options) => JsonLoaded(src, new ActionTrade(JsonActor(src), src));

    public void toJsonFields(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
      toJsonActor(writer);
      writer.WriteRef("Target", m_Target);
    }
    #endregion

    public ActionTrade(Actor actor, Actor target) : base(actor)
    {
      m_Target = target
#if DEBUG
        ?? throw new ArgumentNullException(nameof(target))
#endif
      ;
#if DEBUG
      if (actor.Controller is OrderableAI) throw new InvalidOperationException("tracing");
      if (target.Controller is OrderableAI) throw new InvalidOperationException("tracing");
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
      RogueGame.Game.DoTrade(m_Actor.Controller as Gameplay.AI.OrderableAI, m_Target.Controller as Gameplay.AI.OrderableAI);
    }

        private static string? ReasonIllegal(Actor src, Actor target)
        {
            if (src.Inventory is null) return "nothing to offer";
            if (target.Inventory is null) return "has nothing to trade";
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

        private static string? ReasonCant(Actor src, Actor target)
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

            if (!src.IsPlayer && !target.IsPlayer) {
                var theirs = target.GetRationalTradeableItems(src.Controller as Gameplay.AI.OrderableAI);
                if (null == theirs) return "target unwilling to trade";
                var mine = src.GetRationalTradeableItems(target.Controller as Gameplay.AI.OrderableAI);
                if (null == mine) return "unwilling to trade";
                bool ok = false;
                foreach (var want in theirs) {
                    foreach (var have in mine) {
                        if (   !Gameplay.AI.ObjectiveAI.TradeVeto(have, want)
                            && !Gameplay.AI.ObjectiveAI.TradeVeto(want, have)) {
                            ok = true;
                            break;
                        }
                    }
                }
                if (!ok) return "no mutually acceptable trade";
            }
            return null;
        }

        // For historical reasons, even though using a PC would hard-error Perform()
        // the Can predicate allows PCs
        private static string ReasonCannot(Actor src, Actor target)
        {
            return ReasonIllegal(src, target) ?? ReasonCant(src, target) ?? "";
        }

        public static bool Can(Actor src, Actor target, out string reason)
        {
            reason = ReasonCannot(src, target);
            return string.IsNullOrEmpty(reason);
        }

        public static bool Can(Actor src, Actor target)
        {
            return string.IsNullOrEmpty(ReasonCannot(src, target));
        }
    }
}
