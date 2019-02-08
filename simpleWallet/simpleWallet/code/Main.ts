///<reference path="../lib/neo-ts.d.ts"/>

namespace simpleWallet {


    export class DataInfo {
        static Neo: string = "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";
        static Gas: string = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
        static Pet: string = "0x6112d5ec36d299a6a8c87ebde6f3782f7ac74118";

        static APiUrl: string ="http://localhost:63494/api/mainnet";
        static WIF: string;
        static targetAddr: string = "AH2ADnKSuJrhHefqeJ9j83HcNXPfipwr6V";
        static currentAccount: Account;
        static targetAccount: Account;
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

        refreshAsset(type: string, count: any) {
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

        refreshAssetCount(url: string) {
            NetApi.getAssetUtxo(DataInfo.APiUrl, this.addr, DataInfo.Gas).then((asset) => {
                let totleCount = 0;
                for (let i = 0; i < asset.length; i++) {
                    totleCount += asset[i].value;
                }
                this.refreshAsset("gas", totleCount);
            });
            NetApi.getAssetUtxo(DataInfo.APiUrl, this.addr, DataInfo.Neo).then((asset) => {
                let totleCount = 0;
                for (let i = 0; i < asset.length; i++) {
                    totleCount += asset[i].value;
                }
                this.refreshAsset("neo", totleCount);
            });
            NetApi.getnep5balancebyaddress(DataInfo.APiUrl, this.addr, DataInfo.Pet).then((result) => {
                this.refreshAsset("pet", result);
            });
        }
    }
    export class PageCtr {

        public static start() {
            //------------------账户资产展示
            DataInfo.targetAccount = new Account();
            DataInfo.targetAccount.neoInput = document.getElementById("t_neoinput") as HTMLInputElement;
            DataInfo.targetAccount.gasInput = document.getElementById("t_gasinput") as HTMLInputElement;
            DataInfo.targetAccount.PetInput = document.getElementById("t_petinput") as HTMLInputElement;
            DataInfo.targetAccount.addr = DataInfo.targetAddr;
            DataInfo.targetAccount.refreshAssetCount(DataInfo.APiUrl);

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
                console.log("gas 交易： start！");
                let gasinput = document.getElementById("gascount") as HTMLInputElement;
                let value = parseFloat(gasinput.value);
                this.transactionGas(value, DataInfo.currentAccount, DataInfo.targetAccount);
            };

            let btn_transpet = document.getElementById("trans_pet") as HTMLButtonElement;
            btn_transpet.onclick = () => {
                let petinput = document.getElementById("petcount") as HTMLInputElement;
                let value = parseFloat(petinput.value);
                this.transactionPet(value, DataInfo.currentAccount, DataInfo.targetAccount);
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
                DataInfo.currentAccount.refreshAssetCount(DataInfo.APiUrl);
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

                NetApi.sendrawtransaction(DataInfo.APiUrl, rawdata).then((txid) => {
                    console.warn("发送交易成功txid:" + txid);

                })
            })
        }

        static transactionPet(count: number, from: Account, to: Account) {

        }
    }
}


window.onload = () => {
    simpleWallet.PageCtr.start();
}

