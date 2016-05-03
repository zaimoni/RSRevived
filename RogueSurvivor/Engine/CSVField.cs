// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.CSVField
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

namespace djack.RogueSurvivor.Engine
{
  public class CSVField
  {
    private string m_RawString;

    public CSVField(string rawString)
    {
            m_RawString = rawString;
    }

    public int ParseInt()
    {
      return int.Parse(m_RawString);
    }

    public float ParseFloat()
    {
      return float.Parse(m_RawString);
    }

    public string ParseText()
    {
      return m_RawString.Trim('"');
    }

    public bool ParseBool()
    {
      return ParseInt() > 0;
    }
  }
}
