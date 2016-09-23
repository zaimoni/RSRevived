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
    private readonly int m_FuseDelay;
    private readonly BlastAttack m_Attack;
    private readonly string m_BlastImageID;

    public int FuseDelay {
      get {
        return m_FuseDelay;
      }
    }

    public BlastAttack BlastAttack {
      get {
        return m_Attack;
      }
    }

    public string BlastImage {
      get {
        return m_BlastImageID;
      }
    }

    public ItemExplosiveModel(string aName, string theNames, string imageID, int fuseDelay, BlastAttack attack, string blastImageID)
      : base(aName, theNames, imageID)
    {
      m_FuseDelay = fuseDelay;
      m_Attack = attack;
      m_BlastImageID = blastImageID;
    }
  }
}
