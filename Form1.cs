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
using System.Diagnostics;

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

                br.Close();
            }
            else
                _lastBlock = "000000000019d6689c085ae165831e934ff763ae46a2a6c172b3f1b60a8ce26f";    // Default to the first block if the data doesnt exist
        }

        void SaveCoinData()
        {
            FileStream fs = File.Open(COIN_DATA_FILE, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            bw.Write(_lastBlock);
            bw.Write(_coinData.Count);
            foreach (KeyValuePair<string, long> kvp in _coinData)
            {
                bw.Write(kvp.Key);
                bw.Write(kvp.Value);
            }
            
            bw.Close();
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
                // Fetch the starting block data  
                BlockData bd = FetchStartingBlock(wc, lb);

                // Fetch backwards until we get to our marker
                List<Transaction> transactions = new List<Transaction>();
                while (bd.hash != _lastBlock)
                {
                    Debug.WriteLine("Parsing block: {0}", bd.height);

                    // Process transactions
                    for( int j = bd.tx.Count - 1; j >= 0; j-- )
                    {
                        Tx t = bd.tx[j];
                        List<MoneyPlace> dests = new List<MoneyPlace>();
                        foreach (Out o in t.@out)
                        {
                            MoneyPlace d = new MoneyPlace();
                            d.Address = o.addr;
                            d.Value = o.value;
                            dests.Add(d);
                        }

                        Transaction trans = new Transaction();
                        trans.Hash = t.hash;
                        if (t.inputs[0].prev_out != null)
                        {
                            trans.Sources = new MoneyPlace[t.inputs.Count];
                            for (int i = 0; i < t.inputs.Count; i++)
                            {
                                trans.Sources[i] = new MoneyPlace();
                                trans.Sources[i].Address = t.inputs[i].prev_out.addr;
                                trans.Sources[i].Value = t.inputs[i].prev_out.value;
                            }
                        }
                        else
                            trans.Sources = null;

                        trans.Destinations = dests.ToArray();
                        transactions.Add(trans);
                    }

                    // Cache transactions for now
                    if (transactions.Count > 100000)
                    {
                        CacheTransactions(transactions, bd);                        
                        transactions.Clear();
                    }

                    // go backwards
                    bd = PrevBlock(wc, bd.prev_block);
                }

                // Cache remaining transactions
                CacheTransactions(transactions, bd);

                /*
                // Track coins through the block chain
                for (int i = transactions.Count - 1; i >= 0; i--)
                {
                    Transaction t = transactions[i];
                    if (t.Sources != null)
                    {
                        // coins are taken from sources and given to destinations
                        foreach (MoneyPlace source in t.Sources)
                        {
                            _coinData[source.Address] -= source.Value;
                            if (_coinData[source.Address] < 0)
                            {
                                // Problem!
                                Debug.WriteLine("ERROR: gave away more than we knew about!?");
                                throw new System.Exception("WTF!");
                            }
                            else if (_coinData[source.Address] == 0)
                            {
                                // All out of money - toss this address out
                                _coinData.Remove(source.Address);
                            }
                        }
                    }

                    // Give to the destinations
                    foreach (MoneyPlace dest in t.Destinations)
                    {
                        if (!_coinData.ContainsKey(dest.Address))
                            _coinData[dest.Address] = dest.Value;
                        else
                            _coinData[dest.Address] += dest.Value;
                    }
                }

                // Save the coin data
                SaveCoinData();
                */
            }
        }

        BlockData PrevBlock(WebClient wc, string prev)
        {
            Debug.WriteLine("Waiting for block: " + prev);
            try
            {
                string blockDataJSON = wc.DownloadString(string.Format("https://blockchain.info/block-index/{0}?format=json", prev));
                BlockData bd = JsonConvert.DeserializeObject<BlockData>(blockDataJSON);
                return bd;
            }
            catch (Exception)
            {
                Debug.WriteLine("Exception fetching block: " + prev);
                return PrevBlock(wc, prev);
            }
        }

        BlockData FetchStartingBlock(WebClient wc, LatestBlock lb)
        {
            string fetchHash = lb.hash;
            if (Directory.Exists("Cache"))
            {
                string[] files = Directory.GetFiles("Cache", "*.cache");
                long newest = 0;
                string newestFile = null;
                foreach (string file in files)
                {
                    string ticksStr = Path.GetFileNameWithoutExtension(file);
                    long ticks = Convert.ToInt64(ticksStr);
                    if (ticks > newest)
                    {
                        newest = ticks;
                        newestFile = file;
                    }
                }

                if (newestFile != null)
                {
                    FileStream fs = File.OpenRead(newestFile);
                    BinaryReader br = new BinaryReader(fs);
                    fetchHash = br.ReadString();
                    br.Close();
                }
            }

            string uri = string.Format("https://blockchain.info/block-index/{0}?format=json", fetchHash);
            Debug.WriteLine("Fetching latest: " + uri);
            string blockDataJSON = wc.DownloadString(uri);
            BlockData bd = JsonConvert.DeserializeObject<BlockData>(blockDataJSON);
            return bd;
        }

        void CacheTransactions(List<Transaction> transactions, BlockData bd)
        {
            Debug.WriteLine("Caching at block: " + bd.height.ToString());
            if (!Directory.Exists("Cache"))
                Directory.CreateDirectory("Cache");

            FileStream fs = File.Create(DateTime.Now.Ticks.ToString() + ".cache");
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(bd.prev_block);
            bw.Write(transactions.Count);
            foreach (Transaction t in transactions)
            {
                bw.Write(t.Hash);
                if (t.Sources != null)
                {
                    bw.Write(t.Sources.Length);
                    foreach (MoneyPlace mp in t.Sources)
                    {
                        bw.Write(mp.Address);
                        bw.Write(mp.Value);
                    }
                }
                else
                {
                    bw.Write((int)0);
                }
                bw.Write(t.Destinations.Length);
                foreach (MoneyPlace mp in t.Destinations)
                {
                    bw.Write(mp.Address);
                    bw.Write(mp.Value);
                }
            }
            bw.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FindTheBitcoins();
        }
    }
}
