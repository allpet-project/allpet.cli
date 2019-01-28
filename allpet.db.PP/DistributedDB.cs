using AllPet.db.simple;
using AllPet.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace allpet.db.PP
{
    public class DistributedDB : Module
    {
        DB db = new DB();
        Dictionary<MsgEnum, BaseOrder> actionFactory = new Dictionary<MsgEnum, BaseOrder>();
        public DistributedDB(bool MultiThreadTell = true) : base(MultiThreadTell)
        {

        }

        public override void OnStart()
        {
            this.registeAction(MsgEnum.Put,new PutOrder(db));
        }

        public override void OnTell(IModulePipeline from, byte[] data)
        {
            BaseMsg msg=MsgHelper.DecodeMessage(data);
            if(msg!=null)
            {
                if(this.actionFactory.ContainsKey(msg.msgtype))
                {
                    this.actionFactory[msg.msgtype].handle(from, msg);
                }
            }
        }
        public override void OnTellLocalObj(IModulePipeline from, object obj)
        {
            throw new NotImplementedException();
        }
        void registeAction(MsgEnum type, BaseOrder actionInc)
        {
            this.actionFactory.Add(type, actionInc);
        }
    }

    public abstract class BaseOrder
    {
        protected DB db;
        public BaseOrder(DB dB)
        {
            this.db = dB;
        }
        public abstract void handle(IModulePipeline from, BaseMsg msg);
    }
}
