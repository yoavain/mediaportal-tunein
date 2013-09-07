using System;
using System.IO;
using System.Xml;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Playlists;

namespace RadioTimePlugin
{
  public class PlayListASXIO : IPlayListIO
  {
    public bool Load(PlayList playlist, string fileName, string label)
    {
      return Load(playlist, fileName);
    }

    public bool Load(PlayList playlist, string fileName)
    {
      playlist.Clear();

      try
      {
        string basePath = Path.GetDirectoryName(Path.GetFullPath(fileName));
        XmlDocument doc = new XmlDocument();
        doc.Load(fileName);
        if (doc.DocumentElement == null)
        {
          return false;
        }
        XmlNode nodeRoot = doc.DocumentElement.SelectSingleNode("/asx");
        if (nodeRoot == null)
        {
          return false;
        }
        XmlNodeList nodeEntries = nodeRoot.SelectNodes("entry");
        foreach (XmlNode node in nodeEntries)
        {
          XmlNode srcNode = node.SelectSingleNode("ref");
          if (srcNode != null)
          {
            XmlNode url = srcNode.Attributes.GetNamedItem("href");
            if (url != null)
            {
              if (url.InnerText != null)
              {
                if (url.InnerText.Length > 0)
                {
                  fileName = url.InnerText;
                  if (!(fileName.ToLowerInvariant().StartsWith("http") || fileName.ToLowerInvariant().StartsWith("mms") || fileName.ToLowerInvariant().StartsWith("rtp")))
                    continue;

                  PlayListItem newItem = new PlayListItem(fileName, fileName, 0);
                  newItem.Type = PlayListItem.PlayListItemType.Audio;
                  playlist.Add(newItem);
                }
              }
            }
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Info("exception loading playlist {0} err:{1} stack:{2}", fileName, ex.Message, ex.StackTrace);
      }
      return false;
    }

    public void Save(PlayList playlist, string fileName)
    {
      throw new Exception("The method or operation is not implemented.");
    }
  }
}