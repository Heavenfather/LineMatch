using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using HotfixCore.MemoryPool;
using HotfixCore.Module;
using UnityEngine;
using Logger = GameCore.Log.Logger;

namespace HotfixLogic.Match
{
    public class SpreadParterreElementItem : BlockElementItem
    {
        private int _genNextElement = -1;
        private Tween _popTween;
        private HashSet<Vector2Int> _expansionCoords = new HashSet<Vector2Int>();
        private Tween _moveTween;

        protected override void OnInitialized()
        {
            _genNextElement = Data.NextBlockId;
            base.OnInitialized();
        }

        protected override bool OnDestroy(ElementDestroyContext context)
        {
            Data.EliminateCount--;
            ExpansionElement(context);
            PlayEffect();
            Data.NextBlockId = 0;
            return true;
        }

        public override void DoMove(float delayTime = 0, Ease ease = Ease.OutBounce)
        {
            _moveTween?.Kill();
            _moveTween = this.GameObject.transform.DOLocalMove(Vector3.zero, MatchConst.DropDuration)
                .SetEase(ease, 1.7f, 1).SetAutoKill().SetDelay(delayTime);
        }

        public override bool CanMove()
        {
            return true;
        }

        public override void Clear()
        {
            base.Clear();
            _moveTween?.Kill();
            _moveTween = null;
            _expansionCoords?.Clear();
            _genNextElement = -1;
        }

        private void ExpansionElement(ElementDestroyContext context)
        {
            if (IsCanInfectCoord(Data.GridPos, context.GridSystem))
                _expansionCoords.Add(Data.GridPos);
            var coords = ValidateManager.Instance.EightNeighborDirs;
            foreach (var coord in coords)
            {
                Vector2Int dir = coord + Data.GridPos;
                if (IsCanInfectCoord(dir, context.GridSystem))
                {
                    _expansionCoords.Add(dir);
                }
            }

            PushGroundElementToGrid(context);
        }

        private void PushGroundElementToGrid(ElementDestroyContext context)
        {
            foreach (var coord in _expansionCoords)
            {
                var grid = context.GridSystem.GetGridByCoord(coord);
                if (grid == null)
                    continue;
                var element = GenGroundElement(coord, grid.GameObject);
                element.GameObject.transform.localPosition = Vector3.zero;
                grid.PushElement(element, false);
                context.AddHoldSpreadElement(coord, element.Data.UId);
            }

            if (_genNextElement == (int)ElementIdConst.SpreadGrass)
            {
                SpreadGrassUtil.UpdateGrassBorder();
            }

            G.EventModule.DispatchEvent(GameEventDefine.OnMatchAddTargetNum,
                EventTwoParam<int, int>.Create(_genNextElement,
                    _expansionCoords.Count));
        }

        private bool IsCanInfectCoord(Vector2Int coord, GridSystem gridSystem)
        {
            if (!gridSystem.IsValidPosition(coord.x, coord.y))
                return false;
            var elements = ElementSystem.Instance.GetGridElements(coord, false);
            if (elements == null || elements.Count <= 0)
                return true;
            ElementMapDB db = ConfigMemoryPool.Get<ElementMapDB>();
            for (int i = 0; i < elements.Count; i++)
            {
                //不允许扩展条件
                // 1.元素是背景元素
                // 2.元素是循环收集的元素
                if (elements[i].Data.ElementType == ElementType.Background ||
                    db.IsCircleElement(elements[i].Data.ConfigId))
                    return false;
            }

            return true;
        }

        private ElementBase GenGroundElement(Vector2Int coord, GameObject parent)
        {
            ElementItemData elementData = ElementSystem.Instance.GenElementItemData(_genNextElement, coord.x, coord.y);
            var element = ElementSystem.Instance.GenElement(elementData, parent.transform);
            return element;
        }
    }
}