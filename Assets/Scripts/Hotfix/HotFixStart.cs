using System;
using Cysharp.Threading.Tasks;
using GameCore.Resource;
using GameCore.Utils;
using Hotfix.Define;
using Hotfix.Logic;
using HotfixCore.MemoryPool;
using HotfixCore.Module;
using HotfixCore.MVC;
using HotfixCore.Utils;
using UnityEngine;
using Logger = GameCore.Log.Logger;

public class HotFixStart
{
    public static void Entry()
    {
        Logger.Info("Entry Hotfix Start!!!");
        try
        {
            UnityUtil.AddApplicationQuitListener(OnApplicationQuit);
            UnityUtil.AddUpdateListener(Update);
            UnityUtil.AddFixedUpdateListener(FixedUpdate);
            UnityUtil.AddLateUpdateListener(LateUpdate);
            UnityUtil.AddOnApplicationPauseListener(OnApplicationPause);
            UnityUtil.AddOnApplicationFocusListener(OnApplicationFocus);
            // Application.lowMemory += OnLowMemory;
            ResourceModuleDriver.Instance.AddLowMemoryListen(OnLowMemory);
            Logger.Info("Hotfix Start Success!!!");
            LaunchGame();
        }
        catch (Exception e)
        {
            Logger.Error($"hotfix start error:{e.Message}");
        }
    }

    private static void LaunchGame()
    {
        //创建资源管理者
        ResourceHandler.Instance.Initialize();
        //初始化游戏模块映射
        MVCConfig.BuildMVCConfigMap();
        Logger.Info("MVCConfigMap and ResourceHandler Build Success!!!");
        GameLaunch.Instance.Start().Forget();
    }

    private static void OnApplicationPause(bool pause)
    {
        G.EventModule.DispatchEvent(GameEventDefine.OnGameApplicationPause);
    }

    private static void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            G.EventModule.DispatchEvent(GameEventDefine.OnGameApplicationFocus);
        else
            G.EventModule.DispatchEvent(GameEventDefine.OnNotGameApplicationFocus);
    }
    
    private static void OnApplicationQuit()
    {
        G.EventModule.DispatchEvent(GameEventDefine.OnGameApplicationQuit);
    }

    private static void LateUpdate()
    {
    }

    private static void FixedUpdate()
    {
    }
    
    private static void Update()
    {
        Module.Update(TimeUtils.deltaTime, TimeUtils.unscaledDeltaTime);
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.M))
        {
            MVCManager.Instance.ActiveModule(MVCEnum.MatchEditor.ToString(), true).Forget();
        }
#endif
    }

    private static void OnLowMemory()
    {
        Logger.Warning("Low memory reported...");

        MemoryPool.ClearAll();
        
        G.ObjectPoolModule.ReleaseAllUnused();
        
        G.ResourceModule.OnLowMemory();
    }
}