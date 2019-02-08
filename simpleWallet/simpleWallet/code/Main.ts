///<reference path="../lib/neo-ts.d.ts"/>

namespace simpleWallet {


    export class DataInfo {
        static Neo: string = "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";
        static Gas: string = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
        static Pet: string = null;

        static APiUrl: string ="http://localhost:63494/api/mainnet";
        static WIF: string;
        static targetAddr: string = "AH2ADnKSuJrhHefqeJ9j83HcNXPfipwr6V";
        static currentAccount: Account;
        static targetAccount: Account;
    }
    export class TransactionState {
        static beGasTransing = false;
        static bePetTransing = false;
    }
    export class Account
    {
        addr: string;
        wif: string;
        prikey: string;
        pubkey: string;

        neo: number;
        gas: number;
        pet: number;

        neoInput: HTMLInputElement;
        gasInput: HTMLInputElement;
        PetInput: HTMLInputElement;

        setAssetCount(type: string, count: any) {
            switch (type) {
                case "neo":
                    this.neo = count;
                    this.neoInput.textContent = count.toString();
                    break;
                case "gas":
                    this.gas = count;
                    this.gasInput.textContent = count.toString();
                    break;
                case "pet":
                    this.pet = count;
                    this.PetInput.textContent = (count as Neo.BigInteger).toString();
                    break;
            }
        }

        setFromWIF(wif: string){
            var prikey: Uint8Array;
            var pubkey: Uint8Array;
            var address: string;
            prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            var pubhexstr = prikey.toHexString();
            var prihexstr = pubkey.toHexString();

            this.prikey = prihexstr;
            this.pubkey = pubhexstr;
            this.addr = address;
            this.wif = wif;
        }
        refreshAssetCount(type: string) {
            switch (type) {
                case "gas":
                    NetApi.getAssetUtxo(DataInfo.APiUrl, this.addr, DataInfo.Gas).then((asset) => {
                        let totleCount = 0;
                        for (let i = 0; i < asset.length; i++) {
                            totleCount += asset[i].value;
                        }
                        this.setAssetCount("gas", totleCount);
                    });
                    break;
                case "pet":
                    NetApi.getnep5balancebyaddress(DataInfo.APiUrl, this.addr, DataInfo.Pet).then((result) => {
                        this.setAssetCount("pet", result);
                    });
                    break;
            }
        }
        refreshAllAssetCount() {
            NetApi.getAssetUtxo(DataInfo.APiUrl, this.addr, DataInfo.Gas).then((asset) => {
                let totleCount = 0;
                for (let i = 0; i < asset.length; i++) {
                    totleCount += asset[i].value;
                }
                this.setAssetCount("gas", totleCount);
            });
            NetApi.getAssetUtxo(DataInfo.APiUrl, this.addr, DataInfo.Neo).then((asset) => {
                let totleCount = 0;
                for (let i = 0; i < asset.length; i++) {
                    totleCount += asset[i].value;
                }
                this.setAssetCount("neo", totleCount);
            });
            NetApi.getnep5balancebyaddress(DataInfo.APiUrl, this.addr, DataInfo.Pet).then((result) => {
                this.setAssetCount("pet", result);
            });
        }
    }
    export class PageCtr {

        public static start() {
            tool.loadJson("../lib/config.json", (json) => {
                DataInfo.Pet = json["petid"];
            });

            //------------------账户资产展示
            DataInfo.targetAccount = new Account();
            DataInfo.targetAccount.neoInput = document.getElementById("t_neoinput") as HTMLInputElement;
            DataInfo.targetAccount.gasInput = document.getElementById("t_gasinput") as HTMLInputElement;
            DataInfo.targetAccount.PetInput = document.getElementById("t_petinput") as HTMLInputElement;
            DataInfo.targetAccount.addr = DataInfo.targetAddr;
            DataInfo.targetAccount.refreshAllAssetCount();

            var signBtn = document.getElementById("signin") as HTMLButtonElement;
            var wifinput = document.getElementById("wif") as HTMLInputElement;
            wifinput.value = "KwUhZzS6wrdsF4DjVKt2XQd3QJoidDhckzHfZJdQ3gzUUJSr8MDd";
            signBtn.onclick = () => {
                console.warn("sign!!!" + wifinput.value);
                this.sign(wifinput.value);
            }

            //------------------交易
            let btn_transgas = document.getElementById("trans_gas") as HTMLButtonElement;
            btn_transgas.onclick = () => {
                if (DataInfo.currentAccount == null) {
                    alert("请登录账户！");
                }
                else if (TransactionState.beGasTransing) {
                    alert("gas 交易进行中，请等待！");
                } else {
                    console.log("gas 交易： start！");
                    let gasinput = document.getElementById("gascount") as HTMLInputElement;
                    let value = parseFloat(gasinput.value);
                    this.transactionGas(value, DataInfo.currentAccount, DataInfo.targetAccount);
                }
            };

            let btn_transpet = document.getElementById("trans_pet") as HTMLButtonElement;
            btn_transpet.onclick = () => {
                if (DataInfo.currentAccount == null) {
                    alert("请登录账户！");
                } else if (DataInfo.Pet == null) {
                    alert("petid 未配置成功！");
                }
                else if (TransactionState.bePetTransing) {
                    alert("pet 交易进行中，请等待！");
                } else {
                    let petinput = document.getElementById("petcount") as HTMLInputElement;
                    let value = parseFloat(petinput.value);
                    
                    this.transactionPet(value, DataInfo.currentAccount, DataInfo.targetAccount);
                }
            };


        }

        /**
         * 登录账户
         * @param wif
         */
        static sign(wif: string) {

            DataInfo.currentAccount = new Account();
            DataInfo.currentAccount.neoInput = document.getElementById("c_neoinput") as HTMLInputElement;
            DataInfo.currentAccount.gasInput = document.getElementById("c_gasinput") as HTMLInputElement;
            DataInfo.currentAccount.PetInput = document.getElementById("c_petinput") as HTMLInputElement;

            try {
                DataInfo.currentAccount.setFromWIF(wif);
                DataInfo.currentAccount.refreshAllAssetCount();
            }
            catch
            {
            }
        }

        static transactionGas(count: number, from: Account, to: Account) {
            NetApi.getAssetUtxo(DataInfo.APiUrl, from.addr, DataInfo.Gas).then((utxos) => {
                let trans = tool.CoinTool.makeTran(utxos, to.addr, DataInfo.Gas, Neo.Fixed8.fromNumber(count));
                let msg = trans.GetMessage();
                let prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(from.wif);
                let pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
                let address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

                let signData = ThinNeo.Helper.Sign(msg, prikey);
                trans.AddWitness(signData, pubkey, address);

                let data = trans.GetRawData();
                let rawdata = data.toHexString();


                let txid1 = trans.GetHash().clone().reverse().toHexString();
                console.warn("transaction hash txid:" + txid1);

                //---------------正在交易
                TransactionState.beGasTransing = true;
                document.getElementById("trans_gas_info").innerHTML="正在交易@@@";

                NetApi.sendrawtransaction(DataInfo.APiUrl, rawdata).then(async (txid) => {
                    document.getElementById("trans_gas_info").innerHTML = "发送交易成功,待确认@@@";
                    let func = async () => {
                        let bexisted = await PageCtr.checkTxExisted(txid);
                        if (bexisted) {
                            TransactionState.beGasTransing = false;
                            document.getElementById("trans_gas_info").innerHTML = "null";

                            from.refreshAssetCount("gas");
                            to.refreshAssetCount("gas");

                        } else {
                            //console.log("check again");
                            setTimeout(() => {
                                func();
                            },300);
                        }
                    }
                    func();
                })
            })
        }

        static transactionPet(count: number, from: Account, to: Account) {
            let tasks = [];
            tasks.push(NetApi.getnep5decimals(DataInfo.APiUrl, DataInfo.Pet));
            tasks.push(NetApi.getAssetUtxo(DataInfo.APiUrl, from.addr, DataInfo.Gas));
            Promise.all(tasks).then((res) => {
                let decimal: number = res[0];
                let utxos: tool.UTXO[] = res[1];

                let trans = tool.CoinTool.makeTran(utxos, from.addr, DataInfo.Gas, Neo.Fixed8.Zero);
                trans.type = ThinNeo.TransactionType.InvocationTransaction;
                trans.extdata = new ThinNeo.InvokeTransData();

                var sb = new ThinNeo.ScriptBuilder();
                var scriptaddress = DataInfo.Pet.hexToBytes().reverse();
                //Parameter inversion 
                sb.EmitParamJson(["(address)" + from.addr, "(address)" + to.addr, "(integer)" + count * Math.pow(10, decimal)]);//Parameter list 
                sb.EmitPushString("transfer");//Method
                sb.EmitAppCall(scriptaddress);  //Asset contract 
                (trans.extdata as ThinNeo.InvokeTransData).script = sb.ToArray();
                (trans.extdata as ThinNeo.InvokeTransData).gas = Neo.Fixed8.fromNumber(1.0);

                let msg = trans.GetMessage();
                let prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(from.wif);
                let pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
                let address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

                let signData = ThinNeo.Helper.Sign(msg, prikey);
                trans.AddWitness(signData, pubkey, address);

                let data = trans.GetRawData();
                let rawdata = data.toHexString();


                let txid1 = trans.GetHash().clone().reverse().toHexString();
                console.warn("transaction hash txid:" + txid1);

                //---------------正在交易
                TransactionState.bePetTransing = true;
                document.getElementById("trans_pet_info").innerHTML = "正在交易@@@";

                NetApi.sendrawtransaction(DataInfo.APiUrl, rawdata).then(async (txid) => {
                    document.getElementById("trans_pet_info").innerHTML = "发送交易成功,待确认@@@";
                    let func = async () => {
                        let bexisted = await PageCtr.checkTxExisted(txid);
                        if (bexisted) {
                            TransactionState.beGasTransing = false;
                            document.getElementById("trans_pet_info").innerHTML = "null";

                            from.refreshAssetCount("pet");
                            to.refreshAssetCount("pet");

                        } else {
                            //console.log("check again");
                            setTimeout(() => {
                                func();
                            }, 300);
                        }
                    }
                    func();
                })
            });

        }


        static checkTxExisted(txid: string): Promise<boolean>
        {
            return NetApi.checktxboolexisted(DataInfo.APiUrl, txid).then((beExisted) => {
                return beExisted;
            });
        }

    }
}


window.onload = () => {
    simpleWallet.PageCtr.start();
}

