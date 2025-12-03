using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameConfig;
using Hotfix.Define;
using Hotfix.EventParameter;
using Hotfix.Logic.Match;
using Hotfix.Utils;
using HotfixCore.Module;
using HotfixLogic.Match;
using Spine;
using Spine.Unity;
using UnityEngine;
using Logger = GameCore.Log.Logger;

public class GridLastStepPrompt : MonoBehaviour
{
    [SerializeField] private GameObject _effCoolLine;
    [SerializeField] private GameObject _effWarmLine;

    private GameObject _effLine;

    Queue<GameObject> _effQueue = new Queue<GameObject>();
    List<GameObject> _effShowList = new List<GameObject>();

    HashSet<Vector2Int> _blockCoords = new HashSet<Vector2Int>();

    private int _gridCol;
    private int _gridRow;

    private void Awake() {
        LevelMapImageDB db = ConfigMemoryPool.Get<LevelMapImageDB>();
        int id = db.GetLevelInPage(MatchManager.Instance.CurLevelID, MatchManager.Instance.MaxLevel);
        LevelMapImage config = db[id + 1];

        _effLine = _effCoolLine;
        if (config.lastLine == MatchColorScheme.Warm) {
            _effLine = _effWarmLine;
        }
    }

    public void SetGridSize(int col, int row) {
        _gridCol = col;
        _gridRow = row;
    }

    public void ShowLastStepPrompt() {
        if (MatchManager.Instance.MaxLevel < ConfigMemoryPool.Get<ConstConfigDB>().GetConfigIntVal("MatchLastStepPromptLV")) {
            return;
        }

        _blockCoords.Clear();

        bool xiongMaoFinish = LevelTargetSystem.Instance.CheckTargetComplete(130);


        for (int i = 0; i < _gridCol; i++) {
            for (int j = 0; j < _gridRow; j++) {
                var coord = new Vector2Int(i, j);
                var elements = ElementSystem.Instance.GetGridElements(coord, false);
                if (elements != null && elements.Count > 0) {
                    foreach (var element in elements) {
                        // 如果有水流，直接返回，不提示
                        if (element.Data.ElementType == ElementType.SpreadWater) return;

                        if(element.Data.ElementType != ElementType.Normal &&!ElementSystem.Instance.IsSpecialElement(element.Data.ElementType)
                            && element.Data.ElementType != ElementType.Coin) {

                            if (xiongMaoFinish && element.Data.ConfigId == 130) {
                                continue;
                            }

                            _blockCoords.Add(coord);
                            break;
                        }
                    }
                }
            }
        }

        if (_blockCoords.Count == 0) return;
        UpdateLine();
        G.EventModule.DispatchEvent(GameEventDefine.OnMatchShowLastStepPrompt, EventOneParam<bool>.Create(true));
    }

    public void HideLastStepPrompt() {
        if (_effShowList.Count == 0) return;

        _blockCoords.Clear();
        RecycleEff();

        G.EventModule.DispatchEvent(GameEventDefine.OnMatchShowLastStepPrompt, EventOneParam<bool>.Create(false));
    }

    private void UpdateLine() {
        RecycleEff();

        if (_blockCoords.Count == 0) return;

        foreach (var coord in _blockCoords) {
            // 四个方向：上左右下
            var fourDirPos = MatchTweenUtil.GetNeighborPos(coord, false);
            for (int i = 0; i < fourDirPos.Count; i++) {
                var pos = fourDirPos[i];
                if (_blockCoords.Contains(pos)) continue;

                ShowLineEff(coord, i);
            }
            
        }
    }

    // direction方向 0：上 1：左 2：右 3：下
    private void ShowLineEff(Vector2Int blockCoord, int direction) {
        var obj = PopEffObj();

        var blockPos = GridSystem.GetGridPositionByCoord(blockCoord.x, blockCoord.y);

        switch (direction) {
            case 0:
                obj.transform.position = new Vector3(blockPos.x, blockPos.y - 0.4f, 0);
                obj.transform.localRotation = Quaternion.Euler(0, 0, 90);
                break;
            case 1:
                obj.transform.position = new Vector3(blockPos.x - 0.4f, blockPos.y, 0);
                obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
                break;
            case 2:
                obj.transform.position = new Vector3(blockPos.x + 0.4f, blockPos.y, 0);
                obj.transform.localRotation = Quaternion.Euler(0, 0, 180);
                break;
            case 3:
                obj.transform.position = new Vector3(blockPos.x, blockPos.y + 0.4f, 0);
                obj.transform.localRotation = Quaternion.Euler(0, 0, -90);
                break;
        }
    }

    private void RecycleEff() {
        foreach (var eff in _effShowList) {
            eff.SetActive(false);
            _effQueue.Enqueue(eff);
        }
        _effShowList.Clear();
    }

    private GameObject PopEffObj() {
        GameObject obj;

        if (_effQueue.Count > 0) {
            obj = _effQueue.Dequeue();
        } else {
            obj = Instantiate(_effLine, transform);
        }

        obj.SetActive(true);
        _effShowList.Add(obj);
        return obj;
    }
}
