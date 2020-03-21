// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemGrenadeModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemGrenadeModel : ItemExplosiveModel
  {
    public readonly int MaxThrowDistance;

    public ItemGrenadeModel(Gameplay.GameItems.IDs _id, string aName, string theNames, string imageID, int fuseDelay, BlastAttack attack, string blastImageID, int maxThrowDistance, int stackingLimit, string flavor)
      : base(_id, aName, theNames, imageID, fuseDelay, attack, blastImageID, flavor)
    {
      MaxThrowDistance = maxThrowDistance;
      StackingLimit = stackingLimit;
    }
  }
}
