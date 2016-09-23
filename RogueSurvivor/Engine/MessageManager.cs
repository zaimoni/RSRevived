// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.MessageManager
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics.Contracts;

namespace djack.RogueSurvivor.Engine
{
  internal class MessageManager
  {
    private readonly List<Message> m_Messages = new List<Message>();
    private readonly int m_LinesSpacing;
    private readonly int m_FadeoutFactor;
    private readonly List<Message> m_History;
    private readonly int m_HistorySize;

    public int Count
    {
      get
      {
        return m_Messages.Count;
      }
    }

    public IEnumerable<Message> History
    {
      get
      {
        return (IEnumerable<Message>)m_History;
      }
    }

    public MessageManager(int linesSpacing, int fadeoutFactor, int historySize)
    {
      if (linesSpacing < 0) throw new ArgumentOutOfRangeException("linesSpacing < 0");
      if (fadeoutFactor < 0) throw new ArgumentOutOfRangeException("fadeoutFactor < 0");
      Contract.Requires(0 <= historySize);
      m_LinesSpacing = linesSpacing;
      m_FadeoutFactor = fadeoutFactor;
      m_HistorySize = historySize;
      m_History = new List<Message>(historySize);
    }

    public void Clear()
    {
            m_Messages.Clear();
    }

    public void ClearHistory()
    {
            m_History.Clear();
    }

    public void Add(Message msg)
    {
            m_Messages.Add(msg);
            m_History.Add(msg);
      if (m_History.Count <= m_HistorySize)
        return;
            m_History.RemoveAt(0);
    }

    public void RemoveLastMessage()
    {
      if (m_Messages.Count == 0)
        return;
            m_Messages.RemoveAt(m_Messages.Count - 1);
    }

    public void Draw(IRogueUI ui, int freshMessagesTurn, int gx, int gy)
    {
      for (int index = 0; index < m_Messages.Count; ++index)
      {
        Message message = m_Messages[index];
        int alpha = Math.Max(64, (int) byte.MaxValue - m_FadeoutFactor * (m_Messages.Count - 1 - index));
        bool flag = m_Messages[index].Turn >= freshMessagesTurn;
        Color color = Color.FromArgb(alpha, message.Color);
        if (flag)
          ui.UI_DrawStringBold(color, message.Text, gx, gy, new Color?());
        else
          ui.UI_DrawString(color, message.Text, gx, gy, new Color?());
        gy += m_LinesSpacing;
      }
    }
  }
}
