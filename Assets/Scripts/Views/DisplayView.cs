using UnityEngine;
using R3;
using TMPro;

namespace CasualBonusSlot.Views
{
    /// <summary>
    /// ディスプレイ（コイン数表示）のView
    /// </summary>
    public class DisplayView : MonoBehaviour
    {
        /// <summary>
        /// カウントアップ/ダウンにかける秒数（大きな差分用）
        /// </summary>
        private const float COUNT_ANIMATION_SECONDS = 1.0f;

        /// <summary>
        /// カウントアップ/ダウンにかける秒数（小さな差分用：15枚以下）
        /// </summary>
        private const float COUNT_ANIMATION_SECONDS_SHORT = 0.5f;

        [SerializeField] private TextMeshProUGUI coinText;

        private DisposableBag _disposables = new();
        private readonly SerialDisposable _countDisposable = new();
        private int _currentDisplayedCoins = 0;

        private void Awake()
        {
            _countDisposable.AddTo(ref _disposables);
            UpdateCoinText(_currentDisplayedCoins);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        /// <summary>
        /// コイン数を設定（カウントアップ/ダウンアニメーション付き）
        /// 連続呼び出しされた場合は前のアニメーションを中断して新しい値へ向かう
        /// </summary>
        /// <param name="targetCoins">目標コイン数</param>
        public void SetCoins(int targetCoins)
        {
            // 前のアニメーションを中断
            _countDisposable.Disposable = null;

            int startCoins = _currentDisplayedCoins;
            int diff = targetCoins - startCoins;

            // 差分が0なら何もしない
            if (diff == 0)
            {
                return;
            }

            // 差分が1枚なら即時反映
            if (Mathf.Abs(diff) == 1)
            {
                _currentDisplayedCoins = targetCoins;
                UpdateCoinText(targetCoins);
                return;
            }

            // アニメーション時間を差分に応じて決定
            float animationDuration = Mathf.Abs(diff) <= 15
                ? COUNT_ANIMATION_SECONDS_SHORT
                : COUNT_ANIMATION_SECONDS;

            // 指定秒数かけてカウントアップ/ダウン
            // 0.0f ~ 1.0fの範囲で線形補間
            _countDisposable.Disposable = Observable.EveryUpdate()
                .Select(_ => Time.deltaTime)
                .Scan(0f, (elapsed, delta) => elapsed + delta)
                .TakeWhile(elapsed => elapsed <= animationDuration)
                .Subscribe(elapsed =>
                {
                    float t = Mathf.Clamp01(elapsed / animationDuration);
                    int displayCoins = Mathf.RoundToInt(Mathf.Lerp(startCoins, targetCoins, t));
                    _currentDisplayedCoins = displayCoins;
                    UpdateCoinText(displayCoins);

                    // 最終フレームで目標値を確実に設定
                    if (elapsed >= animationDuration)
                    {
                        _currentDisplayedCoins = targetCoins;
                        UpdateCoinText(targetCoins);
                    }
                })
                .AddTo(ref _disposables);
        }

        /// <summary>
        /// Textコンポーネントを更新
        /// </summary>
        private void UpdateCoinText(int coins)
        {
            if (coinText != null)
            {
                coinText.text = $"COIN:{coins}";
            }
        }
    }
}
