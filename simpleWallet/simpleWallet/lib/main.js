var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var simpleWallet;
(function (simpleWallet) {
    class DataInfo {
    }
    DataInfo.Neo = "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";
    DataInfo.Gas = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
    DataInfo.Pet = "0x6112d5ec36d299a6a8c87ebde6f3782f7ac74118";
    DataInfo.APiUrl = "http://127.0.0.1:63494/";
    simpleWallet.DataInfo = DataInfo;
    class Account {
        refreshAsset(type, count) {
            switch (type) {
                case "neo":
                    this.neo = count;
                    this.neoInput.value = count.toString();
                    break;
                case "gas":
                    this.gas = count;
                    this.gasInput.value = count.toString();
                    break;
                case "pet":
                    this.pet = count;
                    this.PetInput.value = count.toString();
                    break;
            }
        }
        setFromWIF(wif) {
            var prikey;
            var pubkey;
            var address;
            prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            var pubhexstr = prikey.toHexString();
            var prihexstr = pubkey.toHexString();
            this.prikey = prihexstr;
            this.pubkey = pubhexstr;
            this.addr = address;
        }
        refreshAssetCount(url) {
            NetApi.getAssetUtxo(DataInfo.APiUrl, this.addr, DataInfo.Gas).then((asset) => {
                let totleCount = 0;
                for (let i = 0; i < asset.length; i++) {
                    totleCount += asset[i].value;
                }
                this.refreshAsset("gas", totleCount);
            });
        }
    }
    simpleWallet.Account = Account;
    class PageCtr {
        static start() {
            DataInfo.targetAccount = new Account();
            var signBtn = document.getElementById("signin");
            var wifinput = document.getElementById("wif");
            wifinput.value = "KwUhZzS6wrdsF4DjVKt2XQd3QJoidDhckzHfZJdQ3gzUUJSr8MDd";
            signBtn.onclick = () => {
                console.warn("sign!!!" + wifinput.value);
                DataInfo.currentAccount = new Account();
                try {
                    DataInfo.currentAccount.setFromWIF(wifinput.value);
                    DataInfo.currentAccount.refreshAssetCount(DataInfo.APiUrl);
                }
                catch (_a) {
                }
            };
        }
    }
    simpleWallet.PageCtr = PageCtr;
})(simpleWallet || (simpleWallet = {}));
window.onload = () => {
    simpleWallet.PageCtr.start();
};
var NetApi;
(function (NetApi) {
    function getAssetUtxo(url, address, asset) {
        return tool.getassetutxobyaddress(url, address, asset).then((json) => {
            let arr = [];
            let assetInfo = json[0];
            let assetId = assetInfo["asset"];
            let assetArr = assetInfo["arr"];
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
    NetApi.getAssetUtxo = getAssetUtxo;
    class UTXO {
    }
    NetApi.UTXO = UTXO;
})(NetApi || (NetApi = {}));
var tool;
(function (tool) {
    function makeRpcPostBody(method, ..._params) {
        var body = {};
        body["jsonrpc"] = "2.0";
        body["id"] = 1;
        body["method"] = method;
        var params = [];
        for (var i = 0; i < _params.length; i++) {
            params.push(_params[i]);
        }
        body["params"] = params;
        return body;
    }
    tool.makeRpcPostBody = makeRpcPostBody;
})(tool || (tool = {}));
var tool;
(function (tool) {
    function getassetutxobyaddress(url, address, asset) {
        return __awaiter(this, void 0, void 0, function* () {
            var body = tool.makeRpcPostBody("getassetutxobyaddress", address, asset);
            var response = yield fetch(url, { "method": "post", "body": JSON.stringify(body) });
            var res = yield response.json();
            var result = res["result"];
            return result;
        });
    }
    tool.getassetutxobyaddress = getassetutxobyaddress;
})(tool || (tool = {}));
//# sourceMappingURL=main.js.map