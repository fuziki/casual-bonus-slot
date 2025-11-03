using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextMeshProBackground : MonoBehaviour
{
    [SerializeField] private Color backgroundColor = Color.white;
    [SerializeField] private Vector2 padding = new Vector2(10f, 5f);
    [SerializeField] private bool autoUpdate = true;

    private TextMeshProUGUI textMesh;
    private RectTransform textRectTransform;
    private GameObject backgroundObject;
    private Image backgroundImage;
    private RectTransform backgroundRectTransform;

    private string lastText;
    private Vector2 lastTextSize;
    private Color lastBackgroundColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        textRectTransform = textMesh.rectTransform;

        CreateBackground();
    }

    void Start()
    {
        UpdateBackground();
    }

    void Update()
    {
        if (autoUpdate)
        {
            CheckForChanges();
        }
    }

    /// <summary>
    /// 背景オブジェクトを作成
    /// </summary>
    private void CreateBackground()
    {
        // 背景用のGameObjectを作成
        backgroundObject = new GameObject("TextBackground");
        backgroundObject.transform.SetParent(transform.parent, false);

        // TextMeshProの直前に配置（背景として表示するため）
        int textIndex = transform.GetSiblingIndex();
        backgroundObject.transform.SetSiblingIndex(textIndex);

        // RectTransformを追加
        backgroundRectTransform = backgroundObject.AddComponent<RectTransform>();

        // Imageコンポーネントを追加
        backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.color = backgroundColor;

        // 初期設定
        SetupBackgroundTransform();
    }

    /// <summary>
    /// 背景のTransformを設定
    /// </summary>
    private void SetupBackgroundTransform()
    {
        // TextMeshProと同じ位置に配置
        backgroundRectTransform.anchorMin = textRectTransform.anchorMin;
        backgroundRectTransform.anchorMax = textRectTransform.anchorMax;
        backgroundRectTransform.anchoredPosition = textRectTransform.anchoredPosition;
        backgroundRectTransform.pivot = textRectTransform.pivot;
    }

    /// <summary>
    /// 変更をチェックして必要に応じて更新
    /// </summary>
    private void CheckForChanges()
    {
        bool needsUpdate = false;

        // テキストの変更をチェック
        if (lastText != textMesh.text)
        {
            lastText = textMesh.text;
            needsUpdate = true;
        }

        // 背景色の変更をチェック
        if (lastBackgroundColor != backgroundColor)
        {
            lastBackgroundColor = backgroundColor;
            backgroundImage.color = backgroundColor;
            needsUpdate = true;
        }

        // テキストサイズの変更をチェック
        Vector2 currentTextSize = textMesh.GetRenderedValues(false);
        if (lastTextSize != currentTextSize)
        {
            lastTextSize = currentTextSize;
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            UpdateBackground();
        }
    }

    /// <summary>
    /// 背景を更新
    /// </summary>
    public void UpdateBackground()
    {
        if (backgroundRectTransform == null || textMesh == null)
            return;

        // テキストの描画サイズを取得
        Vector2 textSize = textMesh.GetRenderedValues(false);

        // パディングを含めたサイズを計算
        Vector2 backgroundSize = textSize + padding * 2f;

        // 背景のサイズを設定
        backgroundRectTransform.sizeDelta = backgroundSize;

        // 位置を再設定（テキストの中央に配置）
        SetupBackgroundTransform();
    }

    /// <summary>
    /// 背景色を設定
    /// </summary>
    /// <param name="color">設定する色</param>
    public void SetBackgroundColor(Color color)
    {
        backgroundColor = color;
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
    }

    /// <summary>
    /// パディングを設定
    /// </summary>
    /// <param name="newPadding">設定するパディング</param>
    public void SetPadding(Vector2 newPadding)
    {
        padding = newPadding;
        UpdateBackground();
    }

    /// <summary>
    /// 自動更新の有効/無効を設定
    /// </summary>
    /// <param name="enabled">自動更新を有効にするか</param>
    public void SetAutoUpdate(bool enabled)
    {
        autoUpdate = enabled;
    }

    /// <summary>
    /// 手動で背景を更新
    /// </summary>
    public void ForceUpdate()
    {
        UpdateBackground();
    }

    void OnDestroy()
    {
        // 背景オブジェクトを削除
        if (backgroundObject != null)
        {
            if (Application.isPlaying)
            {
                Destroy(backgroundObject);
            }
            else
            {
                DestroyImmediate(backgroundObject);
            }
        }
    }

    void OnValidate()
    {
        // インスペクターで値が変更された時に背景を更新
        if (Application.isPlaying && backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
            UpdateBackground();
        }
    }
}
