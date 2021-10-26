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
    class Term : IEquatable<Term>
    {
        public string word;
        public int doc_freq;

        public bool Equals(Term other)
        {
            if(this.word == other.word) return true;
            return false;
        }
        public override int GetHashCode()
        {
            return this.word.GetHashCode();
        }
    }
    class InvertedIndex
    {
        Dictionary<Term, List<int>> index;
        public InvertedIndex(Dictionary<Term, List<int>> index, string code, int id)
        {
            this.index = index;
            Init(index, code, id);
        }
        void Init(Dictionary<Term, List<int>> index, string code, int id)
        {
            int doc_id = id;
            string content = code;
            // replace by pangu
            List<string> words = WordSplitUtils.Split(content);
            foreach (string word in words)
            {
                Term term = index.Keys.FirstOrDefault(m => m.word == word);
                if (term == null)
                {
                    term = new Term();
                    term.word = word;
                    term.doc_freq = 1;
                    List<int> posting = new List<int>();
                    posting.Add(doc_id);
                    index.Add(term, posting);
                }
                else
                {
                    List<int> posting = index[term];
                    if (!posting.Contains(doc_id))
                    {
                        posting.Add(doc_id);
                        index.Remove(term);
                        term.doc_freq++;
                        index.Add(term, posting);
                    }
                }
            }
        }
        public Dictionary<Term, List<int>> Output()
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
            int nID = 0;
            string code;
            int threadId;
            Dictionary<Term, List<int>> indexes = new Dictionary<Term, List<int>>();
            do
            {
                string sID = nID++.ToString();
                code = cacheProvider.Get<string>(MgConstants.SrcCodeRecordKey, sID);
                if (string.IsNullOrEmpty(code))
                    break;
                threadId = cacheProvider.Get<int>(MgConstants.SrcCodeThreadidKey, sID);
                indexes = BuildCodeIndex(indexes, int.Parse(sID), code, threadId);
            } while (!string.IsNullOrEmpty(code));

            // save to cache.
            foreach (KeyValuePair<Term, List<int>> index in indexes)
            {
                string word = index.Key.word;
                cacheProvider.Add(MgConstants.IndexDocListKey, word, index.Value);
            }
        }

        private Dictionary<Term, List<int>> BuildCodeIndex(Dictionary<Term, List<int>> indexes, int nID, string code, int threadId)
        {
            InvertedIndex invertedIndex = new InvertedIndex(indexes, code, nID);
            //Dictionary<Term, List<int>> newindex = invertedIndex.Output();
            // merge
            //DictionaryUtils.Merge<Term, List<int>>(indexes, newindex);
            return indexes;
        }
    }
}
