1.node start----->调用 connectone 连接配置中initpeer节点。具体有：a.creat new linkobj加入到linknodes中。

2.监听OnPeerLink，本地节点、远程节点相互发送 Tell_ReqJoinPeer消息，以下以本地节点向远程节点发Tell_ReqJoinPeer消息为例。

3.远程节点接收处理消息OnRec_requestjoinPeer，具体有：a.校验    b.完善linkobj的信息id、publick endpoint；处理结束向本地节点发送Tell_ResponseAcceptJoin消息。

4.本地节点接收处理消息OnRec_responseAcceptJoin，具体有：a.完善linkobj的信息hadjoin 。
向远程节点发送消息a. 有私钥的话，签名消息发送 Tell_Request_ProvePeer b.Tell_Request_PeerList。

5.远程节点接收处理消息 OnRecv_RequestProvePeer，具体：a.进行验签，完善linkobj的信息PublicKey；
 接收处理消息 OnRecv_Request_PeerList，向本地节点发送 Tell_Response_PeerList消息。

6.本地节点接收处理消息OnRecv_Response_PeerList，具体：a.将请求到的可联接节点塞到listcanlink中。
