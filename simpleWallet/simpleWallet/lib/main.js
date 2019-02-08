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
    DataInfo.APiUrl = "http://localhost:63494/api/mainnet";
    DataInfo.targetAddr = "AH2ADnKSuJrhHefqeJ9j83HcNXPfipwr6V";
    simpleWallet.DataInfo = DataInfo;
    class Account {
        refreshAsset(type, count) {
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
                    this.PetInput.textContent = count.toString();
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
    simpleWallet.Account = Account;
    class PageCtr {
        static start() {
            DataInfo.targetAccount = new Account();
            DataInfo.targetAccount.neoInput = document.getElementById("t_neoinput");
            DataInfo.targetAccount.gasInput = document.getElementById("t_gasinput");
            DataInfo.targetAccount.PetInput = document.getElementById("t_petinput");
            DataInfo.targetAccount.addr = DataInfo.targetAddr;
            DataInfo.targetAccount.refreshAssetCount(DataInfo.APiUrl);
            var signBtn = document.getElementById("signin");
            var wifinput = document.getElementById("wif");
            wifinput.value = "KwUhZzS6wrdsF4DjVKt2XQd3QJoidDhckzHfZJdQ3gzUUJSr8MDd";
            signBtn.onclick = () => {
                console.warn("sign!!!" + wifinput.value);
                DataInfo.currentAccount = new Account();
                DataInfo.currentAccount.neoInput = document.getElementById("c_neoinput");
                DataInfo.currentAccount.gasInput = document.getElementById("c_gasinput");
                DataInfo.currentAccount.PetInput = document.getElementById("c_petinput");
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
        return tool.getassetutxobyaddress(url, address, asset).then((result) => {
            let arr = [];
            if (result == null || result.length == 0)
                return arr;
            let assetInfo = result[0];
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
    function getnep5balancebyaddress(url, address, asset) {
        return tool.getnep5balancebyaddress(url, address, asset).then((result) => {
            if (result) {
                let count = result[0]["value"];
                let bnum = parseFloat(count);
                return bnum;
            }
            else {
                return 0;
            }
        });
    }
    NetApi.getnep5balancebyaddress = getnep5balancebyaddress;
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
    function getnep5balancebyaddress(url, address, asset) {
        return __awaiter(this, void 0, void 0, function* () {
            var body = tool.makeRpcPostBody("getnep5balancebyaddress", address, asset);
            var response = yield fetch(url, { "method": "post", "body": JSON.stringify(body) });
            var res = yield response.json();
            var result = res["result"];
            return result;
        });
    }
    tool.getnep5balancebyaddress = getnep5balancebyaddress;
})(tool || (tool = {}));
//# sourceMappingURL=main.js.map