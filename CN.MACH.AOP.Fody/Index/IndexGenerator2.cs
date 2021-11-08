using CN.MACH.AI.UnitTest.Core.Utils;
using CN.MACH.AOP.Fody.Comparers;
using CN.MACH.AOP.Fody.Utils;
using DC.ETL.Infrastructure.Cache;
using DC.ETL.Infrastructure.Cache.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.AOP.Fody.Index
{
    class InvertedIndex2
    {

        internal static void Init(Dictionary<byte[], HashSet<int>> index, string code, int id)
        {
            int doc_id = id;
            string content = code;
            // replace by pangu
            HashSet<byte[]> words = WordSplitUtils.SplitRegexBytes(content);
            //HashSet<string> splitWords = new HashSet<string>();
            //foreach (string word in words)
            //{
            //    HashSet<string> splitWords1 = WordSplitUtils.SplitChars(word);
            //    splitWords.UnionWith(splitWords1);
            //}
            //words.UnionWith(splitWords);
            foreach (byte[] word in words)
            {
                bool exist = index.ContainsKey(word);
                if (!exist)
                {
                    HashSet<int> posting = new HashSet<int>();
                    posting.Add(doc_id);
                    index.Add(word, posting);
                }
                else
                {
                   HashSet<int> posting = index[word];
                    if (!posting.Contains(doc_id))
                    {
                        posting.Add(doc_id);
                        // index.Remove(word);
                        // index.Add(word, posting);
                    }
                }
            }
        }
    }

    public class IndexGenerator2
    {
        private readonly ICacheProvider cacheProvider;
        public IndexGenerator2(ICacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        public void Build()
        {
            // 获取所有记录
            long cnt = cacheProvider.Count(MgConstants.SrcCodeRecordKey);
            int nID = 0;
            string code;
            int threadId = 0;
            Dictionary<byte[], HashSet<int>> indexes = new Dictionary<byte[], HashSet<int>>(new ByteArrayComparer());
            do
            {
                string sID = nID++.ToString();
                code = cacheProvider.Get<string>(MgConstants.SrcCodeRecordKey, sID);
                //if (nID > 10000)
                //    break;
                if (string.IsNullOrEmpty(code))
                    break;
                //threadId = cacheProvider.Get<int>(MgConstants.SrcCodeThreadidKey, sID);
                InvertedIndex2.Init(indexes, code, int.Parse(sID));
            } while (!string.IsNullOrEmpty(code));

            // save to cache.
            //foreach (KeyValuePair<byte[], HashSet<int>> index in indexes)
            //{
            //    byte[] word = index.Key;
            //    cacheProvider.Add(MgConstants.IndexDocListKey,Encoding.UTF8.GetString(word), index.Value);
            //}
        }
    }
}
