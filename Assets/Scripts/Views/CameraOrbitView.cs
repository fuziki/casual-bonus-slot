using UnityEngine;
using R3;

namespace CasualBonusSlot.Views
{
    /// <summary>
    /// スワイプ/ドラッグでカメラを軌道回転させるView
    /// ターゲット座標を注視しながら一定距離を保って上下左右に回転
    /// </summary>
    public class CameraOrbitView : MonoBehaviour
    {
        /// <summary>
        /// 注視するターゲットのTransform
        /// </summary>
        [SerializeField] private Transform targetTransform;

        /// <summary>
        /// 回転させるカメラ
        /// </summary>
        [SerializeField] private Camera orbitCamera;

        /// <summary>
        /// 垂直方向の回転可能な角度の上限（degree）
        /// </summary>
        [SerializeField] private float maxVerticalAngle = 20f;

        /// <summary>
        /// 水平方向の回転可能な角度の上限（degree）
        /// </summary>
        [SerializeField] private float maxHorizontalAngle = 30f;

        /// <summary>
        /// スワイプ時の回転速度
        /// </summary>
        [SerializeField] private float rotationSpeed = 0.2f;

        // 初期状態
        private Vector3 _initialTargetPosition;
        private float _initialDistance;
        private float _initialHorizontalAngle;
        private float _initialVerticalAngle;
        private float _currentHorizontalAngle;
        private float _currentVerticalAngle;

        // 入力検知
        private Vector2 _lastInputPosition;
        private bool _isDragging;

        private void Awake()
        {
            // 初期状態を記録
            _initialTargetPosition = targetTransform.position;
            _initialDistance = Vector3.Distance(orbitCamera.transform.position, _initialTargetPosition);

            // Awake時のカメラ位置から初期角度を計算（これを0度基準とする）
            Vector3 direction = orbitCamera.transform.position - _initialTargetPosition;
            _initialHorizontalAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            _initialVerticalAngle = Mathf.Asin(direction.y / _initialDistance) * Mathf.Rad2Deg;

            // 現在角度を初期角度に設定
            _currentHorizontalAngle = _initialHorizontalAngle;
            _currentVerticalAngle = _initialVerticalAngle;
        }

        private void Update()
        {
            HandleInput();
            UpdateCameraPosition();
        }

        /// <summary>
        /// 入力処理（マウス・タッチの両方に対応）
        /// </summary>
        private void HandleInput()
        {
            // マウス入力
            if (Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _lastInputPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }

            // タッチ入力
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    _isDragging = true;
                    _lastInputPosition = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    _isDragging = false;
                }
            }

            // ドラッグ中の回転計算
            if (_isDragging)
            {
                Vector2 currentInputPosition;

                if (Input.touchCount > 0)
                {
                    currentInputPosition = Input.GetTouch(0).position;
                }
                else
                {
                    currentInputPosition = Input.mousePosition;
                }

                Vector2 delta = currentInputPosition - _lastInputPosition;

                // 角度を更新
                _currentHorizontalAngle += delta.x * rotationSpeed;
                _currentVerticalAngle -= delta.y * rotationSpeed;

                // 初期角度を基準として水平・垂直角度を制限
                _currentHorizontalAngle = Mathf.Clamp(_currentHorizontalAngle,
                    _initialHorizontalAngle - maxHorizontalAngle,
                    _initialHorizontalAngle + maxHorizontalAngle);
                _currentVerticalAngle = Mathf.Clamp(_currentVerticalAngle,
                    _initialVerticalAngle - maxVerticalAngle,
                    _initialVerticalAngle + maxVerticalAngle);

                _lastInputPosition = currentInputPosition;
            }
        }

        /// <summary>
        /// カメラ位置を更新
        /// 球面座標系でターゲットを中心に回転
        /// </summary>
        private void UpdateCameraPosition()
        {
            // 角度をラジアンに変換
            float horizontalRad = _currentHorizontalAngle * Mathf.Deg2Rad;
            float verticalRad = _currentVerticalAngle * Mathf.Deg2Rad;

            // ターゲット座標を中心に球面座標から直交座標に変換
            Vector3 targetPosition = targetTransform.position;

            float horizontalDistance = _initialDistance * Mathf.Cos(verticalRad);
            float x = horizontalDistance * Mathf.Sin(horizontalRad);
            float y = _initialDistance * Mathf.Sin(verticalRad);
            float z = horizontalDistance * Mathf.Cos(horizontalRad);

            orbitCamera.transform.position = targetPosition + new Vector3(x, y, z);

            // カメラをターゲットに向ける
            orbitCamera.transform.LookAt(targetPosition);
        }
    }
}
