// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ItemModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

namespace djack.RogueSurvivor.Data
{
  internal class ItemModel
  {
    public djack.RogueSurvivor.Gameplay.GameItems.IDs ID { get; set; }
    private string m_SingleName;
    public string PluralName { get; private set; }
    public bool IsPlural { get; set; }
    public bool IsAn { get; private set; }
    public bool IsProper { get; set; }
    public string ImageID { get; private set; }
    public string FlavorDescription { get; set; }
    public bool IsStackable { get; private set; }
    private int m_StackingLimit;
    public DollPart EquipmentPart { get; set; }
    public bool DontAutoEquip { get; set; }
    public bool IsUnbreakable { get; set; }
    public bool IsUnique { get; set; }
    public bool IsForbiddenToAI { get; set; }

    public string SingleName
    {
      get { return m_SingleName; }
      private set
      {
         m_SingleName = value;
         IsAn = StartsWithVowel(value);
      }
    }

    public int StackingLimit
    {
      get { return m_StackingLimit; }
      set
      {
        m_StackingLimit = value;
        IsStackable = (2 <= value);
      }
    }

    public bool IsEquipable {
      get {
        return EquipmentPart != DollPart.NONE;
      }
    }

    public ItemModel(string aName, string theNames, string imageID)
    {
      SingleName = aName;
      PluralName = theNames;
      ImageID = imageID;
    }

    private bool StartsWithVowel(string name)
    {
      return 0 <= "AEIOUaeiou".IndexOf(name[0]);
    }
  }
}
