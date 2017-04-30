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
    new public ItemMedicineModel Model { get {return base.Model as ItemMedicineModel; } }    
    public int Healing { get { return Model.Healing; } }
    public int StaminaBoost { get { return Model.StaminaBoost; } }
    public int SleepBoost { get { return Model.SleepBoost; } }
    public int InfectionCure { get { return Model.InfectionCure; } }
    public int SanityCure { get { return Model.SanityCure; } }

    public ItemMedicine(ItemMedicineModel model)
      : base(model)
    {
    }
  }
}
