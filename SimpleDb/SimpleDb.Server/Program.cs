using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.Configuration;
using SimpleDb.Server.Actor;
using System;
using System.IO;

namespace SimpleDb.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SimpleDb.Server Start.....");
            var config = ConfigurationFactory.ParseString(@"
            akka {  
                actor {
                    provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                }
                remote {
                    helios.tcp {
                        transport-class = ""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
                        applied-adapters = []
                        transport-protocol = tcp
                        port = 8081
                        hostname = localhost
                    }
                }
            }
            ");
            using (var system = ActorSystem.Create("MyServer", config))
            {
                system.ActorOf<SimpledbActor>("SimpleDb");

                Console.ReadLine();
            }
        }
    }
    
}
