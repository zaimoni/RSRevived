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
    private readonly Sayflags m_Flags;

    public ActionSay(Actor actor, Actor target, string text, Sayflags flags) : base(actor)
    {
      m_Target = target
#if DEBUG
        ?? throw new ArgumentNullException(nameof(target))
#endif
      ;
      m_Text = text;
      m_Flags = flags;
    }

    public override bool IsLegal()
    {
      return true;
    }

    public override void Perform() => m_Actor.Say(m_Target, m_Text, m_Flags);
  }
}
