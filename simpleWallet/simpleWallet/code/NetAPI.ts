namespace NetApi {

    export function getAssetUtxo(url: string, address: string, asset: string): Promise<UTXO[]>{
        return tool.getassetutxobyaddress(url, address, asset).then((json) => {
            let arr: UTXO[] = [];

            let assetInfo = json[0];

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