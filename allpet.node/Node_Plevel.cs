using AllPet.Pipeline.MsgPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace AllPet.Module
{
    partial class Module_Node : Module_MsgPack
    {
        /// <summary>
        /// linkobj告诉我他的plevel
        /// </summary>
        /// <param name="link"></param>
        /// <param name="linkPlevel"></param>
        void getPlevelFromLinkObj(LinkObj link, int linkPlevel)
        {
            if (this.beObserver) return;
            if (linkPlevel >= 0)//当libobj 的plevel=-1的时候，本节点不受它影响
            {
                //linkobj的初值为-1，对方可能发多次消息过来告知plevel变更。
                //进入这个函数方式：1.OnRecv_ResponseAcceptJoin 2.接到广播OnRecv_BoradCast_PeerState(向周边发广播原因：1.本节点优先级变大了。2.收到OnRecv_BoardCast_LosePlevel 周边节点需要重新传递plevel)
                //gaoxiqing
                //这种假设应该是错的，没办法保证只往小的变，如果你真想区分多个命令的先后的话，建议加上时间戳，根据时间戳判断肯定不会错  

                // # 回应 #原因见进入方式，所以不需要时间戳
                if (link.pLevel >= linkPlevel || link.pLevel == -1)
                {
                    link.pLevel = linkPlevel;

                    if (this.pLevel > linkPlevel + 1 || this.pLevel == -1)//linkobj告知本机刷新plevel
                    {
                        this.pLevel = linkPlevel + 1;
                        foreach (var item in this.linkNodes)
                        {
                            //不管linkobj是否hasjoin，都给它发消息.如果linknode最终还是hasjoin=false,就当做发一次无效消息。
                            //判断linkobj hasjoin=ture才发的话，有可能本节点已告知过该节点plevel（request_joinpeer）,response消息还没返回，该节点在本节点向下广播的时候没有收到消息，之后也不会被告知本节点刷新plevel了，
                            //gaoxiqing
                            //不判断hasjoin=ture的话，有时是会出异常的。假设那个连接它真就没有连上，这时你给它发信，肯定出异常。
                            //我觉得对应这种情况，应该在ResponseAcceptJoin中加上刷新level回执消息，无论什么时候收到，都去刷下对方的level

                            //# 回应# 1.onpeerlink之后才会开始发消息，所以连接上了 2.actor发消息不关心有没有连接上，如果异常了，底层没处理好，修就行了。
                            //# 回应# 在ResponseAcceptJoin中加上刷新level回执消息,这种方式也行，但是增加了发消息的频度，这个消息是可以不发的。
                            if ((item.Value.pLevel > this.pLevel || item.Value.pLevel == -1))
                            {
                                Tell_BoradCast_PeerState(item.Value.remoteNode);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// linkobj的plevel无效了
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>本节点是否被影响</returns>
        bool losePlevelFromLinkObj(LinkObj obj)
        {
            if (obj.pLevel != -1 && obj.pLevel < this.pLevel)//判断是否优先级比本节点高
            {
                obj.pLevel = -1;//重置
                if (!this.checklinkNodesPlevelUpThis())
                {
                    this.losePlevel();
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 检测周边有没有优先级比自己高的
        /// </summary>
        /// <returns></returns>
        bool checklinkNodesPlevelUpThis()
        {
            foreach (var linkobj in this.linkNodes.Values)
            {
                if (linkobj.pLevel != -1 && linkobj.pLevel < this.pLevel)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 本节点的plevel变的无意义了，被重置
        /// </summary>
        void losePlevel()
        {
            this.pLevel = -1;
            foreach (var linkobj in this.linkNodes.Values)
            {
                linkobj.pLevel = -1;//连接的plevel也重置，等待回应
                this.Tell_BoardCast_LosePlevel(linkobj.remoteNode);
            }
        }
    }
}
