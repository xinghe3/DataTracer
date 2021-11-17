using CN.MACH.AI.UnitTest.Core.Utils;
using CN.MACH.AOP.Fody.Models;
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
    public class RecordDetailInfo : SrcCodeRecordModel
    {
        public int ID { get; set; }
        // public int ThreadID { get; set; }
    }

    public class IndexSearcher : IndexBase
    {
        public IndexSearcher(ICacheProvider cacheProvider) : base(cacheProvider)
        {
        }

        /// <summary>
        /// 后期再增加模糊搜索等
        /// 目前根据需要仅增加了and
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public List<RecordInfo> Search(string keywords)
        {
            List<RecordInfo> records = new List<RecordInfo>();

            
            string[] words = StringUtils.Split(keywords, " ");

            HashSet<int> IDs = new HashSet<int>();
            foreach (string word in words)
            {
                // and
                var ids1 = cacheProvider.Get<HashSet<int>>(MgConstants.IndexDocListKey, word);
                if (ids1 == null)
                    continue;
                if (IDs.Count > 0)
                    IDs.IntersectWith(ids1);
                else
                    IDs = ids1;
            }
            if (IDs.Count <= 0)
                return records;
            foreach (int ID in IDs)
            {
                int threadId = cacheProvider.Get<int>(MgConstants.SrcCodeThreadidKey, ID.ToString());
                string code = GetRecordCode(ID.ToString());

                records.Add(new RecordInfo()
                {
                    ID = ID, ThreadID = threadId, Txt = code
                });
            }
            return records;
        }

        public List<RecordDetailInfo> Search(int startIndex, int endIndex)
        {
            List<RecordDetailInfo> records = new List<RecordDetailInfo>();
            for (int ID = startIndex; ID <= endIndex; ID++)
            {
                int threadId = cacheProvider.Get<int>(MgConstants.SrcCodeThreadidKey, ID.ToString());
                SrcCodeRecordModel record = cacheProvider.Get<SrcCodeRecordModel>(MgConstants.SrcCodeRecordKey, ID.ToString());
                RecordDetailInfo recordDetailInfo = new RecordDetailInfo();
                recordDetailInfo.InstanceName = record.InstanceName;
                recordDetailInfo.PropertyName = record.PropertyName;
                recordDetailInfo.MethodName = record.MethodName;
                recordDetailInfo.Params = record.Params;
                recordDetailInfo.ID = ID;
                recordDetailInfo.ThreadID = threadId;
                records.Add(recordDetailInfo);
            }
            return records;

        }
    }
}
