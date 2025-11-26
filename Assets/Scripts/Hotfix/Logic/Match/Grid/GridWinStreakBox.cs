using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Hotfix.Utils;
using Spine;
using Spine.Unity;
using UnityEngine;

public class GridWinStreakBox : MonoBehaviour
{
    [SerializeField] private SkeletonAnimation _bottomBoxAnim;
    [SerializeField] private SkeletonAnimation _topBoxAnim;
    [SerializeField] private GameObject _flyObj;
    [SerializeField] private GameObject _boxLight;
    [SerializeField] private GameObject _rocketVertical;
    [SerializeField] private GameObject _rocketHorizontal;
    [SerializeField] private GameObject _bomb;
    [SerializeField] private GameObject _colorBall;
    [SerializeField] private GameObject _boxTrail;
    

    // 位置分布
    private  List<List<Vector3>> _posList = new List<List<Vector3>>();

    const string AnimNameIdle1 = "idle";
    const string AnimNameIdle2 = "idle2";
    const string AnimNameChuchang = "chuchang";
    const string AnimNameRuchang1 = "ruchang";
    const string AnimNameRuchang2 = "ruchang2";

    private List<GameObject> _flyList = new List<GameObject>();
    private List<Vector3> _flyPosList = new List<Vector3>();
    private List<Vector2Int> _flyCoordList = new List<Vector2Int>();
    private List<int> _specialIdList = new List<int>();
    private Action<int, Vector2Int, bool> _flyCallback;


    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);

        _posList.Add(new List<Vector3>{new Vector3(0, -0.06f, 0)});
        _posList.Add(new List<Vector3>{new Vector3(-0.351f, -0.056f, 0), new Vector3(0.323f, -0.056f, 0)});
        _posList.Add(new List<Vector3>{new Vector3(-0.45f, -0.07f, 0), new Vector3(0, -0.06f, -1), new Vector3(0.51f, -0.07f, 0)});

        _bottomBoxAnim.AnimationState.Complete += OnBottomSpineAnimComplete;
        _topBoxAnim.AnimationState.Complete += OnTopSpineAnimComplete;

        _flyObj.SetActive(false);
        _rocketVertical.SetActive(false);
        _rocketHorizontal.SetActive(false);
        _bomb.SetActive(false);
        _colorBall.SetActive(false);
        _boxLight.SetActive(false);
        _boxTrail.SetActive(false);

        _bottomBoxAnim.gameObject.SetActive(false);
        _topBoxAnim.gameObject.SetActive(false);
    }

    private void OnTopSpineAnimComplete(TrackEntry trackEntry) {
        if (trackEntry.Animation.Name == AnimNameRuchang2) {
            _topBoxAnim.AnimationState.SetAnimation(0, AnimNameIdle2, true);
        }
    }

    private void OnBottomSpineAnimComplete(TrackEntry trackEntry) {
        if (trackEntry.Animation.Name == AnimNameRuchang1) {
            PlayFlyItem();
            _bottomBoxAnim.AnimationState.SetAnimation(0, AnimNameIdle1, true);

            _boxLight.SetActive(true);
        } else if (trackEntry.Animation.Name == AnimNameChuchang) {
            _bottomBoxAnim.gameObject.SetActive(false);
        }
    }

    public void SetWinStreakItem(List<int> itemList, List<Vector3> posList, List<Vector2Int> coordList, Action<int, Vector2Int, bool> callback) {
        if (itemList.Count == 0) return;

        _boxTrail.SetActive(false);

        PlayRuchang();

        _flyCallback = callback;

        for (int i = 0; i < itemList.Count; i++) {
            var specialID = itemList[i];
            GameObject obj = Instantiate(_flyObj, transform);

            var spriteObj = obj.transform.Find("sprite");
            var spriteRenderer = spriteObj.GetComponent<SpriteRenderer>();

            if (specialID == 8) {
                // 横向火箭
                spriteRenderer.sprite = _rocketHorizontal.GetComponent<SpriteRenderer>().sprite;
            } else if (specialID == 9) {
                // 炸弹
                // obj = Instantiate(_bomb, transform);
                spriteRenderer.sprite = _bomb.GetComponent<SpriteRenderer>().sprite;
            } else if (specialID == 10) {
                // 彩球
                // obj = Instantiate(_colorBall, transform);
                spriteRenderer.sprite = _colorBall.GetComponent<SpriteRenderer>().sprite;
            } else if (specialID == 11) {
                // 纵向火箭
                // obj = Instantiate(_rocketVertical, transform);
                spriteRenderer.sprite = _rocketVertical.GetComponent<SpriteRenderer>().sprite;
            }

            if (obj!= null) {
                _specialIdList.Add(specialID);
                _flyList.Add(obj);
                _flyPosList.Add(posList[i]);
                _flyCoordList.Add(coordList[i]);
            }
        }

        DOVirtual.DelayedCall(0.5f, () => {
            var itemPosList = _posList[0];
            if (_flyList.Count == 2) {
                itemPosList = _posList[1];
            } else if (_flyList.Count >= 3) {
                itemPosList = _posList[2];
            }
            for (int i = 0; i < _flyList.Count; i++) {
                var obj = _flyList[i];
                var pos = itemPosList[i % itemPosList.Count];
                obj.transform.localPosition = pos;
                obj.SetActive(true);
            }
        });
    }

    private void PlayRuchang() {
        _topBoxAnim.gameObject.SetActive(true);
        _bottomBoxAnim.gameObject.SetActive(true);

        _bottomBoxAnim.AnimationState.SetAnimation(0, AnimNameRuchang1, false);
        _topBoxAnim.AnimationState.SetAnimation(0, AnimNameRuchang2, false);

        AudioUtil.PlaySound("audio/match/match_winstreak_enter");

        _boxTrail.SetActive(true);
    }

    private void PlayChuchang() {
        _topBoxAnim.gameObject.SetActive(false);
        _topBoxAnim.AnimationState.SetAnimation(0, AnimNameIdle2, false);

        _bottomBoxAnim.gameObject.SetActive(true);
        _bottomBoxAnim.AnimationState.SetAnimation(0, AnimNameChuchang, false);

        _boxLight.SetActive(false);
    }

    private void PlayFlyItem() {
        GameObject lastObj = null;
        for (int i = 0; i < _flyList.Count; i++) {
            var curIdx = i;
            var flyPos = _flyPosList[i];
            var obj = _flyList[i];

            var spriteObj = obj.transform.Find("sprite").gameObject;
            var trailObj = obj.transform.Find("spriteTrail").gameObject;
            var bombObj = obj.transform.Find("spriteBomb").gameObject;
            bombObj.SetActive(false);
            


            lastObj = obj;

            var xMinus = (obj.transform.position.x - flyPos.x) * 0.5f;
            Vector3 offsetDir = new Vector3(xMinus, 0.5f, 0);

            var pathPos = flyPos + offsetDir;
            Vector3[] path = new Vector3[] { pathPos, flyPos };

            var seq = DOTween.Sequence();
            seq.AppendInterval(i * 0.2f);
            seq.AppendCallback(() => {
                AudioUtil.PlaySound("audio/match/match_winstreak_booster");
            });
            seq.Append(obj.transform.DOPath(path, 0.4f, PathType.CatmullRom).SetEase(Ease.Linear));
            seq.AppendCallback(() => {
                spriteObj.SetActive(false);
                bombObj.SetActive(true);
                _flyCallback?.Invoke(_specialIdList[curIdx], _flyCoordList[curIdx], obj == lastObj);
                if (obj == lastObj) {
                    PlayChuchang();
                }
            });
        }
    }
}
