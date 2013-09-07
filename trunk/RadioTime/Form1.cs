using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Security.Cryptography;
using System.Windows.Forms;
using RadioTimeOpmlApi;
using RadioTimeOpmlApi.com.radiotime.services;

namespace Test
{
  public partial class Form1 : Form
  {
    RadioTime grabber;
    RadioTimeWebService websrv;
    RadioTimeSetting set = new RadioTimeSetting();
    public Form1()
    {
      InitializeComponent();
      grabber = new RadioTime();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      set.User = textBoxUser.Text;
      grabber.GetData(set.StartupUrl);
      RefreshList();
    }

    private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        textBox2.Text = ((RadioTimeOutline)listView1.SelectedItems[0].Tag).Url;
        grabber.GetData(((RadioTimeOutline)listView1.SelectedItems[0].Tag).Url);
        if (((RadioTimeOutline)listView1.SelectedItems[0].Tag).Type == RadioTimeOutline.OutlineType.audio)
        {
          RadioTimeNowPlaying play = new RadioTimeNowPlaying();
          play.Get(((RadioTimeOutline)listView1.SelectedItems[0].Tag).StationId);
        }
        RefreshList();

      }
    }

    private void RefreshList()
    {
      listView1.Items.Clear();
      foreach (RadioTimeOutline head in grabber.Body)
      {
        ListViewItem item = new ListViewItem(head.Text);
        item.Tag = head;
        listView1.Items.Add(item);
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      grabber.Prev();
      RefreshList();
    }

    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void button3_Click(object sender, EventArgs e)
    {
      grabber.GetData(textBoxUser.Text);
      RefreshList();
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
      grabber.Settings.User = textBoxUser.Text;
    }

    private void button3_Click_1(object sender, EventArgs e)
    {
      grabber.Search(textBox3.Text);
      RefreshList();
    }

    private void button4_Click(object sender, EventArgs e)
    {
      DomainGetRequest req = new DomainGetRequest();
      propertyGrid1.SelectedObject = websrv.Domain_CountryGet(req);
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      websrv = new RadioTimeWebService();
    }

    private void textBox5_TextChanged(object sender, EventArgs e)
    {
      set.PartnerId = textBox5.Text;
    }

    private void Domain_AffiliateGet_Click(object sender, EventArgs e)
    {
      DomainGetRequest req = new DomainGetRequest();
      propertyGrid1.SelectedObject = websrv.Domain_AffiliateGet(req);
    }

    private void button5_Click(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        //StationNowPlayingListGetRequest req = new StationNowPlayingListGetRequest();
        //int i = 0;
        //int.TryParse(((RadioTimeOutline)listView1.SelectedItems[0].Tag).StationId, out i);
        //req.StationIds = new int[] { i };
        //propertyGrid1.SelectedObject = websrv.Station_NowPlayingListGet(req);
        RadioTimeNowPlaying playing = new RadioTimeNowPlaying();
        playing.Grabber = grabber;
        playing.Get(((RadioTimeOutline) listView1.SelectedItems[0].Tag).GuidId);
      }
    }

    private void button6_Click(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        ProgramGetRequest req = new ProgramGetRequest();
        int i = 0;
        int.TryParse(((RadioTimeOutline)listView1.SelectedItems[0].Tag).StationId, out i);
        req.ProgramId = i;
        propertyGrid1.SelectedObject = websrv.Program_Get(req);
      }
    }

    private void button7_Click(object sender, EventArgs e)
    {
        ChannelGetRequest req = new ChannelGetRequest();
        req.ChannelId = 57922;
        propertyGrid1.SelectedObject = websrv.Channel_ChannelItemGet(req);
    }

    private void button8_Click(object sender, EventArgs e)
    {
         
      if (listView1.SelectedItems.Count > 0)
      {
        try
        {
          FavoriteFolderUpdateRequest req = new FavoriteFolderUpdateRequest();
          int i = 0;
          int.TryParse(((RadioTimeOutline)listView1.SelectedItems[0].Tag).StationId, out i);
          req.ItemIds = new int[] { i };
          req.Identification = new Identification();
          req.Identification.UserName = textBoxUser.Text;
          req.Identification.PasswordKey = RadioTimeWebServiceHelper.HashMD5(textBoxPasswd.Text);
          propertyGrid1.SelectedObject = websrv.Favorite_StationListAdd(req);
        }
        catch(System.Web.Services.Protocols.SoapException ex)
        {
          MessageBox.Show(ex.Message);
        }
      }

    }


    // Tuner_Tune example
    private void button9_Click(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        TuneRequest req = new TuneRequest();
        //int i = 0;
        req.StationId = ((RadioTimeOutline)listView1.SelectedItems[0].Tag).StationIdAsInt;
        req.Identification = new Identification();
        req.Identification.UserName = textBoxUser.Text;
        req.Identification.PasswordKey = RadioTimeWebServiceHelper.HashMD5(textBoxPasswd.Text);
        propertyGrid1.SelectedObject = websrv.Tuner_Tune(req);
        MessageBox.Show("Test");
      }

    }

    private void button10_Click(object sender, EventArgs e)
    {
     
      RadioTimeStation station = new RadioTimeStation();
      station.Grabber = grabber;
      station.Get("s109401");
      //if (listView1.SelectedItems.Count > 0)
      //{
      //  StationGetRequest req = new StationGetRequest();
      //  int i = 0;
      //  req.StationId = ((RadioTimeOutline)listView1.SelectedItems[0].Tag).StationIdAsInt;
      //  propertyGrid1.SelectedObject = websrv.Station_Get(req);
      //  MessageBox.Show("Test");
      //}

    }

    private void button11_Click(object sender, EventArgs e)
    {
      WebServiceRequest req = new WebServiceRequest();
      propertyGrid1.SelectedObject = websrv.Channel_ChannelListGet(req);
    }

    private void button12_Click(object sender, EventArgs e)
    {
      WebServiceRequest req = new WebServiceRequest();
      req.Identification = new Identification();
      req.Identification.UserName = textBoxUser.Text;
      req.Identification.PasswordKey = RadioTimeWebServiceHelper.HashMD5(textBoxPasswd.Text);
      propertyGrid1.SelectedObject = websrv.Account_UserAuthenticate(req);
    }

  }
}