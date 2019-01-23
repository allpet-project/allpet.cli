using System;

namespace allpet.db.PP.test
{
    class Program
    {
        static void Main(string[] args)
        {
            var systemR = AllPet.Pipeline.Instance.CreateActorSystem();
            systemR.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            systemR.OpenListen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 8888));
            systemR.RegistPipeline("nodeModule", new NodeModule(systemR));


            var systemL = AllPet.Pipeline.Instance.CreateActorSystem();
            systemL.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            var remote = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 8888);
            var systemref = systemL.Connect(remote).Result;


            var actor = systemL.GetPipeline(null, "127.0.0.1:8888/nodeModule");
            {
                actor.Tell(System.Text.Encoding.UTF8.GetBytes("yeah very good."));
            }
        }
    }
}
