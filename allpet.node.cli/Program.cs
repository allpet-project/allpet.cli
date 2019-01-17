﻿using AllPet.nodecli.httpinterface;
using System;
using System.Collections.Generic;

namespace AllPet.nodecli
{
    class Program
    {
        public static AllPet.Log.ILogger logger;
        public static Config config;
        public static AllPet.node.INode node;//P2P节点
        public static HttpRPC rpc;
        static void Main(string[] args)
        {
            logger = new AllPet.Log.Logger();
            logger.Info("Allpet.Node v0.001");
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
            MenuLoop();
        }

        private static void LoadConfig()
        {
            var configstr = System.IO.File.ReadAllText("config.json");
            config = Config.Parse(configstr);
        }
        private static void StartNode()
        {
            node = AllPet.node.Node.CreateNode();
            node.InitChain(config.dbPath, config.chainInfo);
            node.StartNetwork();
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
