using System.Collections.Generic;
using GameConfig;
using HotfixCore.Extensions;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    public class BoardRenderSystem : IEcsInitSystem,IEcsDestroySystem
    {
        private GameStateContext _context;
        private IBoard _board;
        private Transform _gridRoot;
        private Transform _holeRoot;
        private BoardViewConfig _viewConfig;
        private EcsFilter _filter;
        private EcsPool<GridCellComponent> _cellPool;

        private List<GameObject> _holeLines = new List<GameObject>();

        public void Init(IEcsSystems systems)
        {
            _holeLines.Clear();
            
            _context = systems.GetShared<GameStateContext>();
            _board = _context.Board;
            _gridRoot = _context.SceneView.GetSceneRootTransform("MatchCanvas", "GridBoard/Grid");
            _holeRoot = _context.SceneView.GetSceneRootTransform("MatchCanvas", "GridBoard/Hole");
            
            _viewConfig = _context.SceneView.GetSceneRootComponent<BoardViewConfig>("MatchCanvas","GridBoard");
            _filter = systems.GetWorld().Filter<BoardTag>().Include<GridCellComponent>().End();
            _cellPool = systems.GetWorld().GetPool<GridCellComponent>();

            if (_viewConfig == null)
            {
                Debug.LogError("BoardRenderSystem: BoardViewConfig is missing!");
                return;
            }

            RenderBoardTilesAndLines();
        }

        private void RenderBoardTilesAndLines()
        {
            // 获取所有镂空格子的坐标，用于后续画线判断
            HashSet<Vector2Int> holeCoords = new HashSet<Vector2Int>();
            foreach (var entity in _filter)
            {
                ref GridCellComponent cellComponent = ref _cellPool.Get(entity);
                if (cellComponent.IsBlank)
                {
                    //空白格
                    var grid = CreateViewObject(_viewConfig.HolePrefab, cellComponent.WorldPosition,
                        $"Hole_{cellComponent.Position.x}-{cellComponent.Position.y}", _holeRoot);
                    holeCoords.Add(cellComponent.Position);
                    _board.RegisterViewInstance(cellComponent.Position.x, cellComponent.Position.y, grid);
                }
                else
                {
                    //非空白格
                    var grid = CreateViewObject(_viewConfig.GridCellPrefab, cellComponent.WorldPosition,
                        $"{cellComponent.Position.x}-{cellComponent.Position.y}", _gridRoot);
                    var link = grid.GetOrAddComponent<EntityLink>();
                    link.Link(entity, _context.World);
                    
                    BoxCollider2D collider2D = grid.GetOrAddComponent<BoxCollider2D>();
                    collider2D.size = MatchElementUtil.GridSize;
                    collider2D.offset = Vector2.zero;
                    grid.transform.Find("Icon").GetComponent<SpriteRenderer>().size =
                        new Vector2(MatchElementUtil.GridSize.x, MatchElementUtil.GridSize.y);
                    _board.RegisterViewInstance(cellComponent.Position.x, cellComponent.Position.y, grid);
                }
            }

            // 处理边缘线
            foreach (var holePos in holeCoords)
            {
                DrawLinesForHole(holePos, holeCoords);
            }
        }

        /// <summary>
        /// 迁移 GridBoardBackground.DrawLine 的逻辑
        /// </summary>
        private void DrawLinesForHole(Vector2Int holePos, HashSet<Vector2Int> allHoles)
        {
            Vector3 position = MatchPosUtil.CalculateWorldPosition(holePos.x, holePos.y, 1, 1, ElementDirection.None);

            // 获取8方向邻居状态
            var neighborPos = MatchPosUtil.GetEightNeighborPos(holePos);

            bool isUp = allHoles.Contains(neighborPos[0]);
            bool isDown = allHoles.Contains(neighborPos[1]);
            bool isLeft = allHoles.Contains(neighborPos[2]);
            bool isRight = allHoles.Contains(neighborPos[3]);
            bool isUpRight = allHoles.Contains(neighborPos[4]);
            bool isDownRight = allHoles.Contains(neighborPos[5]);
            bool isUpLeft = allHoles.Contains(neighborPos[6]);
            bool isDownLeft = allHoles.Contains(neighborPos[7]);

            // --- 内角 (Inside Corners) ---
            if (!isUp && !isLeft && !isUpLeft)
                CreateLine(MatchLineType.InsideUpLeft, position, new Vector3(-0.2f, -0.2f, 0));

            if (!isUp && !isRight && !isUpRight)
                CreateLine(MatchLineType.InsideUpRight, position, new Vector3(0.2f, -0.2f, 0));

            if (!isDown && !isLeft && !isDownLeft)
                CreateLine(MatchLineType.InsideDownLeft, position, new Vector3(-0.2f, 0.2f, 0));

            if (!isDown && !isRight && !isDownRight)
                CreateLine(MatchLineType.InsideDownRight, position, new Vector3(0.2f, 0.2f, 0));

            bool upLeftAngle = !isUp && !isLeft && !isUpLeft;
            bool upRightAngle = !isUp && !isRight && !isUpRight;
            bool downLeftAngle = !isDown && !isLeft && !isDownLeft;
            bool downRightAngle = !isDown && !isRight && !isDownRight;

            if (!upLeftAngle && !isUpLeft && isLeft && isUp)
                CreateLine(MatchLineType.OutsideUpLeft, position, new Vector3(-0.4f, -0.4f, 0));

            if (!upRightAngle && !isUpRight && isRight && isUp)
                CreateLine(MatchLineType.OutsideUpRight, position, new Vector3(0.4f, -0.4f, 0));

            if (!downLeftAngle && !isDownLeft && isLeft && isDown)
                CreateLine(MatchLineType.OutsideDownLeft, position, new Vector3(-0.4f, 0.4f, 0));

            if (!downRightAngle && !isDownRight && isRight && isDown)
                CreateLine(MatchLineType.OutsideDownRight, position, new Vector3(0.4f, 0.4f, 0));

            // --- 直线 (Straight Lines) ---
            // 上边线
            if (!isUp && (!upLeftAngle || !upRightAngle))
            {
                CreateResizableLine(MatchLineType.LineUp, position, new Vector3(0, -0.2f, 0),
                    upLeftAngle, upRightAngle, true);
            }

            // 左边线
            if (!isLeft && (!upLeftAngle || !downLeftAngle))
            {
                CreateResizableLine(MatchLineType.LineLeft, position, new Vector3(-0.2f, 0, 0),
                    upLeftAngle, downLeftAngle, false);
            }

            // 右边线
            if (!isRight && (!upRightAngle || !downRightAngle))
            {
                CreateResizableLine(MatchLineType.LineRight, position, new Vector3(0.2f, 0, 0),
                    upRightAngle, downRightAngle, false);
            }

            // 下边线
            if (!isDown && (!downLeftAngle || !downRightAngle))
            {
                CreateResizableLine(MatchLineType.LineDown, position, new Vector3(0, 0.2f, 0),
                    downLeftAngle, downRightAngle, true);
            }
        }

        private void CreateLine(MatchLineType type, Vector3 basePos, Vector3 offset)
        {
            var prefab = _viewConfig.GetLinePrefab(type);
            if (prefab)
            {
                var go = Object.Instantiate(prefab, _holeRoot);
                go.SetVisible(true);
                go.transform.position = basePos + offset;
                _holeLines.Add(go);
            }
        }

        // 处理需要缩放的直线 (根据 GridBoardBackground 的逻辑，线有时候是 0.8f 长，有时候是 0.4f)
        private void CreateResizableLine(MatchLineType type, Vector3 basePos, Vector3 baseOffset,
            bool isSide1Open, bool isSide2Open, bool isHorizontal)
        {
            var prefab = _viewConfig.GetLinePrefab(type);
            if (!prefab) return;

            var go = Object.Instantiate(prefab, _holeRoot);
            go.SetVisible(true);
            _holeLines.Add(go);
            var sp = go.GetComponent<SpriteRenderer>();

            Vector2 size = Vector2.zero;
            Vector3 finalPos = basePos + baseOffset;

            // 逻辑复刻自 GridBoardBackground
            if (!isSide1Open && !isSide2Open)
            {
                size = new Vector2(0.8f, 0.4f);
            }
            else
            {
                size = new Vector2(0.4f, 0.4f);
                float offsetVal = 0.2f;
                if (isHorizontal) // 上下线，调整X
                {
                    if (!isSide1Open) finalPos.x -= offsetVal;
                    else if (!isSide2Open) finalPos.x += offsetVal;
                }
                else // 左右线，调整Y
                {
                    if (!isSide1Open) finalPos.y -= offsetVal;
                    else if (!isSide2Open) finalPos.y += offsetVal;
                }
            }

            if (sp) sp.size = size;
            go.transform.position = finalPos;
        }

        private GameObject CreateViewObject(GameObject prefab, Vector3 pos, string name,Transform parent)
        {
            if (prefab == null) return null;
            var go = Object.Instantiate(prefab, parent);
            go.transform.position = pos;
            go.name = name;
            go.SetVisible(true);
            return go;
        }

        public void Destroy(IEcsSystems systems)
        {
            if (_holeLines.Count > 0)
            {
                for (int i = _holeLines.Count - 1; i >= 0; i--)
                {
                    UnityEngine.Object.Destroy(_holeLines[i]);
                }
            }
            _holeLines.Clear();
        }
    }
}