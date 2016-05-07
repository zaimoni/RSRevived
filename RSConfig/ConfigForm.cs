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
      this.InitializeComponent();
      this.l_GameVersion.Text = "Game version : "+SetupConfig.GAME_VERSION;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.b_SaveExit = new Button();
      this.gb_Video = new GroupBox();
      this.rb_Video_GDI = new RadioButton();
//    this.rb_Video_MDX = new RadioButton();
      this.b_Exit = new Button();
      this.gb_Sound = new GroupBox();
      this.rb_Sound_NoSound = new RadioButton();
      this.rb_Sound_WAV = new RadioButton();
//    this.rb_Sound_MDX = new RadioButton();
      this.panel1 = new Panel();
      this.l_GameVersion = new Label();
      this.gb_Video.SuspendLayout();
      this.gb_Sound.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      this.b_SaveExit.Location = new Point(5, 111);
      this.b_SaveExit.Name = "b_SaveExit";
      this.b_SaveExit.Size = new Size(122, 23);
      this.b_SaveExit.TabIndex = 2;
      this.b_SaveExit.Text = "Save and Exit";
      this.b_SaveExit.UseVisualStyleBackColor = true;
      this.b_SaveExit.Click += new EventHandler(this.b_SaveExit_Click);
      this.gb_Video.AutoSize = true;
      this.gb_Video.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      this.gb_Video.Controls.Add((Control) this.rb_Video_GDI);
//    this.gb_Video.Controls.Add((Control) this.rb_Video_MDX);
      this.gb_Video.Location = new Point(6, 3);
      this.gb_Video.Name = "gb_Video";
      this.gb_Video.Size = new Size(121, 80);
      this.gb_Video.TabIndex = 0;
      this.gb_Video.TabStop = false;
      this.gb_Video.Text = "Video";
      this.rb_Video_GDI.AutoSize = true;
      this.rb_Video_GDI.Location = new Point(7, 44);
      this.rb_Video_GDI.Name = "rb_Video_GDI";
      this.rb_Video_GDI.Size = new Size(50, 17);
      this.rb_Video_GDI.TabIndex = 1;
      this.rb_Video_GDI.TabStop = true;
      this.rb_Video_GDI.Text = "GDI+";
      this.rb_Video_GDI.UseVisualStyleBackColor = true;
      this.rb_Video_GDI.CheckedChanged += new EventHandler(this.rb_Video_GDI_CheckedChanged);
/*
      this.rb_Video_MDX.AutoSize = true;
      this.rb_Video_MDX.Checked = true;
      this.rb_Video_MDX.Location = new Point(7, 20);
      this.rb_Video_MDX.Name = "rb_Video_MDX";
      this.rb_Video_MDX.Size = new Size(108, 17);
      this.rb_Video_MDX.TabIndex = 0;
      this.rb_Video_MDX.TabStop = true;
      this.rb_Video_MDX.Text = "Managed DirectX";
      this.rb_Video_MDX.UseVisualStyleBackColor = true;
      this.rb_Video_MDX.CheckedChanged += new EventHandler(this.rb_Video_MDX_CheckedChanged);
*/
      this.b_Exit.Location = new Point(131, 111);
      this.b_Exit.Name = "b_Exit";
      this.b_Exit.Size = new Size(121, 23);
      this.b_Exit.TabIndex = 3;
      this.b_Exit.Text = "Exit";
      this.b_Exit.UseVisualStyleBackColor = true;
      this.b_Exit.Click += new EventHandler(this.b_Exit_Click);
      this.gb_Sound.AutoSize = true;
      this.gb_Sound.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      this.gb_Sound.Controls.Add((Control) this.rb_Sound_NoSound);
      this.gb_Sound.Controls.Add((Control) this.rb_Sound_WAV);
//    this.gb_Sound.Controls.Add((Control) this.rb_Sound_MDX);
      this.gb_Sound.Location = new Point(134, 3);
      this.gb_Sound.Name = "gb_Sound";
      this.gb_Sound.Size = new Size(121, 102);
      this.gb_Sound.TabIndex = 1;
      this.gb_Sound.TabStop = false;
      this.gb_Sound.Text = "Sound";
      this.rb_Sound_NoSound.AutoSize = true;
      this.rb_Sound_NoSound.Location = new Point(6, 66);
      this.rb_Sound_NoSound.Name = "rb_Sound_NoSound";
      this.rb_Sound_NoSound.Size = new Size(71, 17);
      this.rb_Sound_NoSound.TabIndex = 2;
      this.rb_Sound_NoSound.TabStop = true;
      this.rb_Sound_NoSound.Text = "No sound";
      this.rb_Sound_NoSound.UseVisualStyleBackColor = true;
      this.rb_Sound_NoSound.CheckedChanged += new EventHandler(this.rb_Sound_NoSound_CheckedChanged);
      this.rb_Sound_WAV.AutoSize = true;
      this.rb_Sound_WAV.Location = new Point(7, 43);
      this.rb_Sound_WAV.Name = "rb_Sound_WAV";
      this.rb_Sound_WAV.Size = new Size(71, 17);
      this.rb_Sound_WAV.TabIndex = 1;
      this.rb_Sound_WAV.TabStop = true;
      this.rb_Sound_WAV.Text = "C# native WAV";
      this.rb_Sound_WAV.UseVisualStyleBackColor = true;
      this.rb_Sound_WAV.CheckedChanged += new EventHandler(this.rb_Audio_WAV_CheckedChanged);
/*
      this.rb_Sound_MDX.AutoSize = true;
      this.rb_Sound_MDX.Checked = true;
      this.rb_Sound_MDX.Location = new Point(7, 20);
      this.rb_Sound_MDX.Name = "rb_Sound_MDX";
      this.rb_Sound_MDX.Size = new Size(108, 17);
      this.rb_Sound_MDX.TabIndex = 0;
      this.rb_Sound_MDX.TabStop = true;
      this.rb_Sound_MDX.Text = "Managed DirectX";
      this.rb_Sound_MDX.UseVisualStyleBackColor = true;
      this.rb_Sound_MDX.CheckedChanged += new EventHandler(this.rb_Sound_MDX_CheckedChanged);
*/
      this.panel1.AutoSize = true;
      this.panel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      this.panel1.Controls.Add((Control) this.gb_Sound);
      this.panel1.Controls.Add((Control) this.b_Exit);
      this.panel1.Controls.Add((Control) this.gb_Video);
      this.panel1.Controls.Add((Control) this.b_SaveExit);
      this.panel1.Dock = DockStyle.Bottom;
      this.panel1.Location = new Point(0, 42);
      this.panel1.Name = "panel1";
      this.panel1.Size = new Size(264, 137);
      this.panel1.TabIndex = 4;
      this.l_GameVersion.AutoSize = true;
      this.l_GameVersion.Dock = DockStyle.Top;
      this.l_GameVersion.Location = new Point(0, 0);
      this.l_GameVersion.Name = "l_GameVersion";
      this.l_GameVersion.Size = new Size(106, 13);
      this.l_GameVersion.TabIndex = 5;
      this.l_GameVersion.Text = "<game version here>";
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(264, 179);
      this.Controls.Add((Control) this.l_GameVersion);
      this.Controls.Add((Control) this.panel1);
      this.Name = "ConfigForm";
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text = SetupConfig.GAME_NAME+" Config";
      this.Load += new EventHandler(this.SetupForm_Load);
      this.gb_Video.ResumeLayout(false);
      this.gb_Video.PerformLayout();
      this.gb_Sound.ResumeLayout(false);
      this.gb_Sound.PerformLayout();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    private void b_SaveExit_Click(object sender, EventArgs e)
    {
      SetupConfig.Save();
      this.Close();
    }

    private void b_Exit_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void SetupForm_Load(object sender, EventArgs e)
    {
      SetupConfig.Load();
      switch (SetupConfig.Video)
      {
/*
        case SetupConfig.eVideo.VIDEO_MANAGED_DIRECTX:
          this.rb_Video_MDX.Checked = true;
          break;
*/
        case SetupConfig.eVideo.VIDEO_GDI_PLUS:
          this.rb_Video_GDI.Checked = true;
          break;
      }
      switch (SetupConfig.Sound)
      {
/*               
      case SetupConfig.eSound.SOUND_MANAGED_DIRECTX:
        this.rb_Sound_MDX.Checked = true;
        break;
*/
      case SetupConfig.eSound.SOUND_WAV:
        this.rb_Sound_WAV.Checked = true;
        break;
      case SetupConfig.eSound.SOUND_NOSOUND:
          this.rb_Sound_NoSound.Checked = true;
          break;
      }
    }

/*
    private void rb_Video_MDX_CheckedChanged(object sender, EventArgs e)
    {
      if (!this.rb_Video_MDX.Checked || SetupConfig.Video == SetupConfig.eVideo.VIDEO_MANAGED_DIRECTX)
        return;
      SetupConfig.Video = SetupConfig.eVideo.VIDEO_MANAGED_DIRECTX;
    }
*/

    private void rb_Video_GDI_CheckedChanged(object sender, EventArgs e)
    {
      if (!this.rb_Video_GDI.Checked || SetupConfig.Video == SetupConfig.eVideo.VIDEO_GDI_PLUS)
        return;
      SetupConfig.Video = SetupConfig.eVideo.VIDEO_GDI_PLUS;
    }

        /*
            private void rb_Sound_MDX_CheckedChanged(object sender, EventArgs e)
            {
              if (!this.rb_Sound_MDX.Checked || SetupConfig.Sound == SetupConfig.eSound.SOUND_MANAGED_DIRECTX)
                return;
              SetupConfig.Sound = SetupConfig.eSound.SOUND_MANAGED_DIRECTX;
            }
        */

    private void rb_Audio_WAV_CheckedChanged(object sender, EventArgs e)
    {
      if (!this.rb_Sound_WAV.Checked || SetupConfig.Sound == SetupConfig.eSound.SOUND_WAV)
        return;
      SetupConfig.Sound = SetupConfig.eSound.SOUND_WAV;
    }

    private void rb_Sound_NoSound_CheckedChanged(object sender, EventArgs e)
    {
      if (!this.rb_Sound_NoSound.Checked || SetupConfig.Sound == SetupConfig.eSound.SOUND_NOSOUND)
        return;
      SetupConfig.Sound = SetupConfig.eSound.SOUND_NOSOUND;
    }
  }
}
