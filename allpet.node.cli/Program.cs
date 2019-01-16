using allpet.nodecli.httpinterface;
using System;
using System.Collections.Generic;

namespace allpet.nodecli
{
    class Program
    {
        public static Config config;
        public static HttpRPC rpc;
        static void Main(string[] args)
        {
            var peer = allpet.peer.tcp.PeerV2.CreatePeer();
            peer.Start(new allpet.peer.tcp.PeerOption()
            {

            });
            //init current path.
            //把当前目录搞对，怎么启动都能找到dll了
            var lastpath = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location); ;
            Console.WriteLine("exepath=" + lastpath);
            Environment.CurrentDirectory = lastpath;

            //loadConfig
            LoadConfig();

            //StartNode
            StartNode();
            //InitRPC
            rpc = new HttpRPC();
            rpc.Start();


            InitMenu();
            MenuLoop();
        }

        private static void LoadConfig()
        {
            var configstr = System.IO.File.ReadAllText("config.json");
            var config = Config.Parse(configstr);
        }
        static bool Equal1(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (var i = a.Length - 1; i >= 0; i--)
            {
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }
        static bool Equal2(byte[] a, byte[] b)
        {
            return Convert.ToBase64String(a).Equals(Convert.ToBase64String(b));
        }
        static unsafe bool Equal3_len32(byte[] a, byte[] b)
        {
            fixed (byte* _a = a, _b = b)
            {
                UInt64* __a = (UInt64*)_a;
                UInt64* __b = (UInt64*)_b;
                for (var i = 0; i < 4; i++)
                {
                    if (__a[i] != __b[i])
                        return false;
                }
                return true;
            }
        }
        private static void StartNode()
        {

            {
                var t0 = DateTime.Now;
                Random r = new Random();
                List<byte[]> hashes = new List<byte[]>();
                var data = new byte[6553];

                for (var i = 0; i < 10000; i++)
                {

                    r.NextBytes(data);
                    var sha256 = Allpet.Helper.CalcSha256(data);
                    hashes.Add(sha256);
                }
                var t1 = DateTime.Now;
                Console.WriteLine("hash 10K=" + (t1 - t0).TotalSeconds);
                for (var x = 0; x < 100; x++)
                {
                    for (var i = 0; i < hashes.Count; i++)
                    {
                        bool bEq = Equal1(hashes[0], hashes[0]);
                    }
                }
                var t2 = DateTime.Now;
                Console.WriteLine("equal 1 1M=" + (t2 - t1).TotalSeconds);
                for (var x = 0; x < 100; x++)
                {
                    for (var i = 0; i < hashes.Count; i++)
                    {
                        bool bEq = Equal2(hashes[0], hashes[0]);
                    }
                }
                var t3 = DateTime.Now;
                Console.WriteLine("equal 2 1M=" + (t3 - t2).TotalSeconds);
                for (var x = 0; x < 100; x++)
                {
                    for (var i = 0; i < hashes.Count; i++)
                    {
                        bool bEq = Equal3_len32(hashes[0], hashes[0]);
                    }
                }
                var t4 = DateTime.Now;
                Console.WriteLine("equal 3 1M=" + (t4 - t3).TotalSeconds);

            }
            if (false)
            {//db write performence
                allpet.db.simple.DB db = new db.simple.DB();
                db.Open("db001", true);

                var tableid = new byte[] { 0x01, 0x02, 0x03 };
                var ss = db.UseSnapShot();
                var tableinfo = ss.GetTableInfoData(tableid);
                if (tableinfo == null || tableinfo.Length == 0)
                {
                    var wb = db.CreateWriteBatch();
                    wb.CreateTable(tableid, new byte[] { 0x01, 0x02 });
                    db.WriteBatch(wb);
                }
                var t0 = DateTime.Now;
                Random r = new Random();
                for (var i = 0; i < 10000; i++)
                {
                    var wb = db.CreateWriteBatch();
                    var key = BitConverter.GetBytes(i);
                    for (var j = 0; j < 1; j++)
                    {
                        byte[] tdata = new byte[8000];
                        r.NextBytes(tdata);
                        //db.PutDirect(tableid, key, tdata);
                        wb.Put(tableid, key, tdata);
                    }

                    db.WriteBatch(wb);

                }
                var t1 = DateTime.Now;
                Console.WriteLine("time=" + (t1 - t0).TotalSeconds);
            }
        }

        static void InitMenu()
        {
            AddMenu("exit", "exit application", (words) => { Environment.Exit(0); });
            AddMenu("help", "show help", ShowMenu);
        }
        #region MenuSystem
        static System.Collections.Generic.Dictionary<string, Action<string[]>> menuItem = new System.Collections.Generic.Dictionary<string, Action<string[]>>();
        static System.Collections.Generic.Dictionary<string, string> menuDesc = new System.Collections.Generic.Dictionary<string, string>();

        static void AddMenu(string cmd, string desc, Action<string[]> onMenu)
        {
            menuItem[cmd.ToLower()] = onMenu;
            menuDesc[cmd.ToLower()] = desc;
        }

        static void ShowMenu(string[] words = null)
        {
            Console.WriteLine("== NodeCLI Menu==");
            foreach (var key in menuItem.Keys)
            {
                var line = "  " + key + " - ";
                if (menuDesc.ContainsKey(key))
                    line += menuDesc[key];
                Console.WriteLine(line);
            }
        }
        static void MenuLoop()
        {
            while (true)
            {
                try
                {
                    Console.Write("-->");
                    var line = Console.ReadLine();
                    var words = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 0)
                    {
                        var cmd = words[0].ToLower();
                        if (cmd == "?")
                        {
                            ShowMenu();
                        }
                        else if (menuItem.ContainsKey(cmd))
                        {
                            menuItem[cmd](words);
                        }
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine("err:" + err.Message);
                }
            }
        }
        #endregion
    }
}
