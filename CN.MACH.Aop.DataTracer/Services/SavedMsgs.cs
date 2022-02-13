using CN.MACH.AI.UnitTest.Core.Utils;
using CN.MACH.Aop.DataTracer.Models;
using CN.MACH.AOP.Fody.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.Aop.DataTracer.Services
{
    class SavedMsgs
    {
        private List<RedisMsgRecord> savedMsgs = new List<RedisMsgRecord>();
        private string savedFilePath = "savedmsgs.json";
        public List<RedisMsgRecord> Load()
        {
            string recordStrs = FileUtils.Read(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, savedFilePath), Encoding.UTF8);
            savedMsgs = JsonUtils.Deserialize<List<RedisMsgRecord>>(recordStrs);
            if (savedMsgs == null) savedMsgs = new List<RedisMsgRecord>();
            return savedMsgs;
        }
        public int InsertUpdate(RedisMsgRecord record)
        {
            if (savedMsgs == null || savedMsgs.Count <= 0)
                Load();
            if (record.Id <= 0)
            {
                record.Id = savedMsgs.Count <= 0 ? 1 : savedMsgs.Select(s => s.Id).Max() + 1;
                savedMsgs.Add(record);
            }
            else
            {
                var oldRecord = savedMsgs.FirstOrDefault(s => s.Id == record.Id);
                oldRecord.Clone(record);
            }
            string recordStrs = JsonUtils.Serialize(savedMsgs);
            return FileUtils.Save(recordStrs, System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, savedFilePath), Encoding.UTF8);
        }
        public int Clear()
        {
            if (savedMsgs != null) savedMsgs.Clear();
            return FileUtils.DeleteFile(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, savedFilePath));
        }

        internal int Remove(RedisMsgRecord record)
        {
            if (record == null || record.Id <= 0) return ErrorCode.NULLPOINTER;
            savedMsgs.RemoveAll(r => r.Id == record.Id);
            string recordStrs = JsonUtils.Serialize(savedMsgs);
            return FileUtils.Save(recordStrs, System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, savedFilePath), Encoding.UTF8);
        }
    }
}
