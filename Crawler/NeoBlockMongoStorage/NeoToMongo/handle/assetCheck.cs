using System;
using System.Collections.Generic;
using System.Text;

namespace NeoToMongo
{
    class assetCheck
    {
        public static bool CheckTxAsset(MyJson.JsonNode_Object txItem)
        {
            if (Config.enableAssetFilter)
            {
                var vout_tx = txItem["vout"].AsList();
                var vin_tx = txItem["vin"].AsList();

                if (vout_tx.Count > 0)
                {
                    MyJson.JsonNode_Object voutitem = vout_tx[0].AsDict();
                    var assetID = voutitem["asset"].AsString();
                    return Config.asseFiltertList.Contains(assetID);
                }else
                {
                    //if(vin_tx.Count>0)
                    //{
                    //    Console.WriteLine("不科学！！");
                    //}
                    return false;
                }
            }else
            {
                return true;
            }
        }

        public static bool checkNotifyAsset(MyJson.JsonNode_Object notification)
        {
            if (Config.enableAssetFilter)
            {
                string assetId = notification["contract"].AsString();
                return Config.asseFiltertList.Contains(assetId);
            }else
            {
                return true;
            }

        }
    }
}
