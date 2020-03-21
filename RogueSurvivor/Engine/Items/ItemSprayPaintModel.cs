// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemSprayPaintModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemSprayPaintModel : ItemModel
  {
    public readonly int MaxPaintQuantity;
    public readonly string TagImageID;

    public ItemSprayPaintModel(Gameplay.GameItems.IDs _id, string aName, string theNames, string imageID, int paintQuantity, string tagImageID, string flavor)
      : base(_id, aName, theNames, imageID, flavor, DollPart.LEFT_HAND)
    {
#if DEBUG
      if (string.IsNullOrEmpty(tagImageID)) throw new ArgumentNullException(nameof(tagImageID));
#endif
      MaxPaintQuantity = paintQuantity;
      TagImageID = tagImageID;
    }

    public override Item create() { return new ItemSprayPaint(this); }
    public ItemSprayPaint instantiate() { return new ItemSprayPaint(this); }
  }
}
