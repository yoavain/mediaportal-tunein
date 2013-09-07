namespace RadioTimePlugin
{
    public class DownloadFileObject
    {
        public string FileName { get; set; }

        public string Url { get; set; }

        public DownloadFileObject(string file, string url)
        {
            FileName = file;
            Url = url;
        }
    }
}