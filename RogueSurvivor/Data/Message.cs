// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Message
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Drawing;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Data
{
  internal class Message
  {
    private string m_Text;
    public Color Color { get; set; }
    private readonly int m_Turn;

    public string Text {
      get { 
        return m_Text;
      }
      set { 
        Contract.Requires(null!=value);
        m_Text = value;
      }
    }

    public int Turn {
      get {
        return m_Turn;
      }
    }

    public Message(string text, int turn, Color color)
    {
      Contract.Requires(null!=text);
      m_Text = text;
      Color = color;
      m_Turn = turn;
    }

    public Message(string text, int turn)
      : this(text, turn, Color.White)
    {
    }
  }
}
