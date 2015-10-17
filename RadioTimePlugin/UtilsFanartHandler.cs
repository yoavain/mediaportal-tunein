//***********************************************************************
// Assembly         : RadioTimePlugin
// Author           : ajs
// Created          : 12-10-2015
//
// Last Modified By : ajs
// Last Modified On : 12-10-2015
// Description      : 
//
// Copyright        : Open Source software licensed under the GNU/GPL agreement.
//***********************************************************************

using System;
using System.Collections;
using System.Threading;
using MediaPortal.GUI.Library;

namespace RadioTimePlugin
{
  internal class UtilsFanartHandler
  {
    internal static string GetLastFMAlbum(string artist, string track)
    {
      try
      {
        return FanartHandler.ExternalAccess.GetAlbumForArtistTrack(artist, track);
      }
      catch (MissingMethodException)
      {
        //do nothing    
      }
      catch (Exception ex)
      {
        Log.Error("GetLastFMAlbum: " + ex.ToString());
      }
      return string.Empty;
    }
  }
}
