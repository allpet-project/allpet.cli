
# 编译说明

因为目前处于开发状态，所需库仍未发布到nuget

暂时设置这个临时项目，所有代码堆在一个仓库里，逐渐分。
现在暂时不想因为项目nuget配置分神。



所以首先需要下载编译依赖库，
allpet.http.server
allpet.peer.tcp.interface
allpet.peer.tcp.peerv2
...
...
...
如上项目都放置在d:\git下，则默认输出目录会有
d:\git\allpet.bin 目录，里面会有生成的nuget文件

在 程序-》选项-》程序包源 中 添加一个新源，指向该目录

即可从该源安装所需包

* [组网介绍](./network.md)      
