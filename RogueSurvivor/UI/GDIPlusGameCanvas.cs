// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.UI.GDIPlusGameCanvas
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Security.Permissions;

namespace djack.RogueSurvivor.UI
{
  public class GDIPlusGameCanvas : UserControl, IGameCanvas
  {
    private Color m_ClearColor = Color.CornflowerBlue;
    private readonly List<GDIPlusGameCanvas.IGfx> m_Gfxs = new List<GDIPlusGameCanvas.IGfx>(100);
    private readonly Dictionary<Color, Brush> m_BrushesCache = new Dictionary<Color, Brush>(32);
    private readonly Dictionary<Color, Pen> m_PensCache = new Dictionary<Color, Pen>(32);
    private RogueForm m_RogueForm;
    private Bitmap m_RenderImage;
    private readonly Graphics m_RenderGraphics;
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
        if (m_RogueForm == null) return 1f;
        return (float)m_RogueForm.ClientRectangle.Width / 1024f;
      }
    }

    public float ScaleY
    {
      get {
        if (m_RogueForm == null) return 1f;
        return (float)m_RogueForm.ClientRectangle.Height / 768f;
      }
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public GDIPlusGameCanvas()
    {
      NeedRedraw = true;
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas::InitializeComponent");
      InitializeComponent();
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas create render image");
      m_RenderImage = new Bitmap(1024, 768);
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas get render graphics");
      m_RenderGraphics = Graphics.FromImage((Image)m_RenderImage);
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas create minimap bitmap");
      m_MinimapBitmap = new Bitmap(2*Engine.RogueGame.MAP_MAX_WIDTH, 2*Engine.RogueGame.MAP_MAX_HEIGHT);   // each minimap coordinate is 2x2 pixels
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

    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e);
      m_RogueForm.UI_PostKey(e);
    }

    protected override bool IsInputKey(Keys keyData)
    {
      return true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      base.OnMouseMove(e);
      MouseLocation = e.Location;
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
      base.OnMouseClick(e);
      m_RogueForm.UI_PostMouseButtons(e.Button);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
      base.OnSizeChanged(e);
      NeedRedraw = true;
      Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      double totalMilliseconds1 = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
      if (NeedRedraw) {
        DoDraw(m_RenderGraphics);
        NeedRedraw = false;
      }
      if ((double) ScaleX == 1.0 && (double) ScaleY == 1.0)
        e.Graphics.DrawImageUnscaled((Image) m_RenderImage, 0, 0);
      else
        e.Graphics.DrawImage((Image) m_RenderImage, m_RogueForm.ClientRectangle);
      double totalMilliseconds2 = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
      if (!ShowFPS) return;
      double num = totalMilliseconds2 - totalMilliseconds1;
      if (num == 0.0)
        num = double.Epsilon;
      e.Graphics.DrawString(string.Format("Frame time={0:F} FPS={1:F}", (object) num, (object) (1000.0 / num)), Font, Brushes.Yellow, (float) (ClientRectangle.Right - 200), (float) (ClientRectangle.Bottom - 64));
    }

    private void DoDraw(Graphics g)
    {
      if (m_RogueForm == null) return;
      g.Clear(m_ClearColor);
      foreach (GDIPlusGameCanvas.IGfx gfx in new List<GDIPlusGameCanvas.IGfx>(m_Gfxs))  // Bay12/jorgene0: this collection can change at reincarnation
        gfx.Draw(g);
    }

    public void BindForm(RogueForm form)
    {
      m_RogueForm = form;
      FillGameForm();
    }

    public void FillGameForm()
    {
      Location = new Point(0, 0);
      if (m_RogueForm == null) return;
      Size = MinimumSize = MaximumSize = m_RogueForm.Size;
    }

    public void Clear(Color clearColor)
    {
      m_ClearColor = clearColor;
      m_Gfxs.Clear();
      NeedRedraw = true;
    }

    public void AddImage(Image img, int x, int y)
    {
      m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxImage(img, x, y));
      NeedRedraw = true;
    }

    public void AddImage(Image img, int x, int y, Color color)
    {
      AddImage(img, x, y);
    }

    public void AddImageTransform(Image img, int x, int y, float rotation, float scale)
    {
      m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxImageTransform(img, rotation, scale, x, y));
      NeedRedraw = true;
    }

    public void AddTransparentImage(float alpha, Image img, int x, int y)
    {
      m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxTransparentImage(alpha, img, x, y));
      NeedRedraw = true;
    }

    public void AddPoint(Color color, int x, int y)
    {
      m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxRect(GetPen(color), new Rectangle(x, y, 1, 1)));
      NeedRedraw = true;
    }

    public void AddLine(Color color, int xFrom, int yFrom, int xTo, int yTo)
    {
      m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxLine(GetPen(color), xFrom, yFrom, xTo, yTo));
      NeedRedraw = true;
    }

    public void AddString(Font font, Color color, string text, int gx, int gy)
    {
      m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxString(color, font, text, gx, gy));
      NeedRedraw = true;
    }

    public void AddRect(Color color, Rectangle rect)
    {
      m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxRect(GetPen(color), rect));
      NeedRedraw = true;
    }

    private Pen GetPen(Color color)
    {
      Pen pen;
      if (!m_PensCache.TryGetValue(color, out pen)) {
        pen = new Pen(color);
        m_PensCache.Add(color, pen);
      }
      return pen;
    }

    public void AddFilledRect(Color color, Rectangle rect)
    {
      Brush brush;
      if (!m_BrushesCache.TryGetValue(color, out brush))
      {
        brush = (Brush) new SolidBrush(color);
        m_BrushesCache.Add(color, brush);
      }
      m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxFilledRect(brush, rect));
      NeedRedraw = true;
    }

    public void ClearMinimap(Color color)
    {
      for (int x = 0; x < Engine.RogueGame.MAP_MAX_WIDTH; ++x) {
        for (int y = 0; y < Engine.RogueGame.MAP_MAX_HEIGHT; ++y)
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
          m_MinimapBytes[index3] = (byte)color.B;
          m_MinimapBytes[index3+1] = (byte)color.G;
          m_MinimapBytes[index3+2] = (byte)color.R;
          m_MinimapBytes[index3+3] = byte.MaxValue;
        }
      }
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public void DrawMinimap(int gx, int gy)
    {
      BitmapData bitmapdata = m_MinimapBitmap.LockBits(new Rectangle(0, 0, m_MinimapBitmap.Width, m_MinimapBitmap.Height), ImageLockMode.ReadWrite, m_MinimapBitmap.PixelFormat);
      Marshal.Copy(m_MinimapBytes, 0, bitmapdata.Scan0, m_MinimapBytes.Length);
      m_MinimapBitmap.UnlockBits(bitmapdata);
      m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxImage((Image)m_MinimapBitmap, gx, gy));
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
        Logger.WriteLine(Logger.Stage.RUN_GFX, string.Format("exception when taking screenshot : {0}", (object) ex.ToString()));
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
      components = (IContainer) new Container();
      AutoScaleMode = AutoScaleMode.Font;
    }

    public interface IGfx
    {
      void Draw(Graphics g);
    }

    private class GfxImage : GDIPlusGameCanvas.IGfx
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

    private class GfxImageTransform : GDIPlusGameCanvas.IGfx, IDisposable
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
        m_Matrix.RotateAt(rotation, new PointF((float) (x + img.Width / 2), (float) (y + img.Height / 2)));
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

    private class GfxTransparentImage : GDIPlusGameCanvas.IGfx, IDisposable
    {
      private readonly Image m_Img;
      private readonly int m_X;
      private readonly int m_Y;
      private readonly float m_Alpha;
      private readonly ImageAttributes m_ImgAttributes;
      private bool disposed;

      public GfxTransparentImage(float alpha, Image img, int x, int y)
      {
        m_Img = img;
        m_X = x;
        m_Y = y;
        m_Alpha = alpha;
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

    private class GfxLine : GDIPlusGameCanvas.IGfx
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

    private class GfxString : GDIPlusGameCanvas.IGfx, IDisposable
    {
      private readonly Color m_Color;
      private readonly Font m_Font;
      private readonly string m_Text;
      private readonly int m_X;
      private readonly int m_Y;
      private readonly Brush m_Brush;
      private bool disposed;

      public GfxString(Color color, Font font, string text, int x, int y)
      {
        m_Color = color;
        m_Font = font;
        m_Text = text;
        m_X = x;
        m_Y = y;
        m_Brush = (Brush) new SolidBrush(color);
        disposed = false;
      }

      public void Draw(Graphics g)
      {
        g.DrawString(m_Text, m_Font, m_Brush, (float)m_X, (float)m_Y);
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
          m_Brush.Dispose();
          disposed = true;
          }
      }
    }

    private class GfxFilledRect : GDIPlusGameCanvas.IGfx
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

    private class GfxRect : GDIPlusGameCanvas.IGfx
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
