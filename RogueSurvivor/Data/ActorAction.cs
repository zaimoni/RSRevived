// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorAction
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine;
using System;

namespace djack.RogueSurvivor.Data
{
  internal abstract class ActorAction
  {
    protected readonly RogueGame m_Game;
    protected readonly Actor m_Actor;
    protected string m_FailReason;

    public string FailReason
    {
      get
      {
        return m_FailReason;
      }
      set
      {
                m_FailReason = value;
      }
    }

    protected ActorAction(Actor actor, RogueGame game)
    {
      if (actor == null)
        throw new ArgumentNullException("actor");
      if (game == null)
        throw new ArgumentNullException("game");
            m_Actor = actor;
            m_Game = game;
    }

    public abstract bool IsLegal();

    public abstract void Perform();
  }
}
