using System;
using System.Collections.Generic;
using System.Text;

namespace RadioTimeOpmlApi
{
  public class RadioTimeHead
  {
    private string _title;

    public string Title
    {
      get { return _title; }
      set { _title = value; }
    }

    public RadioTimeHead()
    {
      Title = String.Empty;
    }

    public RadioTimeHead(RadioTimeHead head)
    {
      Title = head.Title;
    }
  }
}
