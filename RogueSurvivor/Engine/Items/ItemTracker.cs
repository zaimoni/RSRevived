// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemTracker
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemTracker : Item, BatteryPowered
    {
    new public ItemTrackerModel Model { get {return (base.Model as ItemTrackerModel)!; } }
    private int m_Batteries;

    public ItemTrackerModel.TrackingFlags Tracking { get { return Model.Tracking; } }

    public bool CanTrackFollowersOrLeader {
      get {
        return (Tracking & ItemTrackerModel.TrackingFlags.FOLLOWER_AND_LEADER) != ItemTrackerModel.TrackingFlags.NONE;
      }
    }

    public bool CanTrackUndeads {
      get {
        return (Tracking & ItemTrackerModel.TrackingFlags.UNDEADS) != ItemTrackerModel.TrackingFlags.NONE;
      }
    }

    public bool CanTrackBlackOps {
      get {
        return (Tracking & ItemTrackerModel.TrackingFlags.BLACKOPS_FACTION) != ItemTrackerModel.TrackingFlags.NONE;
      }
    }

    public bool CanTrackPolice {
      get {
        return (Tracking & ItemTrackerModel.TrackingFlags.POLICE_FACTION) != ItemTrackerModel.TrackingFlags.NONE;
      }
    }

    public int Batteries {
      get {
        return m_Batteries;
      }
      set {
        if (value < 0) value = 0;
        m_Batteries = Math.Min(value, Model.MaxBatteries);
      }
    }

    public int MaxBatteries { get { return Model.MaxBatteries; } }
    public override bool IsUseless { get { return 0 >= m_Batteries; } }

    public ItemTracker(ItemTrackerModel model) : base(model)
    {
      Batteries = model.MaxBatteries;
    }

#if PROTOTYPE
    public override ItemStruct Struct { get { return new ItemStruct(Model.ID, m_Batteries); } }
#endif

    public void Recharge() { Batteries += Math.Max(WorldTime.TURNS_PER_HOUR, Model.MaxBatteries/8); }
  }
}
