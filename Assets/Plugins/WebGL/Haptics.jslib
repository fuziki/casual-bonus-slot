mergeInto(LibraryManager.library, {
  // Unity C#側: [DllImport("__Internal")] private static extern void WebGLTriggerHaptic();
  WebGLTriggerHaptic: function() {
    try {
      // Vibration APIを優先使用 (Android等)
      if (navigator.vibrate) {
        navigator.vibrate(50);
        return;
      }

      // タッチデバイスかチェック
      if (!window.matchMedia("(pointer: coarse)").matches) {
        return;
      }

      var labelEl = document.createElement("label");
      labelEl.ariaHidden = "true";
      labelEl.style.display = "none";

      var inputEl = document.createElement("input");
      inputEl.type = "checkbox";
      inputEl.setAttribute("switch", "");
      labelEl.appendChild(inputEl);

      document.head.appendChild(labelEl);
      labelEl.click();
      document.head.removeChild(labelEl);
    } catch (e) {
      // エラーは無視（古いブラウザでも動作を継続）
    }
  }
});
