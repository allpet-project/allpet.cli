using System;
using System.Linq;

namespace allpet.peer.tcp.test
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Loop();
            while (true) { };
        }

        static async void Loop()
        {
            while(true)
            {
                Console.WriteLine("选择test Task：");
                var str= Console.ReadLine();
                if(str!=String.Empty)
                {
                    switch(str)
                    {
                        case "1":
                            multi_threadSend.test();
                            break;
                    }
                }
            }
        }
    }
}
