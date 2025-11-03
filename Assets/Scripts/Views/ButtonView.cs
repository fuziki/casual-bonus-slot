using UnityEngine;
using R3;

namespace CasualBonusSlot.Views
{
    /// <summary>
    /// 3Dボタンオブジェクトを管理するView
    /// クリック/タップによるRaycast判定でボタン押下を検出
    /// </summary>
    public class ButtonView : MonoBehaviour
    {
        /// <summary>
        /// ボタン押下可能時のMaterial
        /// </summary>
        [SerializeField] private Material enabledMaterial;

        /// <summary>
        /// ボタン押下不可能時のMaterial
        /// </summary>
        [SerializeField] private Material disabledMaterial;

        /// <summary>
        /// MaterialをアタッチするMeshRenderer
        /// </summary>
        [SerializeField] private MeshRenderer meshRenderer;

        /// <summary>
        /// ボタン押下イベント（押下可能時のみ発火）
        /// </summary>
        private Subject<Unit> _onButtonPressed = new Subject<Unit>();
        public Observable<Unit> OnButtonPressed => _onButtonPressed;

        /// <summary>
        /// ボタン押下可能フラグ
        /// </summary>
        private bool _isEnabled = false;

        private Camera _mainCamera;
        private DisposableBag _disposables = new();

        private void Awake()
        {
            _mainCamera = Camera.main;

            // MeshRendererが未設定の場合は自動取得
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            // 初期状態は無効
            SetEnabled(false);

            // 入力検知を開始
            SetupInputDetection();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _onButtonPressed.Dispose();
        }

        /// <summary>
        /// ボタンの押下可能状態を設定
        /// </summary>
        /// <param name="enabled">true: 押下可能、false: 押下不可能</param>
        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;

            // Materialを切り替え
            if (meshRenderer != null)
            {
                meshRenderer.material = enabled ? enabledMaterial : disabledMaterial;
            }
        }

        /// <summary>
        /// 入力検知のセットアップ
        /// マウスクリック・タッチの両方に対応
        /// </summary>
        private void SetupInputDetection()
        {
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    // ボタンが無効な場合は何もしない
                    if (!_isEnabled)
                        return;

                    // マウスクリック検知（エディタ・PC用）
                    if (Input.GetMouseButtonDown(0))
                    {
                        CheckRaycastHit(Input.mousePosition);
                    }

                    // タッチ検知（モバイル用）
                    if (Input.touchCount > 0)
                    {
                        Touch touch = Input.GetTouch(0);
                        if (touch.phase == TouchPhase.Began)
                        {
                            CheckRaycastHit(touch.position);
                        }
                    }
                })
                .AddTo(ref _disposables);
        }

        /// <summary>
        /// スクリーン座標からRayを飛ばしてこのボタンとの交差判定
        /// </summary>
        /// <param name="screenPosition">スクリーン座標</param>
        private void CheckRaycastHit(Vector3 screenPosition)
        {
            if (_mainCamera == null)
                return;

            Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            // Raycastで当たり判定
            if (Physics.Raycast(ray, out hit))
            {
                // このボタンオブジェクトがヒットした場合
                if (hit.collider.gameObject == gameObject)
                {
                    _onButtonPressed.OnNext(Unit.Default);
                }
            }
        }
    }
}
