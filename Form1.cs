using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace KeyHunter
{
    public partial class Form1 : Form
    {
        const string COIN_DATA_FILE = "Coins.data";

        Dictionary<string, long> _coinData;
        string _lastBlock;

        public Form1()
        {
            _coinData = new Dictionary<string, long>();

            InitializeComponent();

            // Load coin data
            LoadCoinData();
        }

        void LoadCoinData()
        {
            if (File.Exists(COIN_DATA_FILE))
            {
                FileStream fs = File.OpenRead(COIN_DATA_FILE);
                BinaryReader br = new BinaryReader(fs);

                _lastBlock = br.ReadString();
                int dataCount = br.ReadInt32();
                for (int i = 0; i < dataCount; i++)
                {
                    string key = br.ReadString();
                    long coins = br.ReadInt64();
                    _coinData[key] = coins;
                }
            }
            else
                _lastBlock = "00000000839a8e6886ab5951d76f411475428afc90947ee320161bbf18eb6048";    // Default to the first block if the data doesnt exist
        }

        void FindTheBitcoins()
        {
            // Get the latest block id
            WebClient wc = new WebClient();
            string latestBlockJSON = wc.DownloadString("https://blockchain.info/latestblock");
            LatestBlock lb = JsonConvert.DeserializeObject<LatestBlock>(latestBlockJSON);

            // Make sure there is actually work to be done here
            if (lb.hash != _lastBlock)
            {
                // Fetch the latest block data
                string blockDataJSON = wc.DownloadString(string.Format("https://blockchain.info/block-index/{0}?format=json", lb.hash));
                BlockData bd = JsonConvert.DeserializeObject<BlockData>(blockDataJSON);

                // Fetch backwards until we get to our marker
                while (bd.hash != _lastBlock)
                {
                    foreach (Tx t in bd.tx)
                    {
                        List<Destination> dests = new List<Destination>();
                        foreach (Out o in t.@out)
                        {
                            Destination d = new Destination();
                            d.Address = o.addr;
                            d.Value = o.value;
                           
                            dests.Add(d);
                        }

                        Transaction trans = new Transaction();
                        trans.Source = t.inputs[0].
                    }
                }



                // Track coins through the block chain
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FindTheBitcoins();
        }
    }
}
