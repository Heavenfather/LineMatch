using HotfixCore.Module;

namespace Hotfix.EventParameter
{
    public struct EventOneParam<T> : IEventParameter
    {
        private T _arg;

        public T Arg => _arg;

        public static EventOneParam<T> Create(T t1)
        {
            var arg = new EventOneParam<T>();
            arg._arg = t1;
            return arg;
        }
    }

    public struct EventTwoParam<T1, T2> : IEventParameter
    {
        private T1 _arg1;
        private T2 _arg2;

        public T1 Arg1 => _arg1;
        public T2 Arg2 => _arg2;

        public static EventTwoParam<T1, T2> Create(T1 t1, T2 t2)
        {
            var arg = new EventTwoParam<T1, T2>();
            arg._arg1 = t1;
            arg._arg2 = t2;
            return arg;
        }
    }

    public struct EventThreeParam<T1, T2, T3> : IEventParameter
    {
        private T1 _arg1;
        private T2 _arg2;
        private T3 _arg3;

        public T1 Arg1 => _arg1;
        public T2 Arg2 => _arg2;
        public T3 Arg3 => _arg3;

        public static EventThreeParam<T1, T2, T3> Create(T1 t1, T2 t2, T3 t3)
        {
            var arg = new EventThreeParam<T1, T2, T3>();
            arg._arg1 = t1;
            arg._arg2 = t2;
            arg._arg3 = t3;
            return arg;
        }
    }
    
    public struct EventFourParam<T1, T2, T3, T4> : IEventParameter
    {
        private T1 _arg1;
        private T2 _arg2;
        private T3 _arg3;
        private T4 _arg4;
        
        public T1 Arg1 => _arg1;
        public T2 Arg2 => _arg2;
        
        public T3 Arg3 => _arg3;
        
        public T4 Arg4 => _arg4;
        
        public static EventFourParam<T1, T2, T3, T4> Create(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            var arg = new EventFourParam<T1, T2, T3, T4>();
            arg._arg1 = t1;
            arg._arg2 = t2;
            arg._arg3 = t3;
            arg._arg4 = t4;
            return arg;
        }
    }
}