using UnityEngine;
using System.Runtime.InteropServices;

namespace CasualBonusSlot.Views
{
    /// <summary>
    /// ハプティクスフィードバックのインターフェース
    /// </summary>
    public interface IHapticFeedback
    {
        void TriggerHaptic();
    }

    /// <summary>
    /// WebGL用のハプティクス実装
    /// </summary>
    public class WebGLHapticFeedback : IHapticFeedback
    {
        [DllImport("__Internal")]
        private static extern void WebGLTriggerHaptic();

        public void TriggerHaptic()
        {
            Debug.Log("Haptic triggered WebGL");
            WebGLTriggerHaptic();
        }
    }

    /// <summary>
    /// WebGL以外用の仮実装
    /// </summary>
    public class FallbackHapticFeedback : IHapticFeedback
    {
        public void TriggerHaptic()
        {
            Debug.Log("Haptic triggered (fallback)");
        }
    }

    /// <summary>
    /// ハプティクスビュー
    /// プラットフォームに応じた実装を自動選択
    /// </summary>
    public class HapticsView : MonoBehaviour
    {
        private IHapticFeedback _hapticFeedback;

        private void Awake()
        {
            _hapticFeedback = CreateHapticFeedback();
        }

        private IHapticFeedback CreateHapticFeedback()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            return new WebGLHapticFeedback();
            #else
            return new FallbackHapticFeedback();
            #endif
        }

        public void TriggerHaptic()
        {
            _hapticFeedback?.TriggerHaptic();
        }
    }
}
