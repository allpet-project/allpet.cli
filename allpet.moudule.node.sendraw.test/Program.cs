using AllPet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace allpet.moudule.node.sendraw.test
{
    class Program
    {
        static string rpcUrl = "http://127.0.0.1:30080";
        static void Main(string[] args)
        {
            Console.Write("Test Size>");
            var len = Console.ReadLine();
            List<string > list = new List<string>();
            int bytelen = int.Parse(len);
            for (int i = 0; i < 10000; i++)
            {
                var data = Get1KData(bytelen,i);
                list.Add(data);
            }

            Test1K(list);

            Console.ReadLine();
        }

        static async void Test1K(List<string> list)
        {
            foreach (var item in list)
            {
                byte[] postdata;
                var url = MakeRpcUrlPost(rpcUrl, "sendrawtransaction", out postdata, new string[] { list[0] });
                var result = await HttpPost(url, postdata);
            }
        }

        static string Get1KData(int bytelen,int index)
        {
            byte[] data = new byte[1024* bytelen];
            Random rand = new Random(65535);
            for (int i=0;i<data.Length;i++)
            {
                var value = rand.Next(0, 255);
                data[i] = (byte)value;
            }
            var magic  = BitConverter.GetBytes(index);
            for(int i=0;i< magic.Length;i++)
            {
                data[i] = magic[i];
            }
            if (index % 2 == 0)
            {
                var magiclen = magic.Length;
                magic = Encoding.UTF8.GetBytes("werwerwerwerwerwerwerwerwerwerwerwerwerwerwerwerwerfwerwerwe");
                for (int i = 0; i < magic.Length; i++)
                {
                    data[magiclen] = magic[i];
                    magiclen++;
                }
            }
            if(index % 5 == 0)
            {
                var datalen = data.Length-1;
                for (int i = 0; i < magic.Length; i++)
                {
                    data[datalen] = magic[i];
                    datalen--;
                }
                var magiclen = magic.Length;
                magic = Encoding.UTF8.GetBytes("zzzxZxzxcdfheryeryw34t34t65345t345t345t345345345345345345");
                for (int i = 0; i < magic.Length; i++)
                {
                    data[magiclen] = magic[i];
                    magiclen++;
                }
            }

            return Helper.Bytes2HexString(data);
        }
        public static string MakeRpcUrlPost(string url, string method, out byte[] data, params string[] _params)
        {
            //if (url.Last() != '/')
            //    url = url + "/";
            var json = new JObject();
            json["id"] = 1;
            json["jsonrpc"] = "1.0";
            json["method"] = method;
            StringBuilder sb = new StringBuilder();
            var array = new JArray();
            for (var i = 0; i < _params.Length; i++)
            {

                array.Add(_params[i]);
            }
            json["params"] = array;
            data = System.Text.Encoding.UTF8.GetBytes(json.ToString());
            return url;
        }

        public static async Task<string> HttpPost(string url, byte[] data)
        {
            WebClient wc = new WebClient();
            wc.Headers["content-type"] = "text/plain;charset=UTF-8";
            byte[] retdata = await wc.UploadDataTaskAsync(url, "POST", data);
            return System.Text.Encoding.UTF8.GetString(retdata);
        }


    }
}
