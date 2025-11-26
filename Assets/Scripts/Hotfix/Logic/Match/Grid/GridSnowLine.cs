using System.Collections;
using System.Collections.Generic;
using Hotfix.Logic.Match;
using HotfixLogic.Match;
using UnityEngine;

public class GridSnowLine : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _angleOutsizeLeftUp;
    [SerializeField] private SpriteRenderer _angleOutsizeRightUp;
    [SerializeField] private SpriteRenderer _angleOutsizeLeftDown;
    [SerializeField] private SpriteRenderer _angleOutsizeRightDown;

    [SerializeField] private SpriteRenderer _angleInsizeLeftUp;
    [SerializeField] private SpriteRenderer _angleInsizeRightUp;
    [SerializeField] private SpriteRenderer _angleInsizeLeftDown;
    [SerializeField] private SpriteRenderer _angleInsizeRightDown;

    [SerializeField] private SpriteRenderer _bgLeftUp;
    [SerializeField] private SpriteRenderer _bgRightUp;
    [SerializeField] private SpriteRenderer _bgLeftDown;
    [SerializeField] private SpriteRenderer _bgRightDown;


    [SerializeField] private SpriteRenderer _lineUp;
    [SerializeField] private SpriteRenderer _lineDown;
    [SerializeField] private SpriteRenderer _lineLeft;
    [SerializeField] private SpriteRenderer _lineRight;


    private Vector2Int _gridPos;

    private List<Vector2Int> _neighborPos = new List<Vector2Int>();

    public void SetGridPos(Vector2Int gridPos)
    {
        _gridPos = gridPos;
        UpdateEightNeighborPos();
    }

    private void UpdateEightNeighborPos()
    {
        _neighborPos.Clear();
        _neighborPos = MatchTweenUtil.GetEightNeighborPos(_gridPos);
    }

    public void HideAllLines() {
        _angleOutsizeLeftUp.gameObject.SetActive(false);
        _angleOutsizeRightUp.gameObject.SetActive(false);
        _angleOutsizeLeftDown.gameObject.SetActive(false);
        _angleOutsizeRightDown.gameObject.SetActive(false);

        _angleInsizeLeftUp.gameObject.SetActive(false);
        _angleInsizeRightUp.gameObject.SetActive(false);
        _angleInsizeLeftDown.gameObject.SetActive(false);
        _angleInsizeRightDown.gameObject.SetActive(false);

        _bgLeftUp.gameObject.SetActive(false);
        _bgRightUp.gameObject.SetActive(false);
        _bgLeftDown.gameObject.SetActive(false);
        _bgRightDown.gameObject.SetActive(false);

        _lineUp.gameObject.SetActive(false);
        _lineDown.gameObject.SetActive(false);
        _lineLeft.gameObject.SetActive(false);
        _lineRight.gameObject.SetActive(false);
    }

    public void UpdateLine(List<Vector2Int> neighborSnows) {
        HideAllLines();

        if (neighborSnows == null || neighborSnows.Count == 0) {
            _angleOutsizeLeftUp.gameObject.SetActive(true);
            _angleOutsizeRightUp.gameObject.SetActive(true);
            _angleOutsizeLeftDown.gameObject.SetActive(true);
            _angleOutsizeRightDown.gameObject.SetActive(true);
            return;
        }

        var hasUp = neighborSnows.Contains(_neighborPos[0]);
        var hasDown = neighborSnows.Contains(_neighborPos[1]);
        var hasLeft = neighborSnows.Contains(_neighborPos[2]);
        var hasRight = neighborSnows.Contains(_neighborPos[3]);
        var hasRightUp = neighborSnows.Contains(_neighborPos[4]);
        var hasRightDown = neighborSnows.Contains(_neighborPos[5]);
        var hasLeftUp = neighborSnows.Contains(_neighborPos[6]);
        var hasLeftDown = neighborSnows.Contains(_neighborPos[7]);

        CheckOutsizeAngle(hasUp, hasDown, hasLeft, hasRight, hasRightUp, hasRightDown, hasLeftUp, hasLeftDown);
        CheckInsizeAngle(hasUp, hasDown, hasLeft, hasRight, hasRightUp, hasRightDown, hasLeftUp, hasLeftDown);
        CheckLine(hasUp, hasDown, hasLeft, hasRight);

    }

    private void CheckOutsizeAngle(bool hasUp, bool hasDown, bool hasLeft, bool hasRight, 
                                    bool hasRightUp, bool hasRightDown, bool hasLeftUp, bool hasLeftDown) {
        
        bool hasOutsizeLeftUp = !hasUp && !hasLeft && !hasLeftUp;
        bool hasOutsizeRightUp = !hasUp && !hasRight && !hasRightUp;
        bool hasOutsizeLeftDown = !hasDown && !hasLeft && !hasLeftDown;
        bool hasOutsizeRightDown = !hasDown && !hasRight && !hasRightDown;

        _angleOutsizeLeftUp.gameObject.SetActive(hasOutsizeLeftUp);
        _angleOutsizeRightUp.gameObject.SetActive(hasOutsizeRightUp);
        _angleOutsizeLeftDown.gameObject.SetActive(hasOutsizeLeftDown);
        _angleOutsizeRightDown.gameObject.SetActive(hasOutsizeRightDown);


        _bgLeftUp.gameObject.SetActive(!hasOutsizeLeftUp);
        _bgRightUp.gameObject.SetActive(!hasOutsizeRightUp);
        _bgLeftDown.gameObject.SetActive(!hasOutsizeLeftDown);
        _bgRightDown.gameObject.SetActive(!hasOutsizeRightDown);
    }

    private void CheckInsizeAngle(bool hasUp, bool hasDown, bool hasLeft, bool hasRight, 
                                   bool hasRightUp, bool hasRightDown, bool hasLeftUp, bool hasLeftDown) {
                                    
        _angleInsizeLeftUp.gameObject.SetActive(hasUp && hasLeft && !hasLeftUp);
        _angleInsizeRightUp.gameObject.SetActive(hasUp && hasRight && !hasRightUp);
        _angleInsizeLeftDown.gameObject.SetActive(hasDown && hasLeft && !hasLeftDown);
        _angleInsizeRightDown.gameObject.SetActive(hasDown && hasRight && !hasRightDown);
    }

    private void CheckLine(bool hasUp, bool hasDown, bool hasLeft, bool hasRight) {
        // 上部分没有雪
        if (!hasUp && (hasLeft || hasRight)) {
            if (hasLeft && hasRight) {
                // 左右两边都有雪
                _lineUp.gameObject.SetActive(true);
                _lineUp.size = new Vector2(0.8f, 0.4f);
                _lineUp.transform.localPosition = new Vector3(0, -0.2f, 0);
            } else {
                var angleLine = hasLeft ? _angleOutsizeRightUp : _angleOutsizeLeftUp;
                var lineSize = new Vector2(0.4f, 0.4f);
                var pos = hasLeft ? new Vector3(-0.2f, -0.2f, 0) : new Vector3(0.2f, -0.2f, 0);

                angleLine.gameObject.SetActive(true);
                _lineUp.gameObject.SetActive(true);
                _lineUp.size = lineSize;
                _lineUp.transform.localPosition = pos;
            }
        }

        // 下部分没有雪
        if (!hasDown && (hasLeft || hasRight)) {
            // 左右两边都有雪
            if (hasLeft && hasRight) {
                _lineDown.gameObject.SetActive(true);
                _lineDown.size = new Vector2(0.8f, 0.4f);
                _lineDown.transform.localPosition = new Vector3(0, 0.2f, 0);
            } else {
                var angleLine = hasLeft ? _angleOutsizeRightDown : _angleOutsizeLeftDown;
                var lineSize = new Vector2(0.4f, 0.4f);
                var pos = hasLeft ? new Vector3(-0.2f, 0.2f, 0) : new Vector3(0.2f, 0.2f, 0);

                angleLine.gameObject.SetActive(true);
                _lineDown.gameObject.SetActive(true);
                _lineDown.size = lineSize;
                _lineDown.transform.localPosition = pos;
            }
        }


        // 左边没有雪
        if (!hasLeft && (hasUp || hasDown)) {
            // 上下两边都有雪
            if (hasUp && hasDown) {
                _lineLeft.gameObject.SetActive(true);
                _lineLeft.size = new Vector2(0.8f, 0.4f);
                _lineLeft.transform.localPosition = new Vector3(-0.2f, 0, 0);
            } else {
                var angleLine = hasDown ? _angleOutsizeLeftUp : _angleOutsizeLeftDown;
                var lineSize = new Vector2(0.4f, 0.4f);
                var pos = hasDown ? new Vector3(-0.2f, 0.2f, 0) : new Vector3(-0.2f, -0.2f, 0);

                angleLine.gameObject.SetActive(true);
                _lineLeft.gameObject.SetActive(true);
                _lineLeft.size = lineSize;
                _lineLeft.transform.localPosition = pos;
            }
        }

        // 右边没有雪
        if (!hasRight && (hasUp || hasDown)) {
            // 上下两边都有雪
            if (hasUp && hasDown) {
                _lineRight.gameObject.SetActive(true);
                _lineRight.size = new Vector2(0.8f, 0.4f);
                _lineRight.transform.localPosition = new Vector3(0.2f, 0, 0);
            } else {
                var angleLine = hasDown ? _angleOutsizeRightUp : _angleOutsizeRightDown;
                var lineSize = new Vector2(0.4f, 0.4f);
                var pos = hasDown ? new Vector3(0.2f, 0.2f, 0) : new Vector3(0.2f, -0.2f, 0);

                angleLine.gameObject.SetActive(true);
                _lineRight.gameObject.SetActive(true);
                _lineRight.size = lineSize;
                _lineRight.transform.localPosition = pos;
            }
        }
    }
}
