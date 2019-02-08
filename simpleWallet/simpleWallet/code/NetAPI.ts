namespace NetApi {

    export function getAssetUtxo(url: string, address: string, asset: string): Promise<UTXO[]>{
        return tool.getassetutxobyaddress(url, address, asset).then((result) => {
            let arr: UTXO[] = [];
            if (result == null || (result as any[]).length==0) return arr;
            let assetInfo = result[0];

            let assetId = assetInfo["asset"];
            let assetArr: any[] = assetInfo["arr"];
            for (let i = 0; i < assetArr.length; i++) {
                let item = assetArr[i];
                let utxo = new UTXO();
                utxo.addr = item["addr"];
                utxo.txid = item["txid"];
                utxo.n = item["n"];
                utxo.asset = item["asset"];
                utxo.value = Number.parseFloat(item["value"]);
                utxo.createHeight = item["createHeight"];
                utxo.used = item["used"];
                utxo.useHeight = item["useHeight"];
                utxo.claimed = item["claimed"];

                arr.push(utxo);
            }

            return arr;
        });

    }

    export function getnep5balancebyaddress(url: string, address: string, asset: string): Promise<number> {
        return tool.getnep5balancebyaddress(url, address, asset).then((result) => {
            if (result) {
                let count = result[0]["value"];
                let bnum = parseFloat(count);
                return bnum;
            } else {
                return 0;
            }
            //console.debug(result);
        });
    }


    export class UTXO {
        addr: string;
        txid: string;
        n: number;
        asset: string;
        value: number;
        createHeight: number;
        used: boolean;
        useHeight: number;
        claimed: string;
    }

}