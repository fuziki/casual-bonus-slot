using UnityEngine;
using R3;
using CasualBonusSlot.Models;

namespace CasualBonusSlot.Views
{
    /// <summary>
    /// リールの視覚表現を管理するView
    /// MaterialのUVオフセットを変更してリール回転を表現
    /// </summary>
    public class ReelView : MonoBehaviour
    {
        // スクロール速度（オフセット/秒）
        private const float SCROLL_SPEED = 5.0f;

        // オフセットの最大値（1周でループ）
        private const float MAX_OFFSET = 1.0f;

        [SerializeField] private Material targetMaterial;

        [Header("リール図柄の順番設定")]
        [SerializeField] private ReelSymbol[] symbolOrder;

        /// <summary>
        /// 設定されているシンボル順序を取得
        /// </summary>
        public ReelSymbol[] SymbolOrder => symbolOrder;

        private DisposableBag _disposables = new();
        private readonly SerialDisposable _spinDisposable = new();
        private bool _isSpinning;
        private bool _isStopping;
        private float _currentOffset;
        private float _targetStopOffset;

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private void Awake()
        {
            _spinDisposable.AddTo(ref _disposables);
        }

        /// <summary>
        /// 現在位置から最も近い次の指定図柄のオフセット値を取得
        /// </summary>
        private float GetNextOffsetForSymbol(ReelSymbol symbol, float currentOffset)
        {
            if (symbolOrder == null || symbolOrder.Length == 0)
            {
                Debug.LogError("symbolOrder is not set!");
                return 0.0f;
            }

            // シンボル数から1つあたりのオフセット距離を計算
            float offsetStep = MAX_OFFSET / symbolOrder.Length;

            // 現在のオフセットを0~1に正規化
            float normalizedCurrent = currentOffset % MAX_OFFSET;

            // 最も近い次の指定図柄を探す
            float minDistance = float.MaxValue;
            float targetOffset = 0.0f;

            for (int i = 0; i < symbolOrder.Length; i++)
            {
                if (symbolOrder[i] == symbol)
                {
                    float offset = offsetStep * i;
                    float distance = offset - normalizedCurrent;

                    // 前進方向のみを考慮（距離が負の場合は1周後）
                    if (distance <= 0)
                    {
                        distance += MAX_OFFSET;
                    }

                    // 最も近い図柄を選択
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        targetOffset = offset;
                    }
                }
            }

            if (minDistance == float.MaxValue)
            {
                Debug.LogError($"Symbol {symbol} not found in symbolOrder!");
                return 0.0f;
            }

            return targetOffset;
        }

        /// <summary>
        /// リールの回転を開始
        /// </summary>
        public void StartSpin()
        {
            if (_isSpinning) return;

            _isSpinning = true;
            _isStopping = false;

            // EveryUpdateでフレーム毎にオフセットを更新
            _spinDisposable.Disposable = Observable.EveryUpdate()
                .Subscribe(_ => UpdateScroll())
                .AddTo(ref _disposables);
        }

        /// <summary>
        /// リールを停止し、指定された図柄まで滑って停止
        /// </summary>
        /// <param name="symbol">停止させる図柄</param>
        public void Stop(ReelSymbol symbol)
        {
            if (!_isSpinning || _isStopping) return;

            _isStopping = true;

            // 現在位置から最も近い次の指定図柄のオフセットを取得
            float targetOffset = GetNextOffsetForSymbol(symbol, _currentOffset);

            // 現在のオフセットから目標オフセットまでの距離を計算
            float normalizedCurrent = _currentOffset % MAX_OFFSET;
            float distance = targetOffset - normalizedCurrent;
            if (distance <= 0)
            {
                distance += MAX_OFFSET;
            }

            _targetStopOffset = _currentOffset + distance;
        }

        /// <summary>
        /// スクロール更新処理
        /// </summary>
        private void UpdateScroll()
        {
            if (_isStopping)
            {
                // 停止中：目標オフセットまでスクロール
                if (_currentOffset < _targetStopOffset)
                {
                    _currentOffset += SCROLL_SPEED * Time.deltaTime;

                    // 目標を超えた場合は正確に設定
                    if (_currentOffset >= _targetStopOffset)
                    {
                        _currentOffset = _targetStopOffset % MAX_OFFSET;
                        targetMaterial.mainTextureOffset = new Vector2(0, _currentOffset);

                        // 停止完了
                        _isSpinning = false;
                        _isStopping = false;
                        _spinDisposable.Disposable = null;
                        return;
                    }
                }

                // マテリアルのオフセットを更新（0~1に丸める）
                targetMaterial.mainTextureOffset = new Vector2(0, _currentOffset % MAX_OFFSET);
            }
            else
            {
                // 通常回転中：無限にスクロール
                _currentOffset += SCROLL_SPEED * Time.deltaTime;
                _currentOffset %= MAX_OFFSET;

                // マテリアルのオフセットを更新
                targetMaterial.mainTextureOffset = new Vector2(0, _currentOffset);
            }
        }
    }
}
