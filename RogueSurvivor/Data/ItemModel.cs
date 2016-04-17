// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.ItemModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

namespace djack.RogueSurvivor.Data
{
  internal class ItemModel
  {
    private int m_ID;
    private string m_SingleName;
    private string m_PluralName;
    private bool m_IsPlural;
    private bool m_IsAn;
    private bool m_IsProper;
    private string m_ImageID;
    private string m_FlavorDescription;
    private bool m_IsStackable;
    private int m_StackingLimit;
    private DollPart m_EquipmentSlot;
    private bool m_DontAutoEquip;
    private bool m_IsUnbreakable;

    public int ID
    {
      get
      {
        return this.m_ID;
      }
      set
      {
        this.m_ID = value;
      }
    }

    public string SingleName
    {
      get
      {
        return this.m_SingleName;
      }
    }

    public string PluralName
    {
      get
      {
        return this.m_PluralName;
      }
    }

    public bool IsPlural
    {
      get
      {
        return this.m_IsPlural;
      }
      set
      {
        this.m_IsPlural = value;
      }
    }

    public bool IsAn
    {
      get
      {
        return this.m_IsAn;
      }
      set
      {
        this.m_IsAn = value;
      }
    }

    public bool IsProper
    {
      get
      {
        return this.m_IsProper;
      }
      set
      {
        this.m_IsProper = value;
      }
    }

    public string ImageID
    {
      get
      {
        return this.m_ImageID;
      }
    }

    public string FlavorDescription
    {
      get
      {
        return this.m_FlavorDescription;
      }
      set
      {
        this.m_FlavorDescription = value;
      }
    }

    public bool IsStackable
    {
      get
      {
        return this.m_IsStackable;
      }
      set
      {
        this.m_IsStackable = value;
      }
    }

    public int StackingLimit
    {
      get
      {
        return this.m_StackingLimit;
      }
      set
      {
        this.m_StackingLimit = value;
      }
    }

    public DollPart EquipmentPart
    {
      get
      {
        return this.m_EquipmentSlot;
      }
      set
      {
        this.m_EquipmentSlot = value;
      }
    }

    public bool IsEquipable
    {
      get
      {
        return this.m_EquipmentSlot != DollPart.NONE;
      }
    }

    public bool DontAutoEquip
    {
      get
      {
        return this.m_DontAutoEquip;
      }
      set
      {
        this.m_DontAutoEquip = value;
      }
    }

    public bool IsUnbreakable
    {
      get
      {
        return this.m_IsUnbreakable;
      }
      set
      {
        this.m_IsUnbreakable = value;
      }
    }

    public ItemModel(string aName, string theNames, string imageID)
    {
      this.m_SingleName = aName;
      this.m_PluralName = theNames;
      this.m_ImageID = imageID;
    }
  }
}
