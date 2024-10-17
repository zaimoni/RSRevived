// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.UniqueItems
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Engine
{
  [Serializable]
  public class UniqueItems : Zaimoni.Serialization.ISerialize
  {
    public UniqueItem TheSubwayWorkerBadge = default;

    public UniqueItems() {}

#region implement Zaimoni.Serialization.ISerialize
    protected UniqueItems(Zaimoni.Serialization.DecodeObjects decode) {
        Zaimoni.Serialization.ISave.Load(decode, ref TheSubwayWorkerBadge, (it,spawned) => {
            TheSubwayWorkerBadge = new(it, spawned);
        });
    }

    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) {
        Zaimoni.Serialization.ISave.Save(encode, in TheSubwayWorkerBadge);
    }
#endregion
  }
}
