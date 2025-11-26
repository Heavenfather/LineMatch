using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Logic.Match;
using HotfixCore.MemoryPool;
using HotfixCore.Module;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HotfixLogic.Match
{
    public class JumpCollectElementItem : CollectElementItem
    {
        protected override bool OnDestroy(ElementDestroyContext context)
        {
            //如果当前不是在最后一行则不算掉落
            if (this.Data.GridPos.y != context.GridSystem.FindLastNotEmptyY(this.Data.GridPos.x))
            {
                return false;
            }

            // 飞到目标那里
            G.EventModule.DispatchEvent(GameEventDefine.OnMatchElementMoveToTarget,
                EventTwoParam<int, Vector3>.Create(this.Data.ConfigId, GameObject.transform.position));
            // MemoryPool.Release(this);
            State = ElementState.CanRecycle;
            return true;
        }

        public void JumpToOther(GridSystem gridSystem)
        {
            Vector2Int jumpCoord = GetJumpGridCoord();
            if(!gridSystem.IsValidPosition(jumpCoord.x, jumpCoord.y))
                return;
            if(ElementSystem.Instance.HaveOverElementLock(jumpCoord,-1,out var _,false))
                return;
            int lastY = gridSystem.FindLastNotEmptyY(this.Data.GridPos.x);
            if(lastY == this.Data.GridPos.y)
                return;
            var elements = ElementSystem.Instance.GetGridElements(jumpCoord, true);
            if(elements == null || elements.Count <= 0)
                return;
            bool result = false;
            int index = -1;
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].CanMove() && elements[i].Data.ElementType != ElementType.JumpCollect)
                {
                    result = true;
                    index = i;
                    break;
                }
            }
            if (result)
            {
                var baseElement = elements[index];
                ShuffleElementManager.Instance.SwapElementEntity(baseElement, this);
                baseElement.DoMove();
                this.DoMove();
                
                ElementAudioManager.Instance.Play("tuzi");
            }
        }

        private Vector2Int GetJumpGridCoord()
        {
            if (Data.Direction == ElementDirection.Up)
            {
                //只向上寻找位置
                Vector2Int coord = new Vector2Int(Data.GridPos.x, Data.GridPos.y - 1);
                return coord;
            }

            if (Data.Direction == ElementDirection.None)
            {
                //四个方向随机
                var neighbor = MatchTweenUtil.GetNeighborPos(Data.GridPos);
                if (neighbor.Count > 0)
                {
                    List<Vector2Int> canJumpCoords = new List<Vector2Int>(neighbor.Count);
                    for (int i = 0; i < neighbor.Count; i++)
                    {
                        var elements = ElementSystem.Instance.GetGridElements(neighbor[i], true);
                        var result = ElementSystem.Instance.TryGetBaseElement(elements, out int _, true);
                        if (result)
                        {
                            canJumpCoords.Add(neighbor[i]);
                        }
                    }

                    if (canJumpCoords.Count > 0)
                        return canJumpCoords[Random.Range(0, canJumpCoords.Count)];
                }
            }

            return new Vector2Int(-1, -1);
        }

        
    }
    
    public static class JumpCollectElementItemUtil
    {
        private static readonly HashSet<int> _jumpedUIds = new HashSet<int>();
        
        public static void CheckJump(HashSet<Vector2Int> destroyedCoords,GridSystem gridSystem)
        {
            _jumpedUIds.Clear();
            bool IsCanJump(Vector2Int elementPos)
            {
                int lastY = gridSystem.FindLastNotEmptyY(elementPos.x);
                if(lastY == elementPos.y)
                    return false;
                foreach (var coord in destroyedCoords)
                {
                    if(coord.x == elementPos.x && coord.y > elementPos.y)
                        return false;
                }
                return true;
            }
            
            var allElements = ElementSystem.Instance.GridElements;
            List<JumpCollectElementItem> waitDropJump = new List<JumpCollectElementItem>();
            foreach (var elements in allElements.Values)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if(_jumpedUIds.Contains(elements[i].Data.UId))
                        continue;
                    if (ElementSystem.Instance.HaveOverElementLock(elements, (int)ElementIdConst.JumpRobi,out var _))
                        continue;
                    if (elements[i] is JumpCollectElementItem jumpElement && IsCanJump(jumpElement.Data.GridPos))
                    {
                        _jumpedUIds.Add(jumpElement.Data.UId);
                        if (destroyedCoords.Contains(jumpElement.Data.GridPos + new Vector2Int(0, -1)))
                        {
                            waitDropJump.Add(jumpElement);
                        }
                        else
                        {
                            jumpElement.JumpToOther(gridSystem);
                        }

                        break;
                    }
                }
            }

            if (waitDropJump.Count > 0)
            {
                G.UIModule.ScreenLock("JumpCollectElementItem", true);
                UniTask.Create(async () =>
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(MatchConst.DropDuration));
                    G.UIModule.ScreenLock("JumpCollectElementItem", false);
                    for (int i = 0; i < waitDropJump.Count; i++)
                    {
                        waitDropJump[i].JumpToOther(gridSystem);
                    }
                }).Forget();
            }
        }
    }
}