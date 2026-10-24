// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionBump
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal class ActionBump : ActorAction, Resolvable, ActorDest
    {
    private readonly Direction m_Direction;
    private readonly Location m_NewLocation;
    private readonly ActorAction m_ConcreteAction;

    public Location dest => m_NewLocation;
    public Direction Direction => m_Direction;
    public ActorAction ConcreteAction => m_ConcreteAction;

    private ActionBump(Actor actor, Direction direction, ActorAction act) : base(actor)
    {
      m_Direction = direction;
      m_NewLocation = actor.Location + direction;   // tentatively excluding this ActionBump class from IsPerformable upgrade due to this
#if DEBUG
      if (!Map.Canonical(ref m_NewLocation)) throw new InvalidOperationException("legal action with denormalized destination");
#else
      Map.Canonical(ref m_NewLocation);  // should be true for a legal action
#endif
      m_ConcreteAction = act;
    }

    public override bool IsLegal()
    {
      bool ret = m_ConcreteAction.IsLegal();
      if (!ret) m_FailReason = m_ConcreteAction.FailReason;
      return ret;
    }

    public override bool IsPerformable()
    {
      bool ret = m_ConcreteAction.IsPerformable();
      if (!ret) m_FailReason = m_ConcreteAction.FailReason;
      return ret;
    }

    public override void Perform() => m_ConcreteAction.Perform();

    static public ActionBump? create(Actor actor, Direction direction) {
        var denorm_loc = actor.Location + direction;
        var act = Rules.IsBumpableFor(actor, in denorm_loc, out var reason);
        if (act is null || !act.IsPerformable()) return null;
        return new(actor, direction, act);
    }

    static public (ActionBump?, string?) create(ActorController controller, Direction direction) {
        Actor actor = controller.ControlledActor;
        var denorm_loc = actor.Location + direction;
        var act = Rules.IsBumpableFor(actor, in denorm_loc, out var reason);
        if (act is null || !act.IsPerformable()) return (null, reason);
        return (new(actor, direction, act), null);
    }

    public override string ToString() { return m_ConcreteAction.ToString(); }
  }
}
