// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.MessageManager
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace djack.RogueSurvivor.Engine
{
  internal class MessageManager
  {
    private readonly List<Message> m_Messages = new List<Message>();
    private int m_LinesSpacing;
    private int m_FadeoutFactor;
    private readonly List<Message> m_History;
    private int m_HistorySize;

    public int Count
    {
      get
      {
        return this.m_Messages.Count;
      }
    }

    public IEnumerable<Message> History
    {
      get
      {
        return (IEnumerable<Message>) this.m_History;
      }
    }

    public MessageManager(int linesSpacing, int fadeoutFactor, int historySize)
    {
      if (linesSpacing < 0)
        throw new ArgumentOutOfRangeException("linesSpacing < 0");
      if (fadeoutFactor < 0)
        throw new ArgumentOutOfRangeException("fadeoutFactor < 0");
      this.m_LinesSpacing = linesSpacing;
      this.m_FadeoutFactor = fadeoutFactor;
      this.m_HistorySize = historySize;
      this.m_History = new List<Message>(historySize);
    }

    public void Clear()
    {
      this.m_Messages.Clear();
    }

    public void ClearHistory()
    {
      this.m_History.Clear();
    }

    public void Add(Message msg)
    {
      this.m_Messages.Add(msg);
      this.m_History.Add(msg);
      if (this.m_History.Count <= this.m_HistorySize)
        return;
      this.m_History.RemoveAt(0);
    }

    public void RemoveLastMessage()
    {
      if (this.m_Messages.Count == 0)
        return;
      this.m_Messages.RemoveAt(this.m_Messages.Count - 1);
    }

    public void Draw(IRogueUI ui, int freshMessagesTurn, int gx, int gy)
    {
      for (int index = 0; index < this.m_Messages.Count; ++index)
      {
        Message message = this.m_Messages[index];
        int alpha = Math.Max(64, (int) byte.MaxValue - this.m_FadeoutFactor * (this.m_Messages.Count - 1 - index));
        bool flag = this.m_Messages[index].Turn >= freshMessagesTurn;
        Color color = Color.FromArgb(alpha, message.Color);
        if (flag)
          ui.UI_DrawStringBold(color, message.Text, gx, gy, new Color?());
        else
          ui.UI_DrawString(color, message.Text, gx, gy, new Color?());
        gy += this.m_LinesSpacing;
      }
    }
  }
}
