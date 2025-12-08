using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using GameConfig;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class GridWaterShadow : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _angleLeftUp;
        [SerializeField] private SpriteRenderer _angleLeftDown;
        [SerializeField] private SpriteRenderer _angleRightUp;
        [SerializeField] private SpriteRenderer _angleRightDown;
        [SerializeField] private SpriteRenderer _lineUp;
        [SerializeField] private SpriteRenderer _lineDown;

        private Vector2Int _gridPos;

        private List<Vector2Int> _neighborPos = new List<Vector2Int>();

        public void SetGridPos(Vector2Int gridPos)
        {
            if(_neighborPos.Count > 0)
                return;
            _gridPos = gridPos;
            UpdateEightNeighborPos();
        }

        private void UpdateEightNeighborPos()
        {
            _neighborPos.Clear();
            for (int i = 0; i < ValidateManager.Instance.NeighborDirs.Length; i++)
            {
                Vector2Int pos = _gridPos + ValidateManager.Instance.NeighborDirs[i];
                _neighborPos.Add(pos);
            }
        }

        public void HideAllShadow() {
            _angleLeftUp.gameObject.SetActive(false);
            _angleLeftDown.gameObject.SetActive(false);
            _angleRightUp.gameObject.SetActive(false);
            _angleRightDown.gameObject.SetActive(false);
            _lineDown.gameObject.SetActive(false);
            _lineUp.gameObject.SetActive(false);
        }

        public void UpdateShadow(List<Vector2Int> neighborWaters) {
            HideAllShadow();

            if (neighborWaters == null || neighborWaters.Count == 0) {
                _angleLeftUp.gameObject.SetActive(true);
                _angleLeftDown.gameObject.SetActive(true);
                _angleRightUp.gameObject.SetActive(true);
                _angleRightDown.gameObject.SetActive(true);
                _lineDown.gameObject.SetActive(false);
                _lineUp.gameObject.SetActive(false);
                return;
            }

            var hasUp = neighborWaters.Contains(_neighborPos[0]);
            var hasLeft = neighborWaters.Contains(_neighborPos[1]);
            var hasRight = neighborWaters.Contains(_neighborPos[2]);
            var hasDown = neighborWaters.Contains(_neighborPos[3]);

            if (hasUp && hasDown) return;

            _angleLeftUp.gameObject.SetActive(!hasUp && !hasLeft);
            _angleLeftDown.gameObject.SetActive(!hasDown && !hasLeft);
            _angleRightUp.gameObject.SetActive(!hasUp && !hasRight);
            _angleRightDown.gameObject.SetActive(!hasDown && !hasRight);


            if (!hasUp && (hasLeft || hasRight)) {
                var upLineSize = Vector2.zero;
                var upLinePos = new Vector2(0, -0.2f);
                if (hasLeft && hasRight) {
                    upLineSize = new Vector2(0.8f, 0.4f);
                } else {
                    upLineSize = new Vector2(0.4f, 0.4f);
                    upLinePos.x = hasLeft ? -0.2f : 0.2f;
                }

                _lineUp.gameObject.SetActive(true);
                _lineUp.size = upLineSize;
                _lineUp.transform.localPosition = upLinePos;
            }

            if (!hasDown && (hasLeft || hasRight)) {
                var downLineSize = Vector2.zero;
                var downLinePos = new Vector2(0, 0.2f);
                if (hasLeft && hasRight) {
                    downLineSize = new Vector2(0.8f, 0.4f);
                } else {
                    downLineSize = new Vector2(0.4f, 0.4f);
                    downLinePos.x = hasLeft ? -0.2f : 0.2f;
                }

                _lineDown.gameObject.SetActive(true);
                _lineDown.size = downLineSize;
                _lineDown.transform.localPosition = downLinePos;
            }
        }

        public void SetWaterFlow(ElementDirection dir) {
            if (dir == ElementDirection.Left) {
                _angleLeftUp.gameObject.SetActive(false);
                _angleLeftDown.gameObject.SetActive(false);
            } else if (dir == ElementDirection.Right) {
                _angleRightUp.gameObject.SetActive(false);
                _angleRightDown.gameObject.SetActive(false);
            } else if (dir == ElementDirection.Up) {
                _angleLeftUp.gameObject.SetActive(false);
                _angleRightUp.gameObject.SetActive(false);
                _lineUp.gameObject.SetActive(false);
            } else if (dir == ElementDirection.Down) {
                _angleLeftDown.gameObject.SetActive(false);
                _angleRightDown.gameObject.SetActive(false);
                _lineDown.gameObject.SetActive(false);
            }
        }
    }


}
