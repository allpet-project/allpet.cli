namespace tool {
    export async function getassetutxobyaddress(url: string, address: string, asset: string): Promise<any>
    {
        var body = makeRpcPostBody("getassetutxobyaddress", address, asset);
        var response = await fetch(url, { "method": "post", "body": JSON.stringify(body) });
        var res = await response.json();
        var result=res["result"];
        return result;
    }

    export async function getnep5balancebyaddress(url: string, address: string, asset: string): Promise<any> {
        var body = makeRpcPostBody("getnep5balancebyaddress", address, asset);
        var response = await fetch(url, { "method": "post", "body": JSON.stringify(body) });
        var res = await response.json();
        var result = res["result"];
        return result;
    }

    export async function getnep5decimals(url: string, asset: string) {
        var body = makeRpcPostBody("getnep5decimals",asset);
        var response = await fetch(url, { "method": "post", "body": JSON.stringify(body) });
        var res = await response.json();
        var result = res["result"];
        return result;
    }

    export async function sendrawtransaction(url: string, rawdata: string): Promise<any> {
        var body = makeRpcPostBody("sendrawtransaction", rawdata);
        var response = await fetch(url, { "method": "post", "body": JSON.stringify(body) });
        var res = await response.json();
        var result = res["result"];
        return result;
    }
    export async function checktxboolexisted(url: string, txid: string): Promise<any> {
        var body = makeRpcPostBody("checktxboolexisted", txid);
        var response = await fetch(url, { "method": "post", "body": JSON.stringify(body) });
        var res = await response.json();
        var result = res["result"];
        return result;
    }
    
}