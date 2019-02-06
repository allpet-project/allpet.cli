﻿namespace tool {
    export async function getassetutxobyaddress(url: string, address: string, asset: string): Promise<any>
    {
        var body = makeRpcPostBody("getassetutxobyaddress", address, asset);
        var response = await fetch(url, { "method": "post", "body": JSON.stringify(body) });
        var res = await response.json();
        var result=res["result"];
        return result;
    }
}