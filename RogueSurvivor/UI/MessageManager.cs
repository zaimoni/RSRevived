// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Engine.MessageManager
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;

using Color = System.Drawing.Color;

#nullable enable

namespace djack.RogueSurvivor.UI
{
  [Serializable]
  public class MessageManager
  {
    private readonly List<Message> m_Messages;
    private readonly int m_LinesSpacing; // normal; bold is 2 more
    private readonly List<Message> m_History;
    private readonly int m_HistorySize;
    private readonly int m_DisplaySize;

    public MessageManager(int linesSpacing, int historySize, int displaySize)
    {
#if DEBUG
      if (0 >= linesSpacing) throw new ArgumentOutOfRangeException(nameof(linesSpacing));
      if (0 >= historySize) throw new ArgumentOutOfRangeException(nameof(historySize));
      if (0 >= displaySize) throw new ArgumentOutOfRangeException(nameof(displaySize));
#endif
      m_LinesSpacing = linesSpacing;
      m_HistorySize = historySize;
      m_DisplaySize = displaySize;
      m_History = new(historySize);
      m_Messages = new(displaySize);
    }

    public void Clear() { m_Messages.Clear(); }
    public void ClearHistory() { m_History.Clear(); }

    public void Add(Message msg)
    {
      if (m_HistorySize <= m_History.Count) m_History.RemoveAt(0);
      if (m_DisplaySize <= m_Messages.Count) m_Messages.RemoveAt(0);
      m_Messages.Add(msg);
      m_History.Add(msg);
    }

    public void Add(IEnumerable<Message> msgs) { foreach(var msg in msgs) Add(msg); }

    public void RemoveLastMessage()
    {
      int count;
      if (0 < (count = m_Messages.Count)) m_Messages.RemoveAt(count - 1);
    }

    public void Draw(IRogueUI ui, int freshMessagesTurn, int gx, int gy) {
      const int FADEOUT = 25;

      for (int index = 0; index < m_Messages.Count; ++index) {
        Message message = m_Messages[index];
        int alpha = Math.Max(64, (int) byte.MaxValue - FADEOUT * (m_Messages.Count - 1 - index));
        var color = Color.FromArgb(alpha, message.Color);
        if (message.Turn >= freshMessagesTurn) ui.UI_DrawStringBold(color, message.Text, gx, gy);
        else ui.UI_DrawString(color, message.Text, gx, gy);
        gy += m_LinesSpacing;
      }
    }

    public void DrawMessageLog(IRogueUI ui, int gx, int gy) {
        foreach (var msg in m_History) {
            ui.UI_DrawString(msg.Color, msg.Text, gx, gy);
            gy += m_LinesSpacing;
        }
    }
  }
}
