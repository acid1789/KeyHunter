using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyHunter
{
    public class Input
    {
        public long sequence { get; set; }
        public Out prev_out { get; set; }
        public string script { get; set; }
    }

    public class Out
    {
        public bool spent { get; set; }
        public int tx_index { get; set; }
        public int type { get; set; }
        public string addr { get; set; }
        public long value { get; set; }
        public int n { get; set; }
        public string script { get; set; }
    }

    public class Tx
    {
        public int lock_time { get; set; }
        public int ver { get; set; }
        public int size { get; set; }
        public List<Input> inputs { get; set; }
        public int time { get; set; }
        public int tx_index { get; set; }
        public int vin_sz { get; set; }
        public string hash { get; set; }
        public int vout_sz { get; set; }
        public string relayed_by { get; set; }
        public List<Out> @out { get; set; }
    }

    public class BlockData
    {
        public string hash { get; set; }
        public int ver { get; set; }
        public string prev_block { get; set; }
        public string mrkl_root { get; set; }
        public int time { get; set; }
        public int bits { get; set; }
        public long fee { get; set; }
        public long nonce { get; set; }
        public int n_tx { get; set; }
        public int size { get; set; }
        public int block_index { get; set; }
        public bool main_chain { get; set; }
        public int height { get; set; }
        public List<Tx> tx { get; set; }
    }

    public class LatestBlock
    {
        public string hash { get; set; }
        public int time { get; set; }
        public int block_index { get; set; }
        public int height { get; set; }
        public List<int> txIndexes { get; set; }
    }
}