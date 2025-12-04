using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Hotfix.Define;
using HotfixCore.Module;
using HotfixLogic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hotfix.Logic.GamePlay
{
    public class GamePlaySceneView : ISceneView
    {
        public string SceneLocation => "scenes/gameplayscene";

        public Scene Scene { get; private set; }

        public List<GameObject> SceneAllObjects { get; private set; } = new List<GameObject>();

        public UniTask LoadScene()
        {
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            SceneAllObjects?.Clear();
            G.SceneModule.LoadScene(SceneLocation, LoadSceneMode.Additive,
                callBack: scene =>
                {
                    Scene = scene;
                    scene.GetRootGameObjects(SceneAllObjects);

                    Camera mainCamera = GetSceneRoot("MainCamera").GetComponent<Camera>();
                    G.UIModule.SetSceneCamera(mainCamera);
                    tcs.TrySetResult();
                },
                progressCallBack: (progress) => { CommonLoading.ShowLoading(LoadingEnum.Match, progress * 0.6f); });
            return tcs.Task;
        }

        public void UnloadScene(Action callback = null)
        {
            G.SceneModule.UnloadAsync(SceneLocation, callback);
        }

        public GameObject GetSceneRoot(string nodeName)
        {
            if (SceneAllObjects == null)
                return null;

            for (int i = 0; i < SceneAllObjects.Count; i++)
            {
                if (SceneAllObjects[i].name == nodeName)
                    return SceneAllObjects[i];
            }

            return null;
        }
        
        public Transform GetSceneRootTransform(string nodeName)
        {
            GameObject root = GetSceneRoot(nodeName);
            if (root == null)
                return null;

            return root.transform;
        }

        public Transform GetSceneRootTransform(string rootName, string nodePath)
        {
            GameObject root = GetSceneRoot(rootName);
            if (root == null)
                return null;

            Transform transform = root.transform.Find(nodePath);
            if (transform == null)
                return null;

            return transform;
        }

        public T GetSceneRootComponent<T>(string rootName, string nodePath) where T : Component
        {
            GameObject root = GetSceneRoot(rootName);
            if (root == null)
                return null;
            if (string.IsNullOrEmpty(nodePath))
                return root.GetComponent<T>();

            T component = root.transform.Find(nodePath)?.GetComponent<T>();
            if (component == null)
                return null;

            return component;
        }
    }
}