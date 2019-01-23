using AllPet.db.simple;
using AllPet.Pipeline;

namespace allpet.db.PP
{
    public class PutAction : BaseAction
    {
        public PutAction(DB dB) : base(dB)
        {
            
        }
        public override void handle(IModulePipeline from, byte[] data)
        {
            ActionData acdata=ActionData.FromRaw(data);
            db.PutDirect(acdata.tableid,acdata.key,acdata.finnaldata);
        }

        class ActionData
        {
            public ActionEnum action;
            public byte[] tableid;
            public byte[] key;
            public byte[] finnaldata;

            public byte[] ToBytes()
            {
                using (var ms = new System.IO.MemoryStream())
                {
                    Pack(ms);
                    return ms.ToArray();
                }
            }
            public static ActionData FromRaw(byte[] data)
            {
                using (var ms = new System.IO.MemoryStream(data))
                {
                    return ActionData.UnPack(ms);
                }
            }

            void Pack(System.IO.Stream stream)
            {
                stream.WriteByte((byte)action);
                StreamHelp.writeLenAndByte(stream,tableid);
                StreamHelp.writeLenAndByte(stream, key);
                StreamHelp.writeLenAndByte(stream, finnaldata);
            }
            static ActionData UnPack(System.IO.Stream stream)
            {
                ActionData data = new ActionData();
                byte[] buf = new byte[255];
                stream.Read(buf, 0, 1);

                StreamHelp.readLenAndByte(stream,out data.tableid);
                StreamHelp.readLenAndByte(stream, out data.key);
                StreamHelp.readLenAndByte(stream, out data.finnaldata);

                return data;
            }
        }
    }
}
