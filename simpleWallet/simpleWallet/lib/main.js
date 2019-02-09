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
    class NetInfo {
    }
    simpleWallet.NetInfo = NetInfo;
    class config {
        static loadFromPath(path = "../lib/config.json", callback) {
            tool.loadJson(path, (json) => {
                let privateNetjson = json["privateNet"];
                config.privateNetInfo.petid = privateNetjson["petid"];
                config.privateNetInfo.APiUrl = privateNetjson["APiUrl"];
                config.privateNetInfo.wif = privateNetjson["wif"];
                config.privateNetInfo.targetAddr = privateNetjson["targetAddr"];
                let mainNetjson = json["mainNet"];
                config.mainNetInfo.petid = mainNetjson["petid"];
                config.mainNetInfo.APiUrl = mainNetjson["APiUrl"];
                config.mainNetInfo.wif = mainNetjson["wif"];
                config.mainNetInfo.targetAddr = mainNetjson["targetAddr"];
                if (callback) {
                    callback();
                }
            });
        }
    }
    config.privateNetInfo = new NetInfo();
    config.mainNetInfo = new NetInfo();
    simpleWallet.config = config;
})(simpleWallet || (simpleWallet = {}));
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
    let NetEnum;
    (function (NetEnum) {
        NetEnum["Main"] = "\u4E3B\u7F51";
        NetEnum["privateChain"] = "\u79C1\u94FE";
    })(NetEnum = simpleWallet.NetEnum || (simpleWallet.NetEnum = {}));
    class DataInfo {
    }
    DataInfo.Neo = "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";
    DataInfo.Gas = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
    DataInfo.Pet = null;
    DataInfo.beMainNet = false;
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
    simpleWallet.Account = Account;
    class PageCtr {
        static start() {
            let changeBtn = document.getElementById("changeChain");
            changeBtn.onclick = () => {
                DataInfo.beMainNet = !DataInfo.beMainNet;
                document.getElementById("NetTag").innerHTML = DataInfo.beMainNet ? "当前网：主网" : "当前网：私链";
                let net = DataInfo.beMainNet ? NetEnum.Main : NetEnum.privateChain;
                this.chooseNet(net);
            };
            simpleWallet.PageCtr.chooseNet(simpleWallet.NetEnum.privateChain);
            let changePetBtn = document.getElementById("changePet");
            let PetInput = document.getElementById("petTag");
            PetInput.value = DataInfo.Pet;
            changePetBtn.onclick = () => {
                let asset = PetInput.value;
                if (asset != null) {
                    DataInfo.Pet = PetInput.value;
                }
            };
            let btn_transgas = document.getElementById("trans_gas");
            btn_transgas.onclick = () => {
                if (this.checkBeforeTransaction()) {
                    if (TransactionState.beGasTransing) {
                        alert("gas 交易进行中，请等待！");
                    }
                    else {
                        console.log("gas 交易： start！");
                        let gasinput = document.getElementById("gascount");
                        let value = parseFloat(gasinput.value);
                        this.transactionGas(value, this.currentAccount, this.targetAccount);
                    }
                }
            };
            let btn_transpet = document.getElementById("trans_pet");
            btn_transpet.onclick = () => {
                if (this.checkBeforeTransaction()) {
                    if (TransactionState.bePetTransing) {
                        alert("pet 交易进行中，请等待！");
                    }
                    else {
                        let petinput = document.getElementById("petcount");
                        let value = parseFloat(petinput.value);
                        this.transactionPet(value, this.currentAccount, this.targetAccount);
                    }
                }
            };
        }
        static chooseNet(net) {
            switch (net) {
                case NetEnum.Main:
                    DataInfo.APiUrl = simpleWallet.config.mainNetInfo.APiUrl;
                    DataInfo.targetAddr = simpleWallet.config.mainNetInfo.targetAddr;
                    DataInfo.WIF = simpleWallet.config.mainNetInfo.wif;
                    DataInfo.Pet = simpleWallet.config.mainNetInfo.petid;
                    break;
                case NetEnum.privateChain:
                    DataInfo.APiUrl = simpleWallet.config.privateNetInfo.APiUrl;
                    DataInfo.targetAddr = simpleWallet.config.privateNetInfo.targetAddr;
                    DataInfo.WIF = simpleWallet.config.privateNetInfo.wif;
                    DataInfo.Pet = simpleWallet.config.privateNetInfo.petid;
                    break;
            }
            var signBtn = document.getElementById("signin");
            var wifinput = document.getElementById("wif");
            wifinput.value = DataInfo.WIF;
            signBtn.onclick = () => {
                console.warn("sign!!!" + wifinput.value);
                let wif = wifinput.value;
                DataInfo.WIF = wif;
                console.log("@设置目标账户");
                this.sign(wif);
            };
            this.sign(DataInfo.WIF);
            let targetInput = document.getElementById("targetAddr");
            var targetBtn = document.getElementById("changeTarget");
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
        static sign(wif) {
            if (this.currentAccount) {
                this.currentAccount.existAccount();
            }
            console.log("@登录账户");
            this.currentAccount = new Account();
            this.currentAccount.neoInput = document.getElementById("c_neoinput");
            this.currentAccount.gasInput = document.getElementById("c_gasinput");
            this.currentAccount.PetInput = document.getElementById("c_petinput");
            try {
                this.currentAccount.setFromWIF(wif);
                this.currentAccount.refreshAllAssetCount();
            }
            catch (_a) {
            }
        }
        static setTargetAddr(addr) {
            if (this.targetAccount) {
                this.targetAccount.existAccount();
            }
            console.log("@设置目标账户");
            this.targetAccount = new Account();
            this.targetAccount.neoInput = document.getElementById("t_neoinput");
            this.targetAccount.gasInput = document.getElementById("t_gasinput");
            this.targetAccount.PetInput = document.getElementById("t_petinput");
            this.targetAccount.addr = DataInfo.targetAddr;
            this.targetAccount.refreshAllAssetCount();
        }
        static checkBeforeTransaction() {
            if (this.currentAccount == null) {
                alert("请登录账户！");
                return false;
            }
            else if (this.targetAccount == null) {
                alert("请设置目标账户！");
                return false;
            }
            else if (DataInfo.Pet == null) {
                alert("petid 未配置成功！");
                return false;
            }
            return true;
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
    simpleWallet.config.loadFromPath("lib/config.json", () => {
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