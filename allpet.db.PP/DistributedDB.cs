using AllPet.db.simple;
using AllPet.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace allpet.db.PP
{
    public class DistributedDB : Pipeline
    {
        DB db = new DB();
        Dictionary<ActionEnum, BaseAction> actionFactory = new Dictionary<ActionEnum, BaseAction>();

        public DistributedDB(IPipelineSystem system) : base(system)
        {
            this.registeAction(ActionEnum.Put,new PutAction(db));
        }
        public override void OnTell(IPipelineRef from, byte[] data)
        {
            ActionEnum actiontype= (ActionEnum)StreamHelp.readByte(data);
            if(this.actionFactory.ContainsKey(actiontype))
            {
                this.actionFactory[actiontype].handle(from,data);
            }
        }

        void registeAction(ActionEnum action, BaseAction actionInc)
        {
            this.actionFactory.Add(action, actionInc);
        }
    }

    public enum ActionEnum
    {
        Put,
        Delete,
        CreateTable,
        DeleteTable
    }

    public abstract class BaseAction
    {
        protected DB db;
        public BaseAction(DB dB)
        {
            this.db = dB;
        }
        public abstract void handle(IPipelineRef from, byte[] data);
    }
}
