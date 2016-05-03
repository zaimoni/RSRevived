// Decompiled with JetBrains decompiler
// Type: djack.RogueSurvivor.Bugreport
// Assembly: RogueSurvivor, Version=0.9.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D2AE4FAE-2CA8-43FF-8F2F-59C173341976
// Assembly location: C:\Private.app\RS9Alpha.Hg\RogueSurvivor.exe

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace djack.RogueSurvivor
{
  public class Bugreport : Form
  {
    private string NL = Environment.NewLine;
    private Exception m_Exception;
    private IContainer components;
    private Button m_OkButton;
    private TextBox m_HeaderTextBox;
    private TextBox m_LogTextBox;

    public Bugreport(Exception e)
    {
            InitializeComponent();
            m_Exception = e;
    }

    private void m_OkButton_Click(object sender, EventArgs e)
    {
            Close();
    }

    private void Bugreport_Load(object sender, EventArgs e)
    {
            m_HeaderTextBox.Text = SetupConfig.GAME_NAME+" encoutered a fatal error." + NL + "Please report all the text in the textbox below to the author (copypaste it, remember to scroll all the way down from start to end)." + NL + "Press OK to exit.";
            m_LogTextBox.Clear();
            m_LogTextBox.AppendText("Start of report." + NL);
            m_LogTextBox.AppendText("-----------------------------------------------" + NL);
            m_LogTextBox.AppendText("EXCEPTION" + NL);
            m_LogTextBox.AppendText(m_Exception.ToString() + NL);
            m_LogTextBox.AppendText("-----------------------------------------------" + NL);
            m_LogTextBox.AppendText("LOG" + NL);
      foreach (string line in Logger.Lines)
                m_LogTextBox.AppendText(line + NL);
            m_LogTextBox.AppendText("-----------------------------------------------" + NL);
            m_LogTextBox.AppendText("End of report." + NL);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && components != null)
                components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
            m_OkButton = new Button();
            m_HeaderTextBox = new TextBox();
            m_LogTextBox = new TextBox();
            SuspendLayout();
            m_OkButton.Dock = DockStyle.Bottom;
            m_OkButton.Location = new Point(0, 343);
            m_OkButton.Name = "m_OkButton";
            m_OkButton.Size = new Size(592, 23);
            m_OkButton.TabIndex = 0;
            m_OkButton.Text = "OK";
            m_OkButton.UseVisualStyleBackColor = true;
            m_OkButton.Click += new EventHandler(m_OkButton_Click);
            m_HeaderTextBox.AcceptsReturn = true;
            m_HeaderTextBox.AcceptsTab = true;
            m_HeaderTextBox.Dock = DockStyle.Top;
            m_HeaderTextBox.Location = new Point(0, 0);
            m_HeaderTextBox.Multiline = true;
            m_HeaderTextBox.Name = "m_HeaderTextBox";
            m_HeaderTextBox.ReadOnly = true;
            m_HeaderTextBox.ScrollBars = ScrollBars.Vertical;
            m_HeaderTextBox.Size = new Size(592, 97);
            m_HeaderTextBox.TabIndex = 1;
            m_LogTextBox.AcceptsReturn = true;
            m_LogTextBox.AcceptsTab = true;
            m_LogTextBox.Location = new Point(12, 103);
            m_LogTextBox.Multiline = true;
            m_LogTextBox.Name = "m_LogTextBox";
            m_LogTextBox.ReadOnly = true;
            m_LogTextBox.ScrollBars = ScrollBars.Vertical;
            m_LogTextBox.Size = new Size(568, 234);
            m_LogTextBox.TabIndex = 2;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(592, 366);
            Controls.Add((Control)m_LogTextBox);
            Controls.Add((Control)m_HeaderTextBox);
            Controls.Add((Control)m_OkButton);
            Name = "Bugreport";
            StartPosition = FormStartPosition.CenterScreen;
            Text = SetupConfig.GAME_NAME+" Error Report";
            TopMost = true;
            Load += new EventHandler(Bugreport_Load);
            ResumeLayout(false);
            PerformLayout();
    }
  }
}
