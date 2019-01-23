using AllPet.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace allpet.db.PP
{
    interface IDbClient
    {

    }

    class DbClient : Pipeline
    {
        public DbClient(IPipelineSystem system) : base(system)
        {
        }

        public override void OnTell(IPipelineRef from, byte[] data)
        {
            throw new NotImplementedException();
        }

        public void put()
        {

        }
    }
}
