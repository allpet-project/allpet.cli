using AllPet.Pipeline;
using SimplDb.Protocol.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SimpleDb.Server.Actor
{
    public class SimpleDbModule : Module
    {
        ulong recvlen = 0;
        ulong recvcount = 0;
        DateTime begin;
        protected AllPet.db.simple.DB simpledb = new AllPet.db.simple.DB();
        public SimpleDbModule() : base(false)
        {
        }
        public override void OnStart()
        {
            var dbPath = SimpleDbConfig.GetInstance().GetDbSetting();
            simpledb.Open(dbPath, true);
        }
        public override void OnTell(IModulePipeline from, byte[] data)
        {
            recvlen += (uint)data.Length;
            if (recvcount == 0)
            {
                begin = DateTime.Now;
            }
            recvcount++;
            if (recvcount % 5000 == 0)
            {
                var end = DateTime.Now;
                Console.WriteLine("recv bytes:" + recvlen + " span=" + (end - begin));
            }

            //Console.WriteLine("SimpleDbModule");
            //var command  = ProtocolFormatter.Deserialize(data);

            //ServerDomain domain = new ServerDomain(this.simpledb, from);
            //domain.ExcuteCommand(command);

        }
    }
}
