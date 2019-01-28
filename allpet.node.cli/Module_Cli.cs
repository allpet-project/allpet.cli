using System;
using System.Collections.Generic;
using System.Text;
using AllPet.nodecli.httpinterface;
using AllPet.Pipeline;

namespace AllPet.nodecli
{
    class Module_Cli : AllPet.Pipeline.Module
    {
        public AllPet.Log.ILogger logger;
        public Config config;
        public AllPet.node.INode node;//P2P节点
        public HttpRPC rpc;
        public override void OnStart()
        {
            logger = new AllPet.Log.Logger();
            logger.Info("Allpet.Node v0.001");

            //this._System.OpenListen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 8888));

            logger.Warn("test warn.");
            logger.Error("show Error.");
            var peer = AllPet.peer.tcp.PeerV2.CreatePeer();
            peer.Start(new AllPet.peer.tcp.PeerOption()
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

            //不能阻塞這個函數，OnStart一定要結束
            System.Threading.ThreadPool.QueueUserWorkItem((s) =>
            {
                MenuLoop();

            });
        }
        public override void OnTell(IModulePipeline from, byte[] data)
        {
        }
        public override void OnTellLocalObj(IModulePipeline from, object obj)
        {
            throw new NotImplementedException();
        }
        private void LoadConfig()
        {
            var configstr = System.IO.File.ReadAllText("config.json");
            config = Config.Parse(configstr);
        }
        private void StartNode()
        {
            node = AllPet.node.Node.CreateNode();
            node.InitChain(config.dbPath, config.chainInfo);
            node.StartNetwork();
        }

        void InitMenu()
        {
            AddMenu("exit", "exit application", (words) =>
            {
                this.Dispose();
            });
            AddMenu("help", "show help", ShowMenu);
        }
        #region MenuSystem
        static System.Collections.Generic.Dictionary<string, Action<string[]>> menuItem = new System.Collections.Generic.Dictionary<string, Action<string[]>>();
        static System.Collections.Generic.Dictionary<string, string> menuDesc = new System.Collections.Generic.Dictionary<string, string>();

        void AddMenu(string cmd, string desc, Action<string[]> onMenu)
        {
            menuItem[cmd.ToLower()] = onMenu;
            menuDesc[cmd.ToLower()] = desc;
        }

        void ShowMenu(string[] words = null)
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
        void MenuLoop()
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
