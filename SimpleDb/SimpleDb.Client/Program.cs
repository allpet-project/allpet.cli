using Akka.Actor;
using Akka.Configuration;
using Newtonsoft.Json;
using SimplDb.Protocol.Sdk;
using SimplDb.Protocol.Sdk.ActorMessage;
using SimplDb.Protocol.Sdk.Message;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace SimpleDb.Client
{
    class Program
    {
        static Config config;
        static void Main(string[] args)
        {
            Console.WriteLine("Client Hello World!");
            config = ConfigurationFactory.ParseString(@"
            akka {  
                actor {
                    provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                }
                remote {
                    helios.tcp {
                        transport-class = ""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
                        applied-adapters = []
                        transport-protocol = tcp
                        port = 0
                        hostname = localhost
                    }
                }
            }
            ");
            using (var system = ActorSystem.Create("MyClient", config))
            {
                string path1 = "akka.tcp://MyServer@localhost:8081/user/SimpleDb";


                Props prop = Props.Create(() => new BaseActor("akka.tcp://MyServer@localhost:8081/user/SimpleDb"));
                var greeting = system.ActorOf(prop);

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
                    var message = new SimpleDbMessage();
                    switch (line)
                    {
                        case "0":
                            break;
                        case "1":
                            message.command = new CreatTableCommand()
                            {
                                TableId = new byte[] { 0x05, 0x02, 0x03 },
                                Data = new byte[8000]
                            };
                            break;
                        case "2":
                            message.command = new PutDirectCommand()
                            {
                                TableId = new byte[] { 0x03, 0x02, 0x03 },
                                Key = new byte[] { 0x10, 0x10 },
                                Data = new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff }
                            };
                            break;
                        case "3":
                            message.command = new GetDirectCommand()
                            {
                                TableId = new byte[] { 0x03, 0x02, 0x03 },
                                Key = new byte[] { 0x10, 0x10 }
                            };
                            prop = Props.Create(() => new GetActor(path1));
                            greeting = system.ActorOf(prop);
                            break;
                        case "4":
                            message.command = new PutUInt64Command()
                            {
                                TableId = new byte[] { 0x02, 0x02, 0x03 },
                                Key = new byte[] { 0x14, 0x13 },
                                Data = 18446744073709551614
                            };
                            break;
                        case "5":
                            message.command = new DeleteCommand()
                            {
                                TableId = new byte[] { 0x02, 0x02, 0x03 },
                                Key = new byte[] { 0x13, 0x13 },
                            };
                            break;
                        case "6":
                            message.command = new DeleteTableCommand()
                            {
                                TableId = new byte[] { 0x02, 0x02, 0x03 }
                            };
                            break;
                        case "7":
                            message.command = new GetUint64Command()
                            {
                                TableId = new byte[] { 0x02, 0x02, 0x03 },
                                Key = new byte[] { 0x14, 0x13 }
                            };
                            prop = Props.Create(() => new GetUInt64Actor(path1));
                            greeting = system.ActorOf(prop);
                            break;

                    }
                    greeting.Tell(message);
                }
            }
        }
        
        
    }
}
