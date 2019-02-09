///<reference path="../lib/neo-ts.d.ts"/>

namespace simpleWallet {
    export enum NetEnum {
        Main="主网",
        privateChain="私链"
    }

    export class DataInfo {

        static Neo: string = "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";
        static Gas: string = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
        static Pet: string = null;

        static beMainNet: boolean = false;
        static APiUrl: string;
        static WIF: string;
        static targetAddr: string;

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
            if (this.addr == null) {
                this.setAssetCount("gas", 0);
                this.setAssetCount("neo", 0);
                this.setAssetCount("pet", 0);
                console.warn("当前账户为空，请设置账户！！");
                return;
            }
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

        existAccount() {
            this.setAssetCount("gas", 0);
            this.setAssetCount("neo", 0);
            this.setAssetCount("pet", 0);
        }
    }
    export class PageCtr {
        static currentAccount: Account;
        static targetAccount: Account;


        public static start() {

            let changeBtn = document.getElementById("changeChain") as HTMLButtonElement;
            changeBtn.onclick = () => {
                DataInfo.beMainNet = !DataInfo.beMainNet;
                document.getElementById("NetTag").innerHTML = DataInfo.beMainNet ? "当前网：主网" : "当前网：私链";
                let net = DataInfo.beMainNet ? NetEnum.Main : NetEnum.privateChain;
                this.chooseNet(net);
            }
            simpleWallet.PageCtr.chooseNet(simpleWallet.NetEnum.privateChain);

            let changePetBtn = document.getElementById("changePet") as HTMLButtonElement;
            let PetInput = document.getElementById("petTag") as HTMLInputElement;
            PetInput.value = DataInfo.Pet;
            changePetBtn.onclick = () => {
                let asset = PetInput.value;
                if (asset != null) {
                    DataInfo.Pet = PetInput.value;
                }
            };
            //------------------交易
            let btn_transgas = document.getElementById("trans_gas") as HTMLButtonElement;
            btn_transgas.onclick = () => {
                if (this.checkBeforeTransaction())
                {
                    if (TransactionState.beGasTransing) {
                        alert("gas 交易进行中，请等待！");
                    } else {
                        console.log("gas 交易： start！");
                        let gasinput = document.getElementById("gascount") as HTMLInputElement;
                        let value = parseFloat(gasinput.value);
                        this.transactionGas(value, this.currentAccount, this.targetAccount);
                    }
                }
            };

            let btn_transpet = document.getElementById("trans_pet") as HTMLButtonElement;
            btn_transpet.onclick = () => {
                if (this.checkBeforeTransaction())
                {
                    if (TransactionState.bePetTransing)
                    {
                        alert("pet 交易进行中，请等待！");
                    } else {
                        let petinput = document.getElementById("petcount") as HTMLInputElement;
                        let value = parseFloat(petinput.value);

                        this.transactionPet(value, this.currentAccount, this.targetAccount);
                    }
                }
            };
        }


        static chooseNet(net: NetEnum) {
            switch (net) {
                case NetEnum.Main:
                    DataInfo.APiUrl = config.mainNetInfo.APiUrl;
                    DataInfo.targetAddr = config.mainNetInfo.targetAddr;
                    DataInfo.WIF = config.mainNetInfo.wif;
                    DataInfo.Pet = config.mainNetInfo.petid;

                    break;
                case NetEnum.privateChain:
                    DataInfo.APiUrl = config.privateNetInfo.APiUrl;
                    DataInfo.targetAddr = config.privateNetInfo.targetAddr;
                    DataInfo.WIF = config.privateNetInfo.wif;
                    DataInfo.Pet = config.privateNetInfo.petid;
                    break;
            }

            //------------------账户资产展示
            var signBtn = document.getElementById("signin") as HTMLButtonElement;
            var wifinput = document.getElementById("wif") as HTMLInputElement;
            wifinput.value = DataInfo.WIF;
            signBtn.onclick = () => {
                console.warn("sign!!!" + wifinput.value);
                let wif = wifinput.value;
                DataInfo.WIF = wif;
                console.log("@设置目标账户");

                this.sign(wif);
            }
            this.sign(DataInfo.WIF);
            

            let targetInput = document.getElementById("targetAddr") as HTMLInputElement;
            var targetBtn = document.getElementById("changeTarget") as HTMLButtonElement;
            targetInput.value = DataInfo.targetAddr;
            targetBtn.onclick = () => {
                
                let addr = targetInput.value;
                if (addr == null) {
                    console.warn("addr 为null，请重新设置目标账户！！");
                    return;
                }
                DataInfo.targetAddr = addr;
                this.setTargetAddr(addr);
            };
            this.setTargetAddr(DataInfo.targetAddr);

        }

        /**
         * 登录账户
         * @param wif
         */
        static sign(wif: string) {
            if (this.currentAccount) {
                this.currentAccount.existAccount();
            }
            console.log("@登录账户");

            this.currentAccount = new Account();
            this.currentAccount.neoInput = document.getElementById("c_neoinput") as HTMLInputElement;
            this.currentAccount.gasInput = document.getElementById("c_gasinput") as HTMLInputElement;
            this.currentAccount.PetInput = document.getElementById("c_petinput") as HTMLInputElement;

            try {
                this.currentAccount.setFromWIF(wif);
                this.currentAccount.refreshAllAssetCount();
            }
            catch
            {
            }
        }
        /**
         * 设置目标账户
         * @param addr
         */
        static setTargetAddr(addr: string) {
            if (this.targetAccount) {
                this.targetAccount.existAccount();
            }
                console.log("@设置目标账户");

                this.targetAccount = new Account();
                this.targetAccount.neoInput = document.getElementById("t_neoinput") as HTMLInputElement;
                this.targetAccount.gasInput = document.getElementById("t_gasinput") as HTMLInputElement;
                this.targetAccount.PetInput = document.getElementById("t_petinput") as HTMLInputElement;
                this.targetAccount.addr = DataInfo.targetAddr;

                this.targetAccount.refreshAllAssetCount();
            
        }

        static checkBeforeTransaction():boolean{
            if (this.currentAccount == null)
            {
                alert("请登录账户！");
                return false;
            } else if (this.targetAccount == null)
            {
                alert("请设置目标账户！");
                return false;
            }
            else if (DataInfo.Pet == null)
            {
                alert("petid 未配置成功！");
                return false;
            }
            return true;
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
    simpleWallet.config.loadFromPath("lib/config.json", () => {
        simpleWallet.PageCtr.start();

    })
}

