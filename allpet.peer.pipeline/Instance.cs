using System;
using System.Collections.Generic;
using System.Text;

namespace AllPet.Pipeline
{
    public class Instance
    {
        public static IPipelineSystem CreateActorSystem()
        {
            return new System();
        }
    }
}
