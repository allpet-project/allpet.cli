using System;
using System.Collections.Generic;
using System.Text;

namespace AllPet.peer.tcp 
{
    public class PeerV2
    {
        public static IPeer CreatePeer(AllPet.Common.ILogger logger)
        {
            return new light.asynctcp.ServerModule(logger);
        }
    }
}
