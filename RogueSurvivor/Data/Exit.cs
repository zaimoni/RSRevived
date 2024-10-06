// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Exit
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

using Point = Zaimoni.Data.Vector2D<short>;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  public sealed class Exit : Zaimoni.Serialization.ISerialize
    {
    private Location m_Location;	// XXX this cannot be public readonly Location: load fails in the runtime library Nov 5 2016.  Retry after compiler upgrade.

    public Map ToMap { get { return m_Location.Map; } }
    public Location Location { get { return m_Location; } }

#nullable enable
    public Exit(Map toMap, in Point toPosition)
    {
      m_Location = new Location(toMap,toPosition);
    }

#region implement Zaimoni.Serialization.ISerialize
    protected Exit(Zaimoni.Serialization.DecodeObjects decode)
    {
      Zaimoni.Serialization.ISave.LoadSigned(decode, ref m_Location, (m, pt) => m_Location = new(m, pt));
    }

    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode)
    {
      Zaimoni.Serialization.ISave.SaveSigned(encode, m_Location);
    }
#endregion


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
      if (actorAt != null) return false; // string.Format("{0} is blocking your way.", actorAt.Name);
      mapObjectAt = Location.MapObject;
      if (mapObjectAt != null && (!mapObjectAt.IsJumpable || (null!=actor && !actor.CanJump)) && !mapObjectAt.IsCouch) return false; // string.Format("{0} is blocking your way.", mapObjectAt.AName);
      return true;
    }
#nullable restore

    public override string ToString()
    {
      return "Exit; destination "+m_Location.ToString();
    }
  }
}
