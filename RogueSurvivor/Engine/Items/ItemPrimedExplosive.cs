// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemPrimedExplosive
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemPrimedExplosive : ItemExplosive
  {
    public int FuseTimeLeft { get; set; }

    public ItemPrimedExplosive(ItemModel model)
      : base(model, model)
    {
      if (!(model is ItemExplosiveModel))
        throw new ArgumentException("model is not ItemExplosiveModel");
            FuseTimeLeft = (model as ItemExplosiveModel).FuseDelay;
    }
  }
}
