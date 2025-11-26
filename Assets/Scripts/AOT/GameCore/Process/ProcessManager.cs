using System;
using System.Collections.Generic;
using System.Reflection;
using GameCore.Log;

namespace GameCore.Process
{
    public sealed class ProcessManager
    {
        private static ProcessManager _instance;

        public static ProcessManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ProcessManager();
                    _instance.OnInitialized();
                }

                return _instance;
            }
        }

        private Dictionary<string, IProcess> _processesMap = new Dictionary<string, IProcess>();
        private IProcess _currentProcess;
        
        private void OnInitialized()
        {
            Logger.Info("ProcessManager OnInitialized()");
            Assembly assembly = null;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a.GetName().Name == "GameCore")
                {
                    assembly = a;
                    break;
                }
            }

            if (assembly != null)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetInterface("IProcess") != null)
                    {
                        var instance = (IProcess)Activator.CreateInstance(type);
                        instance.Init();
                        _processesMap.Add(instance.GetType().FullName, instance);
                    }
                }
            }
            else
            {
                Logger.Error("无法获取程序集:GameCore,流程初始化失败!");
            }
        }

        public void StartUp()
        {
            Enter<ProcessLaunch>();
        }

        public void Enter<T>() where T : IProcess
        {
            string key = typeof(T).FullName;
            if (!_processesMap.ContainsKey(key))
            {
                Logger.Error($"Enter process fail,can't found process {typeof(T).FullName}");
                return;
            }

            if (_currentProcess != null)
            {
                Logger.Debug($"Change process {_currentProcess.GetType().FullName} ==> {key}");
            }
            _currentProcess?.Leave();
            _currentProcess = _processesMap[key];
            _currentProcess.Enter();
        }

        public void StopProcess()
        {
            if (_currentProcess != null)
            {
                _currentProcess.Leave();
                _currentProcess = null;
                Logger.Info("AOT Process Stop!!");
            }
        }

        public void OnUpdate()
        {
            if (_currentProcess != null)
            {
                _currentProcess.Update();
            }
        }
    }
}