using CN.MACH.AOP.Fody.Models;

namespace CN.MACH.AOP.Fody.Recorders
{
    interface ISrcCodeRecorder
    {
        void Init();
        void Push(SrcCodeRecordModel record);
    }

    class RecorderFactory
    {
        public static ISrcCodeRecorder CreateInterface()
        {
            return new FileLogSrcCodeRecorder();
        }
    }
}