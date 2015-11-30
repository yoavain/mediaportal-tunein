using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Timers;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Util;
using RadioTimeOpmlApi;
using System.Globalization;
using System.Collections.Generic;

namespace RadioTimePlugin
{
    public class BaseGui : GUIWindow
    {
        public Queue downloaQueue = new Queue();
        public WebClient Client = new WebClient();
        public Timer updateStationLogoTimer = new Timer(0.2*1000);
        public DownloadFileObject curentDownlodingFile;
        public RadioTime grabber = new RadioTime();
        public Settings _setting = new Settings();
        private RadioTimeStation _station;
        private RadioTimeNowPlaying _nowPlaying;
        private RadioTimeShow _show;
        private string _currentFileName = string.Empty;
        private RadioTimeOutline _currentItem;
        protected const string noPresetFolders = "NOPRESETFOLDERS";
        protected static WaitCursor _waitCursor = null;
        public string PlayGuidId { get; set; }

        public BaseGui()
        {
            g_Player.PlayBackStarted += g_Player_PlayBackStarted;
            g_Player.PlayBackChanged += g_player_PlayBackChanged;
            g_Player.PlayBackEnded += g_player_PlayBackEnded;
            g_Player.PlayBackStopped += g_Player_PlayBackStopped;
        }

        protected virtual void doAdditionalStuffOnStarted()
        {
        }

        protected void g_player_PlayBackChanged(g_Player.MediaType type, int stoptime, string filename)
        {
            // Log.Debug("*** g_player_PlayBackChanged: "+filename);
            // ClearInternalVariables();
            ClearProps();
        }

        protected void g_Player_PlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
        {
            // Log.Debug("*** g_Player_PlayBackStopped: "+filename);
            ClearInternalVariables();
            ClearProps();
        }

        protected void g_player_PlayBackEnded(g_Player.MediaType type, string filename)
        {
            // Log.Debug("*** g_player_PlayBackEnded: "+filename);
            ClearInternalVariables();
            ClearProps();
        }

        protected void g_Player_PlayBackStarted(g_Player.MediaType type, string filename)
        {
            // Log.Debug("_currentItem is assigned: " + (_currentItem != null).ToString());
            // Log.Debug("_nowPlaying is assigned: " + (_nowPlaying != null).ToString());
            // Log.Debug("_station is assigned: " + (_station != null).ToString());
            // Log.Debug("g_player FILE    1: " + g_Player.CurrentFile);
            // Log.Debug("_currentFileName 2: " + _currentFileName);
            // Log.Debug("*** g_Player_PlayBackStarted: "+PlayGuidId);

            if (_currentItem == null || _nowPlaying == null || _station == null)
            {
              return;
            }

            if (g_Player.CurrentFile == _currentFileName || string.IsNullOrEmpty(g_Player.CurrentFile))
            {
              Settings.NowPlaying = _nowPlaying.Clone();
              Settings.NowPlayingStation = _station.Clone();
              if (_show != null)
              {
                Settings.NowPlayingShow = _show.Clone();
              }
              else
              {
                Settings.NowPlayingShow = null;
              }
              PlayGuidId = _station.GuideId;

              GUIPropertyManager.SetProperty("#Play.Current.Thumb", DownloadStationLogo(_currentItem));

              // GUIPropertyManager.SetProperty("#RadioTime.Play.Station", _nowPlaying.Name);
              // GUIPropertyManager.SetProperty("#RadioTime.Play.Station", _station.Name + " " + _station.Frequency + " " + _station.Band););
              GUIPropertyManager.SetProperty("#RadioTime.Play.Station", _station.Name + " " + _station.Frequency + 
                                              (!string.IsNullOrEmpty(_station.Name) && !string.IsNullOrEmpty(_station.Band) && _station.Name.IndexOf(_station.Band) < 0 ? " " + _station.Band : ""));
              // GUIPropertyManager.SetProperty("#RadioTime.Play.StationLogo", GetStationLogoFileName(nowPlaying.Image));
              GUIPropertyManager.SetProperty("#RadioTime.Play.Description", _station.Description);
              GUIPropertyManager.SetProperty("#RadioTime.Play.Station.Description", _station.Description);
              GUIPropertyManager.SetProperty("#RadioTime.Play.Slogan", _station.Slogan);
              GUIPropertyManager.SetProperty("#RadioTime.Play.Language", _station.Language);
              GUIPropertyManager.SetProperty("#RadioTime.Play.GuidId", PlayGuidId);
              //
              UpdateProps();
              //
              GUIPropertyManager.SetProperty("#RadioTime.Play.Image", DownloadStationLogo(_currentItem));
              //
              doAdditionalStuffOnStarted();
              //
              ClearInternalVariables();
            }
        }

        public void UpdatePlayProps()
        {
          // Log.Debug("*** UpdatePlayProps");
          try
          {
            if (!g_Player.Playing)
            {
              return;
            }

            PlayGuidId = (string.IsNullOrEmpty(PlayGuidId)) ? GUIPropertyManager.GetProperty("#RadioTime.Play.GuidId") : PlayGuidId;
            if (string.IsNullOrEmpty(PlayGuidId))
            {
              return;
            }

            _station = new RadioTimeStation();
            _station.Grabber = grabber;
            _station.Get(PlayGuidId);

            if (!string.IsNullOrEmpty(PlayGuidId) && _station.IsAvailable)
            {
              //var nowPlaying = Settings.NowPlaying;
              _nowPlaying = new RadioTimeNowPlaying();
              _nowPlaying.Grabber = grabber;
              _nowPlaying.Get(PlayGuidId, _station.HasSong);
              //
              if (_nowPlaying.IsShow && !string.IsNullOrEmpty(_nowPlaying.ShowGuidId))
              {
                _show = new RadioTimeShow();
                _show.Grabber = grabber;
                _show.Get(_nowPlaying.ShowGuidId) ;
              }
              //
              Settings.NowPlaying = _nowPlaying.Clone();
              Settings.NowPlayingStation = _station.Clone();
              if (_show != null)
              {
                Settings.NowPlayingShow = _show.Clone();
              }
              else
              {
                Settings.NowPlayingShow = null;
              }
              //
              UpdateProps();
            }
          }
          catch (Exception ex)
          {
            Log.Debug("UpdatePlayProps: " + ex.Message);
          }

        }

        protected void UpdateProps()
        {
          // Log.Debug("*** UpdateProps");
          if (_nowPlaying == null || _station == null)
          {
            return;
          }

          try
          {
            //
            GUIPropertyManager.SetProperty("#Play.Current.Thumb", DownloadStationLogo((!string.IsNullOrEmpty(_nowPlaying.Image) ? _nowPlaying.Image : _station.Logo),
                                                                                      (!string.IsNullOrEmpty(_nowPlaying.Image) ? _nowPlaying.Name : _station.Name)));
            // GUIPropertyManager.SetProperty("#RadioTime.Play.Description", (!string.IsNullOrEmpty(_nowPlaying.Description) ? _nowPlaying.Description : _station.Description));
            GUIPropertyManager.SetProperty("#RadioTime.Play.Description", _nowPlaying.Description);
            GUIPropertyManager.SetProperty("#RadioTime.Play.Station.Description", _station.Description);
            // GUIPropertyManager.SetProperty("#RadioTime.Play.Location", (!string.IsNullOrEmpty(_nowPlaying.Location) ? _nowPlaying.Location : _station.Location));
            GUIPropertyManager.SetProperty("#RadioTime.Play.Location", _station.Location);
            GUIPropertyManager.SetProperty("#RadioTime.Play.Duration", _nowPlaying.Duration.ToString());
            GUIPropertyManager.SetProperty("#duration", ToMinutes(_nowPlaying.Duration.ToString()));
            //
            if(_station.HasSong)
            {
              if (!string.IsNullOrEmpty(_station.Artist) && _station.Artist.Equals(_station.Artist.ToLower(), StringComparison.CurrentCulture))
              {
                _station.Artist = _station.Artist.ToUpper().Trim();
              }

              if (string.IsNullOrEmpty(_station.Album) && !string.IsNullOrEmpty(_station.Artist) && !string.IsNullOrEmpty(_station.Song))
              {
                _station.Album = UtilsFanartHandler.GetLastFMAlbum(_station.Artist, _station.Song);
                Log.Debug("Album from FH: Artist: " + _station.Artist + " Track: " + _station.Song + " Album: " + _station.Album);
              }
            }
            //
            GUIPropertyManager.SetProperty("#RadioTime.Play.HasSong", (_station.HasSong ? "true" : string.Empty));
            GUIPropertyManager.SetProperty("#RadioTime.Play.Artist", (_station.HasSong ? _station.Artist : string.Empty));
            GUIPropertyManager.SetProperty("#RadioTime.Play.Album", (_station.HasSong ? _station.Album : string.Empty));
            GUIPropertyManager.SetProperty("#RadioTime.Play.Song", (_station.HasSong ? _station.Song : string.Empty));
            //
            string strGenres = GetDistinct(_station.Genres) ;
            if (_nowPlaying.IsShow && _show != null)
            {
              string strShowGenres = GetDistinct(_show.Genres);
              if (!string.IsNullOrEmpty(strShowGenres)) 
              {
                strGenres = strShowGenres;
              }
            }
            //
            var badTitle = GUIPropertyManager.GetProperty("#Play.Current.Title");
            if (!string.IsNullOrEmpty(badTitle))
            {
              int streamUrlIndex = badTitle.IndexOf("';");
              if (streamUrlIndex == badTitle.Length - 2)
              {
                badTitle = badTitle.Substring(0, streamUrlIndex);
              }
              streamUrlIndex = badTitle.IndexOf(";");
              if (streamUrlIndex == badTitle.Length - 1)
              {
                badTitle = badTitle.Substring(0, streamUrlIndex);
              }
              GUIPropertyManager.SetProperty("#Play.Current.Title", badTitle);
            }
            //
            if(_station.HasSong)
            {
              var curArtist = GUIPropertyManager.GetProperty("#Play.Current.Artist");
              var curAlbum = GUIPropertyManager.GetProperty("#Play.Current.Album");
              var curTitle = GUIPropertyManager.GetProperty("#Play.Current.Title");
              if (string.IsNullOrEmpty(curArtist))
              // if (!string.IsNullOrEmpty(_station.Artist))
              {
                  GUIPropertyManager.SetProperty("#Play.Current.Artist", _station.Artist);
                  curArtist = _station.Artist;
              }
              if (string.IsNullOrEmpty(curAlbum))
              // if (!string.IsNullOrEmpty(_station.Album))
              {
                  GUIPropertyManager.SetProperty("#Play.Current.Album", _station.Album);
                  curAlbum = _station.Album;
              }
              if (string.IsNullOrEmpty(curTitle))
              // if (!string.IsNullOrEmpty(_station.Song))
              {
                  GUIPropertyManager.SetProperty("#Play.Current.Title", _station.Song);
                  curTitle = _station.Song;
              }
              // Fix for Album name = NowPlaying Name, replace with Real Album Name from FH
              if ((!string.IsNullOrEmpty(_station.Name) || !string.IsNullOrEmpty(_nowPlaying.Name)) && 
                  !string.IsNullOrEmpty(curArtist) && 
                  !string.IsNullOrEmpty(curAlbum) && 
                  !string.IsNullOrEmpty(curTitle))
              {
                // Log.Debug("====================================================================================================================================");
                // Log.Debug("=== "+_station.Artist + " - " + curArtist + " - " + _station.Artist.Equals(curArtist, StringComparison.CurrentCultureIgnoreCase));
                // Log.Debug("=== "+_station.Song + " - " + curTitle + " - " + _station.Song.Equals(curTitle, StringComparison.CurrentCultureIgnoreCase));
                // if (!string.IsNullOrEmpty(_station.Name))
                // Log.Debug("=== "+_station.Name + " - " + curAlbum + " - " + _station.Name.Equals(curAlbum, StringComparison.CurrentCultureIgnoreCase));
                // if (!string.IsNullOrEmpty(_station.Name))
                // Log.Debug("=== "+_nowPlaying.Name + " - " + curAlbum + " - " + _nowPlaying.Name.Equals(curAlbum, StringComparison.CurrentCultureIgnoreCase));

                if (!string.IsNullOrEmpty(_nowPlaying.Name) &&
                   _station.Artist.Equals(curArtist, StringComparison.CurrentCultureIgnoreCase) && 
                   _station.Song.Equals(curTitle, StringComparison.CurrentCultureIgnoreCase) && 
                   _station.Name.Equals(curAlbum, StringComparison.CurrentCultureIgnoreCase))
                {
                  GUIPropertyManager.SetProperty("#Play.Current.Album", _station.Album);  
                }
                else if (!string.IsNullOrEmpty(_nowPlaying.Name) &&
                  _station.Artist.Equals(curArtist, StringComparison.CurrentCultureIgnoreCase) && 
                  _station.Song.Equals(curTitle, StringComparison.CurrentCultureIgnoreCase) && 
                  _nowPlaying.Name.Equals(curAlbum, StringComparison.CurrentCultureIgnoreCase))
                {
                  GUIPropertyManager.SetProperty("#Play.Current.Album", _station.Album);  
                }
                // Log.Debug("=== "+GUIPropertyManager.GetProperty("#Play.Current.Album"));
                // Log.Debug("====================================================================================================================================");
              }
            } 
            else if (_show != null)
            {
              if (string.IsNullOrEmpty(GUIPropertyManager.GetProperty("#Play.Current.Artist")))
              {
                GUIPropertyManager.SetProperty("#Play.Current.Artist", _show.Name);
              }
              if (string.IsNullOrEmpty(GUIPropertyManager.GetProperty("#Play.Current.Album")))
              {
                GUIPropertyManager.SetProperty("#Play.Current.Album", _show.Hosts);
              }
              if (string.IsNullOrEmpty(GUIPropertyManager.GetProperty("#Play.Current.Title")))
              {
                GUIPropertyManager.SetProperty("#Play.Current.Title", _show.Description);
              }
              if (!string.IsNullOrEmpty(_show.Location))
              {
                GUIPropertyManager.SetProperty("#RadioTime.Play.Location", _show.Location);
              }
              if (!string.IsNullOrEmpty(_show.Logo))
              {
                GUIPropertyManager.SetProperty("#Play.Current.Thumb", DownloadStationLogo(_show.Logo, _show.Name));
              }
            }
            else
            {
              if (string.IsNullOrEmpty(GUIPropertyManager.GetProperty("#Play.Current.Artist")))
              {
                GUIPropertyManager.SetProperty("#Play.Current.Artist", _nowPlaying.Description);
              }
            }
            //
            if (!string.IsNullOrEmpty(GUIPropertyManager.GetProperty("#Play.Current.Genre")))
            {
              GUIPropertyManager.SetProperty("#Play.Current.Genre", strGenres);
            }
            if (string.IsNullOrEmpty(GUIPropertyManager.GetProperty("#Play.Current.Track")))
            {
              GUIPropertyManager.SetProperty("#Play.Current.Track", GUIPropertyManager.GetProperty("#Play.Current.Artist") + " - " + 
                                                                    GUIPropertyManager.GetProperty("#Play.Current.Album") + " - " + 
                                                                    GUIPropertyManager.GetProperty("#Play.Current.Title"));
            }
            //
            if (_currentItem != null)
            {
              GUIPropertyManager.SetProperty("#Play.Current.Rating", (_currentItem.ReliabilityIdAsInt/10).ToString());
              GUIPropertyManager.SetProperty("#Play.Current.FileType", _currentItem.Formats);
              GUIPropertyManager.SetProperty("#Play.Current.BitRate", _currentItem.Bitrate);

              if (_setting.FormatNames.ContainsKey(_currentItem.Formats))
              {
                GUIPropertyManager.SetProperty("#RadioTime.Play.Format", _setting.FormatNames[_currentItem.Formats]);
              }
              else
              {
                GUIPropertyManager.SetProperty("#RadioTime.Play.Format", string.Empty);
              }
            }
            //
            Log.Debug("*** Station: "+"Name: "+_station.Name+" CallSign: "+_station.CallSign+" Language: "+_station.Language+" Logo: "+_station.Logo);
            Log.Debug("*** Station: "+"Location: "+_station.Location+" Frequency: "+_station.Frequency+" Band: "+_station.Band);
            Log.Debug("*** Station: "+"Slogan: "+_station.Slogan+" Adresss: "+_station.Adresss);
            Log.Debug("*** Station: "+"HasSong: "+_station.HasSong.ToString());
            Log.Debug("*** Station: "+"Artist: ["+_station.Artist+"] Album: ["+_station.Album+"] Song: ["+_station.Song+"]");
            Log.Debug("*** Station: "+"Description: "+_station.Description);
            Log.Debug("*** Station: "+"Genres: "+strGenres);
            /*
            foreach (RadioTimeOutline Similar in _station.Similar)
            {
              Log.Debug("*** Station: "+"Similar: - "+Similar.Text);
            }
            */
            //
            Log.Debug("*** NowPlaying: "+"GuidId: "+_nowPlaying.GuidId+" PresetId: "+_nowPlaying.PresetId);
            Log.Debug("*** NowPlaying: "+"Name: "+_nowPlaying.Name+" Image: "+_nowPlaying.Image+" ShowImage: "+_nowPlaying.ShowImage);
            Log.Debug("*** NowPlaying: "+"Description: "+_nowPlaying.Description+" Location: "+_nowPlaying.Location);
            Log.Debug("*** NowPlaying: "+"Duration: "+_nowPlaying.Duration+" Remains: "+_nowPlaying.Remains);
            //
            if (_show != null)
            {
              Log.Debug("*** Show: "+"GuideId: "+_show.GuideId);
              Log.Debug("*** Show: "+"Name: "+_show.Name + " Hosts: "+_show.Hosts);
              Log.Debug("*** Show: "+"IsPreset: "+_show.IsPreset);
              Log.Debug("*** Show: "+"IsEvent: "+_show.IsEvent);
              Log.Debug("*** Show: "+"HasTopic: "+_show.HasTopic);   
              Log.Debug("*** Show: "+"Language: "+_show.Language);
              Log.Debug("*** Show: "+"Logo: "+_show.Logo);
              Log.Debug("*** Show: "+"Location: "+_show.Location);
              Log.Debug("*** Show: "+"Description: "+_show.Description);
            }
            //
            Log.Debug("*** #Play.Current.Thumb: "+GUIPropertyManager.GetProperty("#Play.Current.Thumb"));
            Log.Debug("*** #Play.Current.Artist: "+GUIPropertyManager.GetProperty("#Play.Current.Artist"));
            Log.Debug("*** #Play.Current.Album: "+GUIPropertyManager.GetProperty("#Play.Current.Album"));
            Log.Debug("*** #Play.Current.Title: "+GUIPropertyManager.GetProperty("#Play.Current.Title"));
            Log.Debug("*** #Play.Current.Track: "+GUIPropertyManager.GetProperty("#Play.Current.Track"));
            Log.Debug("*** #Play.Current.Year: "+GUIPropertyManager.GetProperty("#Play.Current.Year"));
            Log.Debug("*** #Play.Current.Rating: "+GUIPropertyManager.GetProperty("#Play.Current.Rating"));
            Log.Debug("*** #Play.Current.BitRate: "+GUIPropertyManager.GetProperty("#Play.Current.BitRate"));
            Log.Debug("*** #Play.Current.FileType: "+GUIPropertyManager.GetProperty("#Play.Current.FileType"));
          }
          catch (Exception ex)
          {
            Log.Debug("UpdateProps: " + ex.Message);
          }
        }

        protected void ClearInternalVariables()
        {
            _currentFileName = string.Empty;
            _currentItem = null;
            _nowPlaying = null;
            _station = null;
            _show = null;
        }

        protected void ClearProps()
        {
            Settings.NowPlaying = new RadioTimeNowPlaying();
            Settings.NowPlayingStation = new RadioTimeStation();
            Settings.NowPlayingShow = new RadioTimeShow();
            GUIPropertyManager.SetProperty("#RadioTime.Play.Station", string.Empty);
            GUIPropertyManager.SetProperty("#RadioTime.Play.StationLogo", string.Empty);
            GUIPropertyManager.SetProperty("#RadioTime.Play.Duration", string.Empty);
            GUIPropertyManager.SetProperty("#RadioTime.Play.Description", string.Empty);
            GUIPropertyManager.SetProperty("#RadioTime.Play.Location", string.Empty);
            GUIPropertyManager.SetProperty("#RadioTime.Play.Slogan", string.Empty);
            GUIPropertyManager.SetProperty("#RadioTime.Play.Language", string.Empty);
            GUIPropertyManager.SetProperty("#RadioTime.Play.Format", string.Empty);
            GUIPropertyManager.SetProperty("#RadioTime.Play.Image", string.Empty);
            GUIPropertyManager.SetProperty("#RadioTime.Play.HasSong", string.Empty);
            GUIPropertyManager.SetProperty("#RadioTime.Play.Artist", string.Empty);
            GUIPropertyManager.SetProperty("#RadioTime.Play.Album", string.Empty);
            GUIPropertyManager.SetProperty("#RadioTime.Play.Song", string.Empty);
            GUIPropertyManager.SetProperty("#RadioTime.Play.GuidId", string.Empty);
            PlayGuidId = string.Empty;
        }

        protected void ClearPlayProps()
        {
            GUIPropertyManager.SetProperty("#Play.Current.Thumb", string.Empty);
            GUIPropertyManager.SetProperty("#Play.Current.Artist", string.Empty);
            GUIPropertyManager.SetProperty("#Play.Current.Title", string.Empty);
            GUIPropertyManager.SetProperty("#Play.Current.Track", string.Empty);
            GUIPropertyManager.SetProperty("#Play.Current.Album", string.Empty);
            GUIPropertyManager.SetProperty("#Play.Current.Year", string.Empty);
            GUIPropertyManager.SetProperty("#Play.Current.BitRate", string.Empty);
            GUIPropertyManager.SetProperty("#Play.Current.FileType", string.Empty);
            GUIPropertyManager.SetProperty("#Play.Current.Rating", "0");
        }

        public void UpdateSelectedLabels(RadioTimeOutline radioItem)
        {
            GUIPropertyManager.SetProperty("#RadioTime.Selected.NowPlaying", radioItem.CurrentTrack);
            string subtext = radioItem.Subtext;
            if (GUIPropertyManager.GetProperty("#RadioTime.Play.HasSong").Equals("true"))
            {
              PlayGuidId = (string.IsNullOrEmpty(PlayGuidId)) ? GUIPropertyManager.GetProperty("#RadioTime.Play.GuidId") : PlayGuidId;
              if (!string.IsNullOrEmpty(PlayGuidId) && radioItem.GuidId.Equals(PlayGuidId))
              {
                string artist = GUIPropertyManager.GetProperty("#Play.Current.Artist");
                // string album = GUIPropertyManager.GetProperty("#Play.Current.Album");
                string track = GUIPropertyManager.GetProperty("#Play.Current.Title");
                if (!string.IsNullOrEmpty(artist))
                {
                  // subtext = string.Empty + artist + (!string.IsNullOrEmpty(album) ? " - " : "") + album + (!string.IsNullOrEmpty(track) ? " - " : "") + track; // FH not compatible with Artist - Album - Track
                  subtext = string.Empty + artist + (!string.IsNullOrEmpty(track) ? " - " : "") + track;
                }
              }
            }
            GUIPropertyManager.SetProperty("#RadioTime.Selected.Subtext", subtext);
            GUIPropertyManager.SetProperty("#RadioTime.Selected.Reliability", (radioItem.ReliabilityIdAsInt/10).ToString());

            GUIPropertyManager.SetProperty("#RadioTime.Selected.Logo", string.Empty);
            GUIPropertyManager.SetProperty("#RadioTime.Selected.Logo", DownloadStationLogo(radioItem));

            if (_setting.FormatNames.ContainsKey(radioItem.Formats))
                GUIPropertyManager.SetProperty("#RadioTime.Selected.Format", _setting.FormatNames[radioItem.Formats]);
            else
                GUIPropertyManager.SetProperty("#RadioTime.Selected.Format", string.Empty);
            Process();
        }

        public void OnDownloadTimedEvent(object source, ElapsedEventArgs e)
        {
            if (!Client.IsBusy && downloaQueue.Count > 0)
            {
                curentDownlodingFile = (DownloadFileObject) downloaQueue.Dequeue();
                Client.DownloadFileAsync(new Uri(curentDownlodingFile.Url), Path.GetTempPath() + @"\station.png");
            }
        }

        public static string GetLocalImageFileName(string strURL)
        {
            if (strURL == "")
                return string.Empty;
            var url = String.Format("radiotime-{0}.png", Utils.EncryptLine(strURL));
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), url);
            ;
        }

        public string ToMinutes(string minutes)
        {
            if (string.IsNullOrEmpty(minutes))
                return " ";

            var min = 0;
            int.TryParse(minutes, out min);
            var hh = (min/3600);
            min = min - (hh*3600);
            var mm = (min/60);
            min = min - (mm*60);
            return string.Format(((hh > 0) ? "{2}:{0}:{1}" : "{0}:{1}"), mm.ToString("00"), min.ToString("00"), hh.ToString());
        }

        /// <summary>
        /// Does the play.
        /// </summary>
        /// <param name="item">The item.</param>
        public void DoPlay(RadioTimeOutline item)
        {
            ShowWaitCursor();
            try
            {
                _station = null;
                _nowPlaying = null;
                _show = null;
                _currentItem = null;

                if (item == null || string.IsNullOrEmpty(item.GuidId))
                {
                    ErrMessage(Translation.StationNotAvaiable);
                    return;
                }
                PlayGuidId = item.GuidId ;
                _currentItem = item.Clone();

                //RadioTimeStation station = Settings.NowPlayingStation;
                _station = new RadioTimeStation();
                _station.Grabber = grabber;
                _station.Get(PlayGuidId);

                if (_station.IsAvailable)
                {
                    //var nowPlaying = Settings.NowPlaying;
                    _nowPlaying = new RadioTimeNowPlaying();
                    _nowPlaying.Grabber = grabber;
                    _nowPlaying.Get(PlayGuidId, _station.HasSong);

                    if (_nowPlaying.IsShow && !string.IsNullOrEmpty(_nowPlaying.ShowGuidId))
                    {
                        _show = new RadioTimeShow();
                        _show.Grabber = grabber;
                        _show.Get(_nowPlaying.ShowGuidId) ;
                    }

                    var playerType = PlayerType.Video;
                    if (_setting.FormatPlayer.ContainsKey(item.Formats))
                        playerType = _setting.FormatPlayer[item.Formats];

                    try
                    {
                        var playList = new PlayList();
                        //if (item.Url.ToLower().Contains(".pls") || item.Url.ToLower().Contains(".m3u") || item.Url.ToLower().Contains(".asx"))
                        {
                            var TargetFile = Path.GetTempFileName();
                            var client = new WebClient();
                            try
                            {
                                if (item.Url.ToLower().Contains(".pls"))
                                {
                                    client.DownloadFile(item.Url, TargetFile);
                                    IPlayListIO loader = new PlayListPLSEIO();
                                    loader.Load(playList, TargetFile);
                                }
                                else if (item.Url.ToLower().Contains(".asx"))
                                {
                                    client.DownloadFile(item.Url, TargetFile);
                                    IPlayListIO loader = new PlayListASXIO();
                                    loader.Load(playList, TargetFile);
                                }
                                else
                                {
                                    client.DownloadFile(item.Url, TargetFile);
                                    IPlayListIO loader = new PlayListM3uIO();
                                    loader.Load(playList, TargetFile);
                                }
                            }
                            finally
                            {
                                client.Dispose();
                                File.Delete(TargetFile);
                            }

                            //if (playList.Count > 0 && playList[0].FileName.ToLower().StartsWith("http") && playList[0].FileName.ToLower().Contains(".m3u"))
                            //{
                            //  client.DownloadFile(playList[0].FileName, TargetFile);
                            //  IPlayListIO loader1 = new PlayListM3uIO();
                            //  loader1.Load(playList, TargetFile);
                            //  File.Delete(TargetFile);
                            //}

                            TargetFile = Path.GetTempFileName();
                            client = new WebClient();
                            try
                            {
                                if (playList.Count > 0 && playList[0].FileName.ToLower().Contains(".pls"))
                                {
                                    client.DownloadFile(playList[0].FileName, TargetFile);
                                    IPlayListIO loader1 = new PlayListPLSEIO();
                                    loader1.Load(playList, TargetFile);
                                }

                                if (playList.Count > 0 && playList[0].FileName.ToLower().Contains(".asx"))
                                {
                                    client.DownloadFile(playList[0].FileName, TargetFile);
                                    IPlayListIO loader1 = new PlayListASXIO();
                                    loader1.Load(playList, TargetFile);
                                }

                                if (playList.Count > 0 && playList[0].FileName.ToLower().Contains(".m3u"))
                                {
                                    client.DownloadFile(playList[0].FileName, TargetFile);
                                    IPlayListIO loader1 = new PlayListM3uIO();
                                    loader1.Load(playList, TargetFile);
                                    if (playList.Count == 0)
                                    {
                                        IPlayListIO loader2 = new PlayListPLSEIO();
                                        loader2.Load(playList, TargetFile);
                                    }
                                }
                            }
                            finally
                            {
                                client.Dispose();
                                File.Delete(TargetFile);
                            }
                        }

                        if (playList.Count > 0)
                            _currentFileName = playList[0].FileName;
                        else
                            _currentFileName = item.Url;

                        switch (playerType)
                        {
                            case PlayerType.Audio:
                                ClearPlayProps();
                                g_Player.PlayAudioStream(_currentFileName);
                                return;
                            case PlayerType.Video:
                                // test if the station have tv group
                                ClearPlayProps();
                                if (item.GenreId == "g260" || item.GenreId == "g83" || item.GenreId == "g374" ||
                                    item.GenreId == "g2769")
                                    g_Player.PlayVideoStream(_currentFileName);
                                else
                                    g_Player.Play(_currentFileName, g_Player.MediaType.Unknown);
                                return;
                            case PlayerType.Unknow:
                                return;
                            default:
                                return;
                        }
                        // moved to PLAYBACKSTARTED EVENT
                        //if  (isPlaying && g_Player.CurrentFile == playList[0].FileName)
                    }
                    catch (Exception exception)
                    {
                        _currentItem = null;
                        ErrMessage(string.Format(Translation.PlayError, exception.Message));
                        return;
                    }
                }
            }
            finally
            {
                HideWaitCursor();
            }
            ErrMessage(Translation.StationNotAvaiable);
            return;
        }

        public void ErrMessage(string langid)
        {
            var dlgOK = (GUIDialogOK) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_OK);
            if (dlgOK != null)
            {
                dlgOK.SetHeading(Translation.Message);
                dlgOK.SetLine(1, langid);
                dlgOK.SetLine(2, "");
                dlgOK.DoModal(GetID);
            }
        }

        internal static void SetProperty(string property, string value)
        {
            if (property == null)
                return;

            //// If the value is empty always add a space
            //// otherwise the property will keep 
            //// displaying it's previous value
            if (String.IsNullOrEmpty(value))
                value = " ";

            GUIPropertyManager.SetProperty(property, value);
        }

        public string GetStationLogoFileName(RadioTimeOutline radioItem)
        {
            if (string.IsNullOrEmpty(radioItem.Image))
                return string.Empty;
            return Utils.GetCoverArtName(Thumbs.Radio, radioItem.Text);
        }

        public string GetStationLogoFileName(string imgUrl, string name)
        {
            if (string.IsNullOrEmpty(imgUrl))
                return string.Empty;
            return Utils.GetCoverArtName(Thumbs.Radio, name);
        }

        public void LoadLocalPresetStations()
        {
            _setting.PresetStations.Clear();
            for (var i = 0; i < Settings.LocalPresetsNumber; i++)
            {
                _setting.PresetStations.Add(new RadioTimeOutline());
            }
            var ii = 0;
            ShowWaitCursor();
            try
            {
                Process();
                grabber.GetData(_setting.PresetsUrl, false, false, Translation.Presets);

                var folderCount = 0;
                foreach (var body in grabber.Body)
                {
                    if (body.Type == RadioTimeOutline.OutlineType.link)
                        folderCount++;
                }

                if (folderCount == 0)
                {
                    GUIPropertyManager.SetProperty("#RadioTime.Presets.Folder.Name", string.Empty);
                }
                else
                {
                    var i = 0;
                    foreach (var body in grabber.Body)
                    {
                        if (body.GuidId == _setting.FolderId)
                            ii = i;
                        i++;
                    }
                    GUIPropertyManager.SetProperty("#RadioTime.Presets.Folder.Name", grabber.Body[ii].Text);
                    grabber.GetData(grabber.Body[ii].Url, false, false);
                }

                foreach (var body in grabber.Body)
                {
                    if (!string.IsNullOrEmpty(body.PresetNumber) &&
                        body.PresetNumberAsInt - 1 < Settings.LocalPresetsNumber)
                    {
                        _setting.PresetStations[body.PresetNumberAsInt - 1] = body;
                    }
                    Process();
                }

                PopulatePresetsLabels();
            }
            finally
            {
                HideWaitCursor();
            }
        }

        public string DownloadStationLogo(string Image, string Name)
        {
            var localFile = GetStationLogoFileName(Image,  Name);
            if (!File.Exists(localFile) && !string.IsNullOrEmpty(Image))
            {
                downloaQueue.Enqueue(new DownloadFileObject(localFile, Image));
            }
            return localFile;
        }

        public string DownloadStationLogo(RadioTimeOutline radioItem)
        {
            var localFile = GetStationLogoFileName(radioItem);
            if (!File.Exists(localFile) && !string.IsNullOrEmpty(radioItem.Image))
            {
                // downloaQueue.Enqueue(new DownloadFileObject(localFile, radioItem.Image.Replace("q.png", ".png")));
                downloaQueue.Enqueue(new DownloadFileObject(localFile, radioItem.Image));
            }
            return localFile;
        }

        private void PopulatePresetsLabels()
        {
            for (var i = 0; i < Settings.LocalPresetsNumber; i++)
            {
                if (string.IsNullOrEmpty(_setting.PresetStations[i].Text))
                    GUIPropertyManager.SetProperty(string.Format("#RadioTime.Presets.{0}.Name", i + 1),
                        string.Format("<{0}>", Translation.Empty));
                else
                    GUIPropertyManager.SetProperty(string.Format("#RadioTime.Presets.{0}.Name", i + 1),
                        _setting.PresetStations[i].Text);
            }
        }

        public string GetPresetFolder()
        {
            var tempGrabber = new RadioTime();
            tempGrabber.Settings = _setting;
            tempGrabber.GetData(_setting.PresetsUrl, false, Translation.Presets);

            var dlg = (GUIDialogMenu) GUIWindowManager.GetWindow((int) Window.WINDOW_DIALOG_MENU);
            if (dlg == null)
                return null;
            dlg.Reset();
            dlg.SetHeading(Translation.SelectPresetFolder);

            var canShow = false;
            foreach (var body in tempGrabber.Body)
            {
                if (body.Type == RadioTimeOutline.OutlineType.link)
                {
                    dlg.Add(body.Text);
                    canShow = true;
                }
            }

            if (canShow)
            {
                dlg.DoModal(GetID);
                if (dlg.SelectedId == -1)
                    return null;
                return tempGrabber.Body[dlg.SelectedId - 1].GuidId;
            }
            else
                return noPresetFolders;
        }

        internal static string GetDistinct (List<RadioTimeOutline> Input)
        {
          string result = string.Empty;

          if (Input == null || Input.Count <= 0)
            return result ;

          Hashtable ht = new Hashtable();
          try
          {
            string key = string.Empty;
            foreach (RadioTimeOutline Outline in Input)
            {
              key = Outline.Text.ToLower().Trim();
              if (!ht.Contains(key))
              {
                result = result + (!string.IsNullOrEmpty(result) ? ", " : "") + Outline.Text.Trim();
                ht.Add(key, key);
              }
            }
            if (ht != null)
              ht.Clear();
            ht = null;
          }
          catch (Exception ex)
          {
            result = string.Empty;
            Log.Error("GetDistinct: " + ex.ToString());
          }
          return result ;
        }

        public static void ShowWaitCursor()
        {
            //_waitCursor = new WaitCursor();
        }

        public static void HideWaitCursor()
        {
            if (_waitCursor != null)
            {
                _waitCursor.Dispose();
                _waitCursor = null;
            }
        }
    }
}