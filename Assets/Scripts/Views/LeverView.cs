using UnityEngine;
using R3;
using System.Collections;

namespace CasualBonusSlot.Views
{
    /// <summary>
    /// レバー（画面タップ）を管理するView
    /// 画面のどこかをクリック/タップでレバー操作を検知
    /// </summary>
    public class LeverView : MonoBehaviour
    {
        /// <summary>
        /// インジケーター表示までの遅延時間（秒）
        /// </summary>
        private const float INDICATOR_DISPLAY_DELAY = 1.0f;

        /// <summary>
        /// タップと判定する最大時間（秒）
        /// </summary>
        private const float MAX_TAP_DURATION = 0.3f;

        /// <summary>
        /// タップと判定する最大移動距離（ピクセル）
        /// </summary>
        private const float MAX_TAP_DISTANCE = 50f;

        /// <summary>
        /// レバー開始可能時に表示するGameObject
        /// </summary>
        [SerializeField] private GameObject leverIndicatorObject;

        /// <summary>
        /// レバー操作イベント（開始可能時のみ発火）
        /// </summary>
        private Subject<Unit> _onLeverPulled = new Subject<Unit>();
        public Observable<Unit> OnLeverPulled => _onLeverPulled;

        /// <summary>
        /// レバー開始可能フラグ
        /// </summary>
        private bool _isEnabled = false;

        /// <summary>
        /// タッチ開始時の情報
        /// </summary>
        private Vector2 _touchStartPosition;
        private float _touchStartTime;

        /// <summary>
        /// マウス開始時の情報（PC/エディタ用）
        /// </summary>
        private Vector2 _mouseStartPosition;
        private float _mouseStartTime;
        private bool _isMouseDown = false;

        private Coroutine _enableCoroutine;

        private void Awake()
        {
            // 初期状態は無効
            _isEnabled = false;
            leverIndicatorObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _onLeverPulled?.Dispose();

            if (_enableCoroutine != null)
            {
                StopCoroutine(_enableCoroutine);
                _enableCoroutine = null;
            }
        }

        /// <summary>
        /// レバーの開始可能状態を設定
        /// </summary>
        /// <param name="enabled">true: 開始可能、false: 開始不可能</param>
        public void SetEnabled(bool enabled)
        {
            // 前回のコルーチンを停止
            if (_enableCoroutine != null)
            {
                StopCoroutine(_enableCoroutine);
                _enableCoroutine = null;
            }

            // インジケーターを即座に非表示
            if (leverIndicatorObject != null)
            {
                leverIndicatorObject.SetActive(false);
            }

            if (!enabled)
            {
                _isEnabled = false;
                return;
            }

            // 有効化処理
            _isEnabled = false; // 一旦無効化（次フレームまで入力を受け付けない）
            _enableCoroutine = StartCoroutine(EnableWithDelayCoroutine());
        }

        /// <summary>
        /// 遅延してレバーを有効化するコルーチン
        /// </summary>
        private IEnumerator EnableWithDelayCoroutine()
        {
            // 1フレーム待機（WebGL対策：WaitForEndOfFrameを使用）
            yield return new WaitForEndOfFrame();

            // レバーを有効化
            _isEnabled = true;

            // インジケーター表示までの遅延
            yield return new WaitForSeconds(INDICATOR_DISPLAY_DELAY);

            // まだ有効な場合のみインジケーターを表示
            if (_isEnabled && leverIndicatorObject != null)
            {
                leverIndicatorObject.SetActive(true);
            }
        }

        /// <summary>
        /// 入力検知（Update）
        /// 画面のどこかをクリック/タップで検知
        /// </summary>
        private void Update()
        {
            // レバーが無効な場合は何もしない
            if (!_isEnabled)
                return;

            // タッチ検知（モバイル用）
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                // タッチ開始時：位置と時間を記録
                if (touch.phase == TouchPhase.Began)
                {
                    _touchStartPosition = touch.position;
                    _touchStartTime = Time.time;
                }
                // タッチ終了時：タップ判定
                else if (touch.phase == TouchPhase.Ended)
                {
                    float touchDuration = Time.time - _touchStartTime;
                    float touchDistance = Vector2.Distance(touch.position, _touchStartPosition);

                    // タップと判定する条件：時間が短く、移動距離が小さい
                    if (touchDuration <= MAX_TAP_DURATION && touchDistance <= MAX_TAP_DISTANCE)
                    {
                        _onLeverPulled.OnNext(Unit.Default);
                    }
                }
            }

            // マウスクリック検知（エディタ・PC用）
            // マウスボタン押下時：位置と時間を記録
            if (Input.GetMouseButtonDown(0))
            {
                _mouseStartPosition = Input.mousePosition;
                _mouseStartTime = Time.time;
                _isMouseDown = true;
            }
            // マウスボタン解放時：クリック判定
            else if (Input.GetMouseButtonUp(0) && _isMouseDown)
            {
                _isMouseDown = false;

                float clickDuration = Time.time - _mouseStartTime;
                float clickDistance = Vector2.Distance(Input.mousePosition, _mouseStartPosition);

                // クリックと判定する条件：時間が短く、移動距離が小さい
                if (clickDuration <= MAX_TAP_DURATION && clickDistance <= MAX_TAP_DISTANCE)
                {
                    _onLeverPulled.OnNext(Unit.Default);
                }
            }
        }
    }
}
