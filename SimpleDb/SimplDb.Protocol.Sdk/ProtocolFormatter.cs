using SimplDb.Protocol.Sdk.Message;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SimplDb.Protocol.Sdk
{
    public class ProtocolFormatter
    {
        /// <summary>
        /// 序列化命令协议
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static  byte[] Serialize<T>(Method method,T command) where T : ICommand
        {
            MsgHead head = new MsgHead { Version = 1, Method = (ushort)method };

            var data = SimpledbMessageSwitch.CommandToBytes<T>(command);
            head.Len = data.Length;

            var msgHead = SimpledbMessageSwitch.StructToBytes<MsgHead>(head);

            byte[] retData = new byte[msgHead.Length+ data.Length];
            //先放头
            Array.Copy(msgHead, 0, retData, 0, msgHead.Length);
            //再放command
            Array.Copy(data, 0, retData, msgHead.Length, data.Length);

            return retData;
        }
        /// <summary>
        /// 反序列化命令协议
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ICommand Deserialize(byte[] data) 
        {

            MsgHead msgHead = new MsgHead { };

            var headLen = Marshal.SizeOf(msgHead);

            byte[] headByte = new byte[headLen];
            //先把头取出来
            Array.Copy(data, 0, headByte, 0, headLen);
            
            //把头给去掉
            byte[] commandByte = new byte[data.Length - headLen];

            //再把command取出来
            Array.Copy(data, headLen, commandByte, 0, commandByte.Length);

            //判断是执行那个命令
            msgHead = SimpledbMessageSwitch.BytesToStruct<MsgHead>(headByte);
            Method method = (Method)msgHead.Method;
            ICommand command = null;
            switch (method)
            {
                case Method.CreateTable:
                    command =  SimpledbMessageSwitch.BytesToCommand<CreatTableCommand>(commandByte) ;
                    break;
                case Method.Delete:
                    command = SimpledbMessageSwitch.BytesToCommand<DeleteCommand>(commandByte);
                    break;
                case Method.DeleteTable:
                    command = SimpledbMessageSwitch.BytesToCommand<DeleteTableCommand>(commandByte);
                    break;
                case Method.PutDirect:
                    command = SimpledbMessageSwitch.BytesToCommand<PutDirectCommand>(commandByte);
                    break;
                case Method.PutUint64:
                    command = SimpledbMessageSwitch.BytesToCommand<PutUInt64Command>(commandByte);
                    break;
                case Method.GetDirect:
                    command = SimpledbMessageSwitch.BytesToCommand<GetDirectCommand>(commandByte);
                    break;
                case Method.GetUint64:
                    command = SimpledbMessageSwitch.BytesToCommand<GetUint64Command>(commandByte);
                    break;
            }
            return command;
        }

    }
}
