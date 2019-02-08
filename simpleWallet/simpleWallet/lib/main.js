var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var tool;
(function (tool) {
    function loadJson(url, callback) {
        var req = new XMLHttpRequest();
        req.open("GET", url);
        req.responseType = "json";
        req.onreadystatechange = () => {
            if (req.readyState === 4) {
                callback(req.response);
            }
        };
        req.send();
    }
    tool.loadJson = loadJson;
})(tool || (tool = {}));
var simpleWallet;
(function (simpleWallet) {
    class DataInfo {
    }
    DataInfo.Neo = "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";
    DataInfo.Gas = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
    DataInfo.Pet = null;
    simpleWallet.DataInfo = DataInfo;
    class TransactionState {
    }
    TransactionState.beGasTransing = false;
    TransactionState.bePetTransing = false;
    simpleWallet.TransactionState = TransactionState;
    class Account {
        setAssetCount(type, count) {
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
            this.wif = wif;
        }
        refreshAssetCount(type) {
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
    simpleWallet.Account = Account;
    class PageCtr {
        static start() {
            DataInfo.targetAccount = new Account();
            DataInfo.targetAccount.neoInput = document.getElementById("t_neoinput");
            DataInfo.targetAccount.gasInput = document.getElementById("t_gasinput");
            DataInfo.targetAccount.PetInput = document.getElementById("t_petinput");
            DataInfo.targetAccount.addr = DataInfo.targetAddr;
            DataInfo.targetAccount.refreshAllAssetCount();
            var signBtn = document.getElementById("signin");
            var wifinput = document.getElementById("wif");
            wifinput.value = "KwUhZzS6wrdsF4DjVKt2XQd3QJoidDhckzHfZJdQ3gzUUJSr8MDd";
            signBtn.onclick = () => {
                console.warn("sign!!!" + wifinput.value);
                this.sign(wifinput.value);
            };
            let btn_transgas = document.getElementById("trans_gas");
            btn_transgas.onclick = () => {
                if (DataInfo.currentAccount == null) {
                    alert("请登录账户！");
                }
                else if (TransactionState.beGasTransing) {
                    alert("gas 交易进行中，请等待！");
                }
                else {
                    console.log("gas 交易： start！");
                    let gasinput = document.getElementById("gascount");
                    let value = parseFloat(gasinput.value);
                    this.transactionGas(value, DataInfo.currentAccount, DataInfo.targetAccount);
                }
            };
            let btn_transpet = document.getElementById("trans_pet");
            btn_transpet.onclick = () => {
                if (DataInfo.currentAccount == null) {
                    alert("请登录账户！");
                }
                else if (DataInfo.Pet == null) {
                    alert("petid 未配置成功！");
                }
                else if (TransactionState.bePetTransing) {
                    alert("pet 交易进行中，请等待！");
                }
                else {
                    let petinput = document.getElementById("petcount");
                    let value = parseFloat(petinput.value);
                    this.transactionPet(value, DataInfo.currentAccount, DataInfo.targetAccount);
                }
            };
        }
        static sign(wif) {
            DataInfo.currentAccount = new Account();
            DataInfo.currentAccount.neoInput = document.getElementById("c_neoinput");
            DataInfo.currentAccount.gasInput = document.getElementById("c_gasinput");
            DataInfo.currentAccount.PetInput = document.getElementById("c_petinput");
            try {
                DataInfo.currentAccount.setFromWIF(wif);
                DataInfo.currentAccount.refreshAllAssetCount();
            }
            catch (_a) {
            }
        }
        static transactionGas(count, from, to) {
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
                TransactionState.beGasTransing = true;
                document.getElementById("trans_gas_info").innerHTML = "正在交易@@@";
                NetApi.sendrawtransaction(DataInfo.APiUrl, rawdata).then((txid) => __awaiter(this, void 0, void 0, function* () {
                    document.getElementById("trans_gas_info").innerHTML = "发送交易成功,待确认@@@";
                    let func = () => __awaiter(this, void 0, void 0, function* () {
                        let bexisted = yield PageCtr.checkTxExisted(txid);
                        if (bexisted) {
                            TransactionState.beGasTransing = false;
                            document.getElementById("trans_gas_info").innerHTML = "null";
                            from.refreshAssetCount("gas");
                            to.refreshAssetCount("gas");
                        }
                        else {
                            setTimeout(() => {
                                func();
                            }, 300);
                        }
                    });
                    func();
                }));
            });
        }
        static transactionPet(count, from, to) {
            let tasks = [];
            tasks.push(NetApi.getnep5decimals(DataInfo.APiUrl, DataInfo.Pet));
            tasks.push(NetApi.getAssetUtxo(DataInfo.APiUrl, from.addr, DataInfo.Gas));
            Promise.all(tasks).then((res) => {
                let decimal = res[0];
                let utxos = res[1];
                let trans = tool.CoinTool.makeTran(utxos, from.addr, DataInfo.Gas, Neo.Fixed8.Zero);
                trans.type = ThinNeo.TransactionType.InvocationTransaction;
                trans.extdata = new ThinNeo.InvokeTransData();
                var sb = new ThinNeo.ScriptBuilder();
                var scriptaddress = DataInfo.Pet.hexToBytes().reverse();
                sb.EmitParamJson(["(address)" + from.addr, "(address)" + to.addr, "(integer)" + count * Math.pow(10, decimal)]);
                sb.EmitPushString("transfer");
                sb.EmitAppCall(scriptaddress);
                trans.extdata.script = sb.ToArray();
                trans.extdata.gas = Neo.Fixed8.fromNumber(1.0);
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
                TransactionState.bePetTransing = true;
                document.getElementById("trans_pet_info").innerHTML = "正在交易@@@";
                NetApi.sendrawtransaction(DataInfo.APiUrl, rawdata).then((txid) => __awaiter(this, void 0, void 0, function* () {
                    document.getElementById("trans_pet_info").innerHTML = "发送交易成功,待确认@@@";
                    let func = () => __awaiter(this, void 0, void 0, function* () {
                        let bexisted = yield PageCtr.checkTxExisted(txid);
                        if (bexisted) {
                            TransactionState.beGasTransing = false;
                            document.getElementById("trans_pet_info").innerHTML = "null";
                            from.refreshAssetCount("pet");
                            to.refreshAssetCount("pet");
                        }
                        else {
                            setTimeout(() => {
                                func();
                            }, 300);
                        }
                    });
                    func();
                }));
            });
        }
        static checkTxExisted(txid) {
            return NetApi.checktxboolexisted(DataInfo.APiUrl, txid).then((beExisted) => {
                return beExisted;
            });
        }
    }
    simpleWallet.PageCtr = PageCtr;
})(simpleWallet || (simpleWallet = {}));
window.onload = () => {
    tool.loadJson("../lib/config.json", (json) => {
        simpleWallet.DataInfo.Pet = json["petid"];
        simpleWallet.DataInfo.APiUrl = json["APiUrl"];
        simpleWallet.DataInfo.targetAddr = json["targetAddr"];
        simpleWallet.PageCtr.start();
    });
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
                let utxo = new tool.UTXO();
                utxo.addr = item["addr"];
                utxo.txid = item["txid"];
                utxo.n = item["n"];
                utxo.asset = item["asset"];
                utxo.value = Number.parseFloat(item["value"]);
                utxo.count = Neo.Fixed8.parse(item["value"]);
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
    function getnep5decimals(url, asset) {
        return tool.getnep5decimals(url, asset).then((result) => {
            if (result) {
                let count = result[0]["value"];
                return count;
            }
            else {
                return 0;
            }
        });
    }
    NetApi.getnep5decimals = getnep5decimals;
    function sendrawtransaction(url, rawdata) {
        return tool.sendrawtransaction(url, rawdata).then((result) => {
            console.warn(result);
            if (result != null && result[0] != null) {
                let besucced = result[0]["sendrawtransactionresult"];
                if (besucced) {
                    return result[0]["txid"];
                }
                else {
                    return null;
                }
            }
            else {
                return null;
            }
        });
    }
    NetApi.sendrawtransaction = sendrawtransaction;
    function checktxboolexisted(url, txid) {
        return tool.checktxboolexisted(url, txid).then((result) => {
            if (result != null) {
                let bools = result[0]["beExisted"];
                return bools;
            }
            else {
                return false;
            }
        });
    }
    NetApi.checktxboolexisted = checktxboolexisted;
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
    function getnep5decimals(url, asset) {
        return __awaiter(this, void 0, void 0, function* () {
            var body = tool.makeRpcPostBody("getnep5decimals", asset);
            var response = yield fetch(url, { "method": "post", "body": JSON.stringify(body) });
            var res = yield response.json();
            var result = res["result"];
            return result;
        });
    }
    tool.getnep5decimals = getnep5decimals;
    function sendrawtransaction(url, rawdata) {
        return __awaiter(this, void 0, void 0, function* () {
            var body = tool.makeRpcPostBody("sendrawtransaction", rawdata);
            var response = yield fetch(url, { "method": "post", "body": JSON.stringify(body) });
            var res = yield response.json();
            var result = res["result"];
            return result;
        });
    }
    tool.sendrawtransaction = sendrawtransaction;
    function checktxboolexisted(url, txid) {
        return __awaiter(this, void 0, void 0, function* () {
            var body = tool.makeRpcPostBody("checktxboolexisted", txid);
            var response = yield fetch(url, { "method": "post", "body": JSON.stringify(body) });
            var res = yield response.json();
            var result = res["result"];
            return result;
        });
    }
    tool.checktxboolexisted = checktxboolexisted;
})(tool || (tool = {}));
var tool;
(function (tool) {
    class CoinTool {
        static makeTran(utxos, targetaddr, assetid, sendcount) {
            var tran = new ThinNeo.Transaction();
            tran.type = ThinNeo.TransactionType.ContractTransaction;
            tran.version = 0;
            tran.extdata = null;
            tran.attributes = [];
            tran.inputs = [];
            var scraddr = "";
            var us = utxos;
            us.sort((a, b) => {
                return a.count.compareTo(b.count);
            });
            var count = Neo.Fixed8.Zero;
            for (var i = 0; i < us.length; i++) {
                var input = new ThinNeo.TransactionInput();
                input.hash = us[i].txid.hexToBytes().reverse();
                input.index = us[i].n;
                input["_addr"] = us[i].addr;
                tran.inputs.push(input);
                count = count.add(us[i].count);
                scraddr = us[i].addr;
                if (count.compareTo(sendcount) > 0) {
                    break;
                }
            }
            if (count.compareTo(sendcount) >= 0) {
                tran.outputs = [];
                if (sendcount.compareTo(Neo.Fixed8.Zero) > 0) {
                    var output = new ThinNeo.TransactionOutput();
                    output.assetId = assetid.hexToBytes().reverse();
                    output.value = sendcount;
                    output.toAddress = ThinNeo.Helper.GetPublicKeyScriptHash_FromAddress(targetaddr);
                    tran.outputs.push(output);
                }
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
    tool.CoinTool = CoinTool;
    class UTXO {
    }
    tool.UTXO = UTXO;
})(tool || (tool = {}));
//# sourceMappingURL=main.js.map