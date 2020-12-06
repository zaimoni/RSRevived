// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.CSVLine
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;

#nullable enable

namespace djack.RogueSurvivor.Engine
{
  public class CSVLine
  {
    private readonly CSVField[] m_Fields;

    public CSVField this[int field] {
      get {
#if DEBUG
        if (0 > field) throw new InvalidOperationException("0 > field");
#endif
        return m_Fields[field]
#if DEBUG
          ?? throw new InvalidOperationException("m_Fields[field]")
#endif
        ;
      }
      set {
#if DEBUG
        if (0 > field) throw new InvalidOperationException("0 > field");
#endif
        m_Fields[field] = value
#if DEBUG
          ?? throw new InvalidOperationException(nameof(value))
#endif
        ;
      }
    }

    public int FieldsCount { get { return m_Fields.Length; } }

    public CSVLine(string[] src) {
#if DEBUG
        if (null == src || 1 > src.Length) throw new ArgumentNullException(nameof(src));
#endif
        m_Fields = new CSVField[src.Length];
        int i = src.Length;
        while(0 <= --i) m_Fields[i] = new CSVField(src[i]);
    }
  }
}
