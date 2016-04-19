// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Message
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
  internal class Message
  {
    public string Text { get; set; }
    private Color m_Color;
    private readonly int m_Turn;

    
    public Color Color
    {
      get
      {
        return this.m_Color;
      }
      set
      {
        this.m_Color = value;
      }
    }

    public int Turn
    {
      get
      {
        return this.m_Turn;
      }
    }

    public Message(string text, int turn, Color color)
    {
      if (text == null) throw new ArgumentNullException("text");
      Text = text;
      m_Color = color;
      m_Turn = turn;
    }

    public Message(string text, int turn)
      : this(text, turn, Color.White)
    {
    }
  }
}
