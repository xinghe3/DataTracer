using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CN.MACH.AI.UnitTest.Core;

namespace CN.MACH.AI.UnitTest.Core.Utils
{
    public class PathUtils
    {
        static private char _slash = '/';
        static private char _backslash = '\\';
        static public string FormatPath(string srcFilePath, int systemFlg = Constants.Windows)
        {
            if (StringUtils.IsEmptyOrSpace(srcFilePath)) return string.Empty;
            srcFilePath = srcFilePath.Trim();
            // remove character
            if (srcFilePath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
            {
                srcFilePath = srcFilePath.Replace("\r", "").Replace("\n", "");
            }
            string splitflg2 = GetSplitDirChar(systemFlg).ToString();
            for (int i = 1; i < 2; i++)
            {
                string splitflg1 = GetSplitDirChar(i).ToString();
                if (splitflg2 != splitflg1) StringUtils.Replace(ref srcFilePath, splitflg1, splitflg2);
            }
            return srcFilePath;
        }


        static public string GetRelativePath(string RootPath, string absolutionPath)
        {
            RootPath = FormatPath(RootPath);
            absolutionPath = FormatPath(absolutionPath);
            // get common part of two path.
            string commonPath = StringUtils.SameSubString(RootPath, absolutionPath, true, true);


            if (StringUtils.IsEmptyOrSpace(commonPath))// if not has common part
            {
                // can not build relative path.
                return absolutionPath;
            }
            else if (RootPath.Length == commonPath.Length)
            {
                // same path
                if (absolutionPath.Length == commonPath.Length)
                {
                    return "";
                }
                RootPath = GetDirString(RootPath);
                StringUtils.Replace(ref absolutionPath, RootPath, "", StringUtils.ReplaceEnd, true);
                return absolutionPath;
            }
            else
            {
                //RootPath = GetDirPart(RootPath);
                StringUtils.Replace(ref RootPath, commonPath, "", StringUtils.ReplaceEnd, true);
                StringUtils.Replace(ref absolutionPath, commonPath, "", StringUtils.ReplaceEnd, true);

                //string RelativeRootPath = RootPath.Replace(commonPath, "");
                //string RelativeAbsolutionPath = absolutionPath.Replace(commonPath, "");

                // calulate parent path .
                RootPath = ChangeToParentDir(RootPath);
                return ConDirs(RootPath, absolutionPath);
            }


        }
        static public bool IsDirStringBgn(string text)
        {
            if (text.Length < 1) return false;
            string lst = StringUtils.Substring(text, 0, 1);
            return lst == "\\" || lst == "/";
        }
        private static string ChangeToParentDir(string Path)
        {
            int nParentNumber = GetParentDirNumber(Path);
            string ParentDir = "";
            for (int i = 0; i < nParentNumber; i++)
                ParentDir += "..\\";
            return ParentDir;
        }
        private static int GetParentDirNumber(string Path)
        {
            Path = FormatPath(Path);
            string[] PathArr = StringUtils.Split(Path, "\\");
            return PathArr.Length - 1;
        }

        static public string ConDirs(string root, string dir)
        {
            if (!StringUtils.IsExistVisibleChar(root))
            {
                // 说明：not need to connect. if no root
                // 修改者：张白玉
                // 修改日期：2015-3-5 12:28:39
                root = "";
                return dir;
            }
            if (!StringUtils.IsExistVisibleChar(dir)) dir = "";
            bool IsRootDir = IsDirString(root);
            bool IsDirBgn = IsDirStringBgn(dir);
            if (!IsRootDir && !IsDirBgn) return root.Trim() + "\\" + dir.Trim();
            else if (IsRootDir && IsDirBgn) return StringUtils.Left(root, root.Length - 1).Trim() + "\\" + StringUtils.Mid(dir, 1, dir.Length - 1).Trim();
            else return root + dir;
        }
        static public string GetDirString(string text)
        {
            if (text.Length < 1) return text;
            if (!IsDirString(text))
                text = text + "\\";
            return text;
        }

        /// <summary>
        /// split flg in directory
        /// </summary>
        /// <param name="IsWindows">
        /// if 0 get split dir char form system.
        /// else if 1 get windows flg.
        /// else if 2 get unit flg.
        /// </param>
        /// <returns></returns>
        static public char GetSplitDirChar(int systemFlg = Constants.Windows)
        {
            if (systemFlg == Constants.UnknowSystem) systemFlg = Constants.Windows;// check system...
            return systemFlg == Constants.Windows ? _backslash : _slash;
        }

        /// <summary>
        /// split Path into directory name array.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="systemFlg"></param>
        /// <returns></returns>
        static public string[] SplitPathArr(string Path, int systemFlg = 0)
        {
            return StringUtils.Split(Path, GetSplitDirChar(systemFlg).ToString());
        }
        static public string GetExtPart(string text)
        {
            int lstInt = text.LastIndexOf(".");
            if (lstInt < 0) return "";
            return StringUtils.Substring(text, lstInt, text.Length - lstInt);
        }

        static public string GetFileNamePart(string text)
        {
            string File = GetFilePart(text);
            int lstInt = File.LastIndexOf(".");
            if (lstInt < 0) return File;
            return StringUtils.Substring(File, 0, lstInt);
        }

        static public bool IsDirString(string text)
        {
            if (text.Length < 1) return false;
            string lst = StringUtils.Substring(text, text.Length - 1, 1);
            return lst == "\\" || lst == "/";
        }

        static public string GetSDirString(string text)
        {
            if (text.Length < 1) return text;
            if (IsDirString(text))
                text = StringUtils.Left(text, text.Length - 1);
            return text;
        }
        static public string GetFilePart(string text)
        {
            if (!StringUtils.IsExistVisibleChar(text)) text = "";
            text = text.Trim();
            int lstInt1 = text.LastIndexOf("\\");
            int lstInt2 = text.LastIndexOf("/");
            int lstInt = lstInt1 > lstInt2 ? lstInt1 : lstInt2;
            if (lstInt < 0) return text;
            return StringUtils.Substring(text, lstInt + 1, text.Length - lstInt - 1);
        }

        static public string GetDirPart(string text)
        {
            if (!StringUtils.IsExistVisibleChar(text)) text = "";
            text = GetSDirString(text);
            int lstInt = text.LastIndexOf("\\");
            int lstInt2 = text.LastIndexOf("/");
            if (lstInt < 0 && lstInt2 < 0)
            {
                // root 2017-05-11 17:23:34
                if (RegexUtils.MatchExact(text, @"[a-zA-Z]:"))
                    return text + "\\";
                // 说明：error logic return text.now return empty string.
                // 修改者：张白玉
                // 修改日期：2015-9-1 21:56:05
                //Logs.WriteError("path error: "+text);
                return "";
            }
            lstInt = lstInt > lstInt2 ? lstInt : lstInt2;
            return StringUtils.Substring(text, 0, lstInt + 1);
        }
    }
}
