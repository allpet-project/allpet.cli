using System;
using System.Collections.Generic;
using System.Text;
using AllPet.nodecli.httpinterface;
using AllPet.Pipeline;

namespace AllPet.nodecli
{
    class Module_Cli : AllPet.Pipeline.Module
    {
        public AllPet.Common.ILogger logger;
        public Newtonsoft.Json.Linq.JObject configJson;
        public Module_Cli(AllPet.Common.ILogger logger,Newtonsoft.Json.Linq.JObject configJson) :base(false)
        {
            this.logger = logger;
            this.configJson = configJson;
        }
        public override void OnStart()
        {
            logger.Info("Module_Cli::OnStart");
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
