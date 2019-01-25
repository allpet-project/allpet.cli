using System;

namespace SimplDb.Protocol.Sdk
{
    public class BaseDomain
    {
        public BaseDomain()
        {
            
        }

        protected void ApplyChange(ICommand command)
        {
            dynamic d = this;

            d.Handle(Converter.ChangeTo(command, command.GetType()));
        }
    }
}
