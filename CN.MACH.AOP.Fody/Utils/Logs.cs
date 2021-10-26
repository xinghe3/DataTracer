using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Collections;

namespace CN.MACH.AI.UnitTest.Core.Utils
{
    /// <summary>
    /// error level use in logs
    /// </summary>
    class LogErrorLevel
    {
        /// <summary>
        /// normal information.
        /// </summary>
        public const int information = 0;
        /// <summary>
        /// debug information.
        /// </summary>
        public const int debug = 1;
        /// <summary>
        /// warning information.
        /// </summary>
        public const int warning = 2;
        /// <summary>
        /// error information.
        /// </summary>
        public const int error = 3;
        /// <summary>
        /// exception information.
        /// </summary>
        public const int exception = 4;
    }
    /// <summary>
    /// logs
    /// </summary>
    public class Logs
    {
        private static string LogFileDir = "D:\\";

        private const string errorLogPath = "\\error0.txt";///< error information.
        private const string exceptionLogPath = "\\explog.txt";///< exception information.
        
        private static int ErrorLevel = LogErrorLevel.information;

        public static string GetLogDir(string fileName = null)
        {
            //设置文件目录（IP文件夹）
            string fileDir = LogFileDir + "Logs/" + DateTime.Now.Year + "/"
                + DateTime.Now.Month + "/" + DateTime.Now.Day + "/";            // "/" + DateTime.Now.Day;
            if(!string.IsNullOrWhiteSpace(fileName))
            {
                fileDir += PathUtils.GetDirPart(fileName);
            }
            // create dir
            if (!Directory.Exists(fileDir))
            {
                Directory.CreateDirectory(fileDir);
            }
            return fileDir;
        }

        //写异常日志
        public static void WriteExLog(Exception ex)
        {
            //string str = ex.Message + Environment.NewLine + ex.StackTrace;
            WriteExLog(ex,"");
        }

        public static void WriteExLog(Exception ex, params object[] inputs)
        {
            WriteExceptionToFile(exceptionLogPath, ex, inputs);
        }
        private static void WriteExceptionToFile(string logPath, Exception ex, params object[] inputs)
        {
            if (ex == null) return;
            ErrorLevel = ErrorLevel < LogErrorLevel.exception ? LogErrorLevel.exception : ErrorLevel;
            string strInfo = BuildExceptionInformation(ex, inputs);
            WriteLogFile(strInfo, logPath);
        }
        public static string BuildExceptionInformation(Exception ex, params object[] inputs)
        {
            int i = 0;
            while (ex.InnerException != null && (i++) < 10 && 0 <= ex.Message.IndexOf("See the inner exception for details."))
            {
                ex = ex.InnerException;
            }
            string input = BuildLogParams(inputs);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(input);
            sb.AppendLine("(" + ex.GetType().FullName + ") " + ex.Message);
            
            // add 2018-04-09
            sb.AppendLine("Is a systemexception?" + (ex is SystemException));
            sb.AppendLine("Member name:" + ex.TargetSite);
            sb.AppendLine("Class defing member:" + ex.TargetSite.DeclaringType);
            sb.AppendLine("Member type:" + ex.TargetSite.MemberType);
            if (ex.Data != null)
            {
                foreach (DictionaryEntry de in ex.Data)
                {
                    sb.AppendLine("->"+de.Key+":"+de.Value);
                }
                

            }
            // add 2018-04-09

            sb.AppendLine(ex.StackTrace);
            // string strInfo = input + Environment.NewLine + "(" + ex.GetType().FullName + ") " + ex.Message + Environment.NewLine + ex.StackTrace;

            return sb.ToString();
        }
        
        public static void WriteError(params object[] inputs)
        {
            ErrorLevel = ErrorLevel < LogErrorLevel.error ? LogErrorLevel.error : ErrorLevel;
            string input = BuildLogParams(inputs);
            string stackInfo = GetStackTrace();
            input += Environment.NewLine + stackInfo;
            WriteLogFile(input, errorLogPath);
        }
        
        private static string GetStackTrace()
        {
            string stackInfo = new System.Diagnostics.StackTrace(2).ToString();
            string stackNewInfo = StringUtils.Left(stackInfo, "System.Windows.Forms", false);
            if (!string.IsNullOrWhiteSpace(stackNewInfo)) stackInfo = stackNewInfo;
            return stackInfo;
        }
        // format log need to add support of IList<string> datatable hashtable dictionary array
        private static string BuildLogParams(params object[] inputs)
        {
            return ">>" + StringUtils.ToString(inputs);
        }

        private static bool IsNeedWriteLog(string input)
        {
            bool bRet = true;
            if (ErrorLevel == LogErrorLevel.information) return bRet;
            return bRet;
        }
        /// <summary>
        ///  change this function to public write log by custom.
        /// wait for modify
        /// if html add to head and tailer.
        /// <html><head><meta http-quiv="content-type" content="text/html;charset=utf-8" /></head><body>
        /// </body></html>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="fileName"></param>
        public static void WriteLogFile(string input, string fileName)
        {
            if (string.IsNullOrWhiteSpace(input)) return;
            if (!IsNeedWriteLog(input)) return;

            try
            {
                /*指定日志文件的目录*/
                string logdir = GetLogDir(fileName);
                string fname = logdir + PathUtils.GetFilePart(fileName);
                /*定义文件信息对象*/
                FileInfo finfo = new FileInfo(fname);
                /*判断文件是否存在以及是否大于100K*/
                if (finfo.Exists && finfo.Length > 102400)
                {
                    // copy and rename file.
                    string newfname = logdir + PathUtils.GetFileNamePart(fileName) + DateUtils.GetNowString() + PathUtils.GetExtPart(fileName);
                    //DosFileHelper.CopyAndRename(fname, newfname);
                    File.Move(fname, newfname);
                
                    /*删除该文件*/
                    finfo.Delete();
                }
                /*创建只写文件流*/
                using (FileStream fs = finfo.OpenWrite())
                {
                    /*根据上面创建的文件流创建写数据流*/
                    StreamWriter w = new StreamWriter(fs);
                    /*设置写数据流的起始位置为文件流的末尾*/
                    w.BaseStream.Seek(0, SeekOrigin.End);
                    /*写入当前系统时间并换行*/
                    //w.Write("{0} {1} \r\n\r\n", DateTime.Now.ToLongTimeString(),
                    //    DateTime.Now.ToLongDateString());
                    try
                    {
                        /*写入日志内容并换行*/
                        w.Write(input + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        w.Write(ex.Message + Environment.NewLine + ex.StackTrace);
                    }
                    /*清空缓冲区内容，并把缓冲区内容写入基础流*/
                    w.Flush();
                    /*关闭写数据流*/
                    w.Close();
                }
            }
            catch
            {
                //暂不处理
            }
        }

    }
}