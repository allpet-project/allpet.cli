using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AllPet.Pipeline;

namespace allpet.peer.pipeline.test.test
{
    class test3_perform
    {
        public static async Task Test()
        {
            var systemR = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            systemR.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            systemR.OpenListen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 8888));
            systemR.RegistModule("recv", new Recv());
            systemR.Start();

            //客戶端
            var systemL = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            systemL.RegistModule("send", new Send());
            systemL.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            systemL.Start();

            //連接
            var remote = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8888);
            var systemref = await systemL.Connect(remote);
            var module = systemL.GetPipeline(null, "this/send");

            while (true)
            {
                Console.Write("3.perfrom>");
                var line = Console.ReadLine();
                if (line == "exit")
                {
                    systemR.CloseListen();
                    systemR.CloseNetwork();
                    systemR.Dispose();
                    systemL.Dispose();
                    break;
                }
                module.Tell(System.Text.Encoding.UTF8.GetBytes(line));
            }
        }
        public class Send : AllPet.Pipeline.Module
        {
            public override void OnStart()
            {
            }
            public override void OnTell(IModulePipeline from, byte[] data)
            {
                var str = System.Text.Encoding.UTF8.GetString(data);
                var recv = this.GetPipeline("127.0.0.1:8888/recv");
                recv.Tell(new byte[0]);
                Random r = new Random();

                if (str == "1k")
                {

                    for (var i = 0; i < 1000; i++)
                    {
                        recv.Tell(new byte[1024]);
                    }
                }
                if (str == "80m")
                {
                    List<byte[]> datatosend = new List<byte[]>();
                    int count = 10000;
                    for (var i = 0; i < count; i++)
                    {
                        byte[] _data = new byte[1024 * 8];
                        r.NextBytes(_data);
                        datatosend.Add(_data);
                    }

                    for (var i = 0; i < count; i++)
                    {
                        recv.Tell(datatosend[i]);
                    }
                }
                if (str == "800m")
                {
                    List<byte[]> datatosend = new List<byte[]>();
                    int count = 100000;
                    for(var i=0;i<count;i++)
                    {
                        byte[] _data = new byte[1024 * 8];
                        r.NextBytes(_data);
                        datatosend.Add(_data);
                    }

                    for (var i = 0; i < count; i++)
                    {
                        recv.Tell(datatosend[i]);
                    }
                }
            }
        }
        public class Recv : AllPet.Pipeline.Module
        {
            public override void OnStart()
            {

            }
            int recvcount = 0;
            int recvbytes = 0;

            DateTime begin;
            //默认多线程接收
            public override void OnTell(IModulePipeline from, byte[] data)
            {
                if ( data.Length==0)
                {
                    recvcount = 0;
                    recvbytes = 0;
                    return;
                }

                if (recvcount == 0)
                    begin = DateTime.Now;
                recvcount++;
                recvbytes += data.Length;
                if (recvcount % 1000 == 0)
                {
                    Console.WriteLine("recv count=" + recvcount + " size=" + recvbytes);
                    Console.WriteLine("time=" + (DateTime.Now - begin).TotalSeconds);
                }
            }
        }

    }
}
