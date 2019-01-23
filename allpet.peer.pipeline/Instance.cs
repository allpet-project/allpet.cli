using System;
using System.Collections.Generic;
using System.Text;

namespace AllPet.Pipeline
{
    public class PipelineSystem
    {
        public static IPipelineSystem CreatePipelineSystemV1()
        {
            return new PipelineSystemV1();
        }
    }
}
