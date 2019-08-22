// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ActorAction
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal abstract class ActorAction
  {
    protected readonly Actor m_Actor;
    protected string m_FailReason;

    public string FailReason { get { return m_FailReason; } }
    public bool PerformedBy(Actor a) { return m_Actor == a; }

    protected ActorAction(Actor actor)
    {
#if DEBUG
      if (null == actor || actor.IsDead) throw new ArgumentNullException(nameof(actor));
#endif
      m_Actor = actor;
    }

    public abstract bool IsLegal();
    // RS Alpha 10- do not distinguish between IsLegal() and IsPerformable().
    // We have to because we schedule actions for later turns; a legal action is schedulable, but a performable action actually can be done now.
    // Historical code in RogueGame that morally does the work of IsPerformable() should be lifted.
    public virtual bool IsPerformable() { return IsLegal(); }
    public abstract void Perform();

    public static bool Is<T>(ActorAction src) where T:ActorAction { return src is T; }
    public static bool IsNot<T>(ActorAction src) where T:ActorAction { return !(src is T); }

    public static bool Is<T,U>(KeyValuePair<U,ActorAction> src) where T:ActorAction { return src is T; }
    public static bool IsNot<T, U>(KeyValuePair<U,ActorAction> src) where T:ActorAction { return !(src is T); }

  }
}
