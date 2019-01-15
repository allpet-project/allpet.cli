using allpet.nodecli.httpinterface;
using System;

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
            LightDB.LightDB db = new LightDB.LightDB();
            db.Open("db001", new LightDB.DBCreateOption()
            {
                MagicStr = "hi db"
            });
            var tableid = new byte[] { 0x01, 0x02, 0x03 };
            var ss = db.UseSnapShot();
            var tableinfo = ss.GetTableInfoData(tableid);
            if (tableinfo == null || tableinfo.Length == 0)
            {
                var wb = db.CreateWriteTask();
                wb.CreateTable(new LightDB.TableInfo(tableid, "001", "", LightDB.DBValue.Type.String));
                db.Write(wb);
            }
            var t0 = DateTime.Now;
            for (var i = 0; i < 10000; i++)
            {
                var wb = db.CreateWriteTask();
                var key = BitConverter.GetBytes(i);
                wb.Put(tableid, key, LightDB.DBValue.FromValue(LightDB.DBValue.Type.INT32, i));
                db.Write(wb);

            }
            var t1 = DateTime.Now;
            Console.WriteLine("time=" + (t1 - t0).TotalSeconds);
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
