using UnityEngine;

namespace HotfixCore.Module
{
    public class TouchModule : IModule
    {
        public int FingerId { get; private set; } = -1;

        public int TouchCount { get; private set; }

        public TouchPhase TouchPhase { get; private set; }

        public Touch TouchZero { get; private set; }
        public Touch TouchOne { get; private set; }

        public Vector3 InputPos { get; private set; }

        public bool TouchIsValid()
        {
            GetTouch();
            return FingerId >= 0;
        }

        private void GetTouch()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE
                HandleMouseInput();
#else
                HandleTouchInput();
#endif
        }

        private void HandleMouseInput()
        {
            bool onDown = Input.GetMouseButtonDown(0);
            bool down = Input.GetMouseButton(0);
            bool onUp = Input.GetMouseButtonUp(0);

            if (onDown || down || onUp)
            {
                InputPos = Input.mousePosition;
                FingerId = 0; // 使用 0 表示鼠标输入
                if (onDown) TouchPhase = TouchPhase.Began;
                else if (down) TouchPhase = TouchPhase.Moved;
                else if (onUp) TouchPhase = TouchPhase.Ended;

                TouchCount = 1;
            }
            else
            {
                FingerId = -1;
                TouchPhase = TouchPhase.Canceled; // 重置状态
                TouchCount = 0;
            }
        }

        private void HandleTouchInput()
        {
            int touchCount = Input.touchCount;
            if (touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                InputPos = touch.position;
                TouchPhase = touch.phase;
                FingerId = touch.fingerId;
                TouchCount = touchCount;

                if (touchCount > 1)
                {
                    TouchZero = Input.GetTouch(0);
                    TouchOne = Input.GetTouch(1);
                }
            }
            else
            {
                FingerId = -1;
                TouchPhase = TouchPhase.Canceled; // 重置状态
                TouchCount = 0;

                TouchZero = default;
                TouchOne = default;
            }
        }
    }
}