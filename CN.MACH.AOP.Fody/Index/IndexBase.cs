using CN.MACH.AOP.Fody.Models;
using DC.ETL.Infrastructure.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.AOP.Fody.Index
{
    public class IndexBase
    {
        protected readonly ICacheProvider cacheProvider;
        public IndexBase(ICacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }
        protected string GetRecordCode(string sID)
        {
            SrcCodeRecordModel record = cacheProvider.Get<SrcCodeRecordModel>(MgConstants.SrcCodeRecordKey, sID);
            if (record == null) return null;
            StringBuilder sb = new StringBuilder();
            sb.Append(record.InstanceName);
            sb.Append(".");
            if (!string.IsNullOrEmpty(record.PropertyName))
            {

                sb.Append(record.PropertyName);
                sb.Append("=");
                int index = 0;
                foreach (var param in record.Params)
                {
                    if (index > 0) sb.Append(",");
                    sb.Append(param.Value);
                    index++;
                }
            }
            else
            {

                sb.Append(record.MethodName);
                sb.Append("(");
                int index = 0;
                foreach (var param in record.Params)
                {
                    if (index > 0) sb.Append(",");
                    sb.Append(param.Value);
                    index++;
                }
                sb.Append(");");

            }
            return sb.ToString();
        }
    }
}
