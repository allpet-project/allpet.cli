/// <reference path="neo-ts.d.ts" />
declare namespace simpleWallet {
    class NetInfo {
        petid: string;
        APiUrl: string;
        wif: string;
        targetAddr: string;
    }
    class config {
        static privateNetInfo: NetInfo;
        static mainNetInfo: NetInfo;
        static loadFromPath(path: string, callback: () => void): void;
    }
}
declare namespace tool {
    function loadJson(url: string, callback: (json) => void): void;
}
declare namespace simpleWallet {
    enum NetEnum {
        Main = "\u4E3B\u7F51",
        privateChain = "\u79C1\u94FE",
    }
    class DataInfo {
        static Neo: string;
        static Gas: string;
        static Pet: string;
        static beMainNet: boolean;
        static APiUrl: string;
        static WIF: string;
        static targetAddr: string;
    }
    class TransactionState {
        static beGasTransing: boolean;
        static bePetTransing: boolean;
    }
    class Account {
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
        setAssetCount(type: string, count: any): void;
        setFromWIF(wif: string): void;
        refreshAssetCount(type: string): void;
        refreshAllAssetCount(): void;
        existAccount(): void;
    }
    class PageCtr {
        static currentAccount: Account;
        static targetAccount: Account;
        static start(): void;
        static chooseNet(net: NetEnum): void;
        static sign(wif: string): void;
        static setTargetAddr(addr: string): void;
        static checkBeforeTransaction(): boolean;
        static transactionGas(count: number, from: Account, to: Account): void;
        static transactionPet(count: number, from: Account, to: Account): void;
        static checkTxExisted(txid: string): Promise<boolean>;
    }
}
declare namespace NetApi {
    function getAssetUtxo(url: string, address: string, asset: string): Promise<tool.UTXO[]>;
    function getnep5balancebyaddress(url: string, address: string, asset: string): Promise<number>;
    function getnep5decimals(url: string, asset: string): Promise<number>;
    function sendrawtransaction(url: string, rawdata: string): Promise<string>;
    function checktxboolexisted(url: string, txid: string): Promise<boolean>;
}
declare namespace tool {
    function makeRpcPostBody(method: string, ..._params: any[]): {};
}
declare namespace tool {
    function getassetutxobyaddress(url: string, address: string, asset: string): Promise<any>;
    function getnep5balancebyaddress(url: string, address: string, asset: string): Promise<any>;
    function getnep5decimals(url: string, asset: string): Promise<any>;
    function sendrawtransaction(url: string, rawdata: string): Promise<any>;
    function checktxboolexisted(url: string, txid: string): Promise<any>;
}
declare namespace tool {
    class CoinTool {
        static makeTran(utxos: UTXO[], targetaddr: string, assetid: string, sendcount: Neo.Fixed8): ThinNeo.Transaction;
    }
    class UTXO {
        addr: string;
        txid: string;
        n: number;
        asset: string;
        count: Neo.Fixed8;
        name: string;
        value: number;
    }
}
