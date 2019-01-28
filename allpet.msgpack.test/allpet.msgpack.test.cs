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

            byte[] bytes = null;
            var serializer = MsgPack.Serialization.MessagePackSerializer.Get<MessagePackObjectDictionary>();
            //上面的方法比下面的少做一次转换，可能更合适
            //MsgPack.MessagePackObject obj = new MsgPack.MessagePackObject(dict);
            //var serializer = MsgPack.Serialization.MessagePackSerializer.Get<MessagePackObjectDictionary>();

            DateTime begin = DateTime.Now;
            for (var i = 0; i < count; i++)
            {
                bytes = serializer.PackSingleObject(dict);

            }
            var time1 = DateTime.Now;
            {
                var time = (time1 - begin).TotalSeconds;
                var speed = count / time;
                Console.WriteLine("msgpack.pack bytes=" + bytes.Length + ", time=" + time + ", speed(c/s)=" + speed);
            }
            for (var i = 0; i < count; i++)
            {
                var obj2 = MsgPack.Serialization.MessagePackSerializer.UnpackMessagePackObject(bytes);
                //看起来上面的方法比下面这个更快一点点
                //var obj2 = serializer.UnpackSingleObject(bytes);
                var num = obj2.AsDictionary()["key2"].AsInt32();
            }
            var time2 = DateTime.Now;
            {
                var time = (time2 - time1).TotalSeconds;
                var speed = count / time;
                Console.WriteLine("msgpack.unpack bytes=" + bytes.Length + ", time=" + time + ", speed(c/s)=" + speed);
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
                using (var ms = new System.IO.MemoryStream())
                {
                    var bswrite = new Newtonsoft.Json.Bson.BsonWriter(ms);
                    jsonSerializer.Serialize(bswrite, obj);
                    bytes = ms.ToArray();
                }
            }
            DateTime time1 = DateTime.Now;

            {
                var time = (time1 - begin).TotalSeconds;
                var speed = count / time;
                Console.WriteLine("newtonsoft.json.pack bytes=" + bytes.Length + ", time=" + time + ", speed(c/s)=" + speed);
            }
            using (var ms = new System.IO.MemoryStream(bytes))
            {
                for (var i = 0; i < count; i++)
                {
                    ms.Seek(0, System.IO.SeekOrigin.Begin);
                    var jsreader = new Newtonsoft.Json.Bson.BsonReader(ms);

                    var jobj = jsonSerializer.Deserialize(jsreader) as Newtonsoft.Json.Linq.JToken;
                    int num = (int)jobj["key2"];
                }
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
