using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SimplDb.Protocol.Sdk
{
    class SimpledbMessageSwitch
    {
        /// <summary>
        /// 将结构体转换为byte数组
        /// </summary>
        /// <typeparam name="T"> 泛型T</typeparam>
        /// <param name="structObj">结构体对象</param>
        /// <returns></returns>
        public static byte[] StructToBytes<T>(T structObj) where T : struct
        {
            // 获取结构体对象的字节数
            int size = Marshal.SizeOf(structObj);
            byte[] bytes = new byte[size];
            // 申请内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体内容拷贝到上一步申请的内存空间
            Marshal.StructureToPtr(structObj, structPtr, false);
            // 将数据拷贝到byte数组
            Marshal.Copy(structPtr, bytes, 0, size);
            // 释放申请的内存
            Marshal.FreeHGlobal(structPtr);

            return bytes;
        }

        public static T BytesToStruct<T>(byte[] bytes) where T : struct
        {
            T obj = new T();

            int size = Marshal.SizeOf(obj);

            // 如果结构体对象的字节数大于所给byte数组的长度，则返回空
            if (size > bytes.Length)
            {
                return (default(T));
            }

            IntPtr structPtr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, structPtr, size);
            object tempObj = Marshal.PtrToStructure(structPtr, obj.GetType());
            Marshal.FreeHGlobal(structPtr);

            return (T)tempObj;
        }

        public static byte[] CommandToBytes<T>(T command) where T:ICommand
        {
            Type commandType = command.GetType();
            MemoryStream ms = new MemoryStream();
            
            object fieldValue;
            TypeCode typeCode;
            
            int fiedLen = 0;
            foreach (var field in commandType.GetFields())
            {
                fieldValue = field.GetValue(command); // Get value
                typeCode = Type.GetTypeCode(fieldValue.GetType());  // get type
                byte[] temp=null;
                //每个参数前要有类型（固定为1个字节）
                byte[] typeCodeBytes = new byte[] { (byte)typeCode };
                ms.Write(typeCodeBytes, 0, typeCodeBytes.Length);

                if (typeCode == TypeCode.Object || typeCode == TypeCode.String)
                {
                    //如果是Object要有长度
                    if (typeCode == TypeCode.String)
                    {
                        temp = System.Text.Encoding.UTF8.GetBytes(fieldValue.ToString());
                        var lenByte = BitConverter.GetBytes(temp.Length);
                        ms.Write(lenByte, 0, lenByte.Length);
                        ms.Write(temp, 0, temp.Length);
                    }
                    else
                    {                        
                        fiedLen = ((byte[])fieldValue).Length;
                        var lenByte = BitConverter.GetBytes(fiedLen);
                        ms.Write(lenByte, 0, lenByte.Length);
                        ms.Write(((byte[])fieldValue), 0, fiedLen);
                    }
                    continue;
                }

                switch (typeCode)
                {
                    case TypeCode.Single: 
                        {                            
                            temp = BitConverter.GetBytes((Single)fieldValue);
                            break;
                         }
                    case TypeCode.Int32:
                        {
                            temp = BitConverter.GetBytes((Int32)fieldValue);
                            break;
                        }
                    case TypeCode.UInt32:
                        {
                            temp = BitConverter.GetBytes((UInt32)fieldValue);
                            break;
                        }
                    case TypeCode.Int16:
                        {
                            temp = BitConverter.GetBytes((Int16)fieldValue);
                            break;
                        }
                    case TypeCode.UInt16:
                        {
                            temp = BitConverter.GetBytes((UInt16)fieldValue);
                            break;
                        }
                    case TypeCode.Int64:
                        {
                            temp = BitConverter.GetBytes((Int64)fieldValue);
                            break;
                        }
                    case TypeCode.UInt64:
                        {
                            temp = BitConverter.GetBytes((UInt64)fieldValue);
                            break;
                        }
                    case TypeCode.Double:
                        {
                            temp = BitConverter.GetBytes((Double)fieldValue);
                            break;
                        }
                    case TypeCode.Byte:
                        {
                            temp = new byte[1];
                            temp[0]= (Byte)fieldValue;
                            break;
                        }
                    default:
                        break;
                }
                if (temp != null)
                {
                    ms.Write(temp, 0, temp.Length);
                }
                
            }
            return ms.ToArray();
        }

        public static T BytesToCommand<T>(byte[] data) where T : new()
        {
            //获取TypeCode
            var datalen = data.Length;
            T command = new T();


            Type commandType = command.GetType();
            var obj = Activator.CreateInstance(commandType);

            int offset = 0;
            TypeCode typeCode;
            byte[] temp = null;
            object fieldValue = null;

            int fiedLen = 0;
            foreach (var field in commandType.GetFields())
            {
                //先取参数的类型（固定为1个字节）
                typeCode = (TypeCode)data[offset];
                offset += sizeof(byte);

                if (typeCode == TypeCode.Object || typeCode == TypeCode.String)
                {
                    //如果是Object先读长度（int 4个字节）
                    fiedLen = BitConverter.ToInt32(data, offset);
                    offset += sizeof(int);
                    temp = new byte[fiedLen];
                    Array.Copy(data, offset, temp, 0, fiedLen);
                    offset += fiedLen;

                    if (typeCode == TypeCode.String)
                    {
                        var stringValue = System.Text.Encoding.UTF8.GetString(temp);
                        field.SetValue(obj, stringValue);
                    }
                    else
                    {
                        field.SetValue(obj, temp);
                    }
                    continue;
                }

                switch (typeCode)
                {
                    case TypeCode.Single: // float
                        {
                            fiedLen = sizeof(Single);
                            temp = new byte[fiedLen];
                            Array.Copy(data, offset, temp, 0, fiedLen);
                            fieldValue = BitConverter.ToSingle(temp,0);
                            break;
                        }
                    case TypeCode.Int32:
                        {
                            fiedLen = sizeof(Int32);
                            temp = new byte[fiedLen];
                            Array.Copy(data, offset, temp, 0, fiedLen);
                            fieldValue = BitConverter.ToInt32(temp, 0);
                            break;
                        }
                    case TypeCode.UInt32:
                        {
                            fiedLen = sizeof(UInt32);
                            temp = new byte[fiedLen];
                            Array.Copy(data, offset, temp, 0, fiedLen);
                            fieldValue = BitConverter.ToUInt32(temp, 0);
                            break;
                        }
                    case TypeCode.Int16:
                        {
                            fiedLen = sizeof(Int16);
                            temp = new byte[fiedLen];
                            Array.Copy(data, offset, temp, 0, fiedLen);
                            fieldValue = BitConverter.ToInt16(temp, 0);
                            break;
                        }
                    case TypeCode.UInt16:
                        {
                            fiedLen = sizeof(UInt16);
                            temp = new byte[fiedLen];
                            Array.Copy(data, offset, temp, 0, fiedLen);
                            fieldValue = BitConverter.ToUInt16(temp, 0);
                            break;
                        }
                    case TypeCode.Int64:
                        {
                            fiedLen = sizeof(Int64);
                            temp = new byte[fiedLen];
                            Array.Copy(data, offset, temp, 0, fiedLen);
                            fieldValue = BitConverter.ToInt64(temp, 0);
                            break;
                        }
                    case TypeCode.UInt64:
                        {
                            fiedLen = sizeof(UInt64);
                            temp = new byte[fiedLen];
                            Array.Copy(data, offset, temp, 0, fiedLen);
                            fieldValue = BitConverter.ToUInt64(temp, 0);
                            break;
                        }
                    case TypeCode.Double:
                        {
                            fiedLen = sizeof(Double);
                            temp = new byte[fiedLen];
                            Array.Copy(data, offset, temp, 0, fiedLen);
                            fieldValue = BitConverter.ToDouble(temp, 0);
                            break;
                        }
                    case TypeCode.Byte:
                        {
                            fiedLen = sizeof(Byte);
                            temp = new byte[fiedLen];
                            Array.Copy(data, offset, temp, 0, fiedLen);
                            fieldValue = temp[0];
                            break;
                        }
                    default:
                        break;
                }                
                field.SetValue(obj, fieldValue);
                offset += fiedLen;

            }
            return (T)obj;
        }
    }
}
