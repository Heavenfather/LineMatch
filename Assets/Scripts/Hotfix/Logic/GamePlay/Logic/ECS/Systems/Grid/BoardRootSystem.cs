using GameConfig;
using HotfixCore.Adapter;
using HotfixCore.Module;
using HotfixLogic.Match;
using UnityEngine;

namespace Hotfix.Logic.GamePlay
{
    /// <summary>
    /// 对棋盘的根节点的一些操作,它比棋盘初始化更早
    /// 如背景图、摄像机等等
    /// </summary>
    public class BoardRootSystem : IEcsInitSystem, IEcsRunSystem
    {
        private GameStateContext _context;
        private EcsWorld _world;
        private EcsFilter _bgFilter;
        private EcsPool<BoardBgComponent> _bgPool;
        private LevelData _levelData;

        private Camera _mainCamera;
        private Camera _effectCamera;
        private Camera _guideCamera;
        private Background2DScaler _boardBgInstance;
        private SpriteRenderer _boardBgSp;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _context = systems.GetShared<GameStateContext>();
            _levelData = _context.CurrentLevel;
            _boardBgInstance = _context.SceneView.GetSceneRootComponent<Background2DScaler>("MatchCanvas", "BoardBg");
            _boardBgSp = _boardBgInstance.GetComponent<SpriteRenderer>();
            _mainCamera = _context.SceneView.GetSceneRootComponent<Camera>("MainCamera", "");
            _effectCamera = _context.SceneView.GetSceneRootComponent<Camera>("MainCamera", "EffectCamera");
            _guideCamera = _context.SceneView.GetSceneRootComponent<Camera>("GuideCamera", "");
            
            _bgPool = _world.GetPool<BoardBgComponent>();
            
            CreateEntities();
            
            CenterCamera();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _bgFilter)
            {
                ref var bgComponent = ref _bgPool.Get(entity);
                if (bgComponent.IsBoardBgDirty)
                {
                    if (_boardBgSp != null)
                    {
                        _boardBgSp.color = bgComponent.BoardBgColor;
                    }
                    bgComponent.IsBoardBgDirty = false;
                }
            }
        }

        private void CreateEntities()
        {
            // 背景图实体   遵循所有看得见的对象都是实体
            int bgEntity = _world.NewEntity();
            ref BoardBgComponent bgComponent = ref _bgPool.Add(bgEntity);
            bgComponent.IsBoardBgDirty = false;
            ColorUtility.TryParseHtmlString(MatchElementUtil.MatchBgColor, out bgComponent.BoardBgColor);
            _boardBgSp.color = bgComponent.BoardBgColor;
            
            _bgFilter = _world.Filter<BoardBgComponent>().End();
        }

        private void CenterCamera()
        {
            //其实就是将GridSystem里面的逻辑复制过来，后面需要将引导那些都拆分成另外的系统进行处理
            
            const float boardPadding = 0.5f;

            int minX = -1;
            for (int x = 0; x < _levelData.gridCol; x++)
            {
                for (int y = 0; y < _levelData.gridRow; y++)
                {
                    if (!_levelData.grid[x][y].isWhite)
                    {
                        minX = x;
                        break;
                    }
                }

                if (minX >= 0) break;
            }

            int minY = -1;
            for (int y = 0; y < _levelData.gridRow; y++)
            {
                for (int x = 0; x < _levelData.gridCol; x++)
                {
                    if (!_levelData.grid[x][y].isWhite)
                    {
                        minY = y;
                        break;
                    }
                }

                if (minY >= 0) break;
            }


            int maxX = -1;
            for (int x = _levelData.gridCol - 1; x >= 0; x--)
            {
                for (int y = _levelData.gridRow - 1; y >= 0; y--)
                {
                    if (!_levelData.grid[x][y].isWhite)
                    {
                        maxX = x;
                        break;
                    }
                }

                if (maxX >= 0) break;
            }

            int maxY = -1;
            for (int y = _levelData.gridRow - 1; y >= 0; y--)
            {
                for (int x = _levelData.gridCol - 1; x >= 0; x--)
                {
                    if (!_levelData.grid[x][y].isWhite)
                    {
                        maxY = y;
                        break;
                    }
                }

                if (maxY >= 0) break;
            }

            Vector2Int startPos = new Vector2Int(minX, minY);
            Vector2Int endPos = new Vector2Int(maxX, maxY);

            Vector2 centerCoord = new Vector2((startPos.x + endPos.x) / 2.0f, (startPos.y + endPos.y) / 2.0f);
            Vector3 centerPosition = MatchPosUtil.CalculateWorldPosition(centerCoord.x, centerCoord.y, 1, 1, ElementDirection.None);
            SetCameraPos(new Vector3(centerPosition.x, centerPosition.y + 0.8f, _mainCamera.transform.position.z));

            var gridSize = MatchElementUtil.GridSize;
            if (maxX + 1 >= 8 && !MatchManager.Instance.IsEnterByEditor())
            {
                float screenAspect = _mainCamera.aspect;
                float physicalWidth = (endPos.x - startPos.x + 1) * gridSize.x;
                float physicalHeight = (endPos.y - startPos.y + 1) * gridSize.y;
                float boardAspect = physicalWidth / physicalHeight;
                float orthographicSize = 0.0f;
                // 判断应该以宽度还是高度为基准
                if (boardAspect > screenAspect)
                {
                    // 棋盘比屏幕更宽 → 以宽度为基准
                    orthographicSize = (physicalWidth / (2f * screenAspect)) + boardPadding;
                }
                else
                {
                    // 棋盘比屏幕更高 → 以高度为基准
                    orthographicSize = (physicalHeight / 2f) + boardPadding;
                }

                _mainCamera.orthographicSize = Mathf.Max(orthographicSize, 8.2f);
                _effectCamera.orthographicSize = _mainCamera.orthographicSize;
            }

            _guideCamera.transform.position = _mainCamera.transform.position;
            _guideCamera.orthographicSize = _mainCamera.orthographicSize;
            
            _boardBgInstance.ScaleBackground(_mainCamera);
            
            // var guideBg = _guideLevelBg.GetComponent<Background2DScaler>();
            // if (guideBg != null)
            // {
            //     guideBg.ScaleBackground(MainCamera);
            // }

            // _overItemBackground.transform.position = background2DScaler.transform.position;
            // _overItemBackground.transform.localScale = background2DScaler.transform.localScale;
            //
            // if (_isExecuteGuideLevel == false)
            //     _gridBoardBackground.UpdateGridBg(_levelData, startPos, endPos);
        }

        private void SetCameraPos(Vector3 pos)
        {
            _mainCamera.transform.position = pos;
            // _winStreakBox.transform.position = new Vector3(pos.x, pos.y - 5f, -1);
        }
    }
}