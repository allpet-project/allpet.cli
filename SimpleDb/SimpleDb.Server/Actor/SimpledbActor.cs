using Akka.Actor;
using SimplDb.Protocol.Sdk.ActorMessage;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDb.Server.Actor
{
    public class SimpledbActor : TypedActor, IHandle<SimpleDbMessage>
    {
        protected AllPet.db.simple.DB simpledb = new AllPet.db.simple.DB();
        public SimpledbActor()
        {
            var dbPath = SimpleDbConfig.GetInstance().GetDbSetting();
            simpledb.Open(dbPath, true);
        }
        public void Handle(SimpleDbMessage message)
        {
            Console.WriteLine("Hello world!");
            ServerDomain domain = new ServerDomain(this.simpledb,this.Sender);
            domain.ExcuteCommand(message.command);
        }
    }
}
