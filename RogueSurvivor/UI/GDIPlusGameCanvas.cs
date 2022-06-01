// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.UI.GDIPlusGameCanvas
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

#define GDI_PLUS
// #define WINDOWS_SYSTEM_MEDIA

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Zaimoni.Data;

namespace djack.RogueSurvivor.UI
{
  public class GDIPlusGameCanvas : UserControl, IGameCanvas
  {
    private Color m_ClearColor = Color.CornflowerBlue;
    private readonly List<IGfx> m_Gfxs = new List<IGfx>(100);
    private static readonly Dictionary<Color, Brush> m_BrushesCache = new Dictionary<Color, Brush>(32);
    private readonly Dictionary<Color, Pen> m_PensCache = new Dictionary<Color, Pen>(32);
    private Bitmap m_TileImage = new Bitmap(Engine.RogueGame.TILE_SIZE, Engine.RogueGame.TILE_SIZE);    // working space for pre-compositing tiles
    private Bitmap m_RenderImage = new Bitmap(Engine.RogueGame.CANVAS_WIDTH, Engine.RogueGame.CANVAS_HEIGHT);   // image *source* for PaintEventArgs object's Graphics member; image *destination* for m_RenderGraphics
#if GDI_PLUS
    private readonly Graphics m_RenderGraphics;
    private readonly Graphics m_RenderTile;
#endif
#if WINDOWS_SYSTEM_MEDIA
    private readonly System.Windows.Controls.Canvas m_Canvas = new System.Windows.Controls.Canvas();   // requires PresentationFramework assembly
    private readonly System.Windows.Media.DrawingGroup m_DrawingGroup = new System.Windows.Media.DrawingGroup();   // requires PresentationCore assembly
#endif
    private Bitmap m_MinimapBitmap;
    private readonly byte[] m_MinimapBytes;
    private readonly int m_MinimapStride;
    private IContainer components;

    public bool ShowFPS { get; set; }
    public bool NeedRedraw { get; set; }
    public Point MouseLocation { get; set; }

    public float ScaleX
    {
      get {
        var form = RogueForm.Get;
        if (null == form) return 1f;
        return form.ClientRectangle.Width / (float)Engine.RogueGame.CANVAS_WIDTH;
      }
    }

    public float ScaleY
    {
      get {
        var form = RogueForm.Get;
        if (null == form) return 1f;
        return form.ClientRectangle.Height / (float)Engine.RogueGame.CANVAS_HEIGHT;
      }
    }

    public GDIPlusGameCanvas()
    {
      NeedRedraw = true;
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas::InitializeComponent");
      InitializeComponent();
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas create render image");
#if GDI_PLUS
      m_RenderGraphics = Graphics.FromImage(m_RenderImage);
      m_RenderTile = Graphics.FromImage(m_TileImage);
#endif
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas create minimap bitmap");
      m_MinimapBitmap = new Bitmap(2*(1+2*Engine.RogueGame.MINIMAP_RADIUS), 2* (1+2*Engine.RogueGame.MINIMAP_RADIUS));   // each minimap coordinate is 2x2 pixels
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas get minimap stride");
      BitmapData bitmapdata = m_MinimapBitmap.LockBits(new Rectangle(0, 0, m_MinimapBitmap.Width, m_MinimapBitmap.Height), ImageLockMode.ReadWrite, m_MinimapBitmap.PixelFormat);
      m_MinimapStride = bitmapdata.Stride;
      m_MinimapBitmap.UnlockBits(bitmapdata);
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas create minimap bytes");
      m_MinimapBytes = new byte[m_MinimapStride * m_MinimapBitmap.Height];
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas::SetStyle");
      SetStyle(ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint | ControlStyles.EnableNotifyMessage, true);
    }

    protected override void OnCreateControl()
    {
      base.OnCreateControl();
    }

/*
* https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/windows-forms-and-wpf-interoperability-input-architecture
* Because the default HwndSource implementation of the TranslateChar method returns false, WM_CHAR messages are processed using the following logic: 
* The Control.IsInputChar method is overridden to ensure that all WM_CHAR messages are forwarded to hosted elements. 
* If the ALT key is pressed, the message is WM_SYSCHAR. Windows Forms does not preprocess this message through the IsInputChar method. Therefore, the ProcessMnemonic method is overridden to query the WPF AccessKeyManager for a registered accelerator. If a registered accelerator is found, AccessKeyManager processes it. 
* If the ALT key is not pressed, the WPF InputManager class processes the unhandled input. If the input is an accelerator, the AccessKeyManager processes it. The PostProcessInput event is handled for WM_CHAR messages that were not processed.  * 
 */
    // That is, per above no alt keys ever reach here
    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e);    // must be called first to allow seeing *any* keypresses?
      RogueForm.Get.UI_PostKey(e);
    }

    protected override bool IsInputKey(Keys keyData)
    {
      return true;
    }

    // this intercepts all keys regardless of ALT status by itself
    protected override bool ProcessMnemonic(char charCode)
    {
      Keys mod = ModifierKeys;
      if (Keys.None==(mod & Keys.Alt)) return false;    // normal processing works fine if ALT isn't involved
      switch(charCode)
      {
      case 'i': // sees ALT-SHIFT-I
      case 'I':
        RogueForm.Get.UI_PostKey(new KeyEventArgs(Keys.I | mod));
        return true;
      // should be able to do ALT-CTRL-I
      // not sure about ALT-CTRL-SHIFT-I
      }
      return false;
    }

    // mouse event support
    private readonly List<Action<int, int>> core_hover_handlers = new();
    public void Add(Action<int, int> op) {
        core_hover_handlers.Add(op);
    }

    private readonly List<Func<int, int, bool>> aux_hover_handlers = new();
    public void Add(Func<int, int, bool> op) {
        aux_hover_handlers.Add(op);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        lock(aux_hover_handlers) {
        base.OnMouseMove(e);
        MouseLocation = e.Location;
        var ub = aux_hover_handlers.Count;
        // this runs in parallel and locking is ineffective
        foreach(var h in aux_hover_handlers.ToArray()) {
          if (h(e.Location.X, e.Location.Y)) aux_hover_handlers.Remove(h);
        }
        foreach(var h in core_hover_handlers) h(e.Location.X, e.Location.Y);
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
      base.OnMouseClick(e);
      RogueForm.Get.UI_PostMouseButtons(e.Button);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
      base.OnSizeChanged(e);
      NeedRedraw = true;
      Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)   // forces use of System.Drawing.Graphics
    {
      double totalMilliseconds1 = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
      if (NeedRedraw) {
        DoDraw();
        NeedRedraw = false;
      }
      if (ScaleX == 1.0 && ScaleY == 1.0)
        e.Graphics.DrawImageUnscaled(m_RenderImage, 0, 0);
      else
        e.Graphics.DrawImage(m_RenderImage, RogueForm.Get.ClientRectangle);
      if (!ShowFPS) return;
      double num = DateTime.UtcNow.TimeOfDay.TotalMilliseconds - totalMilliseconds1;
      if (0.0 >= num) num = double.Epsilon;
      e.Graphics.DrawString(string.Format("Frame time={0:F} FPS={1:F}", num, 1000.0 / num), Font, Brushes.Yellow, ClientRectangle.Right - 200, ClientRectangle.Bottom - 64);
    }

    private void DoDraw()
    {
      if (null == RogueForm.Get) return;
      m_RenderGraphics.Clear(m_ClearColor);
      foreach (IGfx gfx in new List<IGfx>(m_Gfxs))  // Bay12/jorgene0: this collection can change at reincarnation
        gfx.Draw(m_RenderGraphics);
    }

    public void FillGameForm()
    {
      Location = new Point(0, 0);
      var form = RogueForm.Get;
      if (null != form) Size = MinimumSize = MaximumSize = form.Size;
    }

    public void Clear(Color clearColor)
    {
      m_ClearColor = clearColor;
      m_Gfxs.Clear();
      NeedRedraw = true;
    }

    public void AddImage(Image img, int x, int y)
    {
      m_Gfxs.Add(new GfxImage(img, x, y));
      NeedRedraw = true;
    }

    public void AddImage(Image img, int x, int y, Color tint)
    {
      AddImage(img, x, y);
    }

    public void AddImageTransform(Image img, int x, int y, float rotation, float scale)
    {
      m_Gfxs.Add(new GfxImageTransform(img, rotation, scale, x, y));
      NeedRedraw = true;
    }

    public void AddTransparentImage(float alpha, Image img, int x, int y)
    {
      m_Gfxs.Add(new GfxTransparentImage(alpha, img, x, y));
      NeedRedraw = true;
    }

    public void AddTile(Image img)
    {
      m_RenderTile.Clear(Color.Transparent);
      m_RenderTile.DrawImageUnscaled(img, 0, 0);
    }

    public void AddTile(Image img, Color color)
    {
      AddTile(img);
    }

    public void AppendTile(Image img)
    {
      m_RenderTile.DrawImageUnscaled(img, 0, 0);
    }

    public void AppendTile(Image img, Color color)
    {
      AppendTile(img);
    }

    public void AppendTile(Color color, string text, Font font, int x, int y)
    {
      m_RenderTile.DrawString(text, font, GetColorBrush(color), x, y);
    }

    public void DrawTile(int x, int y)
    {
      m_Gfxs.Add(new GfxImageCopy(m_TileImage, x, y));
      NeedRedraw = true;
    }

    public void AddPoint(Color color, int x, int y)
    {
      m_Gfxs.Add(new GfxRect(GetPen(color), new Rectangle(x, y, 1, 1)));
      NeedRedraw = true;
    }

    public void AddLine(Color color, int xFrom, int yFrom, int xTo, int yTo)
    {
      m_Gfxs.Add(new GfxLine(GetPen(color), xFrom, yFrom, xTo, yTo));
      NeedRedraw = true;
    }

    public void AddString(Font font, Color color, string text, int gx, int gy)
    {
      m_Gfxs.Add(new GfxString(color, font, text, gx, gy));
      NeedRedraw = true;
    }

    public void AddRect(Color color, Rectangle rect)
    {
      m_Gfxs.Add(new GfxRect(GetPen(color), rect));
      NeedRedraw = true;
    }

    private Pen GetPen(Color color)
    {
      if (!m_PensCache.TryGetValue(color, out Pen pen)) {
        pen = new Pen(color);
        m_PensCache.Add(color, pen);
      }
      return pen;
    }

    public static Brush GetColorBrush(Color color)
    {
      if (!m_BrushesCache.TryGetValue(color, out Brush brush)) {
        brush = new SolidBrush(color);
        m_BrushesCache.Add(color, brush);
      }
      return brush;
    }

    public void AddFilledRect(Color color, Rectangle rect)
    {
      Brush brush = GetColorBrush(color);
      m_Gfxs.Add(new GfxFilledRect(brush, rect));
      NeedRedraw = true;
    }

    public void ClearMinimap(Color color)
    {
      for (int x = 0; x < 1+2*Engine.RogueGame.MINIMAP_RADIUS; ++x) {
        for (int y = 0; y < 1+2*Engine.RogueGame.MINIMAP_RADIUS; ++y)
          SetMinimapColor(x, y, color);
      }
    }

    public void SetMinimapColor(int x, int y, Color color)
    {
      int num1 = x * 2;
      int num2 = y * 2;
      for (int index1 = 0; index1 < 2; ++index1) {
        for (int index2 = 0; index2 < 2; ++index2) {
          int index3 = (num2 + index2) * m_MinimapStride + 4 * (num1 + index1);
          m_MinimapBytes[index3] = color.B;
          m_MinimapBytes[index3+1] = color.G;
          m_MinimapBytes[index3+2] = color.R;
          m_MinimapBytes[index3+3] = byte.MaxValue;
        }
      }
    }

    public void DrawMinimap(int gx, int gy)
    {
      BitmapData bitmapdata = m_MinimapBitmap.LockBits(new Rectangle(0, 0, m_MinimapBitmap.Width, m_MinimapBitmap.Height), ImageLockMode.ReadWrite, m_MinimapBitmap.PixelFormat);
      Marshal.Copy(m_MinimapBytes, 0, bitmapdata.Scan0, m_MinimapBytes.Length);
      m_MinimapBitmap.UnlockBits(bitmapdata);
      m_Gfxs.Add(new GfxImage(m_MinimapBitmap, gx, gy));
      NeedRedraw = true;
    }

    public string SaveScreenShot(string filePath)
    {
      Logger.WriteLine(Logger.Stage.RUN_GFX, "taking screenshot...");
      try
      {
        m_RenderImage.Save(filePath, ImageFormat.Png);
      }
      catch (Exception ex)
      {
        Logger.WriteLine(Logger.Stage.RUN_GFX, string.Format("exception when taking screenshot : {0}", ex.ToString()));
        return null;
      }
      Logger.WriteLine(Logger.Stage.RUN_GFX, "taking screenshot... done!");
      return filePath + ".png";
    }

    public string ScreenshotExtension()
    {
      return "png";
    }

    public void DisposeUnmanagedResources()
    {
    }

    protected override void Dispose(bool disposing)
    {
       if (disposing)
            {
            if (null != components)
                {
                components.Dispose();
                components = null;
                }
            if (null != m_TileImage)
                {
                m_TileImage.Dispose();
                m_TileImage = null;
                }
            if (null != m_RenderImage)
                {
                m_RenderImage.Dispose();
                m_RenderImage = null;
                }
            if (null != m_MinimapBitmap)
                {
                m_MinimapBitmap.Dispose();
                m_MinimapBitmap = null;
                }
            }
      DisposeUnmanagedResources();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      components = new Container();
      AutoScaleMode = AutoScaleMode.Font;
    }

    public interface IGfx
    {
      void Draw(Graphics g);
    }

    private class GfxImage : IGfx
    {
      private readonly Image m_Img;
      private readonly int m_X;
      private readonly int m_Y;

      public GfxImage(Image img, int x, int y)
      {
        m_Img = img;
        m_X = x;
        m_Y = y;
      }

      public void Draw(Graphics g)
      {
        g.DrawImageUnscaled(m_Img, m_X, m_Y);
      }
    }

    private class GfxImageCopy : IGfx, IDisposable
    {
      private readonly Image m_Img;
      private readonly int m_X;
      private readonly int m_Y;
      private bool disposed;

      public GfxImageCopy(Image img, int x, int y)
      {
        m_Img = new Bitmap(img);
        m_X = x;
        m_Y = y;
      }

      public void Draw(Graphics g)
      {
        g.DrawImageUnscaled(m_Img, m_X, m_Y);
      }

      public void Dispose()
      {
        Dispose(true);
        GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool disposing)
      {
        if (disposing && !disposed)
          {
          m_Img.Dispose();
          disposed = true;
          }
      }
    }

    private class GfxImageTransform : IGfx, IDisposable
    {
      private readonly Image m_Img;
      private readonly Matrix m_Matrix;
      private readonly int m_X;
      private readonly int m_Y;
      private bool disposed;

      public GfxImageTransform(Image img, float rotation, float scale, int x, int y)
      {
        m_Img = img;
        m_Matrix = new Matrix();
        m_X = x;
        m_Y = y;
        disposed = false;
        m_Matrix.RotateAt(rotation, new PointF(x + img.Width / 2, y + img.Height / 2));
        m_Matrix.Scale(scale, scale);
      }

      public void Draw(Graphics g)
      {
        Matrix transform = g.Transform;
        Matrix matrix = transform.Clone();
        matrix.Multiply(m_Matrix);
        g.Transform = matrix;
        g.DrawImageUnscaled(m_Img, m_X, m_Y);
        g.Transform = transform;
      }

      public void Dispose()
      {
        Dispose(true);
        GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool disposing)
      {
        if (disposing && !disposed)
          {
          m_Matrix.Dispose();
          disposed = true;
          }
      }
    }

    private class GfxTransparentImage : IGfx, IDisposable
    {
      private readonly Image m_Img;
      private readonly int m_X;
      private readonly int m_Y;
      private readonly ImageAttributes m_ImgAttributes;
      private bool disposed;

      public GfxTransparentImage(float alpha, Image img, int x, int y)
      {
        m_Img = img;
        m_X = x;
        m_Y = y;
        disposed = false;
        // relies on default-initialization to zero
        float[][] newColorMatrix = new float[5][];
        newColorMatrix[0] = new float[5];
        newColorMatrix[0][0] = 1f;
        newColorMatrix[1] = new float[5];
        newColorMatrix[1][1] = 1f;
        newColorMatrix[2] = new float[5];
        newColorMatrix[2][2] = 1f;
        newColorMatrix[3] = new float[5];
        newColorMatrix[3][3] = alpha;
        newColorMatrix[4] = new float[5];   // force homogenous coordinates
        newColorMatrix[4][4] = 1f;

        m_ImgAttributes = new ImageAttributes();
        m_ImgAttributes.SetColorMatrix(new ColorMatrix(newColorMatrix), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
      }

      public void Draw(Graphics g)
      {
        g.DrawImage(m_Img, new Rectangle(m_X, m_Y, m_Img.Width, m_Img.Height), 0, 0, m_Img.Width, m_Img.Height, GraphicsUnit.Pixel, m_ImgAttributes);
      }

      public void Dispose()
      {
        Dispose(true);
        GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool disposing)
      {
        if (disposing && !disposed)
          {
          m_ImgAttributes.Dispose();
          disposed = true;
          }
      }
    }

    private class GfxLine : IGfx
    {
      private readonly Pen m_Pen;
      private readonly int m_xFrom;
      private readonly int m_yFrom;
      private readonly int m_xTo;
      private readonly int m_yTo;

      public GfxLine(Pen pen, int xFrom, int yFrom, int xTo, int yTo)
      {
        m_Pen = pen;
        m_xFrom = xFrom;
        m_yFrom = yFrom;
        m_xTo = xTo;
        m_yTo = yTo;
      }

      public void Draw(Graphics g)
      {
        g.DrawLine(m_Pen, m_xFrom, m_yFrom, m_xTo, m_yTo);
      }
    }

    private class GfxString : IGfx
    {
      private readonly Font m_Font;
      private readonly string m_Text;
      private readonly int m_X;
      private readonly int m_Y;
      private readonly Brush m_Brush;

      public GfxString(Color color, Font font, string text, int x, int y)
      {
        m_Font = font;
        m_Text = text;
        m_X = x;
        m_Y = y;
        m_Brush = GetColorBrush(color);
      }

      public void Draw(Graphics g)
      {
        g.DrawString(m_Text, m_Font, m_Brush, m_X, m_Y);
      }
    }

    private class GfxFilledRect : IGfx
    {
      private readonly Brush m_Brush;
      private readonly Rectangle m_Rect;

      public GfxFilledRect(Brush brush, Rectangle rect)
      {
        m_Brush = brush;
        m_Rect = rect;
      }

      public void Draw(Graphics g)
      {
        g.FillRectangle(m_Brush, m_Rect);
      }
    }

    private class GfxRect : IGfx
    {
      private readonly Pen m_Pen;
      private readonly Rectangle m_Rect;

      public GfxRect(Pen pen, Rectangle rect)
      {
        m_Pen = pen;
        m_Rect = rect;
      }

      public void Draw(Graphics g)
      {
        g.DrawRectangle(m_Pen, m_Rect);
      }
    }
  }
}
