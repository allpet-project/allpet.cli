using AllPet;
using AllPet.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace allpet.db.PP
{
    public class NodeModule : Pipeline
    {
        Dictionary<string, IPipelineRef> DataServerDic = new Dictionary<string, IPipelineRef>();
        List<string> serverPath = new List<string>();

        public NodeModule(IPipelineSystem system) : base(system)
        {

        }

        public override void OnTell(IPipelineRef from, byte[] data)
        {
            var database = getServer(data);
            database.Tell(data);
        }

        /// <summary>
        /// 根据data得到挑选 dataserver
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        IPipelineRef getServer(byte[] data)
        {
            int hash = Helper_NEO.CalcHash256(data).GetHashCode() % serverPath.Count;
            var path = serverPath[hash];
            return this.DataServerDic[path];
        }
    }

    
}
