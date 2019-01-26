using Akka.Actor;
using SimplDb.Protocol.Sdk.ActorMessage;
using SimplDb.Protocol.Sdk.ActorMessage.BackMessage;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDb.Client
{
    public class GetUInt64Actor : ReceiveActor, ILogReceive
    {
        private ActorSelection _server;

        public GetUInt64Actor(string path)
        {
            _server = Context.ActorSelection(path);
            Receive<SimpleDbMessage>((msg) =>
            {
                _server.Tell(msg);
            });

            Receive<ULongValueMessage>((msg) =>
            {
                Console.WriteLine("GetUInt64Actor Back :" + msg.msg);
            });


        }
    }
}
