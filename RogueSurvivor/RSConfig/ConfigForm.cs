// Decompiled with JetBrains decompiler
// Type: Setup.ConfigForm
// Assembly: RSConfig, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 5A6245E6-7D9A-4424-BC16-B17B9A5036B9
// Assembly location: C:\Private.app\RS9Alpha.Hg\RSConfig.exe

using djack.RogueSurvivor;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Setup
{
  public class ConfigForm : Form
  {
    private IContainer components;
    private Button b_SaveExit;
    private GroupBox gb_Video;
    private RadioButton rb_Video_GDI;
//  private RadioButton rb_Video_MDX;
    private Button b_Exit;
    private GroupBox gb_Sound;
//  private RadioButton rb_Sound_MDX;
    private Panel panel1;
    private Label l_GameVersion;
    private RadioButton rb_Sound_WAV;
    private RadioButton rb_Sound_NoSound;

    public ConfigForm()
    {
      InitializeComponent();
      l_GameVersion.Text = "Game version : "+SetupConfig.GAME_VERSION;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && components != null) components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      b_SaveExit = new Button();
      gb_Video = new GroupBox();
      rb_Video_GDI = new RadioButton();
//    rb_Video_MDX = new RadioButton();
      b_Exit = new Button();
      gb_Sound = new GroupBox();
      rb_Sound_NoSound = new RadioButton();
      rb_Sound_WAV = new RadioButton();
//    rb_Sound_MDX = new RadioButton();
      panel1 = new Panel();
      l_GameVersion = new Label();
      gb_Video.SuspendLayout();
      gb_Sound.SuspendLayout();
      panel1.SuspendLayout();
      SuspendLayout();
      b_SaveExit.Location = new Point(5, 111);
      b_SaveExit.Name = "b_SaveExit";
      b_SaveExit.Size = new Size(122, 23);
      b_SaveExit.TabIndex = 2;
      b_SaveExit.Text = "Save and Exit";
      b_SaveExit.UseVisualStyleBackColor = true;
      b_SaveExit.Click += new EventHandler(b_SaveExit_Click);
      gb_Video.AutoSize = true;
      gb_Video.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      gb_Video.Controls.Add((Control) rb_Video_GDI);
//    gb_Video.Controls.Add((Control) rb_Video_MDX);
      gb_Video.Location = new Point(6, 3);
      gb_Video.Name = "gb_Video";
      gb_Video.Size = new Size(121, 80);
      gb_Video.TabIndex = 0;
      gb_Video.TabStop = false;
      gb_Video.Text = "Video";
      rb_Video_GDI.AutoSize = true;
      rb_Video_GDI.Location = new Point(7, 44);
      rb_Video_GDI.Name = "rb_Video_GDI";
      rb_Video_GDI.Size = new Size(50, 17);
      rb_Video_GDI.TabIndex = 1;
      rb_Video_GDI.TabStop = true;
      rb_Video_GDI.Text = "GDI+";
      rb_Video_GDI.UseVisualStyleBackColor = true;
      rb_Video_GDI.CheckedChanged += new EventHandler(rb_Video_GDI_CheckedChanged);
/*
      rb_Video_MDX.AutoSize = true;
      rb_Video_MDX.Checked = true;
      rb_Video_MDX.Location = new Point(7, 20);
      rb_Video_MDX.Name = "rb_Video_MDX";
      rb_Video_MDX.Size = new Size(108, 17);
      rb_Video_MDX.TabIndex = 0;
      rb_Video_MDX.TabStop = true;
      rb_Video_MDX.Text = "Managed DirectX";
      rb_Video_MDX.UseVisualStyleBackColor = true;
      rb_Video_MDX.CheckedChanged += new EventHandler(rb_Video_MDX_CheckedChanged);
*/
      b_Exit.Location = new Point(131, 111);
      b_Exit.Name = "b_Exit";
      b_Exit.Size = new Size(121, 23);
      b_Exit.TabIndex = 3;
      b_Exit.Text = "Exit";
      b_Exit.UseVisualStyleBackColor = true;
      b_Exit.Click += new EventHandler(b_Exit_Click);
      gb_Sound.AutoSize = true;
      gb_Sound.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      gb_Sound.Controls.Add(rb_Sound_NoSound);
      gb_Sound.Controls.Add(rb_Sound_WAV);
//    gb_Sound.Controls.Add(rb_Sound_MDX);
      gb_Sound.Location = new Point(134, 3);
      gb_Sound.Name = "gb_Sound";
      gb_Sound.Size = new Size(121, 102);
      gb_Sound.TabIndex = 1;
      gb_Sound.TabStop = false;
      gb_Sound.Text = "Sound";
      rb_Sound_NoSound.AutoSize = true;
      rb_Sound_NoSound.Location = new Point(6, 66);
      rb_Sound_NoSound.Name = "rb_Sound_NoSound";
      rb_Sound_NoSound.Size = new Size(71, 17);
      rb_Sound_NoSound.TabIndex = 2;
      rb_Sound_NoSound.TabStop = true;
      rb_Sound_NoSound.Text = "No sound";
      rb_Sound_NoSound.UseVisualStyleBackColor = true;
      rb_Sound_NoSound.CheckedChanged += rb_Sound_NoSound_CheckedChanged;
      rb_Sound_WAV.AutoSize = true;
      rb_Sound_WAV.Location = new Point(7, 43);
      rb_Sound_WAV.Name = "rb_Sound_WAV";
      rb_Sound_WAV.Size = new Size(71, 17);
      rb_Sound_WAV.TabIndex = 1;
      rb_Sound_WAV.TabStop = true;
      rb_Sound_WAV.Text = "C# native WAV";
      rb_Sound_WAV.UseVisualStyleBackColor = true;
      rb_Sound_WAV.CheckedChanged += rb_Audio_WAV_CheckedChanged;
/*
      rb_Sound_MDX.AutoSize = true;
      rb_Sound_MDX.Checked = true;
      rb_Sound_MDX.Location = new Point(7, 20);
      rb_Sound_MDX.Name = "rb_Sound_MDX";
      rb_Sound_MDX.Size = new Size(108, 17);
      rb_Sound_MDX.TabIndex = 0;
      rb_Sound_MDX.TabStop = true;
      rb_Sound_MDX.Text = "Managed DirectX";
      rb_Sound_MDX.UseVisualStyleBackColor = true;
      rb_Sound_MDX.CheckedChanged += rb_Sound_MDX_CheckedChanged;
*/
      panel1.AutoSize = true;
      panel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      panel1.Controls.Add(gb_Sound);
      panel1.Controls.Add(b_Exit);
      panel1.Controls.Add(gb_Video);
      panel1.Controls.Add(b_SaveExit);
      panel1.Dock = DockStyle.Bottom;
      panel1.Location = new Point(0, 42);
      panel1.Name = "panel1";
      panel1.Size = new Size(264, 137);
      panel1.TabIndex = 4;
      l_GameVersion.AutoSize = true;
      l_GameVersion.Dock = DockStyle.Top;
      l_GameVersion.Location = new Point(0, 0);
      l_GameVersion.Name = "l_GameVersion";
      l_GameVersion.Size = new Size(106, 13);
      l_GameVersion.TabIndex = 5;
      l_GameVersion.Text = "<game version here>";
      AutoScaleDimensions = new SizeF(6f, 13f);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(264, 179);
      Controls.Add(l_GameVersion);
      Controls.Add(panel1);
      Name = "ConfigForm";
      StartPosition = FormStartPosition.CenterScreen;
      Text = SetupConfig.GAME_NAME+" Config";
      Load += SetupForm_Load;
      gb_Video.ResumeLayout(false);
      gb_Video.PerformLayout();
      gb_Sound.ResumeLayout(false);
      gb_Sound.PerformLayout();
      panel1.ResumeLayout(false);
      panel1.PerformLayout();
      ResumeLayout(false);
      PerformLayout();
    }

    private void b_SaveExit_Click(object sender, EventArgs e)
    {
      SetupConfig.Save();
      Close();
    }

    private void b_Exit_Click(object sender, EventArgs e) { Close(); }

    private void SetupForm_Load(object sender, EventArgs e)
    {
      SetupConfig.Load();
      switch (SetupConfig.Video)
      {
/*
        case SetupConfig.eVideo.VIDEO_MANAGED_DIRECTX:
          rb_Video_MDX.Checked = true;
          break;
*/
        case SetupConfig.eVideo.VIDEO_GDI_PLUS:
          rb_Video_GDI.Checked = true;
          break;
      }
      switch (SetupConfig.Sound)
      {
/*               
      case SetupConfig.eSound.SOUND_MANAGED_DIRECTX:
        rb_Sound_MDX.Checked = true;
        break;
*/
      case SetupConfig.eSound.SOUND_WAV:
        rb_Sound_WAV.Checked = true;
        break;
      case SetupConfig.eSound.SOUND_NOSOUND:
        rb_Sound_NoSound.Checked = true;
        break;
      }
    }

/*
    private void rb_Video_MDX_CheckedChanged(object sender, EventArgs e)
    {
      if (!rb_Video_MDX.Checked || SetupConfig.Video == SetupConfig.eVideo.VIDEO_MANAGED_DIRECTX) return;
      SetupConfig.Video = SetupConfig.eVideo.VIDEO_MANAGED_DIRECTX;
    }
*/

    private void rb_Video_GDI_CheckedChanged(object sender, EventArgs e)
    {
      if (!rb_Video_GDI.Checked || SetupConfig.Video == SetupConfig.eVideo.VIDEO_GDI_PLUS) return;
      SetupConfig.Video = SetupConfig.eVideo.VIDEO_GDI_PLUS;
    }

/*
    private void rb_Sound_MDX_CheckedChanged(object sender, EventArgs e)
    {
      if (!rb_Sound_MDX.Checked || SetupConfig.Sound == SetupConfig.eSound.SOUND_MANAGED_DIRECTX) return;
      SetupConfig.Sound = SetupConfig.eSound.SOUND_MANAGED_DIRECTX;
    }
*/

    private void rb_Audio_WAV_CheckedChanged(object sender, EventArgs e)
    {
      if (!rb_Sound_WAV.Checked || SetupConfig.Sound == SetupConfig.eSound.SOUND_WAV) return;
      SetupConfig.Sound = SetupConfig.eSound.SOUND_WAV;
    }

    private void rb_Sound_NoSound_CheckedChanged(object sender, EventArgs e)
    {
      if (!rb_Sound_NoSound.Checked || SetupConfig.Sound == SetupConfig.eSound.SOUND_NOSOUND) return;
      SetupConfig.Sound = SetupConfig.eSound.SOUND_NOSOUND;
    }
  }
}
