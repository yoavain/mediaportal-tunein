using System;
using System.IO;
using System.Net;

namespace RadioTimeOpmlApi
{
    /// <summary>
    /// Provide information about one station
    /// </summary>
    public class RadioTimeNowPlaying : ICloneable
    {
        object ICloneable.Clone()
        {
            return Clone();
        }

        public RadioTimeNowPlaying Clone()
        {
            return (RadioTimeNowPlaying) MemberwiseClone();
        }

        public string GuidId { get; set; }

        public string PresetId { get; set; }

        public string Name { get; set; }

        public string Image { get; set; }

        public string ShowImage { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public int Duration { get; set; }

        public int Remains { get; set; }

        public RadioTime Grabber { get; set; }


        public RadioTimeNowPlaying()
        {
            GuidId = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            Location = string.Empty;
            Duration = 0;
            Remains = 0;
            ShowImage = string.Empty;
            Image = string.Empty;
        }

        /// <summary>
        /// Gets the specified stationid.
        /// </summary>
        /// <param name="stationid">The station id.</param>
        /// <param name="hassong">The station hass song or not</param>
        public void Get(string stationid, bool hassong)
        {
            var gr = new RadioTime();
            gr.Settings = Grabber.Settings;
            GuidId = stationid;
            var url = string.Format("http://opml.radiotime.com/Describe.ashx?c=nowplaying&id={0}&{1}", GuidId,
                Grabber.Settings.GetParamString());
            gr.GetData(url, false, false);

            var bool isshow = false;
            var line = 0;
            foreach (var outline in gr.Body)
            {
                if (outline.Key == "station")
                {
                    Image = outline.Image;
                    Name = outline.Text;
                    PresetId = outline.PresetId;
                    GuidId = outline.GuidId;
                }

                if (outline.Key == "show")
                {
                    ShowImage = string.Format("http://radiotime-logos.s3.amazonaws.com/{0}.png", GuidId);
                    Description = outline.Text;
                    var i = 0;
                    if (int.TryParse(outline.Duration, out i))
                        Duration = i;
                    i = 0;
                    if (int.TryParse(outline.Remain, out i))
                        Remains = i;
                    issgow = true;
                    continue;
                }

                switch (line)
                {
                    case 1: // if station has song then [Artists - Song] else [Description]
                        if (!hassong)
                        {
                          Description = outline.Text;
                        }
                        break;
                    case 2: // if station has song then [Genre | Description] else [Location]
                        if (hassong)
                        {
                          Description = outline.Text;
                        }
                        else
                        {
                          Location = outline.Text;
                        }
                        break;
                    case 3: // if station has song then [Location]
                        if (hassong)
                        {
                          Location = outline.Text;
                        }
                        break;
                }
                line++;
            }

            if (string.IsNullOrEmpty(ShowImage))
                ShowImage = Image;
        }

        /// <summary>
        /// Retrieves the data.
        /// </summary>
        /// <param name="sUrl">The s URL.</param>
        /// <returns></returns>
        private Stream RetrieveData(String sUrl)
        {
            if (sUrl == null || sUrl.Length < 1 || sUrl[0] == '/')
            {
                return null;
            }
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                request = (HttpWebRequest) WebRequest.Create(sUrl);
                // Note: some network proxies require the useragent string to be set or they will deny the http request
                // this is true for instance for EVERY thailand internet connection (also needs to be set for banners/episodethumbs and any other http request we send)
                //request.UserAgent = Settings.UserAgent;
                request.Timeout = 20000;
                response = (HttpWebResponse) request.GetResponse();

                if (response != null) // Get the stream associated with the response.
                    return response.GetResponseStream();
            }
            catch (Exception)
            {
                // can't connect, timeout, etc
            }
            finally
            {
                //if (response != null) response.Close(); // screws up the decompression
            }

            return null;
        }
    }
}
