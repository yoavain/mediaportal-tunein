using System;
using System.Collections.Generic;
using System.Text;

namespace RadioTimePlugin
{
  public class DownloadFileObject
  {
    private string  _fileName;

    public string  FileName
    {
      get { return _fileName; }
      set { _fileName = value; }
    }

    private string _url;

    public string Url
    {
      get { return _url; }
      set { _url = value; }
    }

    public DownloadFileObject(string file, string url)
    {
      FileName = file;
      Url = url;
    }
  }
}
