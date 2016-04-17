// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.UI.DXGameCanvas
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace djack.RogueSurvivor.UI
{
  public class DXGameCanvas : UserControl, IGameCanvas
  {
    private bool m_NeedRedraw = true;
    private Color m_ClearColor = Color.CornflowerBlue;
    private List<DXGameCanvas.IGfx> m_Gfxs = new List<DXGameCanvas.IGfx>(100);
    private Dictionary<Image, Texture> m_ImageToTextures = new Dictionary<Image, Texture>(32);
    private Dictionary<System.Drawing.Font, Microsoft.DirectX.Direct3D.Font> m_FontsToFonts = new Dictionary<System.Drawing.Font, Microsoft.DirectX.Direct3D.Font>(3);
    private Color[,] m_MinimapColors = new Color[200, 200];
    private byte[] m_MinimapBytes = new byte[160000];
    private RogueForm m_RogueForm;
    private bool m_DXInitialized;
    private PresentParameters m_PresentParameters;
    private Device m_Device;
    private Texture m_RenderTexture;
    private Surface m_RenderSurface;
    private RenderToSurface m_RenderToSurface;
    private Sprite m_Sprite;
    private Sprite m_TextSprite;
    private Texture m_BlankTexture;
    private Texture m_MinimapTexture;
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

    public DXGameCanvas()
    {
      Logger.WriteLine(Logger.Stage.INIT_GFX, "DXGameCanvas::InitializeComponent");
      this.InitializeComponent();
      Logger.WriteLine(Logger.Stage.INIT_GFX, "DXGameCanvas::SetStyle");
      this.SetStyle(ControlStyles.UserPaint | ControlStyles.Opaque | ControlStyles.AllPaintingInWmPaint | ControlStyles.EnableNotifyMessage, true);
      Logger.WriteLine(Logger.Stage.INIT_GFX, "DXGameCanvas() done.");
    }

    public void InitDX()
    {
      this.m_PresentParameters = new PresentParameters()
      {
        Windowed = true,
        SwapEffect = SwapEffect.Discard
      };
      Logger.WriteLine(Logger.Stage.INIT_GFX, "creating device...");
      this.m_Device = new Device(0, DeviceType.Hardware, (Control) this, CreateFlags.SoftwareVertexProcessing, new PresentParameters[1]
      {
        this.m_PresentParameters
      });
      Logger.WriteLine(Logger.Stage.INIT_GFX, "device info :");
      AdapterDetails adapterDetails = Manager.Adapters[0].Information;
      Caps caps = this.m_Device.DeviceCaps;
      Logger.WriteLine(Logger.Stage.INIT_GFX, string.Format("- device desc           : {0}", (object) adapterDetails.Description));
      Logger.WriteLine(Logger.Stage.INIT_GFX, string.Format("- max texture size      : {0}x{1}", (object) caps.MaxTextureWidth, (object) caps.MaxTextureHeight));
      Logger.WriteLine(Logger.Stage.INIT_GFX, string.Format("- vertex shader version : {0}", (object) caps.VertexShaderVersion.ToString()));
      Logger.WriteLine(Logger.Stage.INIT_GFX, string.Format("- pixel shader version  : {0}", (object) caps.PixelShaderVersion.ToString()));
      Logger.WriteLine(Logger.Stage.INIT_GFX, "device reset..");
      this.m_Device_DeviceReset((object) this.m_Device, (EventArgs) null);
      this.m_Device.DeviceLost += new EventHandler(this.m_Device_DeviceLost);
      this.m_Device.DeviceReset += new EventHandler(this.m_Device_DeviceReset);
      this.m_DXInitialized = true;
    }

    private void m_Device_DeviceLost(object sender, EventArgs e)
    {
      if (this.m_Device == (Device) null || this.m_Device.Disposed)
        return;
      int result;
      do
      {
        Thread.Sleep(100);
        if (this.m_Device.CheckCooperativeLevel(out result))
          return;
      }
      while (result == -2005530520);
      if (result != -2005530519)
        return;
      this.m_Device.Reset(this.m_PresentParameters);
    }

    private void m_Device_DeviceReset(object sender, EventArgs e)
    {
      this.m_Device.RenderState.CullMode = Cull.None;
      this.m_Device.RenderState.AlphaBlendEnable = true;
      this.m_ImageToTextures.Clear();
      this.m_FontsToFonts.Clear();
      Logger.WriteLine(Logger.Stage.INIT_GFX, "creating sprite...");
      this.m_Sprite = new Sprite(this.m_Device);
      Logger.WriteLine(Logger.Stage.INIT_GFX, "creating text sprite...");
      this.m_TextSprite = new Sprite(this.m_Device);
      Logger.WriteLine(Logger.Stage.INIT_GFX, "creating blank texture...");
      this.m_BlankTexture = new Texture(this.m_Device, new Bitmap("Resources\\Images\\blank_texture.png"), Usage.None, Pool.Managed);
      if (this.m_RenderTexture != (Texture) null)
      {
        Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing render texture...");
        this.m_RenderTexture.Dispose();
        this.m_RenderTexture = (Texture) null;
      }
      Logger.WriteLine(Logger.Stage.INIT_GFX, "creating render texture...");
      this.m_RenderTexture = new Texture(this.m_Device, 1024, 768, 1, Usage.RenderTarget, Microsoft.DirectX.Direct3D.Format.A8R8G8B8, Pool.Default);
      this.m_RenderSurface = this.m_RenderTexture.GetSurfaceLevel(0);
      Logger.WriteLine(Logger.Stage.INIT_GFX, "creating render surface...");
      this.m_RenderToSurface = new RenderToSurface(this.m_Device, 1024, 768, Microsoft.DirectX.Direct3D.Format.A8R8G8B8, false, DepthFormat.Unknown);
      Logger.WriteLine(Logger.Stage.INIT_GFX, "creating minimap texture...");
      this.m_MinimapTexture = new Texture(this.m_Device, 200, 200, 1, Usage.SoftwareProcessing, Microsoft.DirectX.Direct3D.Format.A8R8G8B8, Pool.Managed);
      Logger.WriteLine(Logger.Stage.INIT_GFX, "init done.");
    }

    protected override void OnCreateControl()
    {
      if (!this.DesignMode)
        this.InitDX();
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
      if (!this.m_DXInitialized)
        return;
      double totalMilliseconds1 = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
      if (this.NeedRedraw)
      {
        this.DoDraw(this.m_Device);
        this.m_NeedRedraw = false;
      }
      this.m_Device.Present();
      double totalMilliseconds2 = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
      if (!this.ShowFPS)
        return;
      double num = totalMilliseconds2 - totalMilliseconds1;
      if (num == 0.0)
        num = double.Epsilon;
      e.Graphics.DrawString(string.Format("Frame time={0:F} FPS={1:F}", (object) num, (object) (1000.0 / num)), this.Font, Brushes.Yellow, (float) (this.ClientRectangle.Right - 200), (float) (this.ClientRectangle.Bottom - 64));
    }

    private void DoDraw(Device dev)
    {
      if (this.m_RogueForm == null)
        return;
      this.m_RenderToSurface.BeginScene(this.m_RenderSurface);
      dev.Clear(ClearFlags.Target, this.m_ClearColor, 1f, 0);
      foreach (DXGameCanvas.IGfx gfx in this.m_Gfxs)
        gfx.Draw(dev);
      this.m_RenderToSurface.EndScene(Microsoft.DirectX.Direct3D.Filter.None);
      this.m_Device.BeginScene();
      this.m_Sprite.Begin(SpriteFlags.None);
      this.m_Sprite.Draw2D(this.m_RenderTexture, new Rectangle(0, 0, 1024, 768), new SizeF((float) this.m_RogueForm.ClientRectangle.Width, (float) this.m_RogueForm.ClientRectangle.Height), PointF.Empty, Color.White);
      this.m_Sprite.End();
      this.m_Device.EndScene();
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
      this.m_Gfxs.Add((DXGameCanvas.IGfx) new DXGameCanvas.GfxSprite(this.m_Sprite, new SizeF((float) img.Width, (float) img.Height), img.Width, img.Height, this.GetTexture(img), Color.White, x, y));
      this.m_NeedRedraw = true;
    }

    public void AddImage(Image img, int x, int y, Color tint)
    {
      this.m_Gfxs.Add((DXGameCanvas.IGfx) new DXGameCanvas.GfxSprite(this.m_Sprite, new SizeF((float) img.Width, (float) img.Height), img.Width, img.Height, this.GetTexture(img), tint, x, y));
      this.m_NeedRedraw = true;
    }

    public void AddImageTransform(Image img, int x, int y, float rotation, float scale)
    {
      this.m_Gfxs.Add((DXGameCanvas.IGfx) new DXGameCanvas.GfxSpriteTransform(rotation, scale, this.m_Sprite, new SizeF((float) img.Width, (float) img.Height), img.Width, img.Height, this.GetTexture(img), Color.White, x, y));
    }

    public void AddTransparentImage(float alpha, Image img, int x, int y)
    {
      this.m_Gfxs.Add((DXGameCanvas.IGfx) new DXGameCanvas.GfxSprite(this.m_Sprite, new SizeF((float) img.Width, (float) img.Height), img.Width, img.Height, this.GetTexture(img), Color.FromArgb((int) ((double) byte.MaxValue * (double) alpha), Color.White), x, y));
      this.m_NeedRedraw = true;
    }

    public void AddPoint(Color color, int x, int y)
    {
      this.m_Gfxs.Add((DXGameCanvas.IGfx) new DXGameCanvas.GfxRect(color, new Rectangle(x, y, 1, 1)));
      this.m_NeedRedraw = true;
    }

    public void AddLine(Color color, int xFrom, int yFrom, int xTo, int yTo)
    {
      this.m_Gfxs.Add((DXGameCanvas.IGfx) new DXGameCanvas.GfxLine(color, xFrom, yFrom, xTo, yTo));
      this.m_NeedRedraw = true;
    }

    public void AddString(System.Drawing.Font font, Color color, string text, int gx, int gy)
    {
      this.m_Gfxs.Add((DXGameCanvas.IGfx) new DXGameCanvas.GfxString(this.m_TextSprite, color, this.GetDXFont(font), text, gx, gy));
      this.m_NeedRedraw = true;
    }

    public void AddRect(Color color, Rectangle rect)
    {
      this.m_Gfxs.Add((DXGameCanvas.IGfx) new DXGameCanvas.GfxRect(color, rect));
      this.m_NeedRedraw = true;
    }

    public void AddFilledRect(Color color, Rectangle rect)
    {
      this.m_Gfxs.Add((DXGameCanvas.IGfx) new DXGameCanvas.GfxSprite(this.m_Sprite, new SizeF((float) rect.Width, (float) rect.Height), 4, 4, this.m_BlankTexture, color, rect.Left, rect.Top));
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
          this.m_MinimapColors[num1 + index1, num2 + index2] = color;
      }
    }

    public void DrawMinimap(int gx, int gy)
    {
      int pitch;
      GraphicsStream graphicsStream = this.m_MinimapTexture.LockRectangle(0, LockFlags.None, out pitch);
      for (int index1 = 0; index1 < 200; ++index1)
      {
        for (int index2 = 0; index2 < 200; ++index2)
        {
          Color color = this.m_MinimapColors[index2, index1];
          graphicsStream.Position = (long) (index1 * pitch + index2 * 4);
          graphicsStream.WriteByte(color.B);
          graphicsStream.WriteByte(color.G);
          graphicsStream.WriteByte(color.R);
          graphicsStream.WriteByte(color.A);
        }
      }
      this.m_MinimapTexture.UnlockRectangle(0);
      this.m_Gfxs.Add((DXGameCanvas.IGfx) new DXGameCanvas.GfxSprite(this.m_Sprite, new SizeF(200f, 200f), 200, 200, this.m_MinimapTexture, Color.White, gx, gy));
      this.NeedRedraw = true;
    }

    public string SaveScreenShot(string filePath)
    {
      string str = filePath + "." + this.ScreenshotExtension();
      Logger.WriteLine(Logger.Stage.RUN_GFX, "taking screenshot...");
      try
      {
        SurfaceLoader.Save(filePath, ImageFileFormat.Png, this.m_RenderSurface);
      }
      catch (Exception ex)
      {
        Logger.WriteLine(Logger.Stage.RUN_GFX, string.Format("exception when taking screenshot : {0}", (object) ex.ToString()));
        return (string) null;
      }
      Logger.WriteLine(Logger.Stage.RUN_GFX, "taking screenshot... done!");
      return str;
    }

    public string ScreenshotExtension()
    {
      return "png";
    }

    public void DisposeUnmanagedResources()
    {
      this.DisposeD3D();
    }

    private Texture GetTexture(Image img)
    {
      Texture texture1;
      if (this.m_ImageToTextures.TryGetValue(img, out texture1))
        return texture1;
      Texture texture2 = Texture.FromBitmap(this.m_Device, new Bitmap(img), Usage.SoftwareProcessing, Pool.Managed);
      this.m_ImageToTextures.Add(img, texture2);
      return texture2;
    }

    private Microsoft.DirectX.Direct3D.Font GetDXFont(System.Drawing.Font font)
    {
      Microsoft.DirectX.Direct3D.Font font1;
      if (this.m_FontsToFonts.TryGetValue(font, out font1))
        return font1;
      Microsoft.DirectX.Direct3D.Font font2 = new Microsoft.DirectX.Direct3D.Font(this.m_Device, font);
      this.m_FontsToFonts.Add(font, font2);
      return font2;
    }

    private void DisposeD3D()
    {
      Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing...");
      foreach (Texture texture in this.m_ImageToTextures.Values)
        texture.Dispose();
      this.m_ImageToTextures.Clear();
      foreach (Microsoft.DirectX.Direct3D.Font font in this.m_FontsToFonts.Values)
        font.Dispose();
      this.m_FontsToFonts.Clear();
      if (this.m_BlankTexture != (Texture) null)
      {
        Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing blank texture...");
        this.m_BlankTexture.Dispose();
        this.m_BlankTexture = (Texture) null;
      }
      if (this.m_MinimapTexture != (Texture) null)
      {
        Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing minimap texture...");
        this.m_MinimapTexture.Dispose();
        this.m_MinimapTexture = (Texture) null;
      }
      if (this.m_Sprite != (Sprite) null)
      {
        Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing sprite...");
        this.m_Sprite.Dispose();
        this.m_Sprite = (Sprite) null;
      }
      if (this.m_TextSprite != (Sprite) null)
      {
        Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing text sprite...");
        this.m_TextSprite.Dispose();
        this.m_TextSprite = (Sprite) null;
      }
      if (this.m_RenderToSurface != (RenderToSurface) null)
      {
        Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing render surface...");
        this.m_RenderToSurface.Dispose();
        this.m_RenderToSurface = (RenderToSurface) null;
      }
      if (this.m_RenderTexture != (Texture) null)
      {
        Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing render texture...");
        this.m_RenderTexture.Dispose();
        this.m_RenderTexture = (Texture) null;
      }
      if (this.m_Device != (Device) null)
      {
        Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing device...");
        this.m_Device.Dispose();
        this.m_Device = (Device) null;
      }
      Logger.WriteLine(Logger.Stage.CLEAN_GFX, "disposing done.");
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.SuspendLayout();
      this.AutoScaleMode = AutoScaleMode.None;
      this.Name = "DXGameCanvas";
      this.ResumeLayout(false);
    }

    private interface IGfx
    {
      void Draw(Device dev);
    }

    private class GfxSprite : DXGameCanvas.IGfx
    {
      private readonly Sprite m_Sprite;
      private readonly Texture m_Texture;
      private readonly Color m_Color;
      private readonly int m_X;
      private readonly int m_Y;
      private readonly SizeF m_Size;
      private readonly int m_TexWidth;
      private readonly int m_TexHeight;

      public GfxSprite(Sprite sprite, SizeF size, int texWidth, int texHeight, Texture texture, Color color, int x, int y)
      {
        this.m_Sprite = sprite;
        this.m_Texture = texture;
        this.m_Color = color;
        this.m_X = x;
        this.m_Y = y;
        this.m_Size = size;
        this.m_TexWidth = texWidth;
        this.m_TexHeight = texHeight;
      }

      public void Draw(Device dev)
      {
        this.m_Sprite.Begin(SpriteFlags.AlphaBlend);
        this.m_Sprite.Draw2D(this.m_Texture, new Rectangle(0, 0, this.m_TexWidth, this.m_TexHeight), this.m_Size, new PointF((float) this.m_X, (float) this.m_Y), this.m_Color);
        this.m_Sprite.End();
      }
    }

    private class GfxSpriteTransform : DXGameCanvas.IGfx
    {
      private readonly float m_Rotation;
      private readonly float m_Scale;
      private readonly Sprite m_Sprite;
      private readonly Texture m_Texture;
      private readonly Color m_Color;
      private readonly int m_X;
      private readonly int m_Y;
      private readonly SizeF m_Size;
      private readonly SizeF m_SizeScaled;
      private readonly int m_TexWidth;
      private readonly int m_TexHeight;
      private readonly PointF m_RotationCenter;

      public GfxSpriteTransform(float rotation, float scale, Sprite sprite, SizeF size, int texWidth, int texHeight, Texture texture, Color color, int x, int y)
      {
        this.m_Rotation = (float) ((double) rotation * 3.14159274101257 / 180.0);
        this.m_Scale = scale;
        this.m_Sprite = sprite;
        this.m_Texture = texture;
        this.m_Color = color;
        this.m_X = x;
        this.m_Y = y;
        this.m_Size = size;
        this.m_TexWidth = texWidth;
        this.m_TexHeight = texHeight;
        this.m_SizeScaled = new SizeF(size.Width * scale, size.Height * scale);
        this.m_RotationCenter = new PointF((float) (texWidth / 2), (float) (texHeight / 2));
      }

      public void Draw(Device dev)
      {
        this.m_Sprite.Begin(SpriteFlags.AlphaBlend);
        this.m_Sprite.Draw2D(this.m_Texture, new Rectangle(0, 0, this.m_TexWidth, this.m_TexHeight), this.m_SizeScaled, this.m_RotationCenter, this.m_Rotation, new PointF((float) this.m_X + this.m_SizeScaled.Width / 2f, (float) this.m_Y + this.m_SizeScaled.Height / 2f), this.m_Color);
        this.m_Sprite.End();
      }
    }

    private class GfxLine : DXGameCanvas.IGfx
    {
      private readonly CustomVertex.TransformedColored[] m_Points = new CustomVertex.TransformedColored[2];

      public GfxLine(Color color, int xFrom, int yFrom, int xTo, int yTo)
      {
        int argb = color.ToArgb();
        this.m_Points[0].Position = new Vector4((float) xFrom, (float) yFrom, 0.0f, 1f);
        this.m_Points[0].Color = argb;
        this.m_Points[1].Position = new Vector4((float) xTo, (float) yTo, 0.0f, 1f);
        this.m_Points[1].Color = argb;
      }

      public void Draw(Device dev)
      {
        dev.VertexFormat = VertexFormats.Diffuse | VertexFormats.Transformed;
        dev.SetTexture(0, (BaseTexture) null);
        dev.DrawUserPrimitives(PrimitiveType.LineList, 1, (object) this.m_Points);
      }
    }

    private class GfxString : DXGameCanvas.IGfx
    {
      private readonly Color m_Color;
      private readonly Microsoft.DirectX.Direct3D.Font m_Font;
      private readonly string m_Text;
      private readonly int m_X;
      private readonly int m_Y;
      private readonly Sprite m_TextSprite;

      public GfxString(Sprite textSprite, Color color, Microsoft.DirectX.Direct3D.Font font, string text, int x, int y)
      {
        this.m_Color = color;
        this.m_Font = font;
        this.m_Text = text;
        this.m_X = x;
        this.m_Y = y;
        this.m_TextSprite = textSprite;
      }

      public void Draw(Device dev)
      {
        this.m_TextSprite.Begin(SpriteFlags.AlphaBlend);
        this.m_Font.DrawText(this.m_TextSprite, this.m_Text, this.m_X, this.m_Y, this.m_Color);
        this.m_TextSprite.End();
      }
    }

    private class GfxRect : DXGameCanvas.IGfx
    {
      private static readonly short[] s_Indices = new short[8]
      {
        (short) 0,
        (short) 1,
        (short) 2,
        (short) 3,
        (short) 0,
        (short) 2,
        (short) 1,
        (short) 3
      };
      private readonly CustomVertex.TransformedColored[] m_Points = new CustomVertex.TransformedColored[4];

      public GfxRect(Color color, Rectangle rect)
      {
        int argb = color.ToArgb();
        this.m_Points[0].Position = new Vector4((float) rect.Left, (float) rect.Top, 0.0f, 1f);
        this.m_Points[0].Color = argb;
        this.m_Points[1].Position = new Vector4((float) rect.Right, (float) rect.Top, 0.0f, 1f);
        this.m_Points[1].Color = argb;
        this.m_Points[2].Position = new Vector4((float) rect.Left, (float) rect.Bottom, 0.0f, 1f);
        this.m_Points[2].Color = argb;
        this.m_Points[3].Position = new Vector4((float) rect.Right, (float) rect.Bottom, 0.0f, 1f);
        this.m_Points[3].Color = argb;
      }

      public void Draw(Device dev)
      {
        dev.VertexFormat = VertexFormats.Diffuse | VertexFormats.Transformed;
        dev.SetTexture(0, (BaseTexture) null);
        dev.DrawIndexedUserPrimitives(PrimitiveType.LineList, 0, 4, 4, (object) DXGameCanvas.GfxRect.s_Indices, true, (object) this.m_Points);
      }
    }
  }
}
