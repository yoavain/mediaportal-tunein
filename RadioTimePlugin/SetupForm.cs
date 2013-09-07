using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RadioTimePlugin
{
  public partial class SetupForm : Form
  {
    public Settings _setting = new Settings();
    public SetupForm()
    {
      InitializeComponent();
      this.linkLabel1.Links.Add(0, linkLabel1.Text.Length, "http://www.radiotime.com");
      _setting.Load();
      checkBox4.Checked = _setting.ShowPresets;
      textBox_user.Text = _setting.User;
      textBox_passwd.Text = _setting.Password;
      textBox_name.Text = _setting.PluginName;
      checkBox5.Checked = _setting.UseVideo;
      checkBox2.Checked = _setting.StartWithFastPreset;
      cbJumpNowPlaying.Checked = _setting.JumpNowPlaying;
    }


    private void button1_Click(object sender, EventArgs e)
    {
      _setting.ShowPresets = checkBox4.Checked;
      _setting.User = textBox_user.Text;
      _setting.Password = textBox_passwd.Text;
      _setting.PluginName = textBox_name.Text;
      _setting.UseVideo = checkBox5.Checked;
      _setting.StartWithFastPreset = checkBox2.Checked;
      _setting.JumpNowPlaying = cbJumpNowPlaying.Checked;
      _setting.Save();
      this.Close();
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start((string)e.Link.LinkData);
    }

    private void SetupForm_Load(object sender, EventArgs e)
    {
      
    }

    private void pictureBox1_Click(object sender, EventArgs e)
    {
      System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=PTPNJYH7FMZWL");
    }
  }
}