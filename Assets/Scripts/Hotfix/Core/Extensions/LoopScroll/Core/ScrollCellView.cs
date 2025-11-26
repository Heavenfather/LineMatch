using System;
using HotfixCore.Extensions;
using HotfixCore.Module;
using UnityEngine;
using UnityEngine.EventSystems;

// namespace HotfixCore.Extensions
// {
    /// <summary>
    /// 循环列表回调的复用Cell
    /// </summary>
    public class ScrollCellView : UIBehaviour
    {
        /// <summary>
        /// 实例RectTransform组件
        /// </summary>
        public RectTransform RTransform => this.GetComponent<RectTransform>();

        /// <summary>
        /// 实例模板id 外部请勿更改
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// Cell在列表里的索引值
        /// </summary>
        public int ContentIndex { get; set; }

        /// <summary>
        /// Cell的数据索引值
        /// </summary>
        public int DataIndex { get; private set; }

        private bool _active;

        /// <summary>
        /// 是否显示
        /// </summary>
        public bool Active
        {
            get => _active;
            set => _active = value;
        }

        /// <summary>
        /// 是否模板列表
        /// </summary>
        public bool IsTempList { get; set; }

        /// <summary>
        /// 数据总数
        /// </summary>
        public int DataCount { get; set; }

        private UIBase _controllerUI;

        /// <summary>
        /// 控制该模板的UI
        /// </summary>
        public UIBase ControllerUI
        {
            get => _controllerUI;
            set => _controllerUI ??= value;
        }

        private Type _widgetType;

        /// <summary>
        /// 该模板Widget的Type
        /// </summary>
        public Type WidgetType
        {
            get => _widgetType;
            set => _widgetType ??= value;
        }

        private UIWidget _widget;

        /// <summary>
        /// Cell数据源
        /// </summary>
        public UIWidget Widget
        {
            get => _widget;
            private set
            {
                if (_widget != null) return;
                _widget = value;
            }
        }

        private Action<float> _onPositionChangedCallback;

        /// <summary>
        /// Cell位置发生变化回调
        /// </summary>
        public Action<float> OnPositionChangedCallback
        {
            get => _onPositionChangedCallback;
            set
            {
                if (_onPositionChangedCallback != null)
                    _onPositionChangedCallback -= _onPositionChangedCallback;
                _onPositionChangedCallback += value;
            }
        }

        private Action<ScrollCellView> _refreshCallback;

        /// <summary>
        /// 刷新函数
        /// </summary>
        public Action<ScrollCellView> RefreshCallback
        {
            get => _refreshCallback;
            set => _refreshCallback ??= value;
        }

        private Action<ScrollCellView, ScrollCellView> _tempListCallback;

        /// <summary>
        /// 列表模板刷新函数(参数1位子节点cell，参数2为父节点cell)
        /// </summary>
        public Action<ScrollCellView, ScrollCellView> TempListCallback
        {
            get => _tempListCallback;
            set => _tempListCallback ??= value;
        }

        public virtual void RefreshCellWidget()
        {
            if (IsTempList)
            {
                int count = this.gameObject.transform.childCount;
                if (count <= 0)
                {
                    return;
                }

                if (DataCount <= 0)
                    return;
                for (int i = 0; i < count; i++)
                {
                    var com = this.gameObject.transform.GetChild(i);
                    var childCell = com.gameObject.GetOrAddComponent<ScrollCellView>();
                    childCell.ContentIndex = i;
                    int dataIndex = i + ContentIndex * count;
                    childCell.DataIndex = dataIndex;
                    childCell.IsTempList = false;
                    if (dataIndex >= DataCount)
                    {
                        com.SetVisible(false);
                        continue;
                    }

                    com.SetVisible(true);
                    if (_widgetType != null)
                        childCell.Widget ??= _controllerUI.CreateWidget(_widgetType, childCell.gameObject);
                    TempListCallback?.Invoke(childCell, this);
                }
            }
            else
            {
                this.DataIndex = ContentIndex % DataCount;
                this.Widget ??= _controllerUI.CreateWidget(_widgetType, this.gameObject);
                RefreshCallback?.Invoke(this);
            }
        }

        protected override void OnDestroy()
        {
            if (Widget != null)
            {
                Widget.OnDestroy();
            }

            this.Widget = null;
            base.OnDestroy();
        }
    }
// }