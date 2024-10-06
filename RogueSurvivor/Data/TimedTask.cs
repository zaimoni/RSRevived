// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.TimedTask
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  public abstract class TimedTask : Zaimoni.Serialization.ISerialize
  {
    public int TurnsLeft { get; protected set; }

    public bool IsCompleted { get { return TurnsLeft <= 0; } }

    protected TimedTask(int turnsLeft)
    {
      TurnsLeft = turnsLeft;
    }

#region implement Zaimoni.Serialization.ISerialize
    protected TimedTask(Zaimoni.Serialization.DecodeObjects decode) {
        int stage_int = 0;
        Zaimoni.Serialization.Formatter.Deserialize(decode.src, ref stage_int);
        TurnsLeft = stage_int;
    }

    protected void save(Zaimoni.Serialization.EncodeObjects encode) {
        Zaimoni.Serialization.Formatter.Serialize(encode.dest, TurnsLeft);
    }

    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode) {
        Zaimoni.Serialization.Formatter.Serialize(encode.dest, TurnsLeft);
    }
#endregion

    public void Tick(Map m)
    {
      if (--TurnsLeft > 0) return;
      Trigger(m);
    }

    public abstract void Trigger(Map m);
  }
}
