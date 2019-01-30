using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AllPet.Pipeline;

namespace allpet.peer.pipeline.test.test
{
    class test4_perform
    {
        public static bool betesting = false;
        public static async Task Test()
        {
            var systemR = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1(new AllPet.Common.Logger());
            systemR.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            systemR.OpenListen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 8888));
            systemR.RegistModule("recv", new Recv());
            systemR.Start();

            //客戶端
            var systemL = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1(new AllPet.Common.Logger());
            systemL.RegistModule("send", new Send());
            systemL.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            systemL.Start();

            //連接
            var remote = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8888);
            var systemref = await systemL.ConnectAsync(remote);
            var module = systemL.GetPipeline(null, "this/send");

            while (true)
            {
                if(!betesting)
                {
                    Console.Write("通信速率测试(1=进行测试，exit=退出)：>");
                    var line = Console.ReadLine();
                    if (line == "")
                        continue;
                    if (line == "exit")
                    {
                        systemR.CloseListen();
                        systemR.CloseNetwork();
                        systemR.Dispose();
                        systemL.Dispose();
                        break;
                    }
                    else if (line == "1")
                    {
                        betesting = true;
                        module.Tell(new byte[1]);
                    }
                }
                else
                {
                    //---------------------影响测试效率
                    //Console.Write("exit=退出)：>");
                    var line = Console.ReadLine();
                    //if (line == "exit")
                    //{
                    //    systemR.CloseListen();
                    //    systemR.CloseNetwork();
                    //    systemR.Dispose();
                    //    systemL.Dispose();
                    //    break;
                    //}
                }

            }
        }


        public class Send : AllPet.Pipeline.Module
        {
            public bool beTesting = false;
            private int msg_byte = 1024;
            private int sendIndex = 0;
            public override void OnStart()
            {
            }
            public override void OnTell(IModulePipeline from, byte[] data)
            {
                //var str = System.Text.Encoding.UTF8.GetString(data);
                //var recv = this.GetPipeline("127.0.0.1:8888/recv");
                
                if(data.Length==1)//开始测试
                {
                    beTesting = true;
                    this.perTest();
                }
                else if (data.Length == 2)//测试迭代
                {
                    if (sendIndex <= 10)
                    {
                        this.perTest();
                    }
                    else
                    {
                        test4_perform.betesting = false;
                    }
                }
            }
            public override void OnTellLocalObj(IModulePipeline from, object obj)
            {
                throw new NotImplementedException();
            }
            private void perTest()
            {
                sendIndex++;
                var recv = this.GetPipeline("127.0.0.1:8888/recv");
                //recv.Tell(new byte[1]);
                for (int i = 0; i < 100000; i++)
                {
                    recv.Tell(new byte[msg_byte]);
                }
                //recv.Tell(new byte[2]);
                msg_byte += 2 * 1024;
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
                if (recvcount == 0)
                    begin = DateTime.Now;
                recvcount++;
                recvbytes += data.Length;
                if(recvcount==100000)
                {//接收完毕
                    var end = DateTime.Now;
                    var time = (end - begin).TotalSeconds;
                    double mbs = recvbytes / (1024.0 * 1024.0 * time);

                    Console.WriteLine("发Msg次数：{0}  msg大小：{1}kb  时间{2}s  速率：{3}m/s   ", recvcount, data.Length/1024, time, mbs);
                    {
                        recvcount = 0;
                        recvbytes = 0;
                    }
                    from.Tell(new byte[2]);
                }
            }
            public override void OnTellLocalObj(IModulePipeline from, object obj)
            {
                throw new NotImplementedException();
            }
        }

    }
}
