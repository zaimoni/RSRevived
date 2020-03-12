// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemTrackerModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemTrackerModel : ItemModel
  {
    public readonly TrackingFlags Tracking;
    public readonly int MaxBatteries;

    public ItemTrackerModel(string aName, string theNames, string imageID, TrackingFlags tracking, int maxBatteries, DollPart part, string flavor)
      : base(aName, theNames, imageID, flavor, part, true)
    {
       Tracking = tracking;
       MaxBatteries = maxBatteries;
    }

    // work around lack of const strings
    private static readonly string _track_undead_only = Rules.ZTRACKINGRADIUS.ToString();
    private static readonly string _track_non_undead_only = RogueGame.MINIMAP_RADIUS.ToString();
    private static readonly string _track_both = Rules.ZTRACKINGRADIUS.ToString() + "/" + RogueGame.MINIMAP_RADIUS.ToString();

    public string? RangeDesc { get {
        if (TrackingFlags.UNDEADS == Tracking) return _track_undead_only;
        if (TrackingFlags.NONE == Tracking) return null;
        if (TrackingFlags.NONE == (TrackingFlags.UNDEADS & Tracking)) return _track_non_undead_only;
        return _track_both;
      }
    }

    public override Item create() { return new ItemTracker(this); }
    public ItemTracker instantiate() { return new ItemTracker(this); }

    [System.Flags]
    public enum TrackingFlags
    {
      NONE = 0,
      FOLLOWER_AND_LEADER = 1,
      UNDEADS = 2,
      BLACKOPS_FACTION = 4,
      POLICE_FACTION = 8,
    }
  }
}
