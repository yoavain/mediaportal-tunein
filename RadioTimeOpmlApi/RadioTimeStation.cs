using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Xml;

namespace RadioTimeOpmlApi
{
    public class RadioTimeStation : ICloneable
    {
        object ICloneable.Clone()
        {
            return Clone();
        }

        public RadioTimeStation Clone()
        {
            var returnStation = (RadioTimeStation) MemberwiseClone();
            return returnStation;
        }


        public RadioTimeStation()
        {
            Genres = new List<RadioTimeOutline>();
            Similar = new List<RadioTimeOutline>();
        }

        public string GuideId { get; set; }
        public RadioTime Grabber { get; set; }
        public List<RadioTimeOutline> Genres { get; set; }
        public List<RadioTimeOutline> Similar { get; set; }

        public string Name { get; set; }
        public bool IsPreset { get; set; }
        public bool IsAvailable { get; set; }
        public bool HasSchedule { get; set; }
        public string Language { get; set; }
        public string Logo { get; set; }
        public string Location { get; set; }
        public string Frequency { get; set; }
        public string Slogan { get; set; }
        public string Adresss { get; set; }

        public string CallSign { get; set; }
        public string Band { get; set; }

        public bool HasSong { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Song { get; set; }

        public string Description { get; set; }

        public void Get(string guideid)
        {
            if (string.IsNullOrEmpty(guideid))
                return;

            GuideId = guideid;
            var sUrl = string.Format(
                "http://opml.radiotime.com/Describe.ashx?id={0}&detail=genre,recommendation&{1}", GuideId,
                Grabber.Settings.GetParamString());

            //Log.Debug("Get Station " + sUrl);
            IsAvailable = false;
            HasSong = false; 
            if (string.IsNullOrEmpty(GuideId))
                return;
            IsAvailable = true;
            var response = RetrieveData(sUrl);
            if (response != null)
            {
                Genres.Clear();
                Similar.Clear();
                var reader = new StreamReader(response, Encoding.UTF8, true);
                var sXmlData = reader.ReadToEnd().Replace('\0', ' ');
                response.Close();
                reader.Close();
                try
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(sXmlData);
                    // skip xml node
                    var root = doc.FirstChild.NextSibling;
                    var bodynodes = root.SelectSingleNode("body");
                    var i = 0;
                    foreach (XmlNode node in bodynodes)
                    {
                        switch (i)
                        {
                            case 0:
                                foreach (XmlNode childNode in node.ChildNodes[0].ChildNodes)
                                {
                                    switch (childNode.Name.ToLower())
                                    {
                                        case "name":
                                            Name = childNode.InnerText;
                                            break;
                                        case "language":
                                            Language = childNode.InnerText;
                                            break;
                                        case "frequency":
                                            Frequency = childNode.InnerText;
                                            break;
                                        case "slogan":
                                            Slogan = childNode.InnerText;
                                            break;
                                        case "logo":
                                            Logo = childNode.InnerText;
                                            break;
                                        case "location":
                                            Location = childNode.InnerText;
                                            break;
                                        case "is_preset":
                                            IsPreset = childNode.InnerText == "false" ? false : true;
                                            break;
                                        case "is_available":
                                            IsAvailable = childNode.InnerText == "false" ? false : true;
                                            break;
                                        case "has_schedule":
                                            HasSchedule = childNode.InnerText == "false" ? false : true;
                                            break;
                                        case "call_sign":
                                            CallSign = childNode.InnerText;
                                            break;
                                        case "band":
                                            Band = childNode.InnerText;
                                            break;
                                        case "has_song":
                                            HasSong = childNode.InnerText == "false" ? false : true;
                                            break;
                                        case "current_artist":
                                            Artist = childNode.InnerText;
                                            break;
                                        case "current_album":
                                            Album = childNode.InnerText;
                                            break;
                                        case "current_song":
                                            Song = childNode.InnerText;
                                            break;
                                        case "description":
                                            Description = childNode.InnerText;
                                            break;
                                    }
                                }
                                break;
                            case 1:
                                foreach (XmlNode childNode in node.ChildNodes)
                                {
                                    Genres.Add(new RadioTimeOutline(childNode));
                                }
                                break;
                            case 2:
                                foreach (XmlNode childNode in node.ChildNodes)
                                {
                                    Similar.Add(new RadioTimeOutline(childNode));
                                }
                                break;
                        }

                        i++;
                    }
                }
                catch (Exception)
                {
                }
                Slogan = string.IsNullOrEmpty(Slogan) ? " " : Slogan;
                Language = string.IsNullOrEmpty(Language) ? " " : Language;
                Description = string.IsNullOrEmpty(Description) ? " " : Description;
            }
        }

        private Stream RetrieveData(String sUrl)
        {
            if (string.IsNullOrEmpty(sUrl) || sUrl[0] == '/')
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
