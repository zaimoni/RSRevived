// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Exit
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

// #define Z_VECTOR

using System;

using Point = Zaimoni.Data.Vector2D_short;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  internal class Exit
  {
    private Location m_Location;	// XXX this cannot be public readonly Location: load fails in the runtime library Nov 5 2016.  Retry after compiler upgrade.

    public Map ToMap { get { return m_Location.Map; } }
    public Location Location { get { return m_Location; } }

#nullable enable
    public Exit(Map toMap, in Point toPosition)
    {
      m_Location = new Location(toMap,toPosition);
    }

    // note that if we are pathfinding, we do not have actor anyway.  All livings can jump, however
    // we do not consider actors to block exits when pathfinding
    public string ReasonIsBlocked(Actor? actor=null) {
      if (null!=actor) {
        var actorAt = Location.Actor;
        if (actorAt != null) return string.Format("{0} is blocking your way.", actorAt.Name);
      }
      var mapObjectAt = Location.MapObject;
      if (mapObjectAt != null && (!mapObjectAt.IsJumpable || (null!=actor && !actor.CanJump)) && !mapObjectAt.IsCouch) return string.Format("{0} is blocking your way.", mapObjectAt.AName);
      return "";
    }

    public bool IsNotBlocked(Actor? actor=null) {
      if (null!=actor) {
        var actorAt = Location.Actor;
        if (actorAt != null) return false; // string.Format("{0} is blocking your way.", actorAt.Name);
      }
      var mapObjectAt = Location.MapObject;
      if (mapObjectAt != null && (!mapObjectAt.IsJumpable || (null!=actor && !actor.CanJump)) && !mapObjectAt.IsCouch) return false; // string.Format("{0} is blocking your way.", mapObjectAt.AName);
      return true;
    }


    public bool IsNotBlocked(out Actor? actorAt, out MapObject? mapObjectAt, Actor? actor=null) {   // \todo a couple of cold-path callers would like out vars for the actor and mapObjectAt failure modes
      mapObjectAt = null;
      actorAt = (null != actor) ? Location.Actor : null;
      if (actorAt != null) return true; // string.Format("{0} is blocking your way.", actorAt.Name);
      mapObjectAt = Location.MapObject;
      if (mapObjectAt != null && (!mapObjectAt.IsJumpable || (null!=actor && !actor.CanJump)) && !mapObjectAt.IsCouch) return true; // string.Format("{0} is blocking your way.", mapObjectAt.AName);
      return false;
    }
#nullable restore

    public override string ToString()
    {
      return "Exit; destination "+m_Location.ToString();
    }
  }
}
