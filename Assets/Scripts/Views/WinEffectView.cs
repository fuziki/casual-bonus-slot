using UnityEngine;
using R3;
using System;

namespace CasualBonusSlot.Views
{
    /// <summary>
    /// 当選演出を管理するView
    /// Bell、Cherry、Bonusなどの各役の演出を担当
    /// </summary>
    public class WinEffectView : MonoBehaviour
    {
        /// <summary>
        /// 演出開始までの待機時間（秒）
        /// </summary>
        private const float EFFECT_DELAY = 0.25f;

        /// <summary>
        /// Bellスケールアニメーションの時間（秒）
        /// </summary>
        private const float BELL_SCALE_DURATION = 0.3f;

        /// <summary>
        /// Bell演出の表示時間（秒）
        /// </summary>
        private const float BELL_DISPLAY_DURATION = 0.5f;

        /// <summary>
        /// Cherryスケールアニメーションの時間（秒）
        /// </summary>
        private const float CHERRY_SCALE_DURATION = 0.3f;

        /// <summary>
        /// Cherry演出の表示時間（秒）
        /// </summary>
        private const float CHERRY_DISPLAY_DURATION = 0.5f;

        /// <summary>
        /// Bonusスケールアニメーションの時間（秒）
        /// </summary>
        private const float BONUS_SCALE_DURATION = 0.5f;

        /// <summary>
        /// Bonus演出の表示時間（秒）
        /// </summary>
        private const float BONUS_DISPLAY_DURATION = 2.0f;

        [SerializeField] private GameObject bellEffectObject;
        [SerializeField] private GameObject cherryEffectObject;
        [SerializeField] private GameObject bonusEffectObject;

        private DisposableBag _disposables = new();
        private readonly SerialDisposable _effectDisposable = new();

        private void Awake()
        {
            _effectDisposable.AddTo(ref _disposables);

            // 初期状態では演出オブジェクトを非表示
            bellEffectObject.SetActive(false);
            cherryEffectObject.SetActive(false);
            bonusEffectObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        /// <summary>
        /// Bell当選時の演出を実行
        /// </summary>
        public void PlayBellEffect()
        {
            PlayScaleEffect(bellEffectObject, BELL_SCALE_DURATION, BELL_DISPLAY_DURATION);
        }

        /// <summary>
        /// Cherry当選時の演出を実行
        /// </summary>
        public void PlayCherryEffect()
        {
            PlayScaleEffect(cherryEffectObject, CHERRY_SCALE_DURATION, CHERRY_DISPLAY_DURATION);
        }

        /// <summary>
        /// Bonus当選時の演出を実行
        /// </summary>
        public void PlayBonusEffect()
        {
            PlayScaleEffect(bonusEffectObject, BONUS_SCALE_DURATION, BONUS_DISPLAY_DURATION);
        }

        /// <summary>
        /// スケールアニメーション付きの演出を実行
        /// スケール0から1までアニメーション→表示→非表示
        /// </summary>
        /// <param name="effectObject">表示する演出オブジェクト</param>
        /// <param name="scaleDuration">スケールアニメーション時間（秒）</param>
        /// <param name="displayDuration">表示時間（秒）</param>
        private void PlayScaleEffect(GameObject effectObject, float scaleDuration, float displayDuration)
        {
            // 前の演出を中断
            _effectDisposable.Disposable = null;

            // 待機時間後にアニメーション開始
            _effectDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(EFFECT_DELAY))
                .Subscribe(_ =>
                {
                    // オブジェクトを表示（初期スケールは0）
                    effectObject.transform.localScale = Vector3.zero;
                    effectObject.SetActive(true);

                    // スケールアニメーション（0 → 1）とその後の表示
                    float elapsedTime = 0f;
                    bool animationCompleted = false;

                    Observable.EveryUpdate()
                        .Subscribe(__ =>
                        {
                            if (!animationCompleted)
                            {
                                elapsedTime += Time.deltaTime;
                                if (elapsedTime < scaleDuration)
                                {
                                    float progress = Mathf.Clamp01(elapsedTime / scaleDuration);
                                    // イージング（SmoothStep）
                                    float scale = Mathf.SmoothStep(0f, 1f, progress);
                                    effectObject.transform.localScale = Vector3.one * scale;
                                }
                                else
                                {
                                    // アニメーション完了
                                    effectObject.transform.localScale = Vector3.one;
                                    animationCompleted = true;

                                    // 一定時間表示後に非表示
                                    Observable.Timer(TimeSpan.FromSeconds(displayDuration))
                                        .Subscribe(___ =>
                                        {
                                            effectObject.SetActive(false);
                                        })
                                        .AddTo(ref _disposables);
                                }
                            }
                        })
                        .AddTo(ref _disposables);
                })
                .AddTo(ref _disposables);
        }

    }
}
