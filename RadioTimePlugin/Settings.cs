using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using RadioTimeOpmlApi;
using MediaPortal.Configuration;

namespace RadioTimePlugin
{
  public enum PlayerType
  {
    Audio = 0,
    Video = 1,
    Unknow = 2
  }

  public class Settings : RadioTimeSetting
  {
    public const int LOCAL_PRESETS_NUMBER = 10;
    
    static public RadioTimeStation NowPlayingStation { get; set; }
    static public RadioTimeNowPlaying NowPlaying { get; set; }
    static public string GuideId { get; set; }
    static public string GuideIdDescription { get; set; }
    static public Settings Instance { get; set; }
    public Dictionary<string, PlayerType> FormatPlayer { get; set; }
    public Dictionary<string, string> FormatNames { get; set; }
    public List<string> SearchHistory { get; set; }
    public List<string> ArtistSearchHistory { get; set; }

    public bool StartWithFastPreset { get; set; }
    public bool FirtsStart { get; set; }
    public string FolderId { get; set; }

    public List<RadioTimeOutline> PresetStations { get; set; }

    public new string Password { get; set; }
    public bool ShowPresets { get; set; }

    public Settings()
      : base()
    {
      FormatPlayer = new Dictionary<string, PlayerType>();
      FormatPlayer.Add("wma", PlayerType.Audio);
      FormatPlayer.Add("mp3", PlayerType.Audio);
      FormatPlayer.Add("aac", PlayerType.Audio);
      FormatPlayer.Add("real", PlayerType.Video);
      FormatPlayer.Add("flash", PlayerType.Video);
      FormatPlayer.Add("html", PlayerType.Unknow);
      FormatPlayer.Add("wmpro", PlayerType.Audio);
      FormatPlayer.Add("wmvoice", PlayerType.Audio);
      FormatPlayer.Add("wmvideo", PlayerType.Video);
      FormatPlayer.Add("ogg", PlayerType.Audio);
      FormatPlayer.Add("qt", PlayerType.Video);

      FormatNames = new Dictionary<string, string>();
      FormatNames.Add("wma", "WMAudio v8/9/10");
      FormatNames.Add("mp3", "standard MP3");
      FormatNames.Add("aac", "AAC and AAC+");
      FormatNames.Add("real", "Real Media");
      FormatNames.Add("flash", "RTMP (usually MP3 or AAC encoded)");
      FormatNames.Add("html", "Usually desktop players");
      FormatNames.Add("wmpro", "Windows Media Professional");
      FormatNames.Add("wmvoice", "Windows Media Voice");
      FormatNames.Add("wmvideo", "Windows Media Video v8/9/10");
      FormatNames.Add("ogg", "Ogg Vorbis");
      FormatNames.Add("qt", "Quicktime");

      SearchHistory = new List<string>();
      ArtistSearchHistory = new List<string>();
      PresetStations = new List<RadioTimeOutline>();
      StartWithFastPreset = false;
      FirtsStart = true;
    }

    private string pluginName;

    public string PluginName
    {
      get
      {
        if (!string.IsNullOrEmpty(pluginName))
        {
          return pluginName;
        }
        else
        {
          return "RadioTime";
        }
      }
      set { pluginName = value; }
    }

    private bool usevideo;

    public bool UseVideo
    {
      get { return usevideo; }
      set { usevideo = value; }
    }

    private bool jumpNowPlaying;

    public bool JumpNowPlaying
    {
      get { return jumpNowPlaying; }
      set { jumpNowPlaying = value; }
    }

    public void Save()
    {
      using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        xmlwriter.SetValueAsBool("radiotime", "mp3", Mp3);
        xmlwriter.SetValueAsBool("radiotime", "wma", Wma);
        xmlwriter.SetValueAsBool("radiotime", "real", Real);
        xmlwriter.SetValueAsBool("radiotime", "UseVideo", UseVideo);
        xmlwriter.SetValueAsBool("radiotime", "JumpNowPlaying", JumpNowPlaying); 
        xmlwriter.SetValue("radiotime", "user", User);
        xmlwriter.SetValue("radiotime", "encryptedPassword", PasswordUtility.EncryptData(Password, DataProtectionScope.LocalMachine));
        xmlwriter.RemoveEntry("radiotime", "password");
        xmlwriter.SetValueAsBool("radiotime", "showpresets", ShowPresets);
        xmlwriter.SetValue("radiotime", "pluginname", PluginName);
        xmlwriter.SetValue("radiotime", "FolderId", FolderId);
        xmlwriter.SetValueAsBool("radiotime", "StartWithFastPreset", StartWithFastPreset);

        string s = "";
        foreach (string history in SearchHistory)
        {
          s += history + "|";
        }
        xmlwriter.SetValue("radiotime", "searchHistory", s);

        s = "";
        foreach (string history in ArtistSearchHistory)
        {
          s += history + "|";
        }
        xmlwriter.SetValue("radiotime", "artistSearchHistory", s);

      }
    }

    public void Load()
    {
      using (
        MediaPortal.Profile.Settings xmlreader =
          new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        Mp3 = xmlreader.GetValueAsBool("radiotime", "mp3", true);
        Wma = xmlreader.GetValueAsBool("radiotime", "wma", true);
        Real = xmlreader.GetValueAsBool("radiotime", "real", false);
        ShowPresets = xmlreader.GetValueAsBool("radiotime", "showpresets", false);
        UseVideo = xmlreader.GetValueAsBool("radiotime", "UseVideo", false);
        JumpNowPlaying = xmlreader.GetValueAsBool("radiotime", "JumpNowPlaying", false);
        User = xmlreader.GetValueAsString("radiotime", "user", string.Empty);
        var encryptedPassword = xmlreader.GetValueAsString("radiotime", "encryptedPassword", string.Empty);
        if (!string.IsNullOrEmpty(encryptedPassword))
        {
            Password = PasswordUtility.DecryptData(encryptedPassword, DataProtectionScope.LocalMachine);
        }
        else
        {
            Password = xmlreader.GetValueAsString("radiotime", "password", string.Empty);
        }
        FolderId = xmlreader.GetValueAsString("radiotime", "FolderId", string.Empty);
        PluginName = xmlreader.GetValueAsString("radiotime", "pluginname", "RadioTime");
        StartWithFastPreset = xmlreader.GetValueAsBool("radiotime", "StartWithFastPreset", false);
        
        SearchHistory.Clear();
        ArtistSearchHistory.Clear();
        string searchs = xmlreader.GetValueAsString("radiotime", "searchHistory", "");
        if (!string.IsNullOrEmpty(searchs))
        {
          string[] array = searchs.Split('|');
          for (int i = 0; i < array.Length && i < 25; i++)
          {
            if (!string.IsNullOrEmpty(array[i]))
              SearchHistory.Add(array[i]);
          }
        }

        searchs = xmlreader.GetValueAsString("radiotime", "artistSearchHistory", "");
        if (!string.IsNullOrEmpty(searchs))
        {

          string[] array = searchs.Split('|');
          for (int i = 0; i < array.Length && i < 25; i++)
          {
            if (!string.IsNullOrEmpty(array[i]))
              ArtistSearchHistory.Add(array[i]);
          }

        }

       
        PartnerId = "41";
      }
    }
  }
}
