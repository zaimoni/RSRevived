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
    private string m_Text;
    public readonly Color Color;
    public readonly int Turn;

    public string Text {
      get { 
        return m_Text;
      }
      set { // RogueGame::AddMessage requires this to be public
#if DEBUG
        if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
#endif
        m_Text = value;
      }
    }
    
    public Message(string text, int turn, Color color)
    {
#if DEBUG
      if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));
#endif
      m_Text = text;
      Color = color;
      Turn = turn;
    }

    public Message(string text, int turn)
      : this(text, turn, Color.White)
    {
    }
  }
}
