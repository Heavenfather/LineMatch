using System.Collections.Generic;
using System.IO;
using HotfixLogic.Match;
using UnityEditor;
using UnityEngine;

namespace GameEditor.AssetImport
{
    public class MatchLevelImporter : AssetPostprocessor
    {
        // 目标文件夹路径
        private static readonly string TargetFolder = "assets/artload/match/";

        private static readonly string SaveLevelConfig = "Assets/ArtLoad/Config/LevelDifficulty.json";

        private static LevelDifficulty levelDifficulty;
        
        [MenuItem("Tools/消除/重新生成关卡难度配置")]
        public static void ReloadLevelDifficulty()
        {
            HashSet<string> collector = new HashSet<string>();
            // 收集TargetFolder内的JSON文件
            string[] files = Directory.GetFiles(TargetFolder, "*.json", SearchOption.AllDirectories);
            string path = "";
            for (int i = 0; i < files.Length; i++)
            {
                path = files[i].ToLower();
                if(path.Contains("_coin"))
                    continue;
                if(path.Contains("_guide"))
                    continue;
                collector.Add(files[i].Replace("\\", "/"));
            }
            PrepareConfig();
            if (levelDifficulty != null)
            {
                levelDifficulty.Clear();
            }

            ProcessChangedJsons(collector);
            Debug.Log("重新生成关卡难度配置成功");
        }
        
        // 主处理入口
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedToAssets,
            string[] movedFromAssets)
        {
            // 收集所有变更的JSON文件路径
            var changedJsonPaths = new HashSet<string>();

            CollectChangedAssets(importedAssets, changedJsonPaths);
            CollectChangedAssets(deletedAssets, changedJsonPaths);
            CollectChangedAssets(movedToAssets, changedJsonPaths);
            CollectChangedAssets(movedFromAssets, changedJsonPaths);

            // 实际处理变更文件
            if (changedJsonPaths.Count > 0)
            {
                // 延迟调用确保所有资源导入完成
                EditorApplication.delayCall += () => ProcessChangedJsons(changedJsonPaths);
            }
        }

        // 收集目标文件夹内的JSON文件
        private static void CollectChangedAssets(string[] assets, HashSet<string> collector)
        {
            if (assets == null) return;

            foreach (var asset in assets)
            {
                // 转换为小写保证跨平台一致性
                string lowerPath = asset.ToLower();

                if (Path.GetExtension(lowerPath) != ".json") continue;
                //排除金币关
                if (lowerPath.Contains("_coin")) continue;

                if (lowerPath.StartsWith(TargetFolder))
                {
                    collector.Add(asset);
                }
            }
        }

        // 处理变更的JSON文件
        private static void ProcessChangedJsons(HashSet<string> jsonPaths)
        {
            foreach (var jsonPath in jsonPaths)
            {
                try
                {
                    if(!File.Exists(jsonPath))
                        continue;
                    string jsonContent = File.ReadAllText(jsonPath);

                    var data = JsonUtility.FromJson<LevelData>(jsonContent);
                    PrepareConfig();
                    if (IsLevelA(jsonPath))
                    {
                        int index = levelDifficulty.levelA.FindIndex(x => x.levelId == data.id);
                        if (index >= 0)
                        {
                            levelDifficulty.levelA[index].difficulty = data.difficulty;
                        }
                        else
                        {
                            levelDifficulty.levelA.Add(new DifficultyData()
                            {
                                levelId = data.id,
                                difficulty = data.difficulty
                            });
                        }
                    }
                    else if (IsLevelC(jsonPath))
                    {
                        int index = levelDifficulty.levelC.FindIndex(x => x.levelId == data.id);
                        if (index >= 0)
                        {
                            levelDifficulty.levelC[index].difficulty = data.difficulty;
                        }
                        else
                        {
                            levelDifficulty.levelC.Add(new DifficultyData()
                            {
                                levelId = data.id,
                                difficulty = data.difficulty
                            });
                        }
                    }
                    else
                    {
                        int index = levelDifficulty.levelB.FindIndex(x => x.levelId == data.id);
                        if (index >= 0)
                        {
                            levelDifficulty.levelB[index].difficulty = data.difficulty;
                        }
                        else
                        {
                            levelDifficulty.levelB.Add(new DifficultyData()
                            {
                                levelId = data.id,
                                difficulty = data.difficulty
                            });
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"JSON处理失败: {jsonPath}\n{e}");
                }
            }

            SaveConfig();
        }

        private static void PrepareConfig()
        {
            if (levelDifficulty == null)
            {
                TextAsset asset =
                    AssetDatabase.LoadAssetAtPath<TextAsset>(SaveLevelConfig);
                if (asset != null && !string.IsNullOrEmpty(asset.text))
                    levelDifficulty = JsonUtility.FromJson<LevelDifficulty>(asset.text);
                else
                    levelDifficulty = new LevelDifficulty();
            }
        }

        private static void SaveConfig()
        {
            if(levelDifficulty == null)
                return;
            levelDifficulty.levelA.Sort((a, b) => a.levelId.CompareTo(b.levelId));
            levelDifficulty.levelB.Sort((a, b) => a.levelId.CompareTo(b.levelId));
            levelDifficulty.levelC.Sort((a, b) => a.levelId.CompareTo(b.levelId));
            
            string json = JsonUtility.ToJson(levelDifficulty, false);
            string dir = Path.GetDirectoryName(SaveLevelConfig);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(SaveLevelConfig, json);
            AssetDatabase.Refresh();
        }

        private static bool IsLevelA(string path)
        {
            return path.ToLower().Contains("assets/artload/match/levela/");
        }

        private static bool IsLevelC(string path)
        {
            return path.ToLower().Contains("assets/artload/match/levelc/");
        }
    }
}