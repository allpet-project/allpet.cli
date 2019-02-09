namespace tool {
    export class CoinTool {

        static makeTran(utxos:UTXO[], targetaddr: string, assetid: string, sendcount: Neo.Fixed8): ThinNeo.Transaction {
            //if (sendcount.compareTo(Neo.Fixed8.Zero) <= 0)
            //    throw new Error("can not send zero.");
            var tran = new ThinNeo.Transaction();
            tran.type = ThinNeo.TransactionType.ContractTransaction;
            tran.version = 0;//0 or 1
            tran.extdata = null;

            tran.attributes = [];

            tran.inputs = [];
            var scraddr: string = "";
            var us = utxos;
            us.sort((a, b) => {
                return a.count.compareTo(b.count);
            });
            var count: Neo.Fixed8 = Neo.Fixed8.Zero;
            for (var i = 0; i < us.length; i++) {
                var input = new ThinNeo.TransactionInput();
                input.hash = us[i].txid.hexToBytes().reverse();
                input.index = us[i].n;
                input["_addr"] = us[i].addr;//利用js的隨意性，臨時傳個值
                tran.inputs.push(input);
                count = count.add(us[i].count);
                scraddr = us[i].addr;
                if (count.compareTo(sendcount) > 0) {
                    break;
                }
            }
            if (count.compareTo(sendcount) >= 0)//输入大于等于输出
            {
                tran.outputs = [];
                //输出
                if (sendcount.compareTo(Neo.Fixed8.Zero) > 0) {
                    var output = new ThinNeo.TransactionOutput();
                    output.assetId = assetid.hexToBytes().reverse();
                    output.value = sendcount;
                    output.toAddress = ThinNeo.Helper.GetPublicKeyScriptHash_FromAddress(targetaddr);
                    tran.outputs.push(output);
                }

                //找零
                var change = count.subtract(sendcount);
                if (change.compareTo(Neo.Fixed8.Zero) > 0) {
                    var outputchange = new ThinNeo.TransactionOutput();
                    outputchange.toAddress = ThinNeo.Helper.GetPublicKeyScriptHash_FromAddress(scraddr);
                    outputchange.value = change;
                    outputchange.assetId = assetid.hexToBytes().reverse();
                    tran.outputs.push(outputchange);

                }
            }
            else {
                throw new Error("no enough money.");
            }
            return tran;
        }
    }
    export class UTXO {
        addr: string;
        txid: string;
        n: number;
        asset: string;
        count: Neo.Fixed8;
        name: string;
        value: number;
    }

}