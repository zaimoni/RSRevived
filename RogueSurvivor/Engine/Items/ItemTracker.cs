// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemTracker
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemTracker : Item
  {
    private int m_Batteries;

    public ItemTrackerModel.TrackingFlags Tracking { get; private set; }

    public bool CanTrackFollowersOrLeader
    {
      get
      {
        return (Tracking & ItemTrackerModel.TrackingFlags.FOLLOWER_AND_LEADER) != ItemTrackerModel.TrackingFlags.NONE;
      }
    }

    public bool CanTrackUndeads
    {
      get
      {
        return (Tracking & ItemTrackerModel.TrackingFlags.UNDEADS) != ItemTrackerModel.TrackingFlags.NONE;
      }
    }

    public bool CanTrackBlackOps
    {
      get
      {
        return (Tracking & ItemTrackerModel.TrackingFlags.BLACKOPS_FACTION) != ItemTrackerModel.TrackingFlags.NONE;
      }
    }

    public bool CanTrackPolice
    {
      get
      {
        return (Tracking & ItemTrackerModel.TrackingFlags.POLICE_FACTION) != ItemTrackerModel.TrackingFlags.NONE;
      }
    }

    public int Batteries
    {
      get
      {
        return m_Batteries;
      }
      set
      {
        if (value < 0)
          value = 0;
                m_Batteries = Math.Min(value, (Model as ItemTrackerModel).MaxBatteries);
      }
    }

    public bool IsFullyCharged
    {
      get
      {
        return m_Batteries >= (Model as ItemTrackerModel).MaxBatteries;
      }
    }

    public override bool IsUseless { 
      get { return 0 >= m_Batteries; }
    }

    public ItemTracker(ItemModel model)
      : base(model)
    {
      if (!(model is ItemTrackerModel))
        throw new ArgumentException("model is not a TrackerModel");
      ItemTrackerModel itemTrackerModel = model as ItemTrackerModel;
            Tracking = itemTrackerModel.Tracking;
            Batteries = itemTrackerModel.MaxBatteries;
    }

    public void Recharge()
    {
      Batteries += Math.Max(WorldTime.TURNS_PER_HOUR, (Model as ItemTrackerModel).MaxBatteries/8);
    }
  }
}
