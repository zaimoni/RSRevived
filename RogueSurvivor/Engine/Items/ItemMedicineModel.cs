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
    private readonly int m_Healing;
    private readonly int m_StaminaBoost;
    private readonly int m_SleepBoost;
    private readonly int m_InfectionCure;
    private readonly int m_SanityCure;

    public int Healing {
      get {
        return m_Healing;
      }
    }

    public int StaminaBoost {
      get {
        return m_StaminaBoost;
      }
    }

    public int SleepBoost {
      get {
        return m_SleepBoost;
      }
    }

    public int InfectionCure {
      get {
        return m_InfectionCure;
      }
    }

    public int SanityCure {
      get {
        return m_SanityCure;
      }
    }

    public ItemMedicineModel(string aName, string theNames, string imageID, int healing, int staminaBoost, int sleepBoost, int infectionCure, int sanityCure, string flavor, int stackingLimit = 0)
      : base(aName, theNames, imageID)
    {
      m_Healing = healing;
      m_StaminaBoost = staminaBoost;
      m_SleepBoost = sleepBoost;
      m_InfectionCure = infectionCure;
      m_SanityCure = sanityCure;
      FlavorDescription = flavor;
      if (0<stackingLimit) {
        IsPlural = true;
        StackingLimit = stackingLimit;
      }
    }
  }
}
