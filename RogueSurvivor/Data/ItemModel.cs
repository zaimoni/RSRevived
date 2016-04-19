// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ItemModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

namespace djack.RogueSurvivor.Data
{
  internal class ItemModel
  {
    public int ID { get; set; }
    public string SingleName { get; private set; }
    public string PluralName { get; private set; }
    public bool IsPlural { get; set; }
    public bool IsAn { get; set; }
    public bool IsProper { get; set; }
    public string ImageID { get; private set; }
    public string FlavorDescription { get; set; }
    public bool IsStackable { get; set; }
    public int StackingLimit { get; set; }
    public DollPart EquipmentPart { get; set; }
    public bool DontAutoEquip { get; set; }
    public bool IsUnbreakable { get; set; }

    public bool IsEquipable
    {
      get
      {
        return EquipmentPart != DollPart.NONE;
      }
    }

    public ItemModel(string aName, string theNames, string imageID)
    {
      SingleName = aName;
      PluralName = theNames;
      ImageID = imageID;
    }
  }
}
