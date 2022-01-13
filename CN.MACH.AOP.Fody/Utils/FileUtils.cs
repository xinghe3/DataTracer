using CN.MACH.AI.UnitTest.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.AOP.Fody.Utils
{
    public class FileUtils
    {
        static public bool CheckDirEmpty(string DirPath, bool IsNeedWarning = true)
        {
            if (!CheckDirExist(DirPath)) return true;
            //pbak是要检查是否为空的文件夹路径
            if (Directory.GetDirectories(DirPath).Length > 0 || Directory.GetFiles(DirPath).Length > 0)
            {
                return false;
            }
            return true;
        }

        static public bool CheckDirExist(string DirPath, bool IsNeedWarning = true)
        {
            if (!Directory.Exists(DirPath))
            {
                return false;
            }
            return true;
        }

        public static int Save(string content, string path, Encoding en, bool isDel = true)
        {
            try
            {
                if (string.IsNullOrEmpty(content))
                    return ErrorCode.EmptyParams;
                string Dir = PathUtils.GetDirPart(path);
                int nRet = CreateDirectory(Dir);
                if (nRet != ErrorCode.Success)
                {
                    return nRet;
                }
                /*目录删除*/
                string fname = path;
                if (isDel && CheckFileExist(fname))
                {
                    DeleteFile(fname);
                }
                /*定义文件信息对象*/
                FileInfo finfo = new FileInfo(fname);
                /*创建只写文件流*/
                using (FileStream fs = finfo.OpenWrite())
                {
                    /*根据上面创建的文件流创建写数据流*/
                    StreamWriter w = new StreamWriter(fs, en);
                    /*设置写数据流的起始位置为文件流的末尾*/
                    w.BaseStream.Seek(0, SeekOrigin.End);
                    try
                    {
                        /*写入内容*/
                        w.Write(content);
                    }
                    catch (Exception ex)
                    {
                        Logs.WriteExLog(ex);
                    }
                    /*清空缓冲区内容，并把缓冲区内容写入基础流*/
                    w.Flush();
                    /*关闭写数据流*/
                    w.Close();
                }
            }
            catch (Exception ex)
            {
                Logs.WriteExLog(ex, path);
                return ErrorCode.FileWriteException;
            }
            return ErrorCode.Success;
        }
        public static string Read(string Path, Encoding en)
        {
            if (!CheckFileExist(Path))
                return "";
            //if (CheckFileExist(Path, false))//can not check relative path.
            //{
            //    Logs.WriteWarning("Read File Not Exist",Path);
            //    return "";
            //}
            //Encoding fileEncoding = FileEncoding.GetEncoding(Path, System.Text.Encoding.Unicode);//取得这txt文件的编码
            StringBuilder sb = new StringBuilder(1024);
            FileStream fs = null;
            StreamReader m_streamReader = null;
            string strLine = null;
            try
            {
                fs = new FileStream(Path, FileMode.Open);
                if (fs.CanRead)
                {
                    m_streamReader = new StreamReader(fs, en);

                    m_streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    //string arry = "";
                    strLine = m_streamReader.ReadLine();

                    if (strLine != null)
                    {
                        sb.Append(strLine);
                        sb.AppendLine();

                        do
                        {
                            strLine = m_streamReader.ReadLine();
                            sb.Append(strLine);
                            sb.AppendLine();
                        } while (strLine != null);

                    }
                }
            }
            catch (DirectoryNotFoundException dnfex)
            {
                Logs.WriteExLog(dnfex);
            }
            catch (Exception ex)
            {
                Logs.WriteExLog(ex);
            }
            if (m_streamReader != null)
            {
                m_streamReader.Close();
                m_streamReader.Dispose();
            }
            if (fs != null)
            {
                fs.Close();
                fs.Dispose();
            }
            return sb.ToString();
        }
        static public bool CheckFileExist(string FilePath)
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }
            return true;
        }
        public static int DeleteFile(string savePath)
        {
            if (File.Exists(savePath))
            {
                try
                {
                    File.Delete(savePath);//如果文件已经存在就将已存在的文件删除
                }
                catch (Exception ex)
                {
                    Logs.WriteExLog(ex);
                    return ErrorCode.DeleteDirErr;
                }
            }
            return ErrorCode.Success;
        }

        public static int CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return ErrorCode.ErrParams;
            //创建目录
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (ArgumentException ae)
            {
                Logs.WriteExLog(ae);
                return ErrorCode.ErrParams;
            }
            catch (UnauthorizedAccessException uae)
            {
                Logs.WriteExLog(uae);
                return ErrorCode.CannotAccess;
            }
            catch (Exception ex)
            {
                Logs.WriteExLog(ex, path);
                return ErrorCode.CreateDirOrFileFail;
            }
            return ErrorCode.Success;
        }
    }

}
