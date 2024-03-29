﻿// Decompiled with JetBrains decompiler
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
using Zaimoni.Data;

using ColorString = System.Collections.Generic.KeyValuePair<System.Drawing.Color, string>;

namespace djack.RogueSurvivor
{
  public class RogueForm : Form, IRogueUI
  {
    private const int CP_NOCLOSE_BUTTON = 512;
    private Font m_NormalFont;
    private Font m_BoldFont;
#nullable enable
    private KeyEventArgs? m_InKey;
#nullable restore
    private bool m_HasMouseButtons;
    private MouseButtons m_MouseButtons;
    private IContainer components;
    private IGameCanvas m_GameCanvas;
    private readonly List<string> m_Mods = new List<string>();
    private static RogueForm? s_ooao = null;

#nullable enable
    public static RogueForm Get { get { return s_ooao!; } }

    public IEnumerable<string> Mods { get => new List<string>(m_Mods); }  // value-copy for const correctness.
    public Font NormalFont { get => m_NormalFont; }
    public Font BoldFont { get => m_BoldFont; }
#nullable restore

#region Init
    public RogueForm()
    {
#if DEBUG
      if (null != s_ooao) throw new InvalidOperationException("only one main form");
#endif
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
      SetClientSizeCore(IRogueUI.CANVAS_WIDTH, IRogueUI.CANVAS_HEIGHT);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "Form::SetStyle...");
      SetStyle(ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint, true);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "create font 1...");
      m_NormalFont = new Font("Lucida Console", 8.25f, FontStyle.Regular);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "create font 2...");
      m_BoldFont = new Font("Lucida Console", 8.25f, FontStyle.Bold);
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "register with IRogueUI...");
      IRogueUI.UI = this;
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "create RogueGame...");
      RogueGame.Init();
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "bind form...");
      s_ooao = this;
      m_GameCanvas.FillGameForm();
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating main form done.");
    }

    private void LoadResources()
    {
      // \todo this has to detect mods and provide them in strictly increasing order
      Logger.WriteLine(Logger.Stage.INIT_GFX, "loading images...");
      GameImages.LoadResources(this);
      Logger.WriteLine(Logger.Stage.INIT_GFX, "loading images done");
    }

    private void InitializeComponent()
    {
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "new ComponentResourceManager...");
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (RogueForm));
      Logger.WriteLine(Logger.Stage.INIT_MAIN, "creating GameCanvas...");
#if OBSOLETE
     if (SetupConfig.Video == SetupConfig.eVideo.VIDEO_MANAGED_DIRECTX) {
       Logger.WriteLine(Logger.Stage.INIT_MAIN, "DXGameCanvas implementation...");
       m_GameCanvas = new DXGameCanvas();
     } else {
#endif
       Logger.WriteLine(Logger.Stage.INIT_MAIN, "GDIPlusGameCanvas implementation...");
       m_GameCanvas = new GDIPlusGameCanvas();
#if OBSOLETE
     }
#endif
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

    // cross-platform Staying Alive says this requires implementation for each UI build target.
    private void doEvents() => Application.DoEvents();

    #region Form overloads
    protected override void OnShown(EventArgs e)
    {
      base.OnShown(e);
      try {
        m_Mods.AddRange(Directory.EnumerateDirectories("Mods"));
      } catch (Exception) { // just eat the exception, not a problem if there are no mods
      }
      LoadResources();
      RogueGame.Game.Run();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
      base.OnSizeChanged(e);
      m_GameCanvas.FillGameForm();
      Invalidate(true);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      var game = RogueGame.Game;
      if (game.IsGameRunning) game.StopTheWorld();
    }
#endregion

#region IRogueUI implementation
#nullable enable
    public KeyEventArgs UI_WaitKey()
    {
      while (true) {    // XXX no clean way to do this loop
        doEvents();
        if (null!= m_InKey) break;
        Thread.Sleep(100);
      }
      return Interlocked.Exchange(ref m_InKey, null);
    }

    // Ancestor is RogueGame::WaitKeyOrMouse
    public KeyValuePair<KeyValuePair<KeyEventArgs?, MouseButtons?>, KeyValuePair<int, int>> WaitKeyOrMouse()
    {
      Interlocked.Exchange(ref m_InKey, null); // discard prior key
      var origin = m_GameCanvas.MouseLocation;
      do {
        Thread.Sleep(1);
        doEvents();

        // inline UI_GetMousePosition()
        var mousePos = m_GameCanvas.MouseLocation;
        var key = Interlocked.Exchange(ref m_InKey, null);
        if (null != key) return new(new(key, null), new(mousePos.X, mousePos.Y));
        // inline UI_PeekMouseButtons
        if (m_HasMouseButtons) {
            m_HasMouseButtons = false;
            return new(new(null, m_MouseButtons), new(mousePos.X, mousePos.Y));
        }
        if (origin != mousePos) return new(default, new(mousePos.X, mousePos.Y));
      } while (true);
    }

#nullable restore

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
          Interlocked.Exchange(ref m_InKey, e);
          e.Handled = true;
          break;
      }
    }

    public void UI_PostMouseButtons(MouseButtons buttons)
    {
      m_HasMouseButtons = true;
      m_MouseButtons = buttons;
    }

    // mouse event support
    // formally should be Point, but we don't want to leak System.Drawing types more than we already are
    public void Add(Action<int, int> op) => m_GameCanvas.Add(op);
    public void Add(Func<int, int, bool> op) => m_GameCanvas.Add(op);

    public void UI_SetCursor(Cursor cursor)
    {
      if (cursor == Cursor) return;
      Cursor = cursor;
      doEvents();
    }

    public void WaitEnter()
    {
      if (RogueGame.IsSimulating) return;
      while (UI_WaitKey().KeyCode != Keys.Return);
    }

    public void WaitEscape()
    {
      if (RogueGame.IsSimulating) return;
      while (UI_WaitKey().KeyCode != Keys.Escape);
    }

    public bool WaitEscape(Predicate<KeyEventArgs> ok)
    {
      if (RogueGame.IsSimulating) return false;
      var test = UI_WaitKey(); // yes, no mouse processing
      while(test.KeyCode != Keys.Escape) {
        if (ok(test)) return true;
        test = UI_WaitKey();
      };
      return false;
    }

#nullable enable
    public T? WaitEscape<T>(Func<KeyEventArgs, T?> ok) where T:class
    {
      T? ret;
      do {
        var key = UI_WaitKey();
        if (key.KeyCode == Keys.Escape) return null;
        ret = ok(key);
      } while (null == ret);
      return ret;
    }
#nullable restore

    public bool WaitYesOrNo()
    {
      var key = UI_WaitKey();
      while (Keys.N != key.KeyCode && Keys.Escape != key.KeyCode) {
        if (Keys.Y == key.KeyCode) return true;
        key = UI_WaitKey();
      };
      return false;
    }

    new public bool Modal(Func<KeyEventArgs, bool?> ok) {
      do {
        var key = UI_WaitKey();
        if (Keys.Escape == key.KeyCode) return false;
        var ret = ok(key);
        if (null != ret) return ret.Value;
      } while(true);
    }

    new public T? Modal<T>(Func<KeyEventArgs, T?> ok) where T:struct {
      do {
        var key = UI_WaitKey();
        if (Keys.Escape == key.KeyCode) return null;
        var ret = ok(key);
        if (null != ret) return ret.Value;
      } while(true);
    }

    public void UI_Wait(int msecs)
    {
      UI_Repaint();
      Thread.Sleep(msecs);
    }

    public void UI_Repaint()
    {
      Refresh();
      doEvents();
    }

    // historically UI_Clear(Color.Black)
    public void ClearScreen() => m_GameCanvas.Clear(Color.Black);

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

    public void DrawHeadNote(string text)
    {
      ClearScreen();
      UI_DrawStringBold(Color.White, text, 0, 0, null);
      UI_Repaint();
    }

    public void DrawFootnote(string text)
    {
      UI_DrawStringBold(Color.White, string.Format("<{0}>", text), 0, IRogueUI.CANVAS_HEIGHT - IRogueUI.BOLD_LINE_SPACING, Color.Gray);
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

    // This arguably should be in the IGameCanvas API, but looks like a live context is needed to avoid empirical corrections
    static private Size Measure(string src, Font font)
    {
      Size ret = TextRenderer.MeasureText(src, font);
      ret.Width -= src.Length; // XXX empirical correction
      return ret;
    }

    static private Size[] Measure(IEnumerable<string> src, Font font)
    {
      var hull = Size.Empty;
      var ret = new List<Size>();
      foreach(var str in src) {
        var box = Measure(str, font);
        if (hull.Width < box.Width) hull.Width = box.Width;
        hull.Height += box.Height;
        ret.Add(box);
      }
      ret.Add(hull);
      return ret.ToArray();
    }

    public Size Measure(string src) { return Measure(src, m_BoldFont); }
    public Size[] Measure(IEnumerable<string> src) { return Measure(src, m_BoldFont); }

    public void UI_DrawPopup(string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy)
    {
      Size[] sizeArray = Measure(lines, m_BoldFont);

      Point location = new Point(gx, gy);
      Size size = new Size(sizeArray[^1].Width + 4, sizeArray[^1].Height + 4);
      Rectangle rect = new Rectangle(location, size);
      const int x_margin = 10;
      if (IRogueUI.CANVAS_WIDTH - x_margin <= rect.Right) {
        int delta = (rect.Right - IRogueUI.CANVAS_WIDTH + x_margin);
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
            Size[] linesSize = Measure(lines, m_BoldFont);

            Size titleSize = Measure(title, m_BoldFont);
            if (titleSize.Width > linesSize[^1].Width) linesSize[^1].Width = titleSize.Width;
            linesSize[^1].Height += titleSize.Height;
            const int TITLE_BAR_LINE = 1;
            linesSize[^1].Height += TITLE_BAR_LINE;

            ///////////////////
            // Setup popup box
            ///////////////////
            const int BOX_MARGIN = 2;
            Point boxPos = new Point(gx, gy);
            Size boxSize = new Size(linesSize[^1].Width + 2 * BOX_MARGIN, linesSize[^1].Height + 2 * BOX_MARGIN);
            Rectangle boxRect = new Rectangle(boxPos, boxSize);

            //////////////////
            // Draw popup box
            //////////////////
            m_GameCanvas.AddFilledRect(boxFillColor, boxRect);
            m_GameCanvas.AddRect(boxBorderColor, boxRect);

            //////////////
            // Draw title
            //////////////
            int titleX = boxPos.X + BOX_MARGIN + (linesSize[^1].Width - titleSize.Width) / 2;
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
            Size[] linesSize = Measure(lines, m_BoldFont);

            Size titleSize = Measure(title, m_BoldFont);
            if (titleSize.Width > linesSize[^1].Width) linesSize[^1].Width = titleSize.Width;
            linesSize[^1].Height += titleSize.Height;
            const int TITLE_BAR_LINE = 1;
            linesSize[^1].Height += TITLE_BAR_LINE;

            ///////////////////
            // Setup popup box
            ///////////////////
            const int BOX_MARGIN = 2;
            Point boxPos = new Point(gx, gy);
            Size boxSize = new Size(linesSize[^1].Width + 2 * BOX_MARGIN, linesSize[^1].Height + 2 * BOX_MARGIN);
            Rectangle boxRect = new Rectangle(boxPos, boxSize);

            //////////////////
            // Draw popup box
            //////////////////
            m_GameCanvas.AddFilledRect(boxFillColor, boxRect);
            m_GameCanvas.AddRect(boxBorderColor, boxRect);

            //////////////
            // Draw title
            //////////////
            int titleX = boxPos.X + BOX_MARGIN + (linesSize[^1].Width - titleSize.Width) / 2;
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
