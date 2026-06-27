using System;
using System.Net;
using Falcon.FalconCore.Scripts.Utils.FActions.Base;

namespace Falcon.FalconCore.Scripts.Utils.FActions.Variances.Starts
{
    public class FileGetRequest : StartAction
    {
        private readonly string destination;
        private readonly string url;

        private Exception exception;
        private bool isDone;

        public FileGetRequest(string url, string destination)
        {
            this.url = url;
            this.destination = destination;
        }

        public override Exception Exception => exception;
        public override bool Done => isDone;

        public int progress;

        public override void Invoke()
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadProgressChanged += (s, e) =>
                    {
                        progress = e.ProgressPercentage;
                    };
                    client.DownloadFileCompleted += (s, e) =>
                    {
                        progress = 100;
                        isDone = true;
                    };
                    client.DownloadFileAsync(new Uri(url), destination);
                }
            }
            catch (Exception e)
            {
                exception = e;
            }
        }
    }
}