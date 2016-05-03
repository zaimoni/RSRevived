// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemLightModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemLightModel : ItemModel
  {
    private int m_MaxBatteries;
    private int m_FovBonus;
    private string m_OutOfBatteriesImageID;

    public int MaxBatteries
    {
      get
      {
        return m_MaxBatteries;
      }
    }

    public int FovBonus
    {
      get
      {
        return m_FovBonus;
      }
    }

    public string OutOfBatteriesImageID
    {
      get
      {
        return m_OutOfBatteriesImageID;
      }
    }

    public ItemLightModel(string aName, string theNames, string imageID, int fovBonus, int maxBatteries, string outOfBatteriesImageID)
      : base(aName, theNames, imageID)
    {
            m_FovBonus = fovBonus;
            m_MaxBatteries = maxBatteries;
            m_OutOfBatteriesImageID = outOfBatteriesImageID;
            DontAutoEquip = true;
    }
  }
}
