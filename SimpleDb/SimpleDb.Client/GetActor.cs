using AllPet.Pipeline;
using SimplDb.Protocol.Sdk;
using SimplDb.Protocol.Sdk.Message;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SimpleDb.Client
{
    public class GetActor : Module
    {
        public GetActor() : base(false)
        {
        }
        public override void OnStart()
        {
            
        }
        public override void OnTell(IModulePipeline from, byte[] data)
        {
            if (from == null)
            {
                var actor = this.GetPipeline("127.0.0.1:8888/simpledb");
                
                actor.Tell(data);
            }
            else
            {
                Console.WriteLine("Remote :Back length="+ data.Length);
            }
        }
    }
}
