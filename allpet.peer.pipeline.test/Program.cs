using System;
using System.Threading.Tasks;
using AllPet.Pipeline;

namespace AllPet.Pipeline.test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("pipeline test.");
            TestLoop();
            while (true)
            {
                System.Threading.Thread.Sleep(100);
            }
        }
        async static void TestLoop()
        {
            while (true)
            {
                Console.Write(">");
                var line = Console.ReadLine();
                if (line == "1")
                {
                    await LocalTest();
                }
                if (line == "2")
                {
                   await RemoteTest();
                }
            }
        }
        public async static Task RemoteTest()
        {
            var systemR = AllPet.Pipeline.Instance.CreateActorSystem();
            systemR.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            systemR.OpenListen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 8888));
            systemR.RegistPipeline("hello", new Hello());


            var systemL = AllPet.Pipeline.Instance.CreateActorSystem();
            systemL.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            var remote = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8888);
            var systemref = await systemL.Connect(remote);
            var actor = systemL.GetPipeline(null, "127.0.0.1:8888/hello");
            {
                actor.Tell(System.Text.Encoding.UTF8.GetBytes("yeah very good."));
            }
            while (true)
            {
                Console.Write("1.remote>");
                var line = Console.ReadLine();
                if (line == "exit")
                {
                    systemR.UnRegistPipeline("hello");
                    systemR.Dispose();
                    systemL.Dispose();
                    break;
                }
                actor.Tell(System.Text.Encoding.UTF8.GetBytes(line));

            }
        }
        public async static Task LocalTest()
        {
            var system = AllPet.Pipeline.Instance.CreateActorSystem();
            system.RegistPipeline("hello", new Hello());//actor习惯，连注册这个活都丢线程池，我这里简化一些

            var actor = system.GetPipeline(null, "this/hello");
            {
                actor.Tell(System.Text.Encoding.UTF8.GetBytes("yeah very good."));
            }
            while (true)
            {
                Console.Write("1.local>");
                var line = Console.ReadLine();
                if (line == "exit")
                {
                    system.UnRegistPipeline("hello");
                    system.Dispose();
                    break;
                }
                actor.Tell(System.Text.Encoding.UTF8.GetBytes(line));

            }
        }
    }

    class Hello : AllPet.Pipeline.IPipelineInstance
    {
        public void OnTell(IPipelineRef from, byte[] data)
        {
            Console.WriteLine("recv:" + System.Text.Encoding.UTF8.GetString(data));
        }
    }
}
