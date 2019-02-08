/// <reference path="neo-ts.d.ts" />
declare namespace simpleWallet {
    class DataInfo {
        static Neo: string;
        static Gas: string;
        static Pet: string;
        static APiUrl: string;
        static WIF: string;
        static targetAddr: string;
        static currentAccount: Account;
        static targetAccount: Account;
    }
    class Account {
        addr: string;
        prikey: string;
        pubkey: string;
        neo: number;
        gas: number;
        pet: number;
        neoInput: HTMLInputElement;
        gasInput: HTMLInputElement;
        PetInput: HTMLInputElement;
        refreshAsset(type: string, count: any): void;
        setFromWIF(wif: string): void;
        refreshAssetCount(url: string): void;
    }
    class PageCtr {
        static start(): void;
    }
}
declare namespace NetApi {
    function getAssetUtxo(url: string, address: string, asset: string): Promise<UTXO[]>;
    function getnep5balancebyaddress(url: string, address: string, asset: string): Promise<number>;
    class UTXO {
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
declare namespace tool {
    function makeRpcPostBody(method: string, ..._params: any[]): {};
}
declare namespace tool {
    function getassetutxobyaddress(url: string, address: string, asset: string): Promise<any>;
    function getnep5balancebyaddress(url: string, address: string, asset: string): Promise<any>;
}
