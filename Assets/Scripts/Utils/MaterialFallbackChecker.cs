using UnityEngine;
using System.Collections.Generic;

namespace CasualBonusSlot
{
    /// <summary>
    /// Materialのベースマップが未設定の場合に、指定されたフォールバック画像に自動的に差し替える
    /// OSS公開時にPrivate画像をPublic画像に置き換えるために使用
    /// </summary>
    public class MaterialFallbackChecker : MonoBehaviour
    {
        [System.Serializable]
        public class MaterialFallbackPair
        {
            [Tooltip("Target material to check")]
            public Material targetMaterial;

            [Tooltip("Fallback texture to use when Base Map is not set")]
            public Texture2D fallbackTexture;
        }

        [Header("Material Fallback Settings")]
        [Tooltip("Pairs of target materials and their fallback textures")]
        [SerializeField] private List<MaterialFallbackPair> materialFallbackPairs = new List<MaterialFallbackPair>();

        private void Awake()
        {
            CheckAndApplyFallback();
        }

        /// <summary>
        /// MaterialのBase Mapをチェックし、未設定の場合はフォールバックテクスチャを適用
        /// </summary>
        public void CheckAndApplyFallback()
        {
            int replacedCount = 0;

            foreach (var pair in materialFallbackPairs)
            {
                if (pair.targetMaterial.HasProperty("_BaseMap"))
                {
                    var baseMap = pair.targetMaterial.GetTexture("_BaseMap");
                    if (baseMap == null)
                    {
                        pair.targetMaterial.SetTexture("_BaseMap", pair.fallbackTexture);
                        Debug.Log($"[MaterialFallbackChecker] Applied fallback to material '{pair.targetMaterial.name}'");
                        replacedCount++;
                    }
                }
            }

            if (replacedCount > 0)
            {
                Debug.Log($"[MaterialFallbackChecker] Completed. Applied fallback texture to {replacedCount} material(s).");
            }
        }
    }
}
