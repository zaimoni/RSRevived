// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionSay
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionSay : ActorAction
  {
    private readonly Actor m_Target;
    private readonly string m_Text;
    private readonly RogueGame.Sayflags m_Flags;

    public ActionSay(Actor actor, Actor target, string text, RogueGame.Sayflags flags)
      : base(actor)
    {
#if DEBUG
      if (null == target) throw new ArgumentNullException(nameof(target));
#endif
      m_Target = target;
      m_Text = text;
      m_Flags = flags;
    }

    public override bool IsLegal()
    {
      return true;
    }

    public override void Perform()
    {
      RogueGame.DoSay(m_Actor, m_Target, m_Text, m_Flags);
    }
  }
}
