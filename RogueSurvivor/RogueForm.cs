// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.RogueForm
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Gameplay;
using djack.RogueSurvivor.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms; // causes lock-in to System.Drawing
using ColorString = System.Collections.Generic.KeyValuePair<System.Drawing.Color, string>;

namespace djack.RogueSurvivor
{
  public class RogueForm : Form, IRogueUI
  {
    private const int CP_NOCLOSE_BUTTON = 512;
    private Font m_NormalFont;
    private Font m_BoldFont;
    private KeyEventArgs m_InKey;
    private bool m_HasMouseButtons;
    private MouseButtons m_MouseButtons;
    private IContainer components;
    private IGameCanvas m_GameCanvas;
    private List<string> m_Mods = new List<string>();

    internal static RogueGame Game { get; private set; }    // de-facto singleton
    public IEnumerable<string> Mods { get { return new List<string>(m_Mods); } }  // value-copy for const correctness.

#region Init
    public RogueForm()
    {
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating main form...");
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "Form::InitializeComponent...");
      InitializeComponent();
      Text = SetupConfig.GAME_NAME+" - " + SetupConfig.GAME_VERSION;
      if (SetupConfig.Video == SetupConfig.eVideo.VIDEO_GDI_PLUS)
        Text += " (GDI+)";
      switch (SetupConfig.Sound)
      {
      case SetupConfig.eSound.SOUND_WAV:
        Text += " (sndWAV)";
        break;
      case SetupConfig.eSound.SOUND_NOSOUND:
        Text += " (nosound)";
        break;
      }
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "Form::SetClientSizeCore...");
      SetClientSizeCore(RogueGame.CANVAS_WIDTH, RogueGame.CANVAS_HEIGHT);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "Form::SetStyle...");
      SetStyle(ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint, true);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "create font 1...");
      m_NormalFont = new Font("Lucida Console", 8.25f, FontStyle.Regular);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "create font 2...");
      m_BoldFont = new Font("Lucida Console", 8.25f, FontStyle.Bold);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "create RogueGame...");
      Game = new RogueGame((IRogueUI) this);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "bind form...");
      m_GameCanvas.BindForm(this);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating main form done.");
    }

    private void LoadResources()
    {
      // \todo this has to detect mods and provide them in strictly increasing order
      Logger.WriteLine(Logger.Stage.INIT_GFX, "loading images...");
      GameImages.LoadResources((IRogueUI) this);
      Logger.WriteLine(Logger.Stage.INIT_GFX, "loading images done");
    }

    private void InitializeComponent()
    {
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "new ComponentResourceManager...");
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (RogueForm));
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating GameCanvas...");
//      if (SetupConfig.Video == SetupConfig.eVideo.VIDEO_MANAGED_DIRECTX)
//      {
//        Logger.WriteLine(Logger.Stage.INIT_MAIN, "DXGameCanvas implementation...");
//        this.m_GameCanvas = (IGameCanvas) new DXGameCanvas();
//      }
//      else
//     {
        Logger.WriteLine(Logger.Stage.INIT_MAIN, "GDIPlusGameCanvas implementation...");
            m_GameCanvas = (IGameCanvas) new GDIPlusGameCanvas();
//      }
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "SuspendLayout...");
      SuspendLayout();
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "setup GameCanvas...");
      m_GameCanvas.NeedRedraw = true;
      UserControl userControl = m_GameCanvas as UserControl;
      userControl.Location = new Point(279, 83);
      userControl.Name = "canvasCtrl";
      userControl.Size = new Size(150, 150);
      userControl.TabIndex = 0;
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "setup RogueForm");
      AutoScaleMode = AutoScaleMode.None;
      ClientSize = new Size(800, 600);
      Controls.Add(userControl);
      Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      Name = "RogueForm";
      StartPosition = FormStartPosition.CenterScreen;
      Text = SetupConfig.GAME_NAME;
      WindowState = FormWindowState.Maximized;
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "ResumeLayout");
      ResumeLayout(false);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "InitializeComponent() done.");
    }
#endregion

#region Form overloads
    protected override void OnShown(EventArgs e)
    {
      base.OnShown(e);
      try {
        m_Mods.AddRange(Directory.EnumerateDirectories("Mods"));
      } catch (Exception) { // just eat the exception, not a problem if there are no mods
      }
      LoadResources();
      Game.Run();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
      base.OnSizeChanged(e);
      m_GameCanvas.FillGameForm();
      Invalidate(true);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      if (!Game.IsGameRunning) return;
      Game.StopTheWorld();
    }
#endregion

#region IRogueUI implementation
    // <returns>non-null</returns>
    public KeyEventArgs UI_WaitKey()
    {
      while (true) {    // XXX no clean way to do this loop
        Application.DoEvents();
        if (null!= m_InKey) break;
        Thread.Sleep(100);
      }
      KeyEventArgs tmp = m_InKey;
      m_InKey = null;
      return tmp;
    }

    public KeyEventArgs UI_PeekKey()
    {
      Thread.Sleep(1);
      Application.DoEvents();
      KeyEventArgs tmp = m_InKey;
      m_InKey = null;
      return tmp;
    }

    public void UI_PostKey(KeyEventArgs e)
    {
      switch (e.KeyCode)
      {
        case Keys.Shift:
          break;
        case Keys.Control:
          break;
        case Keys.Alt:
          break;
        case Keys.ShiftKey:
          break;
        case Keys.ControlKey:
          break;
        case Keys.LShiftKey:
          break;
        case Keys.RShiftKey:
          break;
        case Keys.LControlKey:
          break;
        case Keys.RControlKey:
          break;
        default:
          m_InKey = e;
          e.Handled = true;
          break;
      }
    }

    public Point UI_GetMousePosition()
    {
      Thread.Sleep(1);
      Application.DoEvents();
      return m_GameCanvas.MouseLocation;
    }

    public void UI_PostMouseButtons(MouseButtons buttons)
    {
      m_HasMouseButtons = true;
      m_MouseButtons = buttons;
    }

    public MouseButtons? UI_PeekMouseButtons()
    {
      if (!m_HasMouseButtons) return null;
      m_HasMouseButtons = false;
      return m_MouseButtons;
    }

    public void UI_SetCursor(Cursor cursor)
    {
      if (cursor == Cursor) return;
      Cursor = cursor;
      Application.DoEvents();
    }

    public void UI_Wait(int msecs)
    {
      UI_Repaint();
      Thread.Sleep(msecs);
    }

    public void UI_Repaint()
    {
      Refresh();
      Application.DoEvents();
    }

    public void UI_Clear(Color clearColor)
    {
      m_GameCanvas.Clear(clearColor);
    }

    public void UI_DrawImage(string imageID, int gx, int gy)
    {
      m_GameCanvas.AddImage(GameImages.Get(imageID), gx, gy);
    }

    public void UI_DrawImage(string imageID, int gx, int gy, Color tint)
    {
      m_GameCanvas.AddImage(GameImages.Get(imageID), gx, gy, tint);
    }

    public void UI_DrawImageTransform(string imageID, int gx, int gy, float rotation, float scale)
    {
      m_GameCanvas.AddImageTransform(GameImages.Get(imageID), gx, gy, rotation, scale);
    }

    public void UI_DrawGrayLevelImage(string imageID, int gx, int gy)
    {
      m_GameCanvas.AddImage(GameImages.GetGrayLevel(imageID), gx, gy);
    }

    public void UI_DrawTransparentImage(float alpha, string imageID, int gx, int gy)
    {
      m_GameCanvas.AddTransparentImage(alpha, GameImages.Get(imageID), gx, gy);
    }

    public void UI_DrawPoint(Color color, int gx, int gy)
    {
      m_GameCanvas.AddPoint(color, gx, gy);
    }

    public void UI_DrawLine(Color color, int gxFrom, int gyFrom, int gxTo, int gyTo)
    {
      m_GameCanvas.AddLine(color, gxFrom, gyFrom, gxTo, gyTo);
    }

    public void UI_DrawString(Color color, string text, int gx, int gy, Color? shadowColor)
    {
      if (shadowColor.HasValue)
        m_GameCanvas.AddString(m_NormalFont, shadowColor.Value, text, gx + 1, gy + 1);
      m_GameCanvas.AddString(m_NormalFont, color, text, gx, gy);
    }

    public void UI_DrawString(ColorString text, int gx, int gy, Color? shadowColor)
    {
      if (!string.IsNullOrEmpty(text.Value))
        UI_DrawString(text.Key, text.Value, gx, gy, shadowColor);
    }

    public void UI_DrawStringBold(Color color, string text, int gx, int gy, Color? shadowColor)
    {
      if (shadowColor.HasValue)
        m_GameCanvas.AddString(m_BoldFont, shadowColor.Value, text, gx + 1, gy + 1);
      m_GameCanvas.AddString(m_BoldFont, color, text, gx, gy);
    }

    public void UI_DrawStringBold(ColorString text, int gx, int gy, Color? shadowColor = null)
    {
      if (!string.IsNullOrEmpty(text.Value))
        UI_DrawStringBold(text.Key, text.Value, gx, gy, shadowColor);
    }

    public void UI_DrawRect(Color color, Rectangle rect)
    {
#if DEBUG
      if (0 >= rect.Width) throw new ArgumentOutOfRangeException(nameof(rect.Width),rect.Width, "0 >= rect.Width");
      if (0 >= rect.Height) throw new ArgumentOutOfRangeException(nameof(rect.Height),rect.Height, "0 >= rect.Height");
#endif
      m_GameCanvas.AddRect(color, rect);
    }

    public void UI_FillRect(Color color, Rectangle rect)
    {
#if DEBUG
      if (0 >= rect.Width) throw new ArgumentOutOfRangeException(nameof(rect.Width),rect.Width, "0 >= rect.Width");
      if (0 >= rect.Height) throw new ArgumentOutOfRangeException(nameof(rect.Height),rect.Height, "0 >= rect.Height");
#endif
      m_GameCanvas.AddFilledRect(color, rect);
    }

    public void UI_DrawPopup(string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy)
    {
      int num1 = 0;
      int num2 = 0;
      Size[] sizeArray = new Size[lines.Length];
      for (int index = 0; index < lines.Length; ++index) {
        sizeArray[index] = TextRenderer.MeasureText(lines[index], m_BoldFont);
        if (sizeArray[index].Width > num1)
          num1 = sizeArray[index].Width;
        num2 += sizeArray[index].Height;
      }
      Point location = new Point(gx, gy);
      Size size = new Size(num1 + 4, num2 + 4);
      Rectangle rect = new Rectangle(location, size);
      const int x_margin = 10;
      if (RogueGame.CANVAS_WIDTH - x_margin <= rect.Right) {
        int delta = (rect.Right - RogueGame.CANVAS_WIDTH+ x_margin);
        if (delta <= rect.X) {
          rect.X -= delta;
          location.X -= delta;
        } else if (0 < rect.X) {
          rect.X = 0;
          location.X = 0;
        }
      }
      if (RogueGame.MESSAGES_Y <= rect.Bottom) {
        int delta = (rect.Bottom - RogueGame.MESSAGES_Y);
        if (delta <= rect.Y) {
          rect.Y -= delta;
          location.Y -= delta;
        } else if (0 < rect.X) {
          rect.Y = 0;
          location.Y = 0;
        }
      }

      m_GameCanvas.AddFilledRect(boxFillColor, rect);
      m_GameCanvas.AddRect(boxBorderColor, rect);
      int gx1 = location.X + 2;
      int gy1 = location.Y + 2;
      for (int index = 0; index < lines.Length; ++index) {
        m_GameCanvas.AddString(m_BoldFont, textColor, lines[index], gx1, gy1);
        gy1 += sizeArray[index].Height;
      }
    }

    // pre-rendering tiles
    public void AddTile(string imageID)
    {
      m_GameCanvas.AddTile(GameImages.Get(imageID));
    }

    public void AddTile(string imageID, Color color)
    {
      m_GameCanvas.AddTile(GameImages.Get(imageID), color);
    }

    public void AppendTile(string imageID)
    {
      m_GameCanvas.AppendTile(GameImages.Get(imageID));
    }

    public void AppendTile(string imageID, Color color)
    {
      AppendTile(imageID);
    }

    public void AppendTile(Color color, string text, Font font, int x, int y)
    {
      m_GameCanvas.AppendTile(color, text, font, x, y);
    }

    public void DrawTile(int x, int y)
    {
      m_GameCanvas.DrawTile(x, y);
    }

        // alpha10
        public void UI_DrawPopupTitle(string title, Color titleColor, string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy)
        {
            /////////////////
            // Measure lines
            /////////////////
            int longestLineWidth = 0;
            int totalLineHeight = 0;
            Size[] linesSize = new Size[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                linesSize[i] = TextRenderer.MeasureText(lines[i], m_BoldFont);
                if (linesSize[i].Width > longestLineWidth)
                    longestLineWidth = linesSize[i].Width;
                totalLineHeight += linesSize[i].Height;
            }

            Size titleSize = TextRenderer.MeasureText(title, m_BoldFont);
            if (titleSize.Width > longestLineWidth)
                longestLineWidth = titleSize.Width;
            totalLineHeight += titleSize.Height;
            const int TITLE_BAR_LINE = 1;
            totalLineHeight += TITLE_BAR_LINE;

            ///////////////////
            // Setup popup box
            ///////////////////
            const int BOX_MARGIN = 2;
            Point boxPos = new Point(gx, gy);
            Size boxSize = new Size(longestLineWidth + 2 * BOX_MARGIN, totalLineHeight + 2 * BOX_MARGIN);
            Rectangle boxRect = new Rectangle(boxPos, boxSize);

            //////////////////
            // Draw popup box
            //////////////////
            m_GameCanvas.AddFilledRect(boxFillColor, boxRect);
            m_GameCanvas.AddRect(boxBorderColor, boxRect);

            //////////////
            // Draw title
            //////////////
            int titleX = boxPos.X + BOX_MARGIN + (longestLineWidth - titleSize.Width) / 2;
            int titleY = boxPos.Y + BOX_MARGIN;
            int titleLineY = titleY + titleSize.Height + TITLE_BAR_LINE;
            m_GameCanvas.AddString(m_BoldFont, titleColor, title, titleX, titleY);
            m_GameCanvas.AddLine(boxBorderColor, boxRect.Left, titleLineY, boxRect.Right, titleLineY);

            //////////////
            // Draw lines
            //////////////
            int lineX = boxPos.X + BOX_MARGIN;
            int lineY = titleLineY + TITLE_BAR_LINE;

            for (int i = 0; i < lines.Length; i++)
            {
                m_GameCanvas.AddString(m_BoldFont, textColor, lines[i], lineX, lineY);
                lineY += linesSize[i].Height;
            }
        }

        // alpha10
        public void UI_DrawPopupTitleColors(string title, Color titleColor, string[] lines, Color[] colors, Color boxBorderColor, Color boxFillColor, int gx, int gy)
        {
            /////////////////
            // Measure lines
            /////////////////
            int longestLineWidth = 0;
            int totalLineHeight = 0;
            Size[] linesSize = new Size[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                linesSize[i] = TextRenderer.MeasureText(lines[i], m_BoldFont);
                if (linesSize[i].Width > longestLineWidth)
                    longestLineWidth = linesSize[i].Width;
                totalLineHeight += linesSize[i].Height;
            }

            Size titleSize = TextRenderer.MeasureText(title, m_BoldFont);
            if (titleSize.Width > longestLineWidth)
                longestLineWidth = titleSize.Width;
            totalLineHeight += titleSize.Height;
            const int TITLE_BAR_LINE = 1;
            totalLineHeight += TITLE_BAR_LINE;

            ///////////////////
            // Setup popup box
            ///////////////////
            const int BOX_MARGIN = 2;
            Point boxPos = new Point(gx, gy);
            Size boxSize = new Size(longestLineWidth + 2 * BOX_MARGIN, totalLineHeight + 2 * BOX_MARGIN);
            Rectangle boxRect = new Rectangle(boxPos, boxSize);

            //////////////////
            // Draw popup box
            //////////////////
            m_GameCanvas.AddFilledRect(boxFillColor, boxRect);
            m_GameCanvas.AddRect(boxBorderColor, boxRect);

            //////////////
            // Draw title
            //////////////
            int titleX = boxPos.X + BOX_MARGIN + (longestLineWidth - titleSize.Width) / 2;
            int titleY = boxPos.Y + BOX_MARGIN;
            int titleLineY = titleY + titleSize.Height + TITLE_BAR_LINE;
            m_GameCanvas.AddString(m_BoldFont, titleColor, title, titleX, titleY);
            m_GameCanvas.AddLine(boxBorderColor, boxRect.Left, titleLineY, boxRect.Right, titleLineY);

            //////////////
            // Draw lines
            //////////////
            int lineX = boxPos.X + BOX_MARGIN;
            int lineY = titleLineY + TITLE_BAR_LINE;

            for (int i = 0; i < lines.Length; i++)
            {
                m_GameCanvas.AddString(m_BoldFont, colors[i], lines[i], lineX, lineY);
                lineY += linesSize[i].Height;
            }
        }

    public void UI_ClearMinimap(Color color)
    {
      m_GameCanvas.ClearMinimap(color);
    }

    public void UI_SetMinimapColor(int x, int y, Color color)
    {
      m_GameCanvas.SetMinimapColor(x, y, color);
    }

    public void UI_DrawMinimap(int gx, int gy)
    {
      m_GameCanvas.DrawMinimap(gx, gy);
    }

    public float UI_GetCanvasScaleX()
    {
      return m_GameCanvas.ScaleX;
    }

    public float UI_GetCanvasScaleY()
    {
      return m_GameCanvas.ScaleY;
    }

    public string UI_SaveScreenshot(string filePath)
    {
      return m_GameCanvas.SaveScreenShot(filePath);
    }

    public string UI_ScreenshotExtension()
    {
      return m_GameCanvas.ScreenshotExtension();
    }

    public void UI_DoQuit()
    {
      Close();
    }
#endregion

    protected override void Dispose(bool disposing)
    {
      if (disposing)
            {
            if (null != components)
                {
                components.Dispose();
                components = null;
                }
            if (null != m_NormalFont)
                {
                m_NormalFont.Dispose();
                m_NormalFont = null;
                }
            if (null != m_BoldFont)
                {
                m_BoldFont.Dispose();
                m_BoldFont = null;
                }
           }
      if (disposing && m_GameCanvas != null)
        m_GameCanvas.DisposeUnmanagedResources();
      base.Dispose(disposing);
    }
  }
}
