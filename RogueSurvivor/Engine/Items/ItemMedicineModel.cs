// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemMedicineModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemMedicineModel : ItemModel
  {
    private int m_Healing;
    private int m_StaminaBoost;
    private int m_SleepBoost;
    private int m_InfectionCure;
    private int m_SanityCure;

    public int Healing
    {
      get
      {
        return this.m_Healing;
      }
    }

    public int StaminaBoost
    {
      get
      {
        return this.m_StaminaBoost;
      }
    }

    public int SleepBoost
    {
      get
      {
        return this.m_SleepBoost;
      }
    }

    public int InfectionCure
    {
      get
      {
        return this.m_InfectionCure;
      }
    }

    public int SanityCure
    {
      get
      {
        return this.m_SanityCure;
      }
    }

    public ItemMedicineModel(string aName, string theNames, string imageID, int healing, int staminaBoost, int sleepBoost, int infectionCure, int sanityCure)
      : base(aName, theNames, imageID)
    {
      this.m_Healing = healing;
      this.m_StaminaBoost = staminaBoost;
      this.m_SleepBoost = sleepBoost;
      this.m_InfectionCure = infectionCure;
      this.m_SanityCure = sanityCure;
    }
  }
}
