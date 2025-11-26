using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GameCore.Extension;
using Hotfix.Utils;
using HotfixCore.Module;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HotfixCore.Extensions
{
    public static class UnityExtension
    {
        public static void SetVisible(this GameObject gameObject, bool visible)
        {
            if(gameObject == null)
                return;
            if(gameObject.transform == null)
                return;
            if (gameObject.activeSelf != visible)
            {
                gameObject.SetActive(visible);
            }
        }

        public static void SetVisible(this Component component, bool visible)
        {
            if(component == null)
                return;
            if(component.transform == null)
                return;
            if (component.gameObject.activeSelf != visible)
            {
                component.gameObject.SetActive(visible);
            }
        }

        public static T GetOrAddComponent<T>(this MonoBehaviour behaviour) where T : Component
        {
            T t = behaviour.GetComponent<T>();
            if (t == null)
            {
                t = behaviour.gameObject.AddComponent<T>();
            }

            return t;
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T t = gameObject.GetComponent<T>();
            if (t == null)
            {
                t = gameObject.AddComponent<T>();
            }

            return t;
        }

        public static T GetOrAddComponent<T>(this UIBehaviour behaviour) where T : Component
        {
            T t = behaviour.GetComponent<T>();
            if (t == null)
            {
                t = behaviour.gameObject.AddComponent<T>();
            }

            return t;
        }

        public static T GetChildComponent<T>(this Transform transform, string path) where T : Component
        {
            var findTrans = transform.Find(path);
            if (findTrans != null)
            {
                return findTrans.gameObject.GetComponent<T>();
            }

            return null;
        }

        public static T GetChildComponent<T>(this RectTransform transform, string path) where T : Component
        {
            var findTrans = transform.Find(path);
            if (findTrans != null)
            {
                return findTrans.gameObject.GetComponent<T>();
            }

            return null;
        }

        public static Transform FindChild(this Transform transform, string path)
        {
            var findTrans = transform.Find(path);
            return findTrans != null ? findTrans : null;
        }

        public static void SetText(this Button btn, string content)
        {
            var textCom = btn.GetComponentsInChildren<TextMeshProUGUI>();
            if (textCom.Length > 0)
            {
                textCom[0].text = content;
            }
        }

        public static void AddClick(this Button btn, UnityAction action)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                AudioUtil.PlayBtnClick();
                action?.Invoke();
            });
        }

        /// <summary>
        /// 播放UI动画，支持监听完成回调
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="animationName"></param>
        /// <param name="onComplete"></param>
        public static void PlayUIAnimation(this Animator animator, string animationName, Action<bool> onComplete = null)
        {
            var helper = animator.gameObject.GetOrAddComponent<UIAnimationHelper>();
            helper.PlayAnimation(animationName, onComplete);
        }

        /// <summary>
        /// 提前获取粒子系统播放时长
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public static float GetParticleSystemLength(this Component component)
        {
            return GetParticleSystemLength(component.gameObject);
        }

        /// <summary>
        /// 提前获取粒子系统播放时长
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static float GetParticleSystemLength(this GameObject gameObject)
        {
            var pts = gameObject.GetComponentsInChildren<ParticleSystem>();
            float maxDuration = float.MinValue;
            foreach (var pt in pts)
            {
                var duration = pt.GetDuration();
                if (duration > maxDuration)
                    maxDuration = duration;
            }
            return maxDuration;
        }

        public static float GetDuration(this ParticleSystem particleSystem)
        {
            if (particleSystem.emission.enabled == false) return 0.0f;

            if (particleSystem.main.loop) return -1.0f;

            if (particleSystem.emission.rateOverTime.GetMinValue() <= 0)
            {
                return particleSystem.main.startDelay.GetMaxValue() + particleSystem.main.startLifetime.GetMaxValue();
            }

            return particleSystem.main.startDelay.GetMaxValue() + Mathf.Max(particleSystem.main.duration,
                particleSystem.main.startLifetime.GetMaxValue());
        }

        public static float GetMaxValue(this ParticleSystem.MinMaxCurve minMaxCurve)
        {
            switch (minMaxCurve.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return minMaxCurve.constant;
                case ParticleSystemCurveMode.Curve:
                    return minMaxCurve.curve.GetMaxValue();
                case ParticleSystemCurveMode.TwoCurves:
                    var ret1 = minMaxCurve.curveMin.GetMaxValue();
                    var ret2 = minMaxCurve.curveMax.GetMaxValue();
                    return ret1 > ret2 ? ret1 : ret2;
                case ParticleSystemCurveMode.TwoConstants:
                    return minMaxCurve.constantMax;
            }

            return -1.0f;
        }

        public static float GetMinValue(this ParticleSystem.MinMaxCurve minMaxCurve)
        {
            switch (minMaxCurve.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return minMaxCurve.constant;
                case ParticleSystemCurveMode.Curve:
                    return minMaxCurve.curve.GetMinValue();
                case ParticleSystemCurveMode.TwoCurves:
                    var ret1 = minMaxCurve.curveMin.GetMinValue();
                    var ret2 = minMaxCurve.curveMax.GetMinValue();
                    return ret1 < ret2 ? ret1 : ret2;
                case ParticleSystemCurveMode.TwoConstants:
                    return minMaxCurve.constantMin;
            }

            return -1.0f;
        }
        
        public static float GetMaxValue(this AnimationCurve curve)
        {
            var ret = float.MinValue;
            var frames = curve.keys;
            for (int i = 0; i < frames.Length; i++)
            {
                var frame = frames[i];
                var value = frame.value;
                if (value > ret)
                    ret = value;
            }

            return ret;
        }
        
        public static float GetMinValue(this AnimationCurve curve)
        {
            var ret = float.MaxValue;
            var frames = curve.keys;
            for (int i = 0; i < frames.Length; i++)
            {
                var frame = frames[i];
                var value = frame.value;
                if (value < ret)
                    ret = value;
            }

            return ret;
        }

        public static void AddValueChangedListener(this TMP_InputField inputField, UnityAction<string> action)
        {
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.AddListener(action);
        }

        public static void AddEndEditListener(this TMP_InputField inputField, UnityAction<string> action)
        {
            inputField.onEndEdit.RemoveAllListeners();
            inputField.onEndEdit.AddListener(action);
        }

        public static void AddValueChangedListener(this TMP_Dropdown dropdown, UnityAction<int> action)
        {
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener(action);
        }

        public static void SetImageAlpha(this Image image, Sprite sprite, float alpha = 1.0f,bool setNativeSize = false)
        {
            if (image == null)
            {
                throw new Exception($"SetSprite failed. Because image is null.");
            }

            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
            image.sprite = sprite;
            image.DOColor(new Color(image.color.r, image.color.g, image.color.b, alpha), 0.2f).SetAutoKill(true);
            if (setNativeSize)
                image.SetNativeSize();
        }
        
        /// <summary>
        /// 设置图片。
        /// </summary>
        /// <param name="image"></param>
        /// <param name="location">资源定位地址。</param>
        /// <param name="setNativeSize">是否使用原始分辨率。</param>
        /// <param name="alpha">设置最终alpha值。</param>
        public static void SetSprite(this Image image, string location, bool setNativeSize = false)
        {
            if (image == null)
            {
                throw new Exception($"SetSprite failed. Because image is null.");
            }

            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
            ResourceHandler.Instance
                .SetAssetByResources<Sprite>(SetSpriteObject.Create(image, location.ToLower(), setNativeSize))
                .Forget();
        }

        /// <summary>
        /// 设置图片。
        /// </summary>
        /// <param name="spriteRenderer">2D/SpriteRender。</param>
        /// <param name="location">资源定位地址。</param>
        /// <param name="alpha">设置最终alpha值。</param>
        public static void SetSprite(this SpriteRenderer spriteRenderer, string location)
        {
            ResourceHandler.Instance
                .SetAssetByResources<Sprite>(SetSpriteObject.Create(spriteRenderer, location.ToLower()))
                .Forget();
        }

        public static void SetMaterial(this Image image, string location, string packageName = "")
        {
            if (image == null)
            {
                throw new Exception($"SetSprite failed. Because image is null.");
            }

            G.ResourceModule.LoadAssetAsync<Material>(location, material =>
            {
                if (image == null || image.gameObject == null)
                {
                    G.ResourceModule.UnloadAsset(material);
                    material = null;
                    return;
                }

                image.material = material;
                AssetsReference.Ref(material, image.gameObject);
            }, packageName).Forget();
        }

        public static void SetMaterial(this SpriteRenderer spriteRenderer, string location, string packageName = "")
        {
            if (spriteRenderer == null)
            {
                throw new Exception($"SetSprite failed. Because image is null.");
            }

            G.ResourceModule.LoadAssetAsync<Material>(location, material =>
            {
                if (spriteRenderer == null || spriteRenderer.gameObject == null)
                {
                    G.ResourceModule.UnloadAsset(material);
                    material = null;
                    return;
                }

                spriteRenderer.material = material;
                AssetsReference.Ref(material, spriteRenderer.gameObject);
            }, packageName).Forget();
        }

        public static void SetGray(this Component component, bool isGray)
        {
            SetGray(component.gameObject, isGray);
        }

        public static void SetGray(this GameObject gameObject, bool isGray)
        {
            Graphic[] graphics = gameObject.GetComponentsInChildren<Graphic>(false);
            if(graphics == null || graphics.Length <= 0)
                return;

            if (isGray)
            {
                string grayLocation = "material/greyimage";
                G.ResourceModule.LoadAssetAsync<Material>(grayLocation, material =>
                {
                    foreach (var graphic in graphics)
                    {
                        graphic.material = material;
                    }

                    AssetsReference.Ref(material, gameObject);
                }, "").Forget();
            }
            else
            {
                foreach (var graphic in graphics)
                {
                    graphic.material = null;
                }
            }
        }

        public static void SetMaterial(this MeshRenderer meshRenderer, string location, bool needInstance = true,
            string packageName = "")
        {
            if (meshRenderer == null)
            {
                throw new Exception($"SetSprite failed. Because image is null.");
            }

            G.ResourceModule.LoadAssetAsync<Material>(location, material =>
            {
                if (meshRenderer == null || meshRenderer.gameObject == null)
                {
                    G.ResourceModule.UnloadAsset(material);
                    material = null;
                    return;
                }

                meshRenderer.material = needInstance ? Object.Instantiate(material) : material;
                AssetsReference.Ref(material, meshRenderer.gameObject);
            }, packageName).Forget();
        }

        public static void SetSharedMaterial(this MeshRenderer meshRenderer, string location, string packageName = "")
        {
            if (meshRenderer == null)
            {
                throw new Exception($"SetSprite failed. Because image is null.");
            }


            G.ResourceModule.LoadAssetAsync<Material>(location, material =>
            {
                if (meshRenderer == null || meshRenderer.gameObject == null)
                {
                    G.ResourceModule.UnloadAsset(material);
                    material = null;
                    return;
                }

                meshRenderer.sharedMaterial = material;
                AssetsReference.Ref(material, meshRenderer.gameObject);
            }, packageName).Forget();
        }
    }
}