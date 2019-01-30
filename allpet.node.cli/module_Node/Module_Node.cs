using System;
using System.Collections.Generic;
using System.Text;
using AllPet.Pipeline;
using MsgPack;

namespace AllPet.nodecli
{
    class Module_Node : AllPet.Pipeline.MsgPack.Module_MsgPack
    {
        public Module_Node(AllPet.Common.ILogger logger, Newtonsoft.Json.Linq.JObject configJson) : base(true)
        {

        }
        public override void OnStart()
        {
        }

        public override void OnTell(IModulePipeline from, MessagePackObject? obj)
        {
        }
    }
}
