using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CN.MACH.AI.UnitTest.Core.Utils
{
    public class DictionaryUtils
    {
        /// <summary>
        /// add one item to dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="ht"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        static public int Add<T, V>(ref Dictionary<T, V> ht, T key, V value)
        {
            if (ht == null) ht = new Dictionary<T, V>();
            if (key != null)
            {
                if (!ht.ContainsKey(key))
                {
                    ht.Add(key, value);
                }
                else
                    return ErrorCode.KeyHasExists;
            }
            return ErrorCode.Success;
        }
        static public bool IsNullOrEmpty<T, V>(IDictionary<T, V> ht)
        {
            if (ht == null) return true;
            if (ht.Count == 0) return true;
            return false;
        }
        /// <summary>
        /// check if has key in dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="ht"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        static public bool ExistKey<T, V>(IDictionary<T, V> ht, T key)
        {
            if (IsNullOrEmpty(ht)) return false;
            return ht.ContainsKey(key);
        }
        /// <summary>
        /// get value of IDictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="ht"></param>
        /// <param name="key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        static public V GetValue<T, V>(IDictionary<T, V> ht, T key, V Default = default(V))
        {
            V value = Default;
            if (ht == null || ht.Count == 0 || key == null) return value;
            if (!ht.ContainsKey(key)) return value;
            try
            {
                value = ht[key];
                if (value == null) value = Default;
            }
            catch (Exception ex)
            {
                Logs.WriteExLog(ex);
            }
            return value;
        }

        public static Dictionary<T, V> Merge<T, V>(Dictionary<T, V> first, Dictionary<T, V> second)
        {
            if (first == null) first = new Dictionary<T, V>();
            if (second == null) return first;

            foreach (var item in second)
            {
                if (!first.ContainsKey(item.Key))
                    first.Add(item.Key, item.Value);
            }

            return first;
        }
    }
}
