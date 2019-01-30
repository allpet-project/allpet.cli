using AllPet.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace allpet.peer.pipeline.test.test
{
    class test2_remote
    {

        public static async Task Test()
        {
            var logger =new AllPet.Common.Logger();
            //服務器端
            var systemR = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1(logger);
            systemR.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            systemR.OpenListen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 8888));
            systemR.RegistModule("hello", new Hello());
            systemR.RegistModule("hello2", new Hello());
            systemR.Start();


            //客戶端
            var systemL = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1(logger);
            systemL.RegistModule("me", new Local());
            systemL.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            systemL.Start();

            var remote = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8888);

            //連接,也可以GetPipeline的时候自动连接，自动连接就需要处理连接什么时候接通的问题了
            var systemref = await systemL.ConnectAsync(remote);

            var actor = systemL.GetPipeline(null, "this/me");
            while (true)
            {
                Console.Write("1.remote>");
                var line = Console.ReadLine();
                if (line == "exit")
                {
                    systemR.CloseListen();
                    systemR.CloseNetwork();
                    systemR.Dispose();
                    systemL.Dispose();
                    break;
                }
                if (line == "")
                    continue;
                actor.Tell(System.Text.Encoding.UTF8.GetBytes(line));

            }
        }

        class Local : Module
        {
            public override void OnStart()
            {
            }
            public override void OnTell(IModulePipeline from, byte[] data)
            {
                if (from == null)
                {
                    var actor = this.GetPipeline("127.0.0.1:8888/hello");
                    actor.Tell(data);
                }
                else
                {
                    Console.WriteLine("Local got from:" + from.system.remoteaddr + " // " + from.path);
                }
                Console.WriteLine("Local get info=" + global::System.Text.Encoding.UTF8.GetString(data));

            }
            public override void OnTellLocalObj(IModulePipeline from, object obj)
            {
                throw new NotImplementedException();
            }
        }

        class Hello : Module
        {
            /// <summary>
            ///  base(false)表示這個模塊是單綫程投遞的，ontell 保證在 同一個綫程裏面
            ///  base(true)或者沒有，則該模塊的OnTell為多綫程投遞，須自行處理綫程問題
            /// </summary>
            public Hello() : base(false)//這個false 表示這個模塊是單綫程投遞的，ontell 保證在 同一個綫程裏面
            {
            }
            IModulePipeline refhello2;
            public override void OnStart()
            {
                var refhello2 = this.GetPipeline("this/hello2");
                refhello2.Tell(global::System.Text.Encoding.UTF8.GetBytes("abcde"));
            }
            public override void OnTell(IModulePipeline from, byte[] data)
            {
                if (from != null && from.system.remoteaddr != null)//从远程投递而来
                {
                    Console.WriteLine("Hello get from:" + from.system.remoteaddr + " // " + from.path);
                    from.Tell(global::System.Text.Encoding.UTF8.GetBytes("hihihihi"));
                }
                Console.WriteLine("Hello get info=" + global::System.Text.Encoding.UTF8.GetString(data));
            }
            public override void OnTellLocalObj(IModulePipeline from, object obj)
            {
                throw new NotImplementedException();
            }
        }
        class Hello2 : Module
        {

            public override void OnStart()
            {
            }
            public override void OnTell(IModulePipeline from, byte[] data)
            {
                Console.WriteLine("Hello2:" + global::System.Text.Encoding.UTF8.GetString(data));

                from.Tell(global::System.Text.Encoding.UTF8.GetBytes("hello back."));
            }
            public override void OnTellLocalObj(IModulePipeline from, object obj)
            {
                throw new NotImplementedException();
            }
        }

    }

}
