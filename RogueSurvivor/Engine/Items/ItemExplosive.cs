// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemExplosive
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemExplosive : Item
  {
    new public ItemExplosiveModel Model { get {return (base.Model as ItemExplosiveModel)!; } }
    public readonly int PrimedModelID;

    public ItemExplosive(ItemExplosiveModel model, ItemExplosiveModel primedModel, int qty=1)
      : base(model, qty)
    {
      PrimedModelID = (int) primedModel.ID;
    }
  }
}
