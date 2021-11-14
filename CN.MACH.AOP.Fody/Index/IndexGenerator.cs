using CN.MACH.AOP.Fody.Models;
using CN.MACH.AOP.Fody.Utils;
using DC.ETL.Infrastructure.Cache;
using System;
using System.Collections.Generic;

namespace CN.MACH.AOP.Fody.Index
{
    internal class InvertedIndex
    {

        internal static void Init(Dictionary<string, HashSet<int>> index, string code, int id)
        {
            int doc_id = id;
            string content = code;
            // replace by pangu
            HashSet<string> words = WordSplitUtils.SplitRegex(content);
            //HashSet<string> splitWords = new HashSet<string>();
            //foreach (string word in words)
            //{
            //    HashSet<string> splitWords1 = WordSplitUtils.SplitChars(word);
            //    splitWords.UnionWith(splitWords1);
            //}
            //words.UnionWith(splitWords);
            foreach (string word in words)
            {
                bool exist = index.ContainsKey(word);
                if (!exist)
                {
                    HashSet<int> posting = new HashSet<int>();
                    _ = posting.Add(doc_id);
                    index.Add(word, posting);
                }
                else
                {
                    HashSet<int> posting = index[word];
                    if (!posting.Contains(doc_id))
                    {
                        _ = posting.Add(doc_id);
                        // index.Remove(word);
                        // index.Add(word, posting);
                    }
                }
            }
        }
    }

    public class IndexGenerator : IndexBase
    {
        public IndexGenerator(ICacheProvider cacheProvider) : base(cacheProvider)
        {
        }

        public Action<int, long> ProcessNotice { get; set; }

        public void Build()
        {
            // 获取所有记录
            long cnt = cacheProvider.Count(MgConstants.SrcCodeRecordKey);
            int nID = 0;
            string code;
            // int threadId = 0;
            Dictionary<string, HashSet<int>> indexes = new Dictionary<string, HashSet<int>>();
            do
            {
                string sID = nID++.ToString();
                code = GetRecordCode(sID);
                //if (nID > 10000)
                //    break;
                if (string.IsNullOrEmpty(code))
                    continue;
                //threadId = cacheProvider.Get<int>(MgConstants.SrcCodeThreadidKey, sID);
                InvertedIndex.Init(indexes, code, int.Parse(sID));
                if (nID % 50 == 0)
                    ProcessNotice(nID, cnt);
            } while (nID < cnt);

            // save to cache.
            foreach (KeyValuePair<string, HashSet<int>> index in indexes)
            {
                string word = index.Key;
                cacheProvider.Add(MgConstants.IndexDocListKey, word, index.Value);
            }
        }

    }
}
