using Cysharp.Threading.Tasks;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.Extensions;
using HotfixCore.MemoryPool;
using HotfixCore.Module;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic.Match
{
    public class BurningCollectElementItem : BlockElementItem
    {
        protected override void OnInitialized()
        {
            SetOpen(false);
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            if (context.IsBurningCollectItemCanRelease)
            {
                PlayEffect();
                return true;
            }
            int infoIndex =  context.WillDelCoords.FindIndex(x => x.Coord == Data.GridPos);
            var info = context.WillDelCoords[infoIndex];
            int attachCount = info.AttachCount;
            if (!string.IsNullOrEmpty(Data.Extra) && int.TryParse(Data.Extra, out int targetId))
            {
                bool releaseAll = false;
                var tempTargets = LevelTargetSystem.Instance.TempTargetElements;
                
                if (tempTargets.ContainsKey(targetId))
                {
                    int remainCount = tempTargets[targetId];
                    attachCount = Mathf.Min(attachCount, remainCount);
                    releaseAll = remainCount - attachCount <= 0;
                    tempTargets[targetId] -= attachCount;
                    
                    context.AddCalAddedCount(targetId, attachCount);
                    PopCollectItem(targetId, attachCount);
                }

                if (!releaseAll)
                    DoEffect().Forget();
                else
                    ReleaseAllBurningCollectItem(context).Forget();
            }

            return false;
        }

        private async UniTask ReleaseAllBurningCollectItem(ElementDestroyContext context)
        {
            context.IsBurningCollectItemCanRelease = true;
            var allGrid = ElementSystem.Instance.GridElements;
            int perFrameCount = MatchConst.BatchDestroyPerFrameCount;
            int deletedCount = 0;
            foreach (var elements in allGrid.Values)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Data.ElementType == ElementType.BurningCollect)
                    {
                        context.AddWillDelCoord(elements[i].Data.GridPos, elements[i].Data.EliminateStyle, Data.UId);
                        context.GridSystem.DestroyElementByCoord(context, elements[i].Data.GridPos);

                        deletedCount++;
                        if(deletedCount % perFrameCount == 0)
                            await UniTask.Yield();
                    }
                }
            }
        }

        private async UniTask DoEffect()
        {
            SetOpen(true);
            await UniTask.Delay(300);
            if(State == ElementState.Recycle)
                return;
            SetOpen(false);
        }

        private void SetOpen(bool isOpen)
        {
            var icon = this.GameObject.transform.Find("Icon");
            if(icon != null)
                icon.SetVisible(!isOpen);
            var open = this.GameObject.transform.Find("Open");
            if(open != null)
                open.SetVisible(isOpen);
            if (isOpen)
            {
                MatchEffectManager.Instance.PlayObjectEffect(Data.ConfigId, null, this.GameObject.transform);
                
                ElementAudioManager.Instance.Play("beike");
            }
        }

        private void PopCollectItem(int targetId,int count)
        {
            for (int i = 0; i < count; i++)
            {
                // 飞到目标那里
                G.EventModule.DispatchEvent(GameEventDefine.OnMatchElementMoveToTarget,
                    EventTwoParam<int, Vector3>.Create(targetId, GameObject.transform.position));
            }
        }
    }
}