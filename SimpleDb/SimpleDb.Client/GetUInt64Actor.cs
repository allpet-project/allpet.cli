using AllPet.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDb.Client
{
    public class GetUInt64Actor : Module
    {
        public GetUInt64Actor() : base(false)
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
                var longValue = BitConverter.ToUInt64(data);
                Console.WriteLine("Remote :Back length=" + longValue);
            }
        }
    }
}
