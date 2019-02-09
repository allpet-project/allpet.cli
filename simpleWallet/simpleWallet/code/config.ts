namespace simpleWallet {

    export class NetInfo {
        petid: string;
        APiUrl: string;
        wif: string;
        targetAddr: string;
    }
    export class config {

        static privateNetInfo: NetInfo = new NetInfo();
        static mainNetInfo: NetInfo = new NetInfo();


        static loadFromPath(path: string = "../lib/config.json",callback:() => void)
        {
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
            })
        }
    }
}