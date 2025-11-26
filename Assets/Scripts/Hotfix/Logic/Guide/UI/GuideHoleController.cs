using GameConfig;
using HotfixCore.Module;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace HotfixLogic
{
    public struct HoleConfig
    {
        public int Id { get; }
        public Rect HoleData { get; }
        public string NodePath { get; }
        public string EndNodePath { get; }
        public GuideHoleShape HoleShape { get; }

        public HoleConfig(int id, Rect holeData, string nodePath, string endNodePath, GuideHoleShape holeShape)
        {
            Id = id;
            HoleData = holeData;
            NodePath = nodePath;
            HoleShape = holeShape;
            EndNodePath = endNodePath;
        }
    }

    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    public class GuideHoleController : MonoBehaviour, ICanvasRaycastFilter
    {
        [SerializeField] private Rect holeData;

        private string _nodePath;

        // 运行时计算的值
        private Vector2 _rectCenter = Vector2.zero;
        private Vector2 _rectSize = Vector2.zero;
        private float _circleRadius = 0.2f;

        private Material _hollowMaterial;
        private RectTransform _rectTransform;

        // 点击穿透缓存
        private bool _isHoleArea = false;
        private GuideHoleShape _shape = GuideHoleShape.Rectangle;
        private HoleConfig _guideConfig;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            if (_hollowMaterial != null)
            {
                Destroy(_hollowMaterial);
                _hollowMaterial = null;
            }
        }
#if UNITY_EDITOR
        [Button("刷新挖孔")]
#endif
        public void Refresh()
        {
            if (!string.IsNullOrEmpty(_nodePath))
            {
                UpdateHolePositionAndSize(_nodePath);
                UpdateMaterialProperties();
            }
        }

        public void SetUp(HoleConfig config)
        {
            _nodePath = config.NodePath;
            if (string.IsNullOrEmpty(_nodePath))
                return;
            bool isNeedRefreshMaterial = false;
            if (_guideConfig.Id != config.Id)
            {
                isNeedRefreshMaterial = _guideConfig.HoleShape != config.HoleShape;
            }

            holeData = config.HoleData;
            _guideConfig = config;
            _shape = config.HoleShape == GuideHoleShape.Circle ? GuideHoleShape.Circle : GuideHoleShape.Rectangle;
            if (_hollowMaterial == null)
            {
                CreateMaterialInstance();
            }
            else
            {
                if (isNeedRefreshMaterial)
                {
                    DestroyImmediate(_hollowMaterial);
                    _hollowMaterial = null;
                    CreateMaterialInstance();
                }
                else
                {
                    UpdateHolePositionAndSize(_nodePath);
                    UpdateMaterialProperties();
                }
            }
        }

        private void CreateMaterialInstance()
        {
            // 创建材质实例
            string shapeMaterial = "material/guideholerectangle";
            if (_shape == GuideHoleShape.Circle)
                shapeMaterial = "material/guideholecircle";
            G.ResourceModule.LoadAssetAsync<Material>(shapeMaterial, material =>
            {
                if(this.transform == null)
                    return;
                _hollowMaterial = new Material(material);
                var image = GetComponent<Image>();
                image.material = _hollowMaterial;
                UpdateHolePositionAndSize(_nodePath);
                UpdateMaterialProperties();
            }).Forget();
        }

        // 每帧更新挖孔位置和大小
        private void UpdateHolePositionAndSize(string nodePath)
        {
            if (string.IsNullOrEmpty(nodePath)) return;
            // 查找目标节点
            GameObject target = GameObject.Find(nodePath);
            if (target == null) return;
            if (_rectTransform == null)
                _rectTransform = this.GetComponent<RectTransform>();

            Camera uiCamera = G.UIModule.UICamera;
            Rect rect = _rectTransform.rect;
            Vector2 normalizedPos = Vector2.zero;
            Vector2 targetSize = Vector2.zero;

            // 检查目标是否是UI元素
            RectTransform targetRect = target.GetComponent<RectTransform>();
            if (targetRect != null)
            {
                // UI元素处理
                // 获取目标在屏幕空间的中心位置
                Vector3[] targetCorners = new Vector3[4];
                targetRect.GetWorldCorners(targetCorners);

                // 计算UI元素的屏幕中心
                Vector2 screenCenter = uiCamera.WorldToScreenPoint(
                    (targetCorners[0] + targetCorners[2]) * 0.5f
                );

                // 将屏幕位置转换为遮罩UI的本地位置
                Vector2 localPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform,
                    screenCenter,
                    uiCamera,
                    out localPos
                );

                // 转换为归一化坐标 (0-1)
                normalizedPos = new Vector2(
                    (localPos.x - rect.xMin) / rect.width,
                    (localPos.y - rect.yMin) / rect.height
                );

                // 获取UI元素的尺寸（考虑缩放）
                targetSize = new Vector2(
                    targetRect.rect.width * targetRect.localScale.x,
                    targetRect.rect.height * targetRect.localScale.y
                );
            }
            else
            {
                // 2D物体处理 - 统一使用屏幕坐标系统
                Camera mainCamera = G.UIModule.CurrentCamera;

                if (!string.IsNullOrEmpty(_guideConfig.EndNodePath) && nodePath != _guideConfig.EndNodePath)
                {
                    GameObject endNode = GameObject.Find(_guideConfig.EndNodePath);
                    if (endNode != null && endNode.GetComponent<RectTransform>() == null)
                    {
                        // 计算两个世界坐标物体之间的屏幕区域
                        Vector2 center, size;
                        CalculateWorldObjectsScreenRegion(target, endNode, out center, out size);

                        // 转换为中心点和尺寸
                        normalizedPos = new Vector2(
                            (center.x - rect.xMin) / rect.width,
                            (center.y - rect.yMin) / rect.height
                        );

                        targetSize = size;
                    }
                    else
                    {
                        // 只有起始节点或结束节点不是世界坐标物体
                        targetSize = CalculateSingleWorldObjectScreenSize(target, mainCamera, uiCamera);
                        normalizedPos = CalculateSingleWorldObjectNormalizedPosition(target, mainCamera, uiCamera);
                    }
                }
                else
                {
                    // 单个世界坐标物体
                    targetSize = CalculateSingleWorldObjectScreenSize(target, mainCamera, uiCamera);
                    normalizedPos = CalculateSingleWorldObjectNormalizedPosition(target, mainCamera, uiCamera);
                    normalizedPos = new Vector2((normalizedPos.x - rect.xMin) / rect.width, (normalizedPos.y - rect.yMin) / rect.height);
                }
            }

            // 应用位置偏移
            Vector2 offset = new Vector2(holeData.x, holeData.y);
            normalizedPos += new Vector2(
                offset.x / rect.width,
                offset.y / rect.height
            );

            // 应用尺寸偏移
            Vector2 sizeOffset = new Vector2(holeData.width, holeData.height);
            targetSize += sizeOffset;

            // 更新位置和大小
            if (_shape == GuideHoleShape.Rectangle)
            {
                _rectCenter = normalizedPos;
                _rectSize = new Vector2(
                    targetSize.x / rect.width,
                    targetSize.y / rect.height
                );
            }
            else
            {
                _rectCenter = normalizedPos;
                // _rectCenter = new Vector2((normalizedPos.x - rect.xMin) / rect.width, (normalizedPos.y - rect.yMin) / rect.height);
                float maxDimension = Mathf.Max(rect.width, rect.height);
                _circleRadius = targetSize.x / (2 * maxDimension); // 直径转半径
            }
        }

        // 计算单个世界坐标物体的屏幕尺寸
        private Vector2 CalculateSingleWorldObjectScreenSize(GameObject target, Camera mainCamera, Camera uiCamera)
        {
            Bounds bounds = GetObjectBounds(target);
            Vector3[] worldCorners = GetWorldCornersFromBounds(bounds);

            // 转换为屏幕坐标
            Vector2[] screenCorners = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                Vector2 screenPos = mainCamera.WorldToScreenPoint(worldCorners[i]);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, screenPos, uiCamera,
                    out screenCorners[i]);
            }

            // 计算尺寸
            Vector2 min = screenCorners[0];
            Vector2 max = screenCorners[0];
            for (int i = 1; i < 4; i++)
            {
                min = Vector2.Min(min, screenCorners[i]);
                max = Vector2.Max(max, screenCorners[i]);
            }

            return new Vector2(max.x - min.x, max.y - min.y);
        }

        // 计算单个世界坐标物体的归一化位置
        private Vector2 CalculateSingleWorldObjectNormalizedPosition(GameObject target, Camera mainCamera,
            Camera uiCamera)
        {
            Vector3 targetWorldPos = target.transform.position;
            Vector2 screenPos = mainCamera.WorldToScreenPoint(targetWorldPos);
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, screenPos, uiCamera, out localPos);
            return localPos;
        }

        // 计算两个世界坐标物体的屏幕区域（中心点和尺寸）
        private void CalculateWorldObjectsScreenRegion(GameObject startObject, GameObject endObject, out Vector2 center,
            out Vector2 size)
        {
            Camera mainCamera = G.UIModule.CurrentCamera;
            Camera uiCamera = G.UIModule.UICamera;

            // 获取两个物体的世界坐标边界
            Bounds startBounds = GetObjectBounds(startObject);
            Bounds endBounds = GetObjectBounds(endObject);

            // 将世界坐标边界转换为屏幕坐标
            Vector3[] startWorldCorners = GetWorldCornersFromBounds(startBounds);
            Vector3[] endWorldCorners = GetWorldCornersFromBounds(endBounds);

            // 转换为UI局部坐标
            Vector2[] startUICorners = new Vector2[4];
            Vector2[] endUICorners = new Vector2[4];

            for (int i = 0; i < 4; i++)
            {
                Vector2 screenPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform,
                    mainCamera.WorldToScreenPoint(startWorldCorners[i]), uiCamera, out screenPos);
                startUICorners[i] = screenPos;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform,
                    mainCamera.WorldToScreenPoint(endWorldCorners[i]), uiCamera, out screenPos);
                endUICorners[i] = screenPos;
            }

            // 找到最小和最大坐标
            Vector2 minPos = Vector2.Min(startUICorners[0], endUICorners[0]);
            Vector2 maxPos = Vector2.Max(startUICorners[0], endUICorners[0]);

            for (int i = 1; i < 4; i++)
            {
                minPos = Vector2.Min(minPos, Vector2.Min(startUICorners[i], endUICorners[i]));
                maxPos = Vector2.Max(maxPos, Vector2.Max(startUICorners[i], endUICorners[i]));
            }

            // 计算中心点和尺寸
            center = (minPos + maxPos) * 0.5f;
            size = new Vector2(maxPos.x - minPos.x, maxPos.y - minPos.y);
        }

        // 从Bounds获取世界坐标角点
        private Vector3[] GetWorldCornersFromBounds(Bounds bounds)
        {
            Vector3[] corners = new Vector3[4];
            corners[0] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, 0);
            corners[1] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, 0);
            corners[2] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, 0);
            corners[3] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, 0);
            return corners;
        }

        // 获取物体的边界
        private Bounds GetObjectBounds(GameObject obj)
        {
            // 尝试获取2D碰撞器边界
            Collider2D boxCol = obj.GetComponent<Collider2D>();
            if (boxCol != null)
            {
                return boxCol.bounds;
            }

            // 默认边界
            return new Bounds(obj.transform.position, Vector3.one);
        }

        // 更新材质属性
        private void UpdateMaterialProperties()
        {
            if (_hollowMaterial == null) return;
            if (_rectTransform == null)
                _rectTransform = this.GetComponent<RectTransform>();

            // 计算宽高比 (宽度/高度)
            Rect rect = _rectTransform.rect;

            // 设置形状类型和宽高比
            _hollowMaterial.SetFloat("_AspectRatio", 2.0f);

            // 更新材质参数
            if (_shape == GuideHoleShape.Rectangle)
            {
                _hollowMaterial.SetVector("_RectCenter", _rectCenter);
                _hollowMaterial.SetVector("_RectSize", _rectSize);

                // 动态计算归一化圆角半径（基于像素值）
                float desiredCornerRadiusPixels = 40f; // 期望的圆角像素大小
                float normalizedCornerRadius = CalculateNormalizedRadius(
                    desiredCornerRadiusPixels,
                    rect.size
                );
                _hollowMaterial.SetFloat("_CornerRadius", normalizedCornerRadius);
                _hollowMaterial.SetFloat("_RectFeather", 0.0051f);
            }
            else
            {
                _hollowMaterial.SetVector("_CircleCenter", _rectCenter); // 使用同一个中心位置
                _hollowMaterial.SetFloat("_CircleRadius", _circleRadius);
                _hollowMaterial.SetFloat("_CircleFeather", 0.0f);
            }
        }

        private float CalculateNormalizedRadius(float pixelRadius, Vector2 uiSize)
        {
            // 计算UI对角线长度（像素）
            float diagonal = Mathf.Sqrt(uiSize.x * uiSize.x + uiSize.y * uiSize.y);

            // 归一化半径 = 像素半径 / UI对角线长度
            return Mathf.Clamp(pixelRadius / diagonal, 0f, 0.5f);
        }

        // 实现点击穿透接口
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            // 如果没有激活，默认可以点击
            if (!isActiveAndEnabled) return true;
            if (_hollowMaterial == null) return true;

            if (_rectTransform == null)
                _rectTransform = this.GetComponent<RectTransform>();
            // 获取当前材质参数
            Vector2 center = _shape == GuideHoleShape.Rectangle
                ? _hollowMaterial.GetVector("_RectCenter")
                : _hollowMaterial.GetVector("_CircleCenter");

            float aspectRatio = _hollowMaterial.GetFloat("_AspectRatio");
            float radius = _hollowMaterial.GetFloat("_CircleRadius");
            Vector2 size = _hollowMaterial.GetVector("_RectSize");

            // 将屏幕点转换为UI本地点
            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform,
                    screenPoint,
                    eventCamera,
                    out localPoint))
            {
                return true; // 转换失败时默认阻挡
            }

            // 转换为UV坐标 (0-1)
            Rect rect = _rectTransform.rect;
            Vector2 uv = new Vector2(
                (localPoint.x - rect.xMin) / rect.width,
                (localPoint.y - rect.yMin) / rect.height
            );

            // 检查是否在挖孔区域
            bool inHole = false;

            if (_shape == GuideHoleShape.Rectangle)
            {
                Vector2 halfSize = size * 0.5f;
                inHole = uv.x >= center.x - halfSize.x &&
                         uv.x <= center.x + halfSize.x &&
                         uv.y >= center.y - halfSize.y &&
                         uv.y <= center.y + halfSize.y;
            }
            else
            {
                // 使用与Shader相同的宽高比修正
                Vector2 aspectUV = uv;
                aspectUV.y *= aspectRatio;
                Vector2 aspectCenter = center;
                aspectCenter.y *= aspectRatio;

                float dis = Vector2.Distance(aspectUV, aspectCenter);
                inHole = dis <= radius;
            }

            // 在挖孔区域返回false（可穿透），其他区域返回true（阻挡）
            return !inHole;
        }
    }
}