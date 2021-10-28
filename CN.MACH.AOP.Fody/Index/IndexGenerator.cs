using CN.MACH.AI.UnitTest.Core.Utils;
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
    class InvertedIndex
    {
        Dictionary<string, HashSet<int>> index;
        public InvertedIndex(Dictionary<string,HashSet<int>> index, string code, int id)
        {
            this.index = index;
            Init(index, code, id);
        }
        void Init(Dictionary<string,HashSet<int>> index, string code, int id)
        {
            int doc_id = id;
            string content = code;
            // replace by pangu
            List<string> words = WordSplitUtils.Split(content);
            foreach (string word in words)
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
                        index.Remove(word);
                        index.Add(word, posting);
                    }
                }
            }
        }
        public Dictionary<string,HashSet<int>> Output()
        {
            return index;
        }
    }

    public class IndexGenerator
    {
        private readonly ICacheProvider cacheProvider;
        public IndexGenerator(ICacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        public void Build()
        {
            // 获取所有记录
            long cnt = cacheProvider.Count(MgConstants.SrcCodeRecordKey);
            int nID = 0;
            string code;
            int threadId;
            Dictionary<string,HashSet<int>> indexes = new Dictionary<string,HashSet<int>>();
            do
            {
                string sID = nID++.ToString();
                code = cacheProvider.Get<string>(MgConstants.SrcCodeRecordKey, sID);
                if (nID > 10000)
                    break;
                if (string.IsNullOrEmpty(code))
                    break;
                threadId = cacheProvider.Get<int>(MgConstants.SrcCodeThreadidKey, sID);
                indexes = BuildCodeIndex(indexes, int.Parse(sID), code, threadId);
            } while (!string.IsNullOrEmpty(code));

            // save to cache.
            foreach (KeyValuePair<string,HashSet<int>> index in indexes)
            {
                string word = index.Key;
                cacheProvider.Add(MgConstants.IndexDocListKey, word, index.Value);
            }
        }

        private Dictionary<string,HashSet<int>> BuildCodeIndex(Dictionary<string,HashSet<int>> indexes, int nID, string code, int threadId)
        {
            InvertedIndex invertedIndex = new InvertedIndex(indexes, code, nID);
            //Dictionary<string,HashSet<int>> newindex = invertedIndex.Output();
            // merge
            //DictionaryUtils.Merge<string,HashSet<int>>(indexes, newindex);
            return indexes;
        }
    }
}
