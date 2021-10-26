using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CN.MACH.AI.UnitTest.Core.Utils
{
    public class CollectionUtils
    {
        /// <summary>
        /// get string value in list.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="n">from 0</param>
        /// <param name="Default"></param>
        /// <returns></returns>
        static public string GetString(IList<string> list, int n, string Default = "")
        {
            string value = Default;
            try
            {
                if (list != null && list.Count > n)
                    value = list[n];
            }
            catch
            {
            }
            return value;
        }
        /// <summary>
        /// check if list has data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        static public bool IsNullOrEmpty<T>(ICollection<T> list)
        {
            if (list == null) return true;
            if (list.Count == 0) return true;
            return false;
        }
        static public T Pop<T>(IList<T> list, T Default = default(T))
        {
            T res = Get<T>(list, 0, Default);
            Remove<T>(ref list, 0);
            return res;
        }
        public static int Remove<T>(ref IList<T> list, int n)
        {
            if (IsNullOrEmpty(list))
                return ErrorCode.ErrParams;
            if (n < 0 || n > list.Count - 1)
                return ErrorCode.ErrParams;
            list.RemoveAt(n);
            return ErrorCode.Success;
        }

        /// <summary>
        /// get value from list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="n"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        static public T Get<T>(IList<T> list, int n, T Default)
        {
            T value = Default;
            if (IsNullOrEmpty<T>(list)) return value;
            if (list.Count > n && n >= 0)
                try
                {
                    value = list[n];
                    if (value == null) value = Default;
                }
                catch
                {
                }
            return value;
        }
    }
}
