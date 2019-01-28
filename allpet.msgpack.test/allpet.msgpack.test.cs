using MsgPack;
using Newtonsoft.Json;
using System;

namespace bintest
{
    public class Class1
    {
        static void Main(params string[] args)
        {
            int count = 1000000;

            TestMsgPack(count);
            TestBson(count);
            Console.ReadLine();
        }
        static ArraySegment<byte> PackSeg(MessagePackObject obj)
        {
            using (var pack = Packer.Create(new byte[256], true, PackerCompatibilityOptions.None))
            {
                obj.PackToMessage(pack, null);
                return pack.GetResultBytes();
            }
        }
        static MessagePackObject UnPackSeg(ArraySegment<byte> bytes)
        {
            using (var unpack = Unpacker.Create(bytes.Array, bytes.Offset))
            {
                if (unpack.ReadObject(out MessagePackObject obj))
                {
                    return obj;
                }
                return MessagePackObject.Nil;
            }
        }

        static byte[] Pack(MessagePackObject obj)
        {
            using (var pack = Packer.Create(new byte[256], true, PackerCompatibilityOptions.None))
            {
                obj.PackToMessage(pack, null);
                return pack.GetResultBytes().ToArray();
            }
            //return serializer.PackSingleObjectAsBytes(obj).ToArray();
            // return serializer.PackSingleObject(obj);
        }
        static MessagePackObject UnPack(byte[] bytes)
        {
            using (var unpack = Unpacker.Create(bytes))
            {
                if (unpack.ReadObject(out MessagePackObject obj))
                {
                    return obj;
                }
                return MessagePackObject.Nil;
            }
            //return serializer.UnpackSingleObject(bytes);
        }
        private static void TestMsgPack(int count)
        {
            //msgpack
            MessagePackObjectDictionary dict = new MessagePackObjectDictionary();
            //var key1 = new MsgPack.MessagePackObject("key1");
            //var value1 = new MsgPack.MessagePackObject(new byte[] { 1, 2, 3, 4, 5 });
            dict["key1"] = new byte[] { 1, 2, 3, 4, 5 };
            var key2 = new MsgPack.MessagePackObject("key2");
            dict[key2] = 12345;
            dict["adfadf"] = "adfasdf";
            ArraySegment<byte> bytes;
            //byte[] bytes = null;
            //var serializer = MsgPack.Serialization.MessagePackSerializer.Get<MessagePackObjectDictionary>();
            //上面的方法比下面的少做一次转换，可能更合适
            MsgPack.MessagePackObject obj = new MsgPack.MessagePackObject(dict);
            DateTime begin = DateTime.Now;
            for (var i = 0; i < count; i++)
            {

                bytes = PackSeg(obj);
                //bytes = serializer.PackSingleObject(obj);

            }
            var time1 = DateTime.Now;
            {
                var time = (time1 - begin).TotalSeconds;
                var speed = count / time;
                Console.WriteLine("msgpack.pack bytes=" + bytes.Count + ", time=" + time + ", speed(c/s)=" + speed);
            }
            for (var i = 0; i < count; i++)
            {
                //var obj2 =serializer.UnpackSingleObject(bytes);
                var obj2 = UnPackSeg(bytes);
                //看起来上面的方法比下面这个更快一点点
                //var obj2 = serializer.UnpackSingleObject(bytes);
                var num = obj2.AsDictionary()["key2"].AsInt32();
            }
            var time2 = DateTime.Now;
            {
                var time = (time2 - time1).TotalSeconds;
                var speed = count / time;
                Console.WriteLine("msgpack.unpack bytes=" + bytes.Count + ", time=" + time + ", speed(c/s)=" + speed);
            }
        }
        public static byte[] PackBson(Newtonsoft.Json.Linq.JToken json)
        {

            using (var ms = new System.IO.MemoryStream())
            {
                using (var bswrite = new Newtonsoft.Json.Bson.BsonWriter(ms))
                {
                    json.WriteTo(bswrite);

                    var bytes = ms.ToArray();
                    return bytes;
                }
            }
        }

        public static Newtonsoft.Json.Linq.JToken UnPackBson(byte[] bytes)
        {
            using (var ms = new System.IO.MemoryStream(bytes))
            {
                using (var bsreader = new Newtonsoft.Json.Bson.BsonReader(ms))
                {
                    return Newtonsoft.Json.Linq.JToken.ReadFrom(bsreader);
                }
            }
        }
        private static void TestBson(int count)
        {
            Newtonsoft.Json.Linq.JObject obj = new Newtonsoft.Json.Linq.JObject();
            obj["key1"] = new byte[] { 1, 2, 3, 4, 5 };
            obj["key2"] = 12345;
            obj["adfadf"] = "adfasdf";

            byte[] bytes = null;
            var jsonSerializer = new JsonSerializer();
            DateTime begin = DateTime.Now;

            for (var i = 0; i < count; i++)
            {
                bytes = PackBson(obj);
            }
            DateTime time1 = DateTime.Now;

            {
                var time = (time1 - begin).TotalSeconds;
                var speed = count / time;
                Console.WriteLine("newtonsoft.json.pack bytes=" + bytes.Length + ", time=" + time + ", speed(c/s)=" + speed);
            }
            for (var i = 0; i < count; i++)
            {
                var jobj = UnPackBson(bytes);
                int num = (int)jobj["key2"];
            }
            DateTime time2 = DateTime.Now;
            {
                var time = (time2 - time1).TotalSeconds;
                var speed = count / time;
                Console.WriteLine("newtonsoft.json.unpack bytes=" + bytes.Length + ", time=" + time + ", speed(c/s)=" + speed);
            }

        }

    }
}
