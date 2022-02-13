using CN.MACH.Aop.DataTracer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.Aop.DataTracer.Services
{
    class RedisQueryWords
    {
        public RedisQueryWords()
        {
        }
        private ICollection<string> Includes = new List<string>();
        private ICollection<string> Excludes = new List<string>();
        // 内容
        private ICollection<string> IncludeContents = new List<string>();

        public bool Filter(RedisMsgRecord record)
        {
            bool b = true;
            if (record == null || string.IsNullOrEmpty(record.Name)) return false;
            if (Includes != null && Includes.Count > 0)
            {
                foreach (string include in Includes)
                {
                    if (record.Name.Contains(include))
                        return true;
                    return false;
                }
            }
            if (Excludes != null && Excludes.Count > 0)
            {
                foreach (string exclude in Excludes)
                {
                    if (record.Name.Contains(exclude))
                        return false;
                }
            }
            if (IncludeContents != null && IncludeContents.Count > 0)
            {
                foreach (string includeContent in IncludeContents)
                {
                    if (record.Value.Contains(includeContent))
                        return true;
                    return false;
                }
            }
            return b;
        }

        public static RedisQueryWords Build(string words, string content)
        {
            RedisQueryWords query = new RedisQueryWords();
            if(!string.IsNullOrEmpty(words))
            {
                string[] wordsArr = words.Split(' ');
                foreach (string word in wordsArr)
                {
                    if (word.StartsWith("in:"))
                    {
                        query.Includes.Add(word.Replace("in:", ""));
                    }
                    else
                    {
                        query.Excludes.Add(word);

                    }
                }
            }
            if (!string.IsNullOrEmpty(content))
            {
                string[] wordsArr = content.Split(' ');
                foreach (string word in wordsArr)
                {
                    query.IncludeContents.Add(word);
                }
            }
            return query;
        }
    }

}
