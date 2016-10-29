// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.CSVLine
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine
{
  public class CSVLine
  {
    private readonly CSVField[] m_Fields;

    public CSVField this[int field] {
      get {
        Contract.Requires(0 <= field);
        return m_Fields[field];
      }
      set {
        Contract.Requires(0 <= field);
        m_Fields[field] = value;
      }
    }

    public int FieldsCount {
      get {
        return m_Fields.Length;
      }
    }

    public CSVLine(int nbFields)
    {
      Contract.Requires(1 <= nbFields);
      m_Fields = new CSVField[nbFields];
    }
  }
}
