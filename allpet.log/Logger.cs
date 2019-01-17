using System;
using System.Collections.Generic;
using System.Text;

namespace AllPet.Log
{
    public class Logger : ILogger
    {
        public enum OUTPosition
        {
            Console = 0x01,
            Trace = 0x02,
            Debug = 0x04,
            File = 0x08,
            Other = 0x10,
        }
        string _outfilepath;
        public string outfilepath
        {
            get
            {
                return _outfilepath;
            }
            set
            {
                _outfilepath = value;
                if (outfilepath.Contains("/"))
                {
                    var path = System.IO.Path.GetDirectoryName(outfilepath);
                    if (System.IO.Directory.Exists(path) == false)
                        System.IO.Directory.CreateDirectory(path);
                }
            }
        }
        public ILogger otherLogger
        {
            get; set;
        }
        public OUTPosition outtag_info
        {
            get; set;
        }
        public OUTPosition outtag_warn
        {
            get; set;
        }
        public OUTPosition outtag_error
        {
            get; set;
        }
        public Logger()
        {
            var time = DateTime.Now;
            var filetime = time.ToString("yyyyMMdd_HHmmss");
            outfilepath = "log/log_" + filetime + ".log";
            otherLogger = null;
            outtag_info = OUTPosition.Console;
            outtag_warn = OUTPosition.Console | OUTPosition.Trace | OUTPosition.File;
            outtag_error = OUTPosition.Console | OUTPosition.Trace | OUTPosition.File;
        }

        void WriteLine(string tag, OUTPosition outtag, string str)
        {
            if ((outtag & OUTPosition.Console) > 0)
            {
                Console.Write(tag);
                Console.WriteLine(str);
            }
            if ((outtag & OUTPosition.Trace) > 0)
            {
                System.Diagnostics.Trace.Write(tag);
                System.Diagnostics.Trace.WriteLine(str);
            }
            if ((outtag & OUTPosition.Debug) > 0)
            {
                System.Diagnostics.Debug.Write(tag);
                System.Diagnostics.Debug.WriteLine(str);
            }
            if ((outtag & OUTPosition.File) > 0)
            {
                try
                {
                    System.IO.File.AppendAllText(outfilepath, tag + str, System.Text.Encoding.UTF8);
                }
                catch
                {
                    Console.Write("<LOG ERROR>cant write to file:" + outfilepath);
                }
            }

        }
        public void Info(string str)
        {
            WriteLine("<I>", outtag_info, str);
            if (otherLogger != null && (outtag_info & OUTPosition.Other) > 0)
            {
                otherLogger.Info(str);
            }
        }

        public void Warn(string str)
        {
            WriteLine("<W>", outtag_warn, str);
            if (otherLogger != null && (outtag_info & OUTPosition.Other) > 0)
            {
                otherLogger.Warn(str);
            }
        }

        public void Error(string str)
        {
            WriteLine("<E>", outtag_error, str);
            if (otherLogger != null && (outtag_error & OUTPosition.Other) > 0)
            {
                otherLogger.Error(str);
            }
        }
    }

}
