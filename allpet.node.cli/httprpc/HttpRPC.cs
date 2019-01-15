using System;
using System.Collections.Generic;
using System.Text;

namespace allpet.nodecli.httpinterface
{
    class HttpRPC
    {
        public void Start()
        {
            allpet.http.server.httpserver server = new http.server.httpserver();
            server.SetHttpAction("/", async (context) =>
            {
                byte[] writedata = System.Text.Encoding.UTF8.GetBytes("hello world.");
                await context.Response.Body.WriteAsync(writedata);
            });
            server.Start(80);

        }
    }
}
