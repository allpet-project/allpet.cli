# 编译说明

因为目前处于开发状态，所需库仍未发布到nuget

所以首先需要下载编译依赖库，
allpet.http.server
allpet.peer.tcp.interface
allpet.peer.tcp.peerv2
如这三个项目都放置在d:\git下，则默认输出目录会有
d:\git\allpet.bin 目录，里面会有生成的nuget文件

在 程序-》选项-》程序包源 中 添加一个新源，指向该目录

即可从该源安装所需包