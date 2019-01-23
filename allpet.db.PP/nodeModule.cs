using AllPet;
using AllPet.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace allpet.db.PP
{
    public class NodeModule : Module
    {
        Dictionary<string, IModulePipeline> DataServerDic = new Dictionary<string, IModulePipeline>();
        List<string> serverPath = new List<string>();

        public NodeModule(bool MultiThreadTell = true) : base(MultiThreadTell)
        {

        }

        public override void OnStart()
        {

        }
        public override void OnTell(IModulePipeline from, byte[] data)
        {
            var database = getServer(data);
            database.Tell(data);
        }

        /// <summary>
        /// 根据data得到挑选 dataserver
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        IModulePipeline getServer(byte[] data)
        {
            int hash = Helper_NEO.CalcHash256(data).GetHashCode() % serverPath.Count;
            var path = serverPath[hash];
            return this.DataServerDic[path];
        }
    }

    
}
