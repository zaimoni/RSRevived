// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.Items.ItemPrimedExplosive
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Threading;

#nullable enable

namespace djack.RogueSurvivor.Engine.Items
{
  [Serializable]
  internal abstract class ItemPrimedExplosive : ItemExplosive // XXX should be abstract
  {
    private int m_FuseTimeLeft;

    public int FuseTimeLeft { get { return m_FuseTimeLeft; } }

    protected ItemPrimedExplosive(ItemExplosiveModel model) : base(model, model)
    {
      m_FuseTimeLeft = model.FuseDelay;
    }

    protected ItemPrimedExplosive(ItemExplosiveModel model, int delay) : base(model, model)
    {
      m_FuseTimeLeft = delay; // dud would be "much longer than designed"
    }

#region implement Zaimoni.Serialization.ISerialize
    protected ItemPrimedExplosive(Zaimoni.Serialization.DecodeObjects decode) : base(decode) {
        Zaimoni.Serialization.Formatter.Deserialize7bit(decode.src, ref m_FuseTimeLeft);
    }

    new protected void save(Zaimoni.Serialization.EncodeObjects encode) {
        base.save(encode);
        Zaimoni.Serialization.Formatter.Serialize7bit(encode.dest, m_FuseTimeLeft);
    }
#endregion


    public void Cook() { Interlocked.Exchange(ref m_FuseTimeLeft, 0); }    // detonate immediately

    public bool Expire() { return 0 >= Interlocked.Decrement(ref m_FuseTimeLeft); }

    static public bool IsExpired(ItemPrimedExplosive e) { return 0 >= e.m_FuseTimeLeft; }
  }
}
