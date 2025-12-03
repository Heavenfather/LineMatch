#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using GameConfig;
using Hotfix.Define;
using HotfixCore.Extensions;
using UnityEngine;
using HotfixCore.Module;
using HotfixLogic.Match;
using UnityEngine.Events;

namespace HotfixLogic
{
    public partial class widget_elementCell : UIWidget
    {
        private int _id;
        private UnityAction<int> _clickCallback;

        public override void OnCreate()
        {
            btn_cell.AddClick(() => { _clickCallback?.Invoke(_id); });
        }

        public void SetView(int id, bool isSelect, UnityAction<int> clickCallback)
        {
            this._id = id;
            _clickCallback = clickCallback;
            if (id < 0)
            {
                text_id.text = id == MatchEditorConst.WhiteElementId ? "空格" : "四边形";
                img_icon.sprite = null;
            }
            else
            {
                UpdateIcon(id).Forget();
                UpdateShowText(id);
            }

            go_select.SetVisible(isSelect);
        }

        private async UniTask UpdateIcon(int id)
        {
            img_icon.sprite = await MatchEditorUtils.GetElementIcon(id);
            
            var db = ConfigMemoryPool.Get<ElementMapDB>();
            ElementMap config = db[id];
            if (config.elementType == ElementType.Normal)
            {
                img_icon.color = ElementSystem.Instance.GetElementColor(id);
            }
            else
                img_icon.color = Color.white;
        }

        private void UpdateShowText(int id)
        {
            var db = ConfigMemoryPool.Get<ElementMapDB>();
            ref readonly ElementMap config = ref db[id];
            string dir = GetDirectionName(config.direction);
            text_id.text = string.IsNullOrEmpty(dir) ? $"Id:{id}" : $"Id:{id}-{dir}";
        }
        
        private string GetDirectionName(ElementDirection direction)
        {
            switch (direction)
            {
                case ElementDirection.Up:
                    return "上";
                case ElementDirection.Down:
                    return "下";
                case ElementDirection.Left:
                    return "左";
                case ElementDirection.Right:
                    return "右";
            }
            return "";
        }
    }
}
#endif