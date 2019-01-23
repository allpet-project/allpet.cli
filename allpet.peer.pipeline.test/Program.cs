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
            var system = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            system.RegistModule("mainloop", new Module_Loop());
            system.Start();
            var pipe = system.GetPipeline(null, "this/mainloop");
            while (pipe.IsVaild)
            {
                System.Threading.Thread.Sleep(100);
            }
        }
    }
    class Module_Loop : AllPet.Pipeline.Module
    {
        public override void Dispose()
        {
            //如果要重写dispose，必须执行base.Dispose
            base.Dispose();
        }
        public override void OnStart()
        {
            //不要堵死OnStart函數
            System.Threading.ThreadPool.QueueUserWorkItem((s) =>
            {
                TestLoop();
            });
        }
        public override void OnTell(IModulePipeline from, byte[] data)
        {
        }

        async void TestLoop()
        {
            while (true)
            {
                Console.Write(">");
                var line = Console.ReadLine();
                if (line == "1")
                {
                    await LocalTest();//這個測試創建兩個本地actor，并讓他們通訊
                }
                if (line == "2")
                {
                    await RemoteTest();
                }
                if (line == "exit")
                {
                    this.Dispose();//這將會導致這個模塊關閉
                    break;
                }
            }
        }
        public async Task RemoteTest()
        {
            //服務器端
            var systemR = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            systemR.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            systemR.OpenListen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 8888));
            systemR.RegistModule("hello", new Hello());
            systemR.RegistModule("hello2", new Hello());
            systemR.Start();


            //客戶端
            var systemL = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            systemL.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            systemL.Start();

            var remote = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8888);

            //連接
            var systemref = await systemL.Connect(remote);

            //連接以後可以直接獲取一個遠程管道
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
                    systemR.CloseListen();
                    systemR.CloseNetwork();
                    systemR.Dispose();
                    systemL.Dispose();
                    break;
                }
                actor.Tell(System.Text.Encoding.UTF8.GetBytes(line));

            }
        }
        public async Task LocalTest()
        {
            var system = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            system.RegistModule("hello", new Hello());//actor习惯，连注册这个活都丢线程池，我这里简化一些
            system.RegistModule("hello2", new Hello2());//actor习惯，连注册这个活都丢线程池，我这里简化一些
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
                    //不能这样粗暴关闭的，关闭应该由actor内部发起
                    system.Dispose();
                    break;
                }
                actor.Tell(System.Text.Encoding.UTF8.GetBytes(line));

            }
        }
    }

}

class Hello : Module
{
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
        Console.WriteLine("Hello:" + global::System.Text.Encoding.UTF8.GetString(data));
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

}
