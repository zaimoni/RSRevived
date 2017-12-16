// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Data.Message
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Drawing;
using Zaimoni.Data;

namespace djack.RogueSurvivor.Data
{
  internal class Message
  {
    public readonly string Text;
    public readonly Color Color;
    public readonly int Turn;

    public Message(string text, int turn, Color color)
    {
#if DEBUG
      if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));
#endif
      Text = turn.ToString()+" "+text.Capitalize();
      Color = color;
      Turn = turn;
    }

    public Message(string text, int turn)
      : this(text, turn, Color.White)
    {
    }
  }
}
