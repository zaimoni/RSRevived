// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemMedicine
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemMedicine : Item
  {
    public int Healing { get; private set; }

    public int StaminaBoost { get; private set; }

    public int SleepBoost { get; private set; }

    public int InfectionCure { get; private set; }

    public int SanityCure { get; private set; }

    public ItemMedicine(ItemModel model)
      : base(model)
    {
      if (!(model is ItemMedicineModel))
        throw new ArgumentException("model is not a MedecineModel");
      ItemMedicineModel itemMedicineModel = model as ItemMedicineModel;
      this.Healing = itemMedicineModel.Healing;
      this.StaminaBoost = itemMedicineModel.StaminaBoost;
      this.SleepBoost = itemMedicineModel.SleepBoost;
      this.InfectionCure = itemMedicineModel.InfectionCure;
      this.SanityCure = itemMedicineModel.SanityCure;
    }
  }
}
