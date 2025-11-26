using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameEditor.Common
{
    public class SceneObjectPicker2D : EditorWindow
    {
        [InitializeOnLoad]
        public static class SceneViewInterceptor
        {
            static SceneViewInterceptor()
            {
                SceneView.duringSceneGui += OnSceneGUI;
            }

            private static void OnSceneGUI(SceneView sceneView)
            {
                Event e = Event.current;

                string currentSceneName = SceneManager.GetActiveScene().name;
                if(currentSceneName.Contains("PuzzleEditorScene"))
                    return;
                // 检测右键点击
                if (e.type == EventType.MouseDown && e.button == 1)
                {
                    Vector2 mousePosition = e.mousePosition;
                    List<GameObject> hitObjects = new List<GameObject>();

                    // 1. 优先检测UI元素
                    DetectUIObjects(mousePosition, hitObjects);

                    // 2. 检测2D物理对象
                    Detect2DObjects(mousePosition, hitObjects);

                    // 3. 显示选择菜单
                    if (hitObjects.Count > 0)
                    {
                        ShowSelectionMenu(hitObjects);
                        e.Use(); // 标记事件已处理
                    }
                }
            }

            // 检测UI对象（包括没有Raycast Target的）
            private static void DetectUIObjects(Vector2 mousePosition, List<GameObject> results)
            {
                // 获取所有Canvas
                Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();

                foreach (Canvas canvas in allCanvases)
                {
                    // 只处理渲染中的Canvas
                    if (!canvas.gameObject.activeInHierarchy || !canvas.enabled)
                        continue;

                    // 获取Canvas下所有RectTransform
                    RectTransform[] rectTransforms = canvas.GetComponentsInChildren<RectTransform>(true);

                    foreach (RectTransform rectTransform in rectTransforms)
                    {
                        // 跳过禁用的对象
                        if (!rectTransform.gameObject.activeSelf) continue;

                        // 转换鼠标位置到局部坐标
                        Vector2 localPoint;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            rectTransform,
                            new Vector2(mousePosition.x,
                                SceneView.lastActiveSceneView.camera.pixelHeight - mousePosition.y),
                            SceneView.lastActiveSceneView.camera,
                            out localPoint);

                        // 检查是否在矩形内
                        if (rectTransform.rect.Contains(localPoint))
                        {
                            results.Add(rectTransform.gameObject);
                        }
                    }
                }
            }

            // 检测2D物理对象
            private static void Detect2DObjects(Vector2 mousePosition, List<GameObject> results)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                RaycastHit2D[] hits2D = Physics2D.RaycastAll(ray.origin, ray.direction);

                foreach (RaycastHit2D hit in hits2D)
                {
                    if (hit.collider != null && !results.Contains(hit.collider.gameObject))
                    {
                        results.Add(hit.collider.gameObject);
                    }
                }
            }

            // 显示选择菜单
            private static void ShowSelectionMenu(List<GameObject> objects)
            {
                GenericMenu menu = new GenericMenu();

                // 添加UI对象（带UI标记）
                foreach (GameObject go in objects)
                {
                    if (go.GetComponent<RectTransform>() != null)
                    {
                        string path = GetHierarchyPath(go);
                        menu.AddItem(new GUIContent($"UI/{path.Split("/")[^1]}"), false, () => SelectObject(go));
                    }
                }

                // 添加分隔线
                if (objects.Count > 0 && objects[0].GetComponent<RectTransform>() != null)
                {
                    menu.AddSeparator("");
                }

                // 添加2D对象
                foreach (GameObject go in objects)
                {
                    if (go.GetComponent<RectTransform>() == null)
                    {
                        string path = GetHierarchyPath(go);
                        menu.AddItem(new GUIContent($"2D/{path}"), false, () => SelectObject(go));
                    }
                }

                // 添加取消选项
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Cancel"), false, null);

                menu.ShowAsContext();
            }

            // 获取层级路径
            private static string GetHierarchyPath(GameObject go)
            {
                string path = go.name;
                Transform parent = go.transform.parent;

                while (parent != null)
                {
                    path = parent.name + "/" + path;
                    parent = parent.parent;
                }

                return path;
            }

            private static void SelectObject(GameObject go)
            {
                Selection.activeGameObject = go;
                SceneView.lastActiveSceneView.FrameSelected();
                EditorGUIUtility.PingObject(go);
            }
        }
    }
}