// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemWeaponModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemWeaponModel : ItemModel
  {
    private Attack m_Attack;

    public Attack Attack
    {
      get
      {
        return this.m_Attack;
      }
    }

    public ItemWeaponModel(string aName, string theNames, string imageID, Attack attack)
      : base(aName, theNames, imageID)
    {
      this.m_Attack = attack;
    }
  }
}
