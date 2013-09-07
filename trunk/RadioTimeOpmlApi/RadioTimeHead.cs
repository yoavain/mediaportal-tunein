using System;

namespace RadioTimeOpmlApi
{
    public class RadioTimeHead
    {
        public string Title { get; set; }

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