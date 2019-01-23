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
            var systemR = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            systemR.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            systemR.OpenListen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 8888));
            systemR.RegistPipeline("hello", new Hello(systemR));
            systemR.RegistPipeline("hello2", new Hello(systemR));
            systemR.Start();


            var systemL = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            systemL.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            systemL.Start();

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
            var system = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            system.RegistPipeline("hello", new Hello(system));//actor习惯，连注册这个活都丢线程池，我这里简化一些
            system.RegistPipeline("hello2", new Hello2(system));//actor习惯，连注册这个活都丢线程池，我这里简化一些

            system.Start();
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

    class Hello : Module
    {
        public Hello(ISystem system) : base(system,false)
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
            Console.WriteLine("Hello:" + global::System.Text.Encoding.UTF8.GetString(data));
        }
    }
    class Hello2 : Module
    {
        public Hello2(ISystem system) : base(system,false)
        {

        }
        public override void OnStart()
        {
        }
        public override void OnTell(IModulePipeline from, byte[] data)
        {
            Console.WriteLine("Hello2:" + global::System.Text.Encoding.UTF8.GetString(data));

            from.Tell(global::System.Text.Encoding.UTF8.GetBytes("hello back."));
        }
    }
}
