using System;
using System.Collections.Generic;
using System.Text;

namespace RadioTimeOpmlApi
{
  public class RadioTimeSetting
  {
    public RadioTimeSetting()
    {
      User = string.Empty;
      Mp3 = true;
      Wma = true;
      Real = false;
    }

    public RadioTimeSetting(RadioTimeSetting parent)
    {
      User = parent.User;
      Mp3 = parent.Mp3;
      Wma = parent.Wma;
      Real = parent.Real;
      Language = parent.Language;
      PartnerId = parent.PartnerId;
      Password = parent.Password;
    }

    public string Password { get; set; }

    public string User { get; set; }

    public bool Mp3 { get; set; }

    public bool Wma { get; set; }

    public bool Real { get; set; }

    public string PartnerId { get; set; }

    public string Language { get; set; }

    public string UpdateUrl(string sUrl)
    {
      if (sUrl.EndsWith(".aspx"))
      {
        sUrl += "?";
      }
      string opUser = "&username=";
      if (!string.IsNullOrEmpty(User))
      {
        int ipos = sUrl.IndexOf(opUser);
        if (ipos > 0)
        {
          sUrl = sUrl.Remove(ipos);
          sUrl= sUrl + opUser + User.Trim();
        }
        else
        {
          sUrl = sUrl + opUser + User.Trim();
        }
      } 
      return sUrl;
    }

    /// <summary>
    /// Gets the startup URL.
    /// </summary>
    /// <value>The startup URL.</value>
    public string StartupUrl
    {
      get
      {
        if (!string.IsNullOrEmpty(GetParamString()))
        {
          return "http://opml.radiotime.com/Index.aspx?" + GetParamString();
        }
        else
        {
          return "http://opml.radiotime.com/Index.aspx";
        }
      }
    }

    public string GenresUrl
    {
      get
      {
        if (!string.IsNullOrEmpty(GetParamString()))
        {
          return "http://opml.radiotime.com/Describe.ashx?c=genres&" + GetParamString();
        }
        else
        {
          return "http://opml.radiotime.com/Describe.ashx?c=genres";
        }
      }
    }

    /// <summary>
    /// Gets the presets URL.
    /// </summary>
    /// <value>The presets URL.</value>
    public string PresetsUrl
    {
      get
      {
        if (!string.IsNullOrEmpty(GetParamString()))
        {
          return "http://opml.radiotime.com/Browse.ashx?c=presets&" + GetParamString();
        }
        else
        {
          return "http://opml.radiotime.com/Browse.ashx?c=presets";
        }
      }
    }
	
    public string GetParamString()
    {
      string s = "";
      string ext = string.Empty;
      if (Mp3)
        ext += "mp3,";
      if (Wma)
        ext += "wma,";
      if (Real)
        ext += "real";
      s += "formats=wma,mp3,aac,real,flash,wmpro,wmvoice,wmvideo,ogg,qt";
      if (!string.IsNullOrEmpty(User.Trim()))
      {
        s += "&username=" + User;
      }
      if (!string.IsNullOrEmpty(PartnerId))
      {
        s += "&partnerID=" + PartnerId;
      }
      if (!string.IsNullOrEmpty(Language))
      {
        s += "&locale=" + Language;
      }
      return s;
    }
  }
}
