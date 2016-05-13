// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.RogueForm
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Gameplay;
using djack.RogueSurvivor.UI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Security.Permissions;

namespace djack.RogueSurvivor
{
  public class RogueForm : Form, IRogueUI
  {
    private const int CP_NOCLOSE_BUTTON = 512;
    private Font m_NormalFont;
    private Font m_BoldFont;
    private bool m_HasKey;
    private KeyEventArgs m_InKey;
    private bool m_HasMouseButtons;
    private MouseButtons m_MouseButtons;
    private IContainer components;
    private IGameCanvas m_GameCanvas;

    internal RogueGame Game { get; private set; }

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
            SetClientSizeCore(1024, 768);
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
            Controls.Add((Control) userControl);
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
    protected override CreateParams CreateParams
    {
      [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
      get
      {
        CreateParams createParams = base.CreateParams;
        createParams.ClassStyle |= CP_NOCLOSE_BUTTON;
        return createParams;
      }
    }

    protected override void OnShown(EventArgs e)
    {
      base.OnShown(e);
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
      e.Cancel = true;
      int num = (int) MessageBox.Show("The game is still running. Please quit inside the game.");
        }
#endregion

#region IRogueUI implementation
    public KeyEventArgs UI_WaitKey()
    {
      m_HasKey = false;
      while (true)
      {
        Application.DoEvents();
        if (!m_HasKey)
          Thread.Sleep(1);
        else
          break;
      }
      return m_InKey;
    }

    public KeyEventArgs UI_PeekKey()
    {
      Thread.Sleep(1);
      Application.DoEvents();
      if (!m_HasKey)
        return (KeyEventArgs) null;
            m_HasKey = false;
      return m_InKey;
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
                    m_HasKey = true;
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
      if (!m_HasMouseButtons)
        return new MouseButtons?();
            m_HasMouseButtons = false;
      return new MouseButtons?(m_MouseButtons);
    }

    public void UI_SetCursor(Cursor cursor)
    {
      if (cursor == Cursor)
        return;
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

    public void UI_DrawStringBold(Color color, string text, int gx, int gy, Color? shadowColor)
    {
      if (shadowColor.HasValue)
                m_GameCanvas.AddString(m_BoldFont, shadowColor.Value, text, gx + 1, gy + 1);
            m_GameCanvas.AddString(m_BoldFont, color, text, gx, gy);
    }

    public void UI_DrawRect(Color color, Rectangle rect)
    {
      if (rect.Width <= 0 || rect.Height <= 0)
        throw new ArgumentOutOfRangeException("rectangle Width/Height <= 0");
            m_GameCanvas.AddRect(color, rect);
    }

    public void UI_FillRect(Color color, Rectangle rect)
    {
      if (rect.Width <= 0 || rect.Height <= 0)
        throw new ArgumentOutOfRangeException("rectangle Width/Height <= 0");
            m_GameCanvas.AddFilledRect(color, rect);
    }

    public void UI_DrawPopup(string[] lines, Color textColor, Color boxBorderColor, Color boxFillColor, int gx, int gy)
    {
      int num1 = 0;
      int num2 = 0;
      Size[] sizeArray = new Size[lines.Length];
      for (int index = 0; index < lines.Length; ++index)
      {
        sizeArray[index] = TextRenderer.MeasureText(lines[index], m_BoldFont);
        if (sizeArray[index].Width > num1)
          num1 = sizeArray[index].Width;
        num2 += sizeArray[index].Height;
      }
      Point location = new Point(gx, gy);
      Size size = new Size(num1 + 4, num2 + 4);
      Rectangle rect = new Rectangle(location, size);
            m_GameCanvas.AddFilledRect(boxFillColor, rect);
            m_GameCanvas.AddRect(boxBorderColor, rect);
      int gx1 = location.X + 2;
      int gy1 = location.Y + 2;
      for (int index = 0; index < lines.Length; ++index)
      {
                m_GameCanvas.AddString(m_BoldFont, textColor, lines[index], gx1, gy1);
        gy1 += sizeArray[index].Height;
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

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
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
