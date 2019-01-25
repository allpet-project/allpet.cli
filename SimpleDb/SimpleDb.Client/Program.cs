using Newtonsoft.Json;
using SimplDb.Protocol.Sdk;
using SimplDb.Protocol.Sdk.Message;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace SimpleDb.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client Hello World!");
            TestLoop();
            Console.ReadLine();
        }

        static void TestLoop()
        {
            while (true)
            {
                Console.WriteLine("0.TestNet>");
                Console.WriteLine("1.CreateTable>");
                Console.WriteLine("2.PutDirect>");
                Console.WriteLine("3.GetDirect>");
                Console.WriteLine("4.PutUInt64>");
                Console.WriteLine("5.DeleteDirect>");
                Console.WriteLine("6.DeleteTable>");
                Console.WriteLine("7.GetUInt64>");
                var line = Console.ReadLine();
                if (line == "0")
                {
                    TestNetTransfer();
                }
                if (line == "1")
                {
                    CreateTable();
                }
                if (line == "2")
                {                   
                    PutDirect();
                }
                if (line == "3")
                {
                    GetDirect();
                }
                if (line == "4")
                {
                    PutUInt64();
                }
                if (line == "5")
                {
                    DeleteDirect();
                }
                if (line == "6")
                {
                    DeleteTable();
                }
                if (line == "7")
                {
                    GetUInt64();
                }
            }
        }
        private static void TestNetTransfer()
        {
            var systemL = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            systemL.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            var remote = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8888);
            var systemref = systemL.Connect(remote).Result;
            systemL.Start();
            var actor = systemL.GetPipeline(null, "127.0.0.1:8888/simpledb");
            {
                for (var i = 0; i < 10000; i++)
                {
                    actor.Tell(new byte[8096]);
                }
            }
            systemL.CloseListen();
            systemL.CloseNetwork();
            systemL.Dispose();
        }
        private static void CreateTable()
        {
            var systemL = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            systemL.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            var remote = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8888);
            var systemref = systemL.Connect(remote).Result;
            systemL.Start();
            var actor = systemL.GetPipeline(null, "127.0.0.1:8888/simpledb");
            {              

                CreatTableCommand command = new CreatTableCommand()
                {
                     TableId = new byte[] { 0x04, 0x02, 0x03 },
                     Data  = new byte[8000]
                };
                var bytes = ProtocolFormatter.Serialize<CreatTableCommand>(Method.CreateTable, command);
                actor.Tell(bytes);
            }
            systemL.CloseListen();
            systemL.CloseNetwork();
            systemL.Dispose();
        }
        private static void PutDirect()
        {
            var systemL = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            systemL.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            var remote = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8888);
            var systemref = systemL.Connect(remote).Result;
            systemL.Start();
            var actor = systemL.GetPipeline(null, "127.0.0.1:8888/simpledb");
            {
                PutDirectCommand command = new PutDirectCommand()
                {
                    TableId = new byte[] { 0x03, 0x02, 0x03 },
                    Key = new byte[] { 0x10, 0x10 },
                    Data = new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff }
                };
                var bytes = ProtocolFormatter.Serialize< PutDirectCommand>(Method.PutDirect,command);
                actor.Tell(bytes);
            }
            systemL.CloseListen();
            systemL.CloseNetwork();
            systemL.Dispose();
        }
        private async static void GetDirect()
        {
            var server = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            server.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            server.RegistModule("me", new GetActor());
            server.Start();
            var remote = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8888);

            //連接
            var systemref = await server.Connect(remote);
            
            var actor = server.GetPipeline(null, "this/me");
            GetDirectCommand command = new GetDirectCommand()
            {
                TableId = new byte[] { 0x03, 0x02, 0x03 },
                Key = new byte[] { 0x10, 0x10 }
            };
            var bytes = ProtocolFormatter.Serialize<GetDirectCommand>(Method.GetDirect, command);
            actor.Tell(bytes);
            //var actor = server.GetPipeline(null, "127.0.0.1:8888/get");
            //{
            //    MemoryStream ms = new MemoryStream();
            //    GetDirectCommand command = new GetDirectCommand()
            //    {
            //        TableId = new byte[] { 0x02, 0x02, 0x03 },
            //        Key = new byte[] { 0x13, 0x13 }
            //    };
            //    BinaryFormatter bf = new BinaryFormatter();
            //    bf.Serialize(ms, command);
            //    actor.Tell(ms.ToArray());
            //}
            server.CloseListen();
            server.CloseNetwork();
            server.Dispose();
        }        

        private static void PutUInt64()
        {
            var systemL = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            systemL.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            var remote = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8888);
            var systemref = systemL.Connect(remote).Result;
            systemL.Start();
            var actor = systemL.GetPipeline(null, "127.0.0.1:8888/simpledb");
            {
                PutUInt64Command command = new PutUInt64Command()
                {
                    TableId = new byte[] { 0x02, 0x02, 0x03 },
                    Key = new byte[] { 0x14, 0x13 },
                    Data = 18446744073709551614
                };
                var bytes = ProtocolFormatter.Serialize<PutUInt64Command>(Method.PutUint64, command);
                actor.Tell(bytes);
            }
            systemL.CloseListen();
            systemL.CloseNetwork();
            systemL.Dispose();
        }
        private async static void GetUInt64()
        {
            var server = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            server.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            server.RegistModule("me", new GetUInt64Actor());
            server.Start();
            var remote = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8888);

            //連接
            var systemref = await server.Connect(remote);

            var actor = server.GetPipeline(null, "this/me");
            GetUint64Command command = new GetUint64Command()
            {
                TableId = new byte[] { 0x02, 0x02, 0x03 },
                Key = new byte[] { 0x14, 0x13 }
            };
            var bytes = ProtocolFormatter.Serialize<GetUint64Command>(Method.GetUint64, command);
            actor.Tell(bytes);

            server.CloseListen();
            server.CloseNetwork();
            server.Dispose();
        }

        private static void DeleteDirect()
        {
            var systemL = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            systemL.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            var remote = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8888);
            var systemref = systemL.Connect(remote).Result;
            systemL.Start();
            var actor = systemL.GetPipeline(null, "127.0.0.1:8888/simpledb");
            {
                DeleteCommand command = new DeleteCommand()
                {
                    TableId = new byte[] { 0x02, 0x02, 0x03 },
                    Key = new byte[] { 0x13, 0x13 },
                };
                var bytes = ProtocolFormatter.Serialize<DeleteCommand>(Method.Delete, command);
                actor.Tell(bytes);
            }

            systemL.CloseListen();
            systemL.CloseNetwork();
            systemL.Dispose();
        }

        private static void DeleteTable()
        {
            var systemL = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            systemL.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            var remote = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8888);
            var systemref = systemL.Connect(remote).Result;
            systemL.Start();
            var actor = systemL.GetPipeline(null, "127.0.0.1:8888/simpledb");
            {
                DeleteTableCommand command = new DeleteTableCommand()
                {
                    TableId = new byte[] { 0x02, 0x02, 0x03 }
                };
                var bytes = ProtocolFormatter.Serialize<DeleteTableCommand>(Method.DeleteTable, command);
                actor.Tell(bytes);
            }

            systemL.CloseListen();
            systemL.CloseNetwork();
            systemL.Dispose();
        }
    }
}
