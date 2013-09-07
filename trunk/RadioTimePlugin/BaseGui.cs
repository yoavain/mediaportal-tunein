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
        private string _currentFileName = string.Empty;
        private RadioTimeOutline _currentItem;
        protected const string noPresetFolders = "NOPRESETFOLDERS";
        protected static WaitCursor _waitCursor = null;

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
            //ClearInternalVariables();
            ClearProps();
        }

        protected void g_Player_PlayBackStopped(g_Player.MediaType type, int stoptime, string filename)
        {
            ClearInternalVariables();
            ClearProps();
        }

        protected void g_player_PlayBackEnded(g_Player.MediaType type, string filename)
        {
            ClearInternalVariables();
            ClearProps();
        }

        protected void g_Player_PlayBackStarted(g_Player.MediaType type, string filename)
        {
            //Log.Debug("_currentItem is assigned: " + (_currentItem != null).ToString());
            //Log.Debug("_nowPlaying is assigned: " + (_nowPlaying != null).ToString());
            //Log.Debug("_station is assigned: " + (_station != null).ToString());
            //Log.Debug("g_player FILE    1: " + g_Player.CurrentFile);
            //Log.Debug("_currentFileName 2: " + _currentFileName);

            if (_currentItem == null || _nowPlaying == null || _station == null)
                return;

            if (g_Player.CurrentFile == _currentFileName || string.IsNullOrEmpty(g_Player.CurrentFile))
            {
                Settings.NowPlaying = _nowPlaying.Clone();
                Settings.NowPlayingStation = _station.Clone();

                GUIPropertyManager.SetProperty("#Play.Current.Thumb", DownloadStationLogo(_currentItem));

                GUIPropertyManager.SetProperty("#RadioTime.Play.Station", _nowPlaying.Name);
                //GUIPropertyManager.SetProperty("#RadioTime.Play.StationLogo", GetStationLogoFileName(nowPlaying.Image));
                GUIPropertyManager.SetProperty("#RadioTime.Play.Duration", _nowPlaying.Duration.ToString());
                GUIPropertyManager.SetProperty("#RadioTime.Play.Description", _nowPlaying.Description);
                GUIPropertyManager.SetProperty("#duration", ToMinutes(_nowPlaying.Duration.ToString()));
                GUIPropertyManager.SetProperty("#RadioTime.Play.Location", _nowPlaying.Location);
                GUIPropertyManager.SetProperty("#RadioTime.Play.Slogan", _station.Slogan);
                GUIPropertyManager.SetProperty("#RadioTime.Play.Language", _station.Slogan);

                var titleString = _nowPlaying.Name;
                if (!string.IsNullOrEmpty(_nowPlaying.Description))
                    if (!string.IsNullOrEmpty(titleString))
                        titleString = titleString + " / " + _nowPlaying.Description;
                    else
                        titleString = _nowPlaying.Description;
                if (!string.IsNullOrEmpty(_nowPlaying.Location))
                    if (!string.IsNullOrEmpty(titleString))
                        titleString = titleString + " / " + _nowPlaying.Location;
                    else
                        titleString = _nowPlaying.Location;

                //Log.Debug("#Play.Current.Album: " + GUIPropertyManager.GetProperty("#Play.Current.Album"));
                //Log.Debug("titleString: " + titleString);

                if (string.IsNullOrEmpty(GUIPropertyManager.GetProperty("#Play.Current.Album").Trim()))
                    GUIPropertyManager.SetProperty("#Play.Current.Album", titleString);
                //_nowPlaying.Name + "/" + _nowPlaying.Description + "/" + _nowPlaying.Location);

                if (_setting.FormatNames.ContainsKey(_currentItem.Formats))
                    GUIPropertyManager.SetProperty("#RadioTime.Play.Format", _setting.FormatNames[_currentItem.Formats]);
                else
                    GUIPropertyManager.SetProperty("#RadioTime.Play.Format", " ");

                GUIPropertyManager.SetProperty("#RadioTime.Play.Image", DownloadStationLogo(_currentItem));

                doAdditionalStuffOnStarted();

                ClearInternalVariables();
            }
        }

        protected void ClearInternalVariables()
        {
            _currentFileName = string.Empty;
            _currentItem = null;
            _nowPlaying = null;
            _station = null;
        }

        protected void ClearProps()
        {
            Settings.NowPlaying = new RadioTimeNowPlaying();
            Settings.NowPlayingStation = new RadioTimeStation();
            GUIPropertyManager.SetProperty("#RadioTime.Play.Station", " ");
            GUIPropertyManager.SetProperty("#RadioTime.Play.StationLogo", " ");
            GUIPropertyManager.SetProperty("#RadioTime.Play.Duration", " ");
            GUIPropertyManager.SetProperty("#RadioTime.Play.Description", " ");
            GUIPropertyManager.SetProperty("#RadioTime.Play.Location", " ");
            GUIPropertyManager.SetProperty("#RadioTime.Play.Slogan", " ");
            GUIPropertyManager.SetProperty("#RadioTime.Play.Language", " ");
            GUIPropertyManager.SetProperty("#RadioTime.Play.Format", " ");
            GUIPropertyManager.SetProperty("#RadioTime.Play.Image", " ");
        }

        protected void ClearPlayProps()
        {
            GUIPropertyManager.SetProperty("#Play.Current.Thumb", " ");
            GUIPropertyManager.SetProperty("#Play.Current.Artist", " ");
            GUIPropertyManager.SetProperty("#Play.Current.Title", " ");
            GUIPropertyManager.SetProperty("#Play.Current.Track", " ");
            GUIPropertyManager.SetProperty("#Play.Current.Album", " ");
            GUIPropertyManager.SetProperty("#Play.Current.Year", " ");
            GUIPropertyManager.SetProperty("#Play.Current.Rating", "0");
        }

        public void UpdateSelectedLabels(RadioTimeOutline radioItem)
        {
            GUIPropertyManager.SetProperty("#RadioTime.Selected.NowPlaying", radioItem.CurrentTrack);
            GUIPropertyManager.SetProperty("#RadioTime.Selected.Subtext", radioItem.Subtext);
            GUIPropertyManager.SetProperty("#RadioTime.Selected.Reliability",
                (radioItem.ReliabilityIdAsInt/10).ToString());

            GUIPropertyManager.SetProperty("#RadioTime.Selected.Logo", " ");
            GUIPropertyManager.SetProperty("#RadioTime.Selected.Logo", DownloadStationLogo(radioItem));

            if (_setting.FormatNames.ContainsKey(radioItem.Formats))
                GUIPropertyManager.SetProperty("#RadioTime.Selected.Format", _setting.FormatNames[radioItem.Formats]);
            else
                GUIPropertyManager.SetProperty("#RadioTime.Selected.Format", " ");
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
            if (hh > 0)
                return string.Format("{0}:{1}:{2}", hh.ToString(), mm.ToString("00"), min.ToString("00"));
            return string.Format("{0}:{1}", mm.ToString("00"), min.ToString("00"));
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
                _currentItem = item.Clone();

                //RadioTimeStation station = Settings.NowPlayingStation;
                _station = new RadioTimeStation();
                _station.Grabber = grabber;
                _station.Get(item.GuidId);

                if (string.IsNullOrEmpty(item.GuidId) || _station.IsAvailable)
                {
                    //var nowPlaying = Settings.NowPlaying;
                    _nowPlaying = new RadioTimeNowPlaying();
                    _nowPlaying.Grabber = grabber;
                    _nowPlaying.Get(item.GuidId);

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
            for (var i = 0; i < Settings.LOCAL_PRESETS_NUMBER; i++)
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
                    GUIPropertyManager.SetProperty("#RadioTime.Presets.Folder.Name", " ");
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
                        body.PresetNumberAsInt - 1 < Settings.LOCAL_PRESETS_NUMBER)
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

        public string DownloadStationLogo(RadioTimeOutline radioItem)
        {
            var localFile = GetStationLogoFileName(radioItem);
            if (!File.Exists(localFile) && !string.IsNullOrEmpty(radioItem.Image))
            {
                downloaQueue.Enqueue(new DownloadFileObject(localFile, radioItem.Image.Replace("q.png", ".png")));
            }
            return localFile;
        }

        private void PopulatePresetsLabels()
        {
            for (var i = 0; i < Settings.LOCAL_PRESETS_NUMBER; i++)
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