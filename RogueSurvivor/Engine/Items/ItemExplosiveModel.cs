// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemExplosiveModel
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
  internal class ItemExplosiveModel : ItemModel
  {
    private int m_FuseDelay;
    private BlastAttack m_Attack;
    private string m_BlastImageID;

    public int FuseDelay
    {
      get
      {
        return this.m_FuseDelay;
      }
    }

    public BlastAttack BlastAttack
    {
      get
      {
        return this.m_Attack;
      }
    }

    public string BlastImage
    {
      get
      {
        return this.m_BlastImageID;
      }
    }

    public ItemExplosiveModel(string aName, string theNames, string imageID, int fuseDelay, BlastAttack attack, string blastImageID)
      : base(aName, theNames, imageID)
    {
      this.m_FuseDelay = fuseDelay;
      this.m_Attack = attack;
      this.m_BlastImageID = blastImageID;
    }
  }
}
