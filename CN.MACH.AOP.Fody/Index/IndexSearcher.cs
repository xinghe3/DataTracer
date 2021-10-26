using DC.ETL.Infrastructure.Cache;
using DC.ETL.Infrastructure.Cache.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.AOP.Fody.Index
{
    public class RecordInfo
    {
        public int ID { get; set; }
        public int ThreadID { get; set; }
        public string Txt { get; set; }
    }

    public class IndexSearcher
    {
        private readonly ICacheProvider cacheProvider;
        public IndexSearcher(ICacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        /// <summary>
        /// 后期再增加模糊搜索等
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public List<RecordInfo> Search(string keywords)
        {
            List<RecordInfo> records = new List<RecordInfo>();
            List<int> IDs = cacheProvider.Get<List<int>>(MgConstants.IndexDocListKey, keywords);
            if (IDs == null)
                return records;
            foreach (int ID in IDs)
            {
                int threadId = cacheProvider.Get<int>(MgConstants.SrcCodeThreadidKey, ID.ToString());
                string code = cacheProvider.Get<string>(MgConstants.SrcCodeRecordKey, ID.ToString());
                records.Add(new RecordInfo()
                {
                    ID = ID, ThreadID = threadId, Txt = code
                });
            }
            return records;
        }
    }
}
