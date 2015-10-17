using System;
using System.ComponentModel;
using System.IO;
using MediaPortal.GUI.Library;
using RadioTimeOpmlApi;
using Action = MediaPortal.GUI.Library.Action;

namespace RadioTimePlugin
{
    public class PresetsGUI : BaseGui
    {
        [SkinControl(2)] protected GUIButtonControl homeButton = null;
        [SkinControl(3)] protected GUIButtonControl folderButton = null;

        public override int GetID
        {
            get { return 25653; }

            set { }
        }

        public override bool Init()
        {
            grabber = new RadioTime();

            updateStationLogoTimer.AutoReset = true;
            updateStationLogoTimer.Enabled = false;
            updateStationLogoTimer.Elapsed -= OnDownloadTimedEvent;
            updateStationLogoTimer.Elapsed += OnDownloadTimedEvent;
            Client.DownloadFileCompleted += Client_DownloadFileCompleted;
            return Load(GUIGraphicsContext.Skin + @"\RadioTimePresets.xml");
        }

        protected override void OnPageLoad()
        {
            updateStationLogoTimer.Enabled = true;

            _setting = Settings.Instance;
            grabber.Settings.User = _setting.User;
            grabber.Settings.Password = _setting.Password;
            grabber.Settings.PartnerId = _setting.PartnerId;
            LoadLocalPresetStations();

            if (String.IsNullOrEmpty(GUIPropertyManager.GetProperty("#RadioTime.Presets.Folder.Name").Trim()))
                GUIControl.DisableControl(GetID, folderButton.GetID);
            else
                GUIControl.EnableControl(GetID, folderButton.GetID);

            foreach (var name in Translation.Strings.Keys)
            {
                SetProperty("#RadioTime.Translation." + name + ".Label", Translation.Strings[name]);
            }

            GUIControl.FocusControl(GetID, GetFocusControlId());

            base.OnPageLoad();
        }

        protected override void OnPageDestroy(int newWindowId)
        {
            updateStationLogoTimer.Enabled = false;
            base.OnPageDestroy(newWindowId);
        }

        public override bool OnMessage(GUIMessage message)
        {
            //Log.Error(" PresetsGUI OnMessage: " + message.Message.ToString());

            if (message.Message == GUIMessage.MessageType.GUI_MSG_SETFOCUS && message.TargetControlId > 100 &&
                message.TargetControlId <= Settings.LocalPresetsNumber + 100)
            {
                if (_setting.PresetStations.Count >= message.TargetControlId - 100)
                    UpdateSelectedLabels(_setting.PresetStations[message.TargetControlId - 100 - 1]);
            }
            return base.OnMessage(message);
        }

        protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
        {
            if (control.GetType() == typeof (GUIButtonControl))
            {
                if (controlId > 100 && controlId <= Settings.LocalPresetsNumber + 100 &&
                    _setting.PresetStations[controlId - 100 - 1] != null &&
                    _setting.PresetStations[controlId - 100 - 1].GuidId != null)
                {
                    DoPlay(_setting.PresetStations[controlId - 100 - 1]);
                    if (_setting.JumpNowPlaying)
                        GUIWindowManager.ActivateWindow(25652);
                }
            }
            if (control == homeButton)
            {
                GUIWindowManager.ActivateWindow(25650);
            }
            else if (control == folderButton)
            {
                var s = GetPresetFolder();
                if (s == noPresetFolders)
                {
                    ErrMessage(Translation.NoPresetFoldersFound);
                    GUIControl.DisableControl(GetID, folderButton.GetID);
                }
                else if (s != null)
                {
                    _setting.FolderId = s;
                    _setting.Save();
                    LoadLocalPresetStations();
                }
            }

            base.OnClicked(controlId, control, actionType);
        }

        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if ((e.Error == null) && (!string.IsNullOrEmpty(curentDownlodingFile.FileName)))
            {
                File.Copy(Path.GetTempPath() + @"\station.png", curentDownlodingFile.FileName, true);
                var focusID = GetFocusControlId();
                if (focusID > 100 && focusID <= Settings.LocalPresetsNumber + 100)
                    UpdateSelectedLabels(_setting.PresetStations[focusID - 100 - 1]);
            }
        }
    }
}