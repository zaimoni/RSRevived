// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemGrenade
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal class ItemGrenade : ItemExplosive
  {
    new public ItemGrenadeModel Model { get {return (base.Model as ItemGrenadeModel)!; } }
    public ItemGrenade(ItemGrenadeModel model, ItemGrenadePrimedModel primedModel, int qty=1)
      : base(model, primedModel, qty)
    {
    }
  }
}
