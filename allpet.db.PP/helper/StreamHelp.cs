namespace allpet.db.PP
{
    class StreamHelp
    {
        static byte[] buf = new byte[255];
        public static void readLenAndByte(System.IO.Stream stream,out byte[] data)
        {
            stream.Read(buf, 0, 1);
            var idlen = buf[0];
            data = new byte[idlen];
            stream.Read(data, 0, idlen);
        }

        public static void writeLenAndByte(System.IO.Stream stream,byte[] data)
        {
            stream.WriteByte((byte)data.Length);
            stream.Write(data, 0, data.Length);
        }

        public static int readByte(byte[] data)
        {
            using (var ms = new System.IO.MemoryStream(data))
            {
                return ms.ReadByte();
            }

        }
    }
}
