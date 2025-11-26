using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic.Match
{
    public class LineController : MonoBehaviour
    {
        private static LineController _instance;

        public static LineController Instance => _instance;

        [SerializeField] private LineRenderer _underLine;
        [SerializeField] private LineRenderer _overLine;

        private List<Vector3> _underLinePoints = new List<Vector3>();
        private List<Vector3> _overLinePoints = new List<Vector3>();

        private void Awake()
        {
            _instance = this;
            _underLine.startWidth = 0.12f;
            _underLine.endWidth = 0.12f;
            _overLine.startWidth = 0.12f;
            _overLine.endWidth = 0.12f;
        }

        public void SetLineColor(Color color)
        {
            _underLine.startColor = color;
            _underLine.endColor = color;
            _overLine.startColor = color;
            _overLine.endColor = color;
        }
        
        public void AddUnderPoint(Vector3 newLinePos)
        {
            if(_underLinePoints.Count >0 && newLinePos == _underLinePoints[^1])
                return;
            _underLinePoints.Add(newLinePos);
            _underLine.positionCount = _underLinePoints.Count;
            _underLine.SetPositions(_underLinePoints.ToArray());
        }

        public int GetUnderLinePointCount()
        {
            return _underLine.positionCount;
        }

        public void RemoveUnderPoint()
        {
            if(_underLinePoints.Count <= 0)
                return;
            _underLinePoints.RemoveAt(_underLinePoints.Count - 1);
            _underLine.positionCount = _underLinePoints.Count;
            _underLine.SetPositions(_underLinePoints.ToArray());
        }
        
        public void SetOverLinePoint(int index, Vector3 newPoint)
        {
            if (index > _overLinePoints.Count)
                _overLinePoints.Add(newPoint);
            else
                _overLinePoints[index - 1] = newPoint;
            _overLine.positionCount = _overLinePoints.Count;
            _overLine.SetPositions(_overLinePoints.ToArray());
        }

        public int GetOverLinePointCount()
        {
            return _overLinePoints.Count;
        }

        public void ClearOverLine()
        {
            _overLinePoints.Clear();
            _overLine.positionCount = 0;
        }

        public void ClearUnderLine()
        {
            _underLinePoints.Clear();
            if (this.transform != null && this._underLine != null && this._underLine.transform != null)
                _underLine.positionCount = 0;
        }

        public void SetLineLayer(string layerName)
        {
            _underLine.gameObject.layer = LayerMask.NameToLayer(layerName);
            _overLine.gameObject.layer = LayerMask.NameToLayer(layerName);
        }
        
        public void ClearAllLines()
        {
            _underLine.positionCount = 0;
            _overLine.positionCount = 0;
            _underLinePoints.Clear();
            _overLinePoints.Clear();
        }
    }
}