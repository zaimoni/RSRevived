// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemGrenadePrimedModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemGrenadePrimedModel : ItemExplosiveModel
  {
    public readonly ItemGrenadeModel GrenadeModel;

    public ItemGrenadePrimedModel(Gameplay.GameItems.IDs _id, string aName, string theNames, string imageID, ItemGrenadeModel grenadeModel)
      : base(_id, aName, theNames, imageID, grenadeModel.FuseDelay, grenadeModel.BlastAttack, grenadeModel.BlastImage, grenadeModel.FlavorDescription)
    {
#if DEBUG
      if (null == grenadeModel) throw new ArgumentNullException(nameof(grenadeModel));
#endif
      GrenadeModel = grenadeModel;
    }

    public override Item create() { return new ItemGrenade(GrenadeModel,this); }
    public ItemGrenade instantiate() { return new ItemGrenade(GrenadeModel, this); }
  }
}
