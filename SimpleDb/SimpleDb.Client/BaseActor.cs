using Akka.Actor;
using SimplDb.Protocol.Sdk.ActorMessage;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDb.Client
{
    public class BaseActor:ReceiveActor, ILogReceive
    {
        private ActorSelection _server;

        public BaseActor(string path)
        {
            _server = Context.ActorSelection(path);
            Receive<SimpleDbMessage>((msg) =>
            {
                _server.Tell(msg);
            });

        }
    }
}
