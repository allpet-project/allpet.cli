using System;
using System.Collections.Generic;
using System.Text;

namespace AllPet.Pipeline
{
    public class PipelineSystem
    {
        public static ISystem CreatePipelineSystemV1(AllPet.Common.ILogger logger)
        {
            return new PipelineSystemV1(logger);
        }
    }
}
