using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Globalization;

namespace RadioTimeOpmlApi
{
    public class RadioTime
    {
        /// <summary>
        /// Gets or sets a value indicating whether [cache is used].
        /// </summary>
        /// <value><c>true</c> if [cache is used]; otherwise, <c>false</c>.</value>
        public bool CacheIsUsed { get; set; }

        /// <summary>
        /// Gets or sets the curent URL.
        /// </summary>
        /// <value>The curent URL.</value>
        public String CurentUrl { get; set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public RadioTime Parent { get; set; }

        /// <summary>
        /// Gets or sets the parent URL.
        /// </summary>
        /// <value>The parent URL.</value>
        public string ParentUrl { get; set; }

        /// <summary>
        /// Gets or sets the head description.
        /// </summary>
        /// <value>The head.</value>
        public RadioTimeHead Head { get; set; }

        /// <summary>
        /// Gets or sets the body elements.
        /// </summary>
        /// <value>The body.</value>
        public List<RadioTimeOutline> Body { get; set; }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>The settings.</value>
        public RadioTimeSetting Settings { get; set; }

        /// <summary>
        /// Gets or sets the cache.
        /// </summary>
        /// <value>The cache.</value>
        public Dictionary<string, RadioTime> Cache { get; set; }

        /// <summary>
        /// Gets or sets the parent URL.
        /// </summary>
        /// <value>The parent URL.</value>
        public string NavigationTitle { get; set; }

        public object Selected { get; set; }

        /// <summary>
        /// Adds the preset.
        /// </summary>
        /// <param name="id">The preset id.</param>
        public void AddPreset(string id, string folderid, string prestnumber)
        {
            var client = new WebClient();
            var url = string.Format(
                "http://opml.radiotime.com/Preset.ashx?c=add&id={0}&partnerId={1}&username={2}&password={3}&folderId={4}&presetNumber={5}",
                id, Settings.PartnerId, Settings.User, Settings.Password, folderid, prestnumber);
            client.DownloadString(url);
        }

        /// <summary>
        /// Removes the preset.
        /// </summary>
        /// <param name="id">The preset id.</param>
        public void RemovePreset(string id)
        {
            var client = new WebClient();
            var url = string.Format(
                "http://opml.radiotime.com/Preset.ashx?c=remove&id={0}&partnerId={1}&username={2}&password={3}", id,
                Settings.PartnerId, Settings.User, Settings.Password);
            client.DownloadString(url);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadioTime"/> class.
        /// </summary>
        public RadioTime()
        {
            CurentUrl = "http://opml.radiotime.com/Index.aspx";
            ParentUrl = CurentUrl;
            Head = new RadioTimeHead();
            Body = new List<RadioTimeOutline>();
            Settings = new RadioTimeSetting();
            Cache = new Dictionary<string, RadioTime>();
            Parent = null;
            NavigationTitle = "";
            Selected = null;
        }

        public RadioTime(RadioTime parent)
        {
            Head = new RadioTimeHead(parent.Head);
            Body = new List<RadioTimeOutline>(parent.Body);
            Settings = new RadioTimeSetting(parent.Settings);
            ParentUrl = parent.ParentUrl;
            CurentUrl = parent.CurentUrl;
            Parent = parent.Parent;
            Cache = parent.Cache;
            NavigationTitle = parent.NavigationTitle;
        }

        /// <summary>
        /// Prevs this instance.
        /// </summary>
        public void Prev()
        {
            if (Parent != null)
            {
                Selected = CurentUrl;
                Head = Parent.Head;
                Body = Parent.Body;
                Settings = Parent.Settings;
                ParentUrl = Parent.ParentUrl;
                CurentUrl = Parent.CurentUrl;
                Parent = Parent.Parent;
                NavigationTitle = Parent.NavigationTitle;
            }
        }

        /// <summary>
        /// Resets all variabiles and clear the cache.
        /// </summary>
        public void Reset()
        {
            ClearCache();
            Parent = null;
            Body.Clear();
            ParentUrl = string.Empty;
            CurentUrl = string.Empty;
            NavigationTitle = "";
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void ClearCache()
        {
            Cache.Clear();
        }

        /// <summary>
        /// Searches by the specified search string.
        /// </summary>
        /// <param name="sStr">The search string.</param>
        public void Search(string sStr)
        {
            Search(sStr, string.Empty);
        }

        /// <summary>
        /// Searches by the specified search string.
        /// </summary>
        /// <param name="sStr">The search string.</param>
        /// <param name="navigationTitle">navigation title.</param>
        public void Search(string sStr, string navigationTitle)
        {
            var s = string.Format("http://opml.radiotime.com/Search.aspx?query={0}&{1}", sStr,
                Settings.GetParamString());
            GetData(s, navigationTitle == string.Empty ? sStr : navigationTitle + ": " + sStr);
        }

        /// <summary>
        /// Searches the artist by specified search string.
        /// </summary>
        /// <param name="sStr">The search string.</param>
        public void SearchArtist(string sStr)
        {
            SearchArtist(sStr, string.Empty);
        }

        /// <summary>
        /// Searches the artist by specified search string.
        /// </summary>
        /// <param name="sStr">The search string.</param>
        /// <param name="navigationTitle">navigation title.</param>
        public void SearchArtist(string sStr, string navigationTitle)
        {
            var s = string.Format("http://opml.radiotime.com/Search.ashx?c=song,artist&query={0}&{1}", sStr,
                Settings.GetParamString());
            GetData(s, navigationTitle == string.Empty ? sStr : navigationTitle + ": " + sStr);
        }

        /// <summary>
        /// Gets the online data.
        /// </summary>
        /// <param name="sUrl">The s URL.</param>
        /// <returns></returns>
        public bool GetData(string sUrl)
        {
            return GetData(sUrl, true);
        }

        /// <summary>
        /// Gets the online data.
        /// </summary>
        /// <param name="sUrl">The s URL.</param>
        /// <param name="navigationTitle">navigation title.</param>
        /// <returns></returns>
        public bool GetData(string sUrl, string navigationTitle)
        {
            return GetData(sUrl, true, true, navigationTitle);
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="sUrl">The s URL.</param>
        /// <param name="useCache">if set to <c>true</c> [use cache].</param>
        /// <returns></returns>
        public bool GetData(string sUrl, bool useCache)
        {
            return GetData(sUrl, useCache, true);
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="sUrl">The s URL.</param>
        /// <param name="useCache">if set to <c>true</c> [use cache].</param>
        /// <param name="navigationTitle">navigation title.</param>
        /// <returns></returns>
        public bool GetData(string sUrl, bool useCache, string navigationTitle)
        {
            return GetData(sUrl, useCache, true, navigationTitle);
        }

        /// <summary>
        /// Get and parse the online data.
        /// </summary>
        /// <param name="sUrl">The s URL.</param>
        /// <param name="useCache">if set to <c>true</c> [use cache].</param>
        /// <param name="useNavigationLogic">if set to <c>true</c> [use navigation logic].</param>
        /// <returns></returns>
        public bool GetData(string sUrl, bool useCache, bool useNavigationLogic)
        {
            return GetData(sUrl, useCache, useNavigationLogic, string.Empty);
        }

        /// <summary>
        /// Get and parse the online data.
        /// </summary>
        /// <param name="sUrl">The s URL.</param>
        /// <param name="useCache">if set to <c>true</c> [use cache].</param>
        /// <param name="useNavigationLogic">if set to <c>true</c> [use navigation logic].</param>
        /// <param name="navigationTitle">navigation title.</param>
        /// <returns></returns>
        public bool GetData(string sUrl, bool useCache, bool useNavigationLogic, string navigationTitle)
        {
            //Log.Debug("GetData " + sUrl);

            if (string.IsNullOrEmpty(sUrl))
                return false;

            CacheIsUsed = useCache;

            if (useCache && Cache.ContainsKey(sUrl))
            {
                if (useNavigationLogic)
                    Parent = new RadioTime(this);
                ParentUrl = CurentUrl;
                CurentUrl = sUrl;
                Head = new RadioTimeHead(Cache[sUrl].Head);
                Body = new List<RadioTimeOutline>(Cache[sUrl].Body);

                if (useNavigationLogic)
                {
                    var properTitle = Head.Title == string.Empty ? navigationTitle : Head.Title;
                    NavigationTitle = NavigationTitle == string.Empty
                        ? properTitle
                        : NavigationTitle + " / " + properTitle;
                    Parent.NavigationTitle = NavigationTitle;
                }
                return true;
            }
            else
            {
                var response = RetrieveData(sUrl);
                if (response != null)
                {
                    if (sUrl != CurentUrl)
                    {
                        if (useNavigationLogic)
                        {
                            Parent = new RadioTime(this);
                        }
                        ParentUrl = CurentUrl;
                        CurentUrl = sUrl;
                    }
                    // Get the stream associated with the response.
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
                        var headnodetitle = root.SelectSingleNode("head/title");
                        if (headnodetitle != null)
                        {
                            Head.Title = headnodetitle.InnerText.Trim();
                        }
                        else
                        {
                            Head.Title = "";
                        }
                        var bodynodes = root.SelectNodes("body/outline");
                        Body.Clear();
                        foreach (XmlNode node in bodynodes)
                        {
                            if (node.HasChildNodes)
                            {
                                foreach (XmlNode childnode in node.ChildNodes)
                                {
                                    Body.Add(new RadioTimeOutline(childnode));
                                }
                            }
                            else
                            {
                                Body.Add(new RadioTimeOutline(node));
                            }
                        }

                        if (useCache)
                        {
                            Cache.Add(sUrl, new RadioTime(this));
                        }
                    }
                    catch (XmlException)
                    {
                        return false;
                    }
                    response.Close();

                    if (useNavigationLogic)
                    {
                        var properTitle = Head.Title == string.Empty ? navigationTitle : Head.Title;
                        NavigationTitle = NavigationTitle == string.Empty
                            ? properTitle
                            : NavigationTitle + " / " + properTitle;
                        Parent.NavigationTitle = NavigationTitle;
                    }

                    return true;
                }
                return true;
            }
        }

        /// <summary>
        /// Retrieves the online data.
        /// </summary>
        /// <param name="sUrl">The s URL.</param>
        /// <returns></returns>
        private Stream RetrieveData(String sUrl)
        {
            if (sUrl == null || sUrl.Length < 1 || sUrl[0] == '/')
            {
                return null;
            }
            //sUrl = this.Settings.UpdateUrl(sUrl);
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                request = (HttpWebRequest) WebRequest.Create(sUrl);
                request.Timeout = 20000;
                request.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name + "," + CultureInfo.CurrentCulture.TwoLetterISOLanguageName + ";q=0.7,en;q=0.3");
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