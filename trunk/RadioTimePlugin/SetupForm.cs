using System;
using System.Windows.Forms;

namespace RadioTimePlugin
{
  public partial class SetupForm : Form
  {
    public Settings Setting = new Settings();
    public SetupForm()
    {
      InitializeComponent();
      linkLabel1.Links.Add(0, linkLabel1.Text.Length, "http://www.tunein.com");
      Setting.Load();
      checkBox4.Checked = Setting.ShowPresets;
      textBox_user.Text = Setting.User;
      textBox_passwd.Text = Setting.Password;
      textBox_name.Text = Setting.PluginName;
      checkBox5.Checked = Setting.UseVideo;
      checkBox2.Checked = Setting.StartWithFastPreset;
      cbJumpNowPlaying.Checked = Setting.JumpNowPlaying;
    }


    private void button1_Click(object sender, EventArgs e)
    {
      Setting.ShowPresets = checkBox4.Checked;
      Setting.User = textBox_user.Text;
      Setting.Password = textBox_passwd.Text;
      Setting.PluginName = textBox_name.Text;
      Setting.UseVideo = checkBox5.Checked;
      Setting.StartWithFastPreset = checkBox2.Checked;
      Setting.JumpNowPlaying = cbJumpNowPlaying.Checked;
      Setting.Save();
      Close();
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      System.Diagnostics.Process.Start((string)e.Link.LinkData);
    }

    private void SetupForm_Load(object sender, EventArgs e)
    {
      
    }
  }
}