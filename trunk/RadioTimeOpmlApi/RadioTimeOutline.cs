using System;
using System.Xml;

namespace RadioTimeOpmlApi
{
    public class RadioTimeOutline : ICloneable
    {
        public enum OutlineType
        {
            audio,
            link,
            unknow
        };

        object ICloneable.Clone()
        {
            return Clone();
        }

        public RadioTimeOutline Clone()
        {
            return (RadioTimeOutline) MemberwiseClone();
        }

        public RadioTimeOutline()
        {
            Formats = "";
        }

        public RadioTimeOutline(XmlNode node)
        {
            if (node.Attributes["type"] != null)
            {
                switch (node.Attributes["type"].Value)
                {
                    case "link":
                        Type = OutlineType.link;
                        break;
                    case "audio":
                        Type = OutlineType.audio;
                        break;
                    default:
                        Type = OutlineType.unknow;
                        break;
                }
            }

            Text = node.Attributes["text"] != null ? node.Attributes["text"].Value : string.Empty;

            if (node.Attributes["url"] != null)
                Url = node.Attributes["url"].Value;

            if (node.Attributes["URL"] != null)
                Url = node.Attributes["URL"].Value;

            Image = node.Attributes["image"] != null ? node.Attributes["image"].Value : string.Empty;

            Bitrate = node.Attributes["bitrate"] != null ? node.Attributes["bitrate"].Value : string.Empty;

            Subtext = node.Attributes["subtext"] != null ? node.Attributes["subtext"].Value : string.Empty;

            if (node.Attributes["formats"] != null)
            {
                Formats = node.Attributes["formats"].Value;
                if (Formats.Contains(","))
                    Formats = Formats.Split(',')[0];
            }
            else
                Formats = string.Empty;

            Duration = node.Attributes["duration"] != null ? node.Attributes["duration"].Value : string.Empty;

            Start = node.Attributes["start"] != null ? node.Attributes["start"].Value : string.Empty;

            GuidId = node.Attributes["guide_id"] != null ? node.Attributes["guide_id"].Value : string.Empty;

            PresetId = node.Attributes["preset_id"] != null ? node.Attributes["preset_id"].Value : string.Empty;

            CurrentTrack = node.Attributes["current_track"] != null
                ? node.Attributes["current_track"].Value
                : string.Empty;

            Key = node.Attributes["key"] != null ? node.Attributes["key"].Value : string.Empty;

            GenreId = node.Attributes["genre_id"] != null ? node.Attributes["genre_id"].Value : string.Empty;

            Remain = node.Attributes["seconds_remaining"] != null
                ? node.Attributes["seconds_remaining"].Value
                : string.Empty;

            Reliability = node.Attributes["reliability"] != null ? node.Attributes["reliability"].Value : string.Empty;

            PresetNumber = node.Attributes["preset_number"] != null
                ? node.Attributes["preset_number"].Value
                : string.Empty;

            //Now_playing_id = Type == OutlineType.audio ? node.Attributes["now_playing_id"].Value : string.Empty;
        }

        public string GenreId { get; set; }
        public OutlineType Type { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public string Now_playing_id { get; set; }
        public string Bitrate { get; set; }
        public string CurrentTrack { get; set; }
        public string Subtext { get; set; }
        public string Formats { get; set; }
        public string Duration { get; set; }
        public string Remain { get; set; }
        public string Reliability { get; set; }
        public string Start { get; set; }
        public string StationId { get; set; }
        public string GuidId { get; set; }
        public string PresetId { get; set; }
        public string Key { get; set; }
        public string PresetNumber { get; set; }

        /// <summary>
        /// Gets the station id as int.
        /// </summary>
        /// <value>The station id as int.</value>
        public int StationIdAsInt
        {
            get
            {
                var i = 0;
                int.TryParse(StationId, out i);
                return i;
            }
        }

        public int ReliabilityIdAsInt
        {
            get
            {
                var i = 0;
                int.TryParse(Reliability, out i);
                return i;
            }
        }

        public int PresetNumberAsInt
        {
            get
            {
                var i = 0;
                int.TryParse(PresetNumber, out i);
                return i;
            }
        }
    }
}