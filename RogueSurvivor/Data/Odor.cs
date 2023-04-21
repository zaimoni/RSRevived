// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Odor
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

namespace djack.RogueSurvivor.Data
{
  [Serializable]
  public enum Odor : byte
  {
    LIVING,
    UNDEAD_MASTER,
    SUPPRESSOR  // alpha 10
  }

    [Serializable]
  public class OdorScent : Zaimoni.Serialization.ISerialize
  {
    public const int MIN_STRENGTH = 1;
    public const int MAX_STRENGTH = 9*WorldTime.TURNS_PER_HOUR;

    private short m_Strength;
    public readonly Odor Odor;

    public int Strength {
      get {
        return m_Strength;
      }
      set {
        if (MIN_STRENGTH > value) m_Strength = 0;
        else if (MAX_STRENGTH < value) m_Strength = MAX_STRENGTH;
        else m_Strength = (short)value;
      }
    }

    public OdorScent(Odor odor, int strength)
    {
      Odor = odor;
      Strength = strength;
    }

#region implement Zaimoni.Serialization.ISerialize
    public OdorScent(Zaimoni.Serialization.DecodeObjects decode)
    {
        byte stage = default;
        Zaimoni.Serialization.Formatter.Deserialize(decode.src, ref stage);
        Odor = (Odor)stage;
        Zaimoni.Serialization.Formatter.Deserialize7bit(decode.src, ref m_Strength);
    }

    void Zaimoni.Serialization.ISerialize.save(Zaimoni.Serialization.EncodeObjects encode)
    {
        Zaimoni.Serialization.Formatter.Serialize(encode.dest, (byte)Odor);
        Zaimoni.Serialization.Formatter.Serialize7bit(encode.dest, m_Strength);
    }
#endregion

    public bool Decay(short decay_rate)
    {
      bool ret = MIN_STRENGTH > (m_Strength -= decay_rate);
      if (ret) m_Strength = 0;
      return ret;
    }
  }
}
