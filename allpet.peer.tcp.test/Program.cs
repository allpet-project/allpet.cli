using System;
using System.Linq;

namespace allpet.peer.tcp.test
{
    class Program
    {
        static System.Collections.Concurrent.ConcurrentDictionary<int, ulong> linkedid = new System.Collections.Concurrent.ConcurrentDictionary<int, ulong>();
        static System.Collections.Concurrent.ConcurrentDictionary<ulong, bool> hasLinked = new System.Collections.Concurrent.ConcurrentDictionary<ulong, bool>();
        static void Main(string[] args)
        {
            var logger = new AllPet.Common.Logger();
            var peer = AllPet.peer.tcp.PeerV2.CreatePeer(logger);
            peer.Start(new AllPet.peer.tcp.PeerOption()
            {

            });
            var ep = new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 888);

            peer.OnAccepted += (id, endpoint) =>
              {
                  Console.WriteLine("accepted:" + id);

              };

            peer.OnConnected += (id, endpoint) =>
            {
                hasLinked[id] = true;
            };
            peer.OnClosed += (id) =>
              {

              };
            peer.OnLinkError += (ulong id, Exception err) =>
              {

              };
            long recvcount = 0;
            peer.OnRecv += (ulong id, byte[] _data) =>
              {
                  var len = BitConverter.ToUInt32(_data, 0);
                  var _str = System.Text.Encoding.UTF8.GetString(_data, 4, _data.Length - 4);
                  System.Threading.Interlocked.Increment(ref recvcount);
                  if (recvcount % 1000 == 0)
                  {
                      Console.WriteLine("onrecv:count="+ recvcount + " len=" + len + " txt=" + _str);
                  }
              };
            peer.Listen(ep);

            //connect it;
            logger.Warn("connect 10");
            for (var i = 0; i < 10; i++)
            {
                linkedid[i] = peer.Connect(ep);
            }
            while (hasLinked.Count < linkedid.Count)
            {
                System.Threading.Thread.Sleep(1);
            }
            logger.Warn("connected 10");

            logger.Warn("send 1k");
            Random r = new Random();
            var str = "abcdefghijklmnoqrstuvwxyzabcdefghijklmnoqrstuvwxyzabcdefghijklmnoqrstuvwxyzabcdefghijklmnoqrstuvwxyz";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(str);
            byte[] datahead = BitConverter.GetBytes(data.Length);
            data = datahead.Concat(data).ToArray();
            for (var i = 0; i < 100000; i++)
            {
                var target = r.Next(linkedid.Count);
                var targetid = linkedid[target];

                System.Threading.ThreadPool.QueueUserWorkItem((s) =>
                {
                    peer.Send(targetid, data);

                });
            }
            Console.ReadLine();
        }
    }
}
