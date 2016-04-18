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

namespace djack.RogueSurvivor.UI
{
  public class GDIPlusGameCanvas : UserControl, IGameCanvas
  {
    private bool m_NeedRedraw = true;
    private Color m_ClearColor = Color.CornflowerBlue;
    private List<GDIPlusGameCanvas.IGfx> m_Gfxs = new List<GDIPlusGameCanvas.IGfx>(100);
    private Dictionary<Color, Brush> m_BrushesCache = new Dictionary<Color, Brush>(32);
    private Dictionary<Color, Pen> m_PensCache = new Dictionary<Color, Pen>(32);
    private RogueForm m_RogueForm;
    private Bitmap m_RenderImage;
    private Graphics m_RenderGraphics;
    private Bitmap m_MinimapBitmap;
    private byte[] m_MinimapBytes;
    private int m_MinimapStride;
    private IContainer components;

    public bool ShowFPS { get; set; }

    public bool NeedRedraw
    {
      get
      {
        return this.m_NeedRedraw;
      }
      set
      {
        this.m_NeedRedraw = value;
      }
    }

    public Point MouseLocation { get; set; }

    public float ScaleX
    {
      get
      {
        if (this.m_RogueForm == null)
          return 1f;
        return (float) this.m_RogueForm.ClientRectangle.Width / 1024f;
      }
    }

    public float ScaleY
    {
      get
      {
        if (this.m_RogueForm == null)
          return 1f;
        return (float) this.m_RogueForm.ClientRectangle.Height / 768f;
      }
    }

    public GDIPlusGameCanvas()
    {
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas::InitializeComponent");
      this.InitializeComponent();
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas create render image");
      this.m_RenderImage = new Bitmap(1024, 768);
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas get render graphics");
      this.m_RenderGraphics = Graphics.FromImage((Image) this.m_RenderImage);
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas create minimap bitmap");
      this.m_MinimapBitmap = new Bitmap(200, 200);
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas get minimap stride");
      BitmapData bitmapdata = this.m_MinimapBitmap.LockBits(new Rectangle(0, 0, this.m_MinimapBitmap.Width, this.m_MinimapBitmap.Height), ImageLockMode.ReadWrite, this.m_MinimapBitmap.PixelFormat);
      this.m_MinimapStride = bitmapdata.Stride;
      this.m_MinimapBitmap.UnlockBits(bitmapdata);
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas create minimap bytes");
      this.m_MinimapBytes = new byte[this.m_MinimapStride * this.m_MinimapBitmap.Height];
      Logger.WriteLine(Logger.Stage.INIT_GFX, "GDIPlusGameCanvas::SetStyle");
      this.SetStyle(ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint | ControlStyles.EnableNotifyMessage, true);
    }

    protected override void OnCreateControl()
    {
      base.OnCreateControl();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e);
      this.m_RogueForm.UI_PostKey(e);
    }

    protected override bool IsInputKey(Keys keyData)
    {
      return true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      base.OnMouseMove(e);
      this.MouseLocation = e.Location;
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
      base.OnMouseClick(e);
      this.m_RogueForm.UI_PostMouseButtons(e.Button);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
      base.OnSizeChanged(e);
      this.NeedRedraw = true;
      this.Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      double totalMilliseconds1 = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
      if (this.NeedRedraw)
      {
        this.DoDraw(this.m_RenderGraphics);
        this.m_NeedRedraw = false;
      }
      if ((double) this.ScaleX == 1.0 && (double) this.ScaleY == 1.0)
        e.Graphics.DrawImageUnscaled((Image) this.m_RenderImage, 0, 0);
      else
        e.Graphics.DrawImage((Image) this.m_RenderImage, this.m_RogueForm.ClientRectangle);
      double totalMilliseconds2 = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
      if (!this.ShowFPS)
        return;
      double num = totalMilliseconds2 - totalMilliseconds1;
      if (num == 0.0)
        num = double.Epsilon;
      e.Graphics.DrawString(string.Format("Frame time={0:F} FPS={1:F}", (object) num, (object) (1000.0 / num)), this.Font, Brushes.Yellow, (float) (this.ClientRectangle.Right - 200), (float) (this.ClientRectangle.Bottom - 64));
    }

    private void DoDraw(Graphics g)
    {
      if (this.m_RogueForm == null)
        return;
      g.Clear(this.m_ClearColor);
      foreach (GDIPlusGameCanvas.IGfx gfx in this.m_Gfxs)
        gfx.Draw(g);
    }

    public void BindForm(RogueForm form)
    {
      this.m_RogueForm = form;
      this.FillGameForm();
    }

    public void FillGameForm()
    {
      this.Location = new Point(0, 0);
      if (this.m_RogueForm == null)
        return;
      this.Size = this.MinimumSize = this.MaximumSize = this.m_RogueForm.Size;
    }

    public void Clear(Color clearColor)
    {
      this.m_ClearColor = clearColor;
      this.m_Gfxs.Clear();
      this.m_NeedRedraw = true;
    }

    public void AddImage(Image img, int x, int y)
    {
      this.m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxImage(img, x, y));
      this.m_NeedRedraw = true;
    }

    public void AddImage(Image img, int x, int y, Color color)
    {
      this.AddImage(img, x, y);
    }

    public void AddImageTransform(Image img, int x, int y, float rotation, float scale)
    {
      this.m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxImageTransform(img, rotation, scale, x, y));
      this.m_NeedRedraw = true;
    }

    public void AddTransparentImage(float alpha, Image img, int x, int y)
    {
      this.m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxTransparentImage(alpha, img, x, y));
      this.m_NeedRedraw = true;
    }

    public void AddPoint(Color color, int x, int y)
    {
      this.m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxRect(this.GetPen(color), new Rectangle(x, y, 1, 1)));
      this.m_NeedRedraw = true;
    }

    public void AddLine(Color color, int xFrom, int yFrom, int xTo, int yTo)
    {
      this.m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxLine(this.GetPen(color), xFrom, yFrom, xTo, yTo));
      this.m_NeedRedraw = true;
    }

    public void AddString(Font font, Color color, string text, int gx, int gy)
    {
      this.m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxString(color, font, text, gx, gy));
      this.m_NeedRedraw = true;
    }

    public void AddRect(Color color, Rectangle rect)
    {
      this.m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxRect(this.GetPen(color), rect));
      this.m_NeedRedraw = true;
    }

    private Pen GetPen(Color color)
    {
      Pen pen;
      if (!this.m_PensCache.TryGetValue(color, out pen))
      {
        pen = new Pen(color);
        this.m_PensCache.Add(color, pen);
      }
      return pen;
    }

    public void AddFilledRect(Color color, Rectangle rect)
    {
      Brush brush;
      if (!this.m_BrushesCache.TryGetValue(color, out brush))
      {
        brush = (Brush) new SolidBrush(color);
        this.m_BrushesCache.Add(color, brush);
      }
      this.m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxFilledRect(brush, rect));
      this.m_NeedRedraw = true;
    }

    public void ClearMinimap(Color color)
    {
      for (int x = 0; x < 100; ++x)
      {
        for (int y = 0; y < 100; ++y)
          this.SetMinimapColor(x, y, color);
      }
    }

    public void SetMinimapColor(int x, int y, Color color)
    {
      int num1 = x * 2;
      int num2 = y * 2;
      for (int index1 = 0; index1 < 2; ++index1)
      {
        for (int index2 = 0; index2 < 2; ++index2)
        {
          int num3 = (num2 + index2) * this.m_MinimapStride + 4 * (num1 + index1);
          byte[] numArray1 = this.m_MinimapBytes;
          int index3 = num3;
          int num4 = 1;
          int num5 = index3 + num4;
          int num6 = (int) color.B;
          numArray1[index3] = (byte) num6;
          byte[] numArray2 = this.m_MinimapBytes;
          int index4 = num5;
          int num7 = 1;
          int num8 = index4 + num7;
          int num9 = (int) color.G;
          numArray2[index4] = (byte) num9;
          byte[] numArray3 = this.m_MinimapBytes;
          int index5 = num8;
          int num10 = 1;
          int num11 = index5 + num10;
          int num12 = (int) color.R;
          numArray3[index5] = (byte) num12;
          byte[] numArray4 = this.m_MinimapBytes;
          int index6 = num11;
          int num13 = 1;
          int num14 = index6 + num13;
          int num15 = (int) byte.MaxValue;
          numArray4[index6] = (byte) num15;
        }
      }
    }

    public void DrawMinimap(int gx, int gy)
    {
      BitmapData bitmapdata = this.m_MinimapBitmap.LockBits(new Rectangle(0, 0, this.m_MinimapBitmap.Width, this.m_MinimapBitmap.Height), ImageLockMode.ReadWrite, this.m_MinimapBitmap.PixelFormat);
      Marshal.Copy(this.m_MinimapBytes, 0, bitmapdata.Scan0, this.m_MinimapBytes.Length);
      this.m_MinimapBitmap.UnlockBits(bitmapdata);
      this.m_Gfxs.Add((GDIPlusGameCanvas.IGfx) new GDIPlusGameCanvas.GfxImage((Image) this.m_MinimapBitmap, gx, gy));
      this.NeedRedraw = true;
    }

    public string SaveScreenShot(string filePath)
    {
      Logger.WriteLine(Logger.Stage.RUN_GFX, "taking screenshot...");
      try
      {
        this.m_RenderImage.Save(filePath, ImageFormat.Png);
      }
      catch (Exception ex)
      {
        Logger.WriteLine(Logger.Stage.RUN_GFX, string.Format("exception when taking screenshot : {0}", (object) ex.ToString()));
        return (string) null;
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
      this.components = (IContainer) new Container();
      this.AutoScaleMode = AutoScaleMode.Font;
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
        this.m_Img = img;
        this.m_X = x;
        this.m_Y = y;
      }

      public void Draw(Graphics g)
      {
        g.DrawImageUnscaled(this.m_Img, this.m_X, this.m_Y);
      }
    }

    private class GfxImageTransform : GDIPlusGameCanvas.IGfx
    {
      private readonly Image m_Img;
      private readonly Matrix m_Matrix;
      private readonly int m_X;
      private readonly int m_Y;

      public GfxImageTransform(Image img, float rotation, float scale, int x, int y)
      {
        m_Img = img;
        m_Matrix = new Matrix();
        m_X = x;
        m_Y = y;
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
        float[][] newColorMatrix = new float[5][];
        float[] numArray3 = new float[5];
        numArray3[0] = 1f;
        newColorMatrix[0] = numArray3;
        float[] numArray6 = new float[5];
        numArray6[1] = 1f;
        newColorMatrix[1] = numArray6;
        float[] numArray9 = new float[5];
        numArray9[2] = 1f;
        newColorMatrix[2] = numArray9;
        float[] numArray12 = new float[5];
        numArray12[3] = alpha;
        newColorMatrix[3] = numArray12;
        float[] numArray15 = new float[5];
        numArray15[4] = 1f;
        newColorMatrix[4] = numArray15;
        m_ImgAttributes = new ImageAttributes();
        m_ImgAttributes.SetColorMatrix(new ColorMatrix(newColorMatrix), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
      }

      public void Draw(Graphics g)
      {
        g.DrawImage(this.m_Img, new Rectangle(this.m_X, this.m_Y, this.m_Img.Width, this.m_Img.Height), 0, 0, this.m_Img.Width, this.m_Img.Height, GraphicsUnit.Pixel, this.m_ImgAttributes);
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
        this.m_Pen = pen;
        this.m_xFrom = xFrom;
        this.m_yFrom = yFrom;
        this.m_xTo = xTo;
        this.m_yTo = yTo;
      }

      public void Draw(Graphics g)
      {
        g.DrawLine(this.m_Pen, this.m_xFrom, this.m_yFrom, this.m_xTo, this.m_yTo);
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
        g.DrawString(this.m_Text, this.m_Font, this.m_Brush, (float) this.m_X, (float) this.m_Y);
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
        this.m_Brush = brush;
        this.m_Rect = rect;
      }

      public void Draw(Graphics g)
      {
        g.FillRectangle(this.m_Brush, this.m_Rect);
      }
    }

    private class GfxRect : GDIPlusGameCanvas.IGfx
    {
      private readonly Pen m_Pen;
      private readonly Rectangle m_Rect;

      public GfxRect(Pen pen, Rectangle rect)
      {
        this.m_Pen = pen;
        this.m_Rect = rect;
      }

      public void Draw(Graphics g)
      {
        g.DrawRectangle(this.m_Pen, this.m_Rect);
      }
    }
  }
}
