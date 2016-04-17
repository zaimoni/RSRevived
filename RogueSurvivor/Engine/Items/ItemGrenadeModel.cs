// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemGrenadeModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemGrenadeModel : ItemExplosiveModel
  {
    private int m_MaxThrowDistance;

    public int MaxThrowDistance
    {
      get
      {
        return this.m_MaxThrowDistance;
      }
    }

    public ItemGrenadeModel(string aName, string theNames, string imageID, int fuseDelay, BlastAttack attack, string blastImageID, int maxThrowDistance)
      : base(aName, theNames, imageID, fuseDelay, attack, blastImageID)
    {
      this.m_MaxThrowDistance = maxThrowDistance;
    }
  }
}
