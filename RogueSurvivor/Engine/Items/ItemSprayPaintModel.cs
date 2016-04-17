// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemSprayPaintModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemSprayPaintModel : ItemModel
  {
    private int m_MaxPaintQuantity;
    private string m_TagImageID;

    public int MaxPaintQuantity
    {
      get
      {
        return this.m_MaxPaintQuantity;
      }
    }

    public string TagImageID
    {
      get
      {
        return this.m_TagImageID;
      }
    }

    public ItemSprayPaintModel(string aName, string theNames, string imageID, int paintQuantity, string tagImageID)
      : base(aName, theNames, imageID)
    {
      if (tagImageID == null)
        throw new ArgumentNullException("tagImageID");
      this.m_MaxPaintQuantity = paintQuantity;
      this.m_TagImageID = tagImageID;
    }
  }
}
