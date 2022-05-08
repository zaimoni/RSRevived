// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Actions.ActionPush
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Actions
{
  internal interface ObjectDest
  {
    Location obj_dest { get; }
  }

  internal class ActionPush : ActorAction, ActorDest,ObjectDest
  {
    private readonly MapObject m_Object;
    private readonly Location m_To;

    public MapObject Target { get { return m_Object; } }
    public Location obj_dest { get { return m_To; } }
    public Location dest { get { return m_Object.Location; } }

    static public bool CanConstruct(MapObject pushObj, Direction pushDir) {
        var x = pushObj.Location + pushDir;
        return Map.Canonical(ref x);
    }

    public ActionPush(Actor actor, MapObject pushObj, Direction pushDir) : base(actor)
    {
#if DEBUG
      if (null == pushDir) throw new ArgumentNullException(nameof(pushDir));
#endif
      m_Object = pushObj
#if DEBUG
        ?? throw new ArgumentNullException(nameof(pushObj))
#endif
      ;
      m_To = pushObj.Location + pushDir;
#if DEBUG
      // will be ok when tactical pushing is implemented
      if (actor.Controller is Gameplay.AI.OrderableAI && m_To.Map.HasExitAt(m_To.Position) && m_To.Map.IsInBounds(m_To.Position) && !m_Object.IsJumpable) throw new InvalidOperationException("Blocking exit with push");
#endif
      if (!Map.Canonical(ref m_To)) throw new InvalidOperationException("pushed off map");
    }

#nullable enable

    public override bool IsLegal()
    {
      if (m_Actor.CanPush(m_Object))
        return m_Object.CanPushTo(m_To, out m_FailReason);
      return false;
    }

    public override void Perform()
    {
      RogueGame.Game.DoPush(m_Actor, m_Object, in m_To);
    }

    static public ActionPush? Random(Actor m_Actor, MapObject obj) {
      var rules = Rules.Get;
      var dir = rules.RollDirection();
      var dest = obj.Location + dir;
      // don't block exit with push
      if (m_Actor.Controller is Gameplay.AI.OrderableAI && dest.Map.HasExitAt(dest.Position) && dest.Map.IsInBounds(dest.Position) && !obj.IsJumpable) return null;
      if (!Map.Canonical(ref dest)) return null;
      var tmp = new ActionPush(m_Actor, obj, dir);
      return tmp.IsPerformable() ? tmp : null;
    }
  }
}
