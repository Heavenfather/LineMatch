using System.Collections.Generic;
using GameConfig;
using Hotfix.Logic.Match;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class GridBoardBackground : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _holeTile;
        [SerializeField] private List<SpriteRenderer> lineSprites;

        private List<SpriteRenderer> _lineList = new List<SpriteRenderer>();
        private Dictionary<Vector2Int, SpriteRenderer> _holeCoordMap = new Dictionary<Vector2Int, SpriteRenderer>();

        public void UpdateGridBg(LevelData levelData, Vector2Int startPos, Vector2Int endPos)
        {
            Clear();
            
            for (int x = startPos.x; x <= endPos.x; x++)
            {
                for (int y = startPos.y; y <= endPos.y; y++)
                {
                    if (levelData.grid[x][y].isWhite)
                    {
                        var holePos = GridSystem.GetGridPositionByCoord(x, y);
                        var hole = GameObject.Instantiate(_holeTile, _holeTile.transform.parent);
                        hole.transform.position = holePos;
                        hole.gameObject.SetActive(true);

                        Vector2Int coord = new Vector2Int(x, y);
                        _holeCoordMap.Add(coord, hole);
                    }
                }
            }

            foreach (var hole in _holeCoordMap)
            {
                DrawLine(hole.Key);
            }
        }

        public void Clear()
        {
            ClearHoleList();
            ClearLineList();
        }

        private void DrawLine(Vector2Int holePos)
        {
            var position = GridSystem.GetGridPositionByCoord(holePos.x, holePos.y);

            var neighborPos = MatchTweenUtil.GetEightNeighborPos(holePos);

            var isUp = _holeCoordMap.ContainsKey(neighborPos[0]);
            var isDown = _holeCoordMap.ContainsKey(neighborPos[1]);
            var isLeft = _holeCoordMap.ContainsKey(neighborPos[2]);
            var isRight = _holeCoordMap.ContainsKey(neighborPos[3]);
            var isUpRight = _holeCoordMap.ContainsKey(neighborPos[4]);
            var isDownRight = _holeCoordMap.ContainsKey(neighborPos[5]);
            var isUpLeft = _holeCoordMap.ContainsKey(neighborPos[6]);
            var isDownLeft = _holeCoordMap.ContainsKey(neighborPos[7]);


            var upLeftAngle = !isUp && !isLeft && !isUpLeft;
            var upRightAngle = !isUp && !isRight && !isUpRight;
            var downLeftAngle = !isDown && !isLeft && !isDownLeft;
            var downRightAngle = !isDown && !isRight && !isDownRight;

            if (upLeftAngle)
            {
                var line = InstantiateLineSprite(MatchLineType.InsideUpLeft);
                line.transform.position = new Vector3(position.x - 0.2f, position.y - 0.2f, 0);
            }

            if (upRightAngle)
            {
                var line = InstantiateLineSprite(MatchLineType.InsideUpRight);
                line.transform.position = new Vector3(position.x + 0.2f, position.y - 0.2f, 0);
            }

            if (downLeftAngle)
            {
                var line = InstantiateLineSprite(MatchLineType.InsideDownLeft);
                line.transform.position = new Vector3(position.x - 0.2f, position.y + 0.2f, 0);
            }

            if (downRightAngle)
            {
                var line = InstantiateLineSprite(MatchLineType.InsideDownRight);
                line.transform.position = new Vector3(position.x + 0.2f, position.y + 0.2f, 0);
            }


            var upLeftCorner = !upLeftAngle && !isUpLeft && isLeft && isUp;
            var upRightCorner = !upRightAngle && !isUpRight && isRight && isUp;
            var downLeftCorner = !downLeftAngle && !isDownLeft && isLeft && isDown;
            var downRightCorner = !downRightAngle && !isDownRight && isRight && isDown;
            if (upLeftCorner)
            {
                var line = InstantiateLineSprite(MatchLineType.OutsideUpLeft);
                line.transform.position = new Vector3(position.x - 0.4f, position.y - 0.4f, 0);
            }

            if (upRightCorner)
            {
                var line = InstantiateLineSprite(MatchLineType.OutsideUpRight);
                line.transform.position = new Vector3(position.x + 0.4f, position.y - 0.4f, 0);
            }

            if (downLeftCorner)
            {
                var line = InstantiateLineSprite(MatchLineType.OutsideDownLeft);
                line.transform.position = new Vector3(position.x - 0.4f, position.y + 0.4f, 0);
            }

            if (downRightCorner)
            {
                var line = InstantiateLineSprite(MatchLineType.OutsideDownRight);
                line.transform.position = new Vector3(position.x + 0.4f, position.y + 0.4f, 0);
            }


            var upLine = !isUp && (!upLeftAngle || !upRightAngle);
            var leftLine = !isLeft && (!upLeftAngle || !downLeftAngle);
            var rightLine = !isRight && (!upRightAngle || !downRightAngle);
            var downLine = !isDown && (!downLeftAngle || !downRightAngle);

            if (upLine)
            {
                var line = InstantiateLineSprite(MatchLineType.LineUp);
                var size = Vector2.zero;
                var pos = new Vector3(position.x, position.y - 0.2f, 0);
                if (!upLeftAngle && !upRightAngle)
                {
                    size = new Vector2(0.8f, 0.4f);
                }
                else
                {
                    size = new Vector2(0.4f, 0.4f);
                    if (!upLeftAngle)
                    {
                        pos.x -= 0.2f;
                    }
                    else if (!upRightAngle)
                    {
                        pos.x += 0.2f;
                    }
                }

                line.transform.position = pos;
                line.size = size;
            }

            if (leftLine)
            {
                var line = InstantiateLineSprite(MatchLineType.LineLeft);
                var size = Vector2.zero;
                var pos = new Vector3(position.x - 0.2f, position.y, 0);
                if (!upLeftAngle && !downLeftAngle)
                {
                    size = new Vector2(0.8f, 0.4f);
                }
                else
                {
                    size = new Vector2(0.4f, 0.4f);
                    if (!upLeftAngle)
                    {
                        pos.y -= 0.2f;
                    }
                    else if (!downLeftAngle)
                    {
                        pos.y += 0.2f;
                    }
                }

                line.transform.position = pos;
                line.size = size;
            }

            if (rightLine)
            {
                var line = InstantiateLineSprite(MatchLineType.LineRight);
                var size = Vector2.zero;
                var pos = new Vector3(position.x + 0.2f, position.y, 0);
                if (!upRightAngle && !downRightAngle)
                {
                    size = new Vector2(0.8f, 0.4f);
                }
                else
                {
                    size = new Vector2(0.4f, 0.4f);
                    if (!upRightAngle)
                    {
                        pos.y -= 0.2f;
                    }
                    else if (!downRightAngle)
                    {
                        pos.y += 0.2f;
                    }
                }

                line.transform.position = pos;
                line.size = size;
            }

            if (downLine)
            {
                var line = InstantiateLineSprite(MatchLineType.LineDown);
                var size = Vector2.zero;
                var pos = new Vector3(position.x, position.y + 0.2f, 0);
                if (!downLeftAngle && !downRightAngle)
                {
                    size = new Vector2(0.8f, 0.4f);
                }
                else
                {
                    size = new Vector2(0.4f, 0.4f);
                    if (!downLeftAngle)
                    {
                        pos.x -= 0.2f;
                    }
                    else if (!downRightAngle)
                    {
                        pos.x += 0.2f;
                    }
                }

                line.transform.position = pos;
                line.size = size;
            }
        }

        private SpriteRenderer InstantiateLineSprite(MatchLineType lineType)
        {
            var spRenderer = lineSprites[(int)lineType];
            var lineSp = GameObject.Instantiate(spRenderer, spRenderer.transform.parent);
            lineSp.gameObject.SetActive(true);

            _lineList.Add(lineSp);

            return lineSp;
        }
        
        private void ClearHoleList()
        {
            foreach (var item in _holeCoordMap)
            {
                DestroyImmediate(item.Value.gameObject);
            }

            _holeCoordMap.Clear();
        }

        private void ClearLineList()
        {
            foreach (var item in _lineList)
            {
                DestroyImmediate(item.gameObject);
            }

            _lineList.Clear();
        }

    }
}