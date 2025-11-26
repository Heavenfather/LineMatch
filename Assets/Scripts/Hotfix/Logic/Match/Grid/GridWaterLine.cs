using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using GameConfig;
using UnityEngine;

namespace HotfixLogic.Match
{
    public class GridWaterLine : MonoBehaviour
    {
        [SerializeField] private Transform _leftObj;
        [SerializeField] private Transform _rightObj;
        [SerializeField] private Transform _upObj;
        [SerializeField] private Transform _downObj;

        [SerializeField] private LineRenderer _lineHorizontal;
        [SerializeField] private LineRenderer _lineVertical;

        void Start()
        {
            _lineHorizontal.startWidth = 0.8f;
            _lineHorizontal.endWidth = 0.8f;

            _lineVertical.startWidth = 0.8f;
            _lineVertical.endWidth = 0.8f;
        }

        void Update()
        {
            DrawnLine();
        }

        private void DrawnLine() {
            _lineHorizontal.SetPosition(0, _leftObj.position);
            _lineHorizontal.SetPosition(1, _rightObj.position);

            _lineVertical.SetPosition(0, _downObj.position);
            _lineVertical.SetPosition(1, _upObj.position);
        }

        public void SetMovePosition(ElementDirection dir, Vector3 position, bool immediately = false) {

            Transform moveObj = null;
            switch (dir) {
                case ElementDirection.Left:
                    moveObj = _leftObj;
                    break;
                case ElementDirection.Right:
                    moveObj = _rightObj;
                    break;
                case ElementDirection.Up:
                    moveObj = _upObj;
                    break;
                case ElementDirection.Down:
                    moveObj = _downObj;
                    break;
            }

            if (moveObj == null) return;

            if (immediately) {
                moveObj.position = position;
                DrawnLine();
            } else {
                moveObj.transform.DOMove(position, 0.25f);
            }
        }

        public void ResetLine() {
            _leftObj.localPosition = Vector3.zero;
            _rightObj.localPosition = Vector3.zero;
            _upObj.localPosition = Vector3.zero;
            _downObj.localPosition = Vector3.zero;
        }
    }
}
