// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionBuildFortification
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

using Point = Zaimoni.Data.Vector2D<short>;

#nullable enable

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionBuildFortification : ActorAction, NotSchedulable
  {
    private readonly Location m_Dest;
    private readonly bool m_IsLarge;

    public ActionBuildFortification(Actor actor, in Location dest, bool isLarge) : base(actor)
    {
      m_Dest = dest;
      m_IsLarge = isLarge;
    }

    public ActionBuildFortification(Actor actor, Direction dir, bool isLarge) : base(actor)
    {
      m_Dest = m_Actor.Location + dir;
      m_IsLarge = isLarge;
    }

    public override bool IsLegal()
    {
      m_FailReason = ReasonCant(m_Actor, m_IsLarge) ?? ReasonCant(in m_Dest);
      return string.IsNullOrEmpty(m_FailReason);
    }

    public override void Perform()
    {
      RogueGame.Game.DoBuildFortification(m_Actor, in m_Dest, m_IsLarge);
    }

    static public string? ReasonCant(Actor a, bool isLarge)
    {
      if (0 >= a.MySkills.GetSkillLevel(Gameplay.Skills.IDs.CARPENTRY)) return "no skill in carpentry";

      int num = a.BarricadingMaterialNeedForFortification(isLarge);
      if (a.CountItems<Items.ItemBarricadeMaterial>() < num) return string.Format("not enough barricading material, need {0}.", (object) num);
      return null;
    }

    static public string? ReasonCant(in Location loc)
    {
      if (!loc.TileModel.IsWalkable) return  "cannot build on walls";
      if (loc.HasMapObject || loc.StrictHasActorAt) return "blocked";
      return null;
    }

    static public string? ReasonCant(Actor a, in Location loc, bool isLarge) => ReasonCant(a, isLarge) ?? ReasonCant(in loc);
  }
}
