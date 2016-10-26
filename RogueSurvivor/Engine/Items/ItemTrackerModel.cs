// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemTrackerModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemTrackerModel : ItemModel
  {
    private readonly ItemTrackerModel.TrackingFlags m_Tracking;
    private readonly int m_MaxBatteries;

    public ItemTrackerModel.TrackingFlags Tracking {
      get {
        return m_Tracking;
      }
    }

    public int MaxBatteries {
      get {
        return m_MaxBatteries;
      }
    }

    public ItemTrackerModel(string aName, string theNames, string imageID, ItemTrackerModel.TrackingFlags tracking, int maxBatteries)
      : base(aName, theNames, imageID)
    {
       m_Tracking = tracking;
       m_MaxBatteries = maxBatteries;
       DontAutoEquip = true;
    }

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
