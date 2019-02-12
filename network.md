# Allpet的P2P组网介绍
    P2P的组网不可能平白无故的产生,首先要有初始节点，要有若干个初始节点，提供初始的P2P服务，保证P2P网络运行。
    每个节点打开的时候，先尝试连接初始节点，每当连到一个初始节点，会询问初始节点有多少个节点连接到你，
    把它们全部共享给我，然后再去尝试连接这些共享的节点，这个过程是会不断重复的，每当连上新的节点（非初始节点），
    就去问问它，他有多少节点，把它们共享给我，再去尝试连接这些新的节点，循环往复，这就是P2P组网的过程。  
     
     P2P组网分为两种：消极和积极。    
	 消极：client连上server后，不需要再做什么，server就可以和client相互通信.     
	 积极：client连上server后，还要始终监听一个固定端口，积极提供外网通道，方能维护和server的相互通信.      
	 目前要想提供稳定的通讯环境，必须要采用积极P2P，客户端积极提供外网通道。所谓的外网通道如下所示：   
	 1.本身是外网电脑，直接公开自己端口，并且监听这个端口。   
	 2.本身不是外网电脑，通过网关提供通信，那么就要先主动配置好，网关的端口到本身的端口的映射，并公开网关的IP和端口。    
	 
	 组网第一步的要求有两点：    
	 1.能够做新加入的节点的发现。定期的和其它节点通信，发现新加入的节点，更新本身的连接列表。    
	 2.能够断线重连。首先各个待连接的节点要有权重评级，权重高的优先连接，权重可以根据断裂的次数升降级。    
	   可以存储各个连接节点，下次开机可以直接根据这些存储的节点列表，先尝试连接。    
	 
	组网的第二步，要区分记账节点，从已连接的节点中区分出记账节点，单独加入一个列表中。    
	记账人的区分，详见`allpet.module.node` 的配置。区分出记账人后就衍生出下列功能：    
	1.记账人间的广播，也称之为`小广播`。所有共识，只需要记账人间的广播就可以了，其它节点间广播都是在浪费流量。        
	2.sendraw广播，就是其它节点要想发一个消息，需要设计一个连接到记账的人优先级，根据这个优先级做上行广播。      
	  本质上来说sendraw就是发消息给记账人，记账人的优先级设计为0，连到记账人的节点的优先级加1，后续节点依次递增。        
	  那么发送节点只要根据这个优先级，上行寻找，就能找到它和记账人间的最短通路，沿着这个通路将消息发送给记账人就可以了。     
	3.bordcast广播，记账人之间有什么数据。它们已经达成共识的信息后，将共识结果大广播出来。       
      sendraw是只会发给小于本身节点优先级的，而大广播只会发给大于等于本身节点的，并且告诉它们接着向外散播，     
      它们接到消息后也会帮着向外散播，这样就形成了全网广播。
	 

##模块介绍
* allpet.common    
   公共模块。主要提供`Config`和`Logger`的公共处理   
 
    `Config`   	
	是对json配置文件处理实现，示例如下：    
	```csharp
	 var config_cli = config.GetJson("config.json", ".ModulesConfig.Cli") as JObject;
	 var endpoint = config.GetIPEndPoint("config.json", ".ListenEndPoint");
	```
	
	`Logger`   	
	Log类型有三种：   
	```csharp
		public enum LogType
        {
            Info,
            Warn,
            Error,
        }
	```
	Log输出方式可以叠加使用，共有5种：  
	```csharp
		public enum OUTPosition
        {
            Console = 0x01,
            Trace = 0x02,
            Debug = 0x04,
            File = 0x08,
            Other = 0x10,
        }
	```
	Trace和Debug由系统捕获，输出因系统而定。     
	Info的默认输出方式为：`Console`    
	Warn的默认输出方式为：`Console`、`Trace`、`File`    
	Error的默认输出方式为：`Console`、`Trace`、`File`         
	
* allpet.Peer.Tcp.PeerV2   
   网络通信模块。提供网络通信处理       
   
* allpet.peer.pipeline   
   管线模块。是一个类似于Actor模型的实现。示例如下：
   ```csharp
	    //让GetPipeline来自动连接,此时remotenode 不可立即通讯，等回调，见RegNetEvent
        var remotenode = this.GetPipeline(p.ToString() + "/node");//模块的名称是固定的
	```
	调用`GetPipeline`后就会立马尝试连接，根据网络连接状态，调用网络连接事件，网络断开时，也会调用关闭连接事件。      
	
* allpet.httpserver   
   http服务模块。      
   
* allpet.module.node   
   Node模块。主要提供P2P组网节点的连接和数据收发功能。    
   记录了所有连接自己的管道，目前没有大小的限制，以后根据需要限制连接的管道数量     
   ```javascript
	{
	  "ModulesConfig": {
		"Cli": {
		  "Open": true

		},
		"Node": {
		  "Open": true,
		  "PublicEndPoint": "0.0.0.0:2080",
		  "InitPeer": [
			"127.0.0.1:2081",
			"212.64.86.72:2081"
		  ],
		  "ChainInfo": {
			"MagicStr": "HelloKitty",
			"InitOwner": [ "AdsNmzKPPG7HfmQpacZ4ixbv9XJHJs2ACz" ]
		  },
		  //给节点配置key，配置了key他就可以用来证明自己的身份，比如共识
		  "Key_Nep2": "6PYT8kA51wDoG1zHMVPVEnWKM2ptRoUTSxa2EWumAEahBtkuimfMuwWsBj",
		  //Key密码，如果配置了Nep2 确没有配置密码，则需要输入密码
		  "Key_Password": "12345678"
		},
		"RPC": {
		  "Open": true,
		  "HttpListenEndPoint": "0.0.0.0:30080",
		  //https 端口设置为零表示不开https，开https就必须提供pfx证书和密码 一起
		  "HttpsListenEndPoint": "0.0.0.0:0",
		  "HttpsPFXFilePath": "",
		  "HttpsPFXFilePassword": ""
		}
	  },
	  "ListenEndPoint": "0.0.0.0:2080"
	}
	```   
	如上所示，node有三种类型：`Cli`、`Node`、`RPC` 。根据需要可以配置成不同类型的节点。
    ListenEndPoint：配置监听端口，配置成0.0.0.0意味着监听本机的所有IP。
	
	`Cli`   
	Open :配置是否加载Cli模块，开启Cli连接     
	
	`Node`  
	Open:配置是否加载node模块，开启node连接      
	PublicEndPoint:配置公开端口，IP配置成0.0.0.0意味着不知道自己的公网IP，向服务器发起连接后，服务器自己可以获取到客户端的IP。端口号配置成0意味着不公开客户端IP     
	InitPeer:配置初始节点的IP和端口    
	ChainInfo：配置区分不同链的依据。
	          MagicStr：为一个魔法字串。
	          InitOwner：初始记账人。
    Key_Nep2和Key_Password：配置识别身份的key，根据这个配置计算私钥和公钥，和ChainInfo.InitOwner配置共同匹配识别出记账人节点    
	
	`RPC`   
	Open :配置是否加载RPC模块，开启RPC连接。     
	HttpListenEndPoint：配置RPChttp服务监听端口。     
	HttpsListenEndPoint：配置RPChttps服务监听端口。
    HttpsPFXFilePath：https的pfx证书。
	HttpsPFXFilePassword：https的密码
	
* allpet.module.rpc     
   RPC服务模块。     
   
* allpet.node.cli   
   Cli服务节点
   
  
