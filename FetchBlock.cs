using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.Diagnostics;

namespace KeyHunter
{
    class FetchBlock
    {
        WebClient _wc;
        BlockData _result;

        public FetchBlock(string blockHash)
        {
            _wc = new WebClient();
            _wc.DownloadStringCompleted += _wc_DownloadStringCompleted;
            _result = null;
            _wc.DownloadStringAsync(new Uri(string.Format("https://blockchain.info/block-index/{0}?format=json", blockHash)));

            Debug.WriteLine("Fetching block: " + blockHash);
        }

        public BlockData Wait()
        {
            if (_result == null)
            {
                Debug.WriteLine("Waiting for block");
                while (_result == null)
                    Thread.Sleep(10);
            }
            return _result;
        }

        private void _wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {            
            _result = JsonConvert.DeserializeObject<BlockData>(e.Result);
            Debug.WriteLine("Finished block: ({0}){1}", _result.height.ToString(), _result.hash);
        }
    }
}
