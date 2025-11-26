using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameEditor.BuildPipeline
{
    public class BuildEditorWindows : OdinEditorWindow
    {
        [MenuItem("Build/Window")]
        private static void ShowWindow()
        {
            var window = GetWindow<BuildEditorWindows>();
            window.Show();
        }

        protected override void Initialize()
        {
        }

        [AssetsOnly, LabelText("添加构建列表")] public List<BuildSetting> BuildTasks = new List<BuildSetting>();

        [Button("开始构建")]
        public void StartBuild()
        {
            BuildTasks.Sort((a, b) =>
            {
                //按照是否是buildApp 进行排序 buildApp的往后排
                if (a.buildApp && !b.buildApp)
                {
                    return 1;
                }

                if (!a.buildApp && b.buildApp)
                {
                    return -1;
                }
                
                return 0;
            });

            foreach (var task in BuildTasks)
            {
                Debug.Log($"开始执行:{task.name}");
                BuildContext context = new BuildContext(false, task);
                BuildCLI.StartBuild(task.pipeLine, context);
            }
        }
    }
}