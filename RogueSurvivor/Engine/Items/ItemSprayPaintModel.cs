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
  internal sealed class ItemSprayPaintModel : Data.Model.Item
  {
    public readonly int MaxPaintQuantity;
    public readonly string TagImageID;

    public ItemSprayPaintModel(Gameplay.Item_IDs _id, string aName, string theNames, string imageID, int paintQuantity, string tagImageID, string flavor)
      : base(_id, aName, theNames, imageID, flavor, DollPart.LEFT_HAND)
    {
#if DEBUG
      if (string.IsNullOrEmpty(tagImageID)) throw new ArgumentNullException(nameof(tagImageID));
#endif
      MaxPaintQuantity = paintQuantity;
      TagImageID = tagImageID;
    }

    public override ItemSprayPaint create() => new(this);
    public override ItemSprayPaint from(in Item_s src) => new(this, src.QtyLike);
  }
}
