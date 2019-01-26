using Akka.Actor;
using SimplDb.Protocol.Sdk.ActorMessage;
using SimplDb.Protocol.Sdk.Message.BackMessage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SimpleDb.Client
{
    public class GetActor : ReceiveActor, ILogReceive
    {
        private ActorSelection _server;

        public GetActor(string path)
        {
            _server = Context.ActorSelection(path);
            Receive<SimpleDbMessage>((msg) =>
            {
                _server.Tell(msg);
            });

            Receive<ByteValueMessage>((msg) =>
            {
                Console.WriteLine("GetActor Back :"+ msg.msg.Length);
            });


        }
    }
    
}
