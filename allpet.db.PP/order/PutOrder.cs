using AllPet.db.simple;
using AllPet.Pipeline;

namespace allpet.db.PP
{
    public class PutOrder : BaseOrder
    {
        public PutOrder(DB dB) : base(dB)
        {
            
        }
        public override void handle(IModulePipeline from, BaseMsg msg)
        {
            var realmsg = msg as Msg_put;
            db.PutDirect(realmsg.tableid, realmsg.key, realmsg.value);
        }
    }
}
