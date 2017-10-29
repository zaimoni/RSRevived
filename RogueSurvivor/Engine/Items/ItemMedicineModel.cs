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
    public readonly int Healing;
    public readonly int StaminaBoost;
    public readonly int SleepBoost;
    public readonly int InfectionCure;
    public readonly int SanityCure;

    public ItemMedicineModel(string aName, string theNames, string imageID, int healing, int staminaBoost, int sleepBoost, int infectionCure, int sanityCure, string flavor, int stackingLimit = 0)
      : base(aName, theNames, imageID, flavor)
    {
      Healing = healing;
      StaminaBoost = staminaBoost;
      SleepBoost = sleepBoost;
      InfectionCure = infectionCure;
      SanityCure = sanityCure;
      if (1<stackingLimit) {
        IsPlural = true;
        StackingLimit = stackingLimit;
      }
    }
  }
}
