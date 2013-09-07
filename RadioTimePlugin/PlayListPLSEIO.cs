using System;
using System.IO;
using System.Net;
using System.Text;
using MediaPortal.Util;
using MediaPortal.Playlists;

namespace RadioTimePlugin
{
    public class PlayListPLSEIO : IPlayListIO
    {
        private const string START_PLAYLIST_MARKER = "[playlist]";
        private const string PLAYLIST_NAME = "PlaylistName";

        public bool Load(PlayList playlist, string fileName)
        {
            return Load(playlist, fileName, null);
        }

        public bool Load(PlayList playlist, string fileName, string label)
        {
            var basePath = String.Empty;
            Stream stream;

            if (fileName.ToLower().StartsWith("http"))
            {
                // We've got a URL pointing to a pls
                var client = new WebClient();
                client.Proxy.Credentials = CredentialCache.DefaultCredentials;
                var buffer = client.DownloadData(fileName);
                stream = new MemoryStream(buffer);
            }
            else
            {
                // We've got a plain pls file
                basePath = Path.GetDirectoryName(Path.GetFullPath(fileName));
                stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            playlist.Clear();
            playlist.Name = Path.GetFileName(fileName);
            var fileEncoding = Encoding.Default;
            var file = new StreamReader(stream, fileEncoding, true);
            if (file == null)
            {
                stream.Close();
                return false;
            }

            string line;
            line = file.ReadLine();
            if (line == null)
            {
                stream.Close();
                file.Close();
                return false;
            }

            var strLine = line.Trim();
            //CUtil::RemoveCRLF(strLine);
            if (strLine != START_PLAYLIST_MARKER)
            {
                if (strLine.StartsWith("http") || strLine.StartsWith("HTTP") ||
                    strLine.StartsWith("mms") || strLine.StartsWith("MMS") ||
                    strLine.StartsWith("rtp") || strLine.StartsWith("RTP"))
                {
                    var newItem = new PlayListItem(strLine, strLine, 0);
                    newItem.Type = PlayListItem.PlayListItemType.AudioStream;
                    playlist.Add(newItem);
                    stream.Close();
                    file.Close();
                    return true;
                }
                fileEncoding = Encoding.Default;
                stream.Close();
                file.Close();
                stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                file = new StreamReader(stream, fileEncoding, true);

                //file.Close();
                //return false;
            }
            var infoLine = "";
            var durationLine = "-1";
            fileName = "";
            line = file.ReadLine();
            while (line != null)
            {
                strLine = line.Trim();
                //CUtil::RemoveCRLF(strLine);
                var equalPos = strLine.IndexOf("=");
                if (equalPos > 0)
                {
                    var leftPart = strLine.Substring(0, equalPos);
                    equalPos++;
                    var valuePart = strLine.Substring(equalPos);
                    leftPart = leftPart.ToLower();
                    if (leftPart.StartsWith("file"))
                    {
                        if (valuePart.Length > 0 && valuePart[0] == '#')
                        {
                            line = file.ReadLine();
                            continue;
                        }

                        if (fileName.Length != 0)
                        {
                            var newItem = new PlayListItem(infoLine, fileName, 0);
                            playlist.Add(newItem);
                            fileName = "";
                            infoLine = "";
                            durationLine = "-1";
                        }
                        fileName = valuePart;
                    }
                    if (leftPart.StartsWith("title"))
                    {
                        infoLine = valuePart;
                    }
                    else
                    {
                        if (infoLine == "")
                        {
                            // For a URL we need to set the label in for the Playlist name, in order to be played.
                            if (label != null && fileName.ToLower().StartsWith("http"))
                            {
                                infoLine = label;
                            }
                            else
                            {
                                infoLine = fileName;
                            }
                        }
                    }
                    if (leftPart.StartsWith("length"))
                    {
                        durationLine = valuePart;
                    }
                    if (leftPart == "playlistname")
                    {
                        playlist.Name = valuePart;
                    }

                    if (infoLine.Length > 0 && fileName.Length > 0)
                    {
                        var duration = Int32.Parse(durationLine);

                        // Remove trailing slashes. Might cause playback issues
                        if (fileName.EndsWith("/"))
                        {
                            fileName = fileName.Substring(0, fileName.Length - 1);
                        }

                        var newItem = new PlayListItem(infoLine, fileName, duration);
                        if (fileName.ToLower().StartsWith("http:") || fileName.ToLower().StartsWith("https:") ||
                            fileName.ToLower().StartsWith("mms:") || fileName.ToLower().StartsWith("rtp:"))
                        {
                            newItem.Type = PlayListItem.PlayListItemType.AudioStream;
                        }
                        else
                        {
                            Utils.GetQualifiedFilename(basePath, ref fileName);
                            newItem.FileName = fileName;
                            newItem.Type = PlayListItem.PlayListItemType.Audio;
                        }
                        playlist.Add(newItem);
                        fileName = "";
                        infoLine = "";
                        durationLine = "0";
                    }
                }
                line = file.ReadLine();
            }
            stream.Close();
            file.Close();

            if (fileName.Length > 0)
            {
                var newItem = new PlayListItem(infoLine, fileName, 0);
            }

            return true;
        }

        public void Save(PlayList playlist, string fileName)
        {
            using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                writer.WriteLine(START_PLAYLIST_MARKER);
                for (var i = 0; i < playlist.Count; i++)
                {
                    var item = playlist[i];
                    writer.WriteLine("File{0}={1}", i + 1, item.FileName);
                    writer.WriteLine("Title{0}={1}", i + 1, item.Description);
                    writer.WriteLine("Length{0}={1}", i + 1, item.Duration);
                }
                writer.WriteLine("NumberOfEntries={0}", playlist.Count);
                writer.WriteLine("Version=2");
            }
        }
    }
}