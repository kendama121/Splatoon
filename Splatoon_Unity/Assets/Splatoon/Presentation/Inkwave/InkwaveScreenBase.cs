using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TextCore.Text;

namespace Splatoon.Presentation.Inkwave
{
    /// <summary>
    /// INKWAVE 各画面の共通基底クラス。UIDocument参照+共通的なrootアクセス+OnShowライフサイクル。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public abstract class InkwaveScreenBase : MonoBehaviour
    {
        /// <summary>UIDocument(同GameObject上)</summary>
        protected UIDocument _doc;
        /// <summary>ルートVisualElement</summary>
        protected VisualElement _root;

        /// <summary>日本語フォントアセット(全画面共通でResourcesからロード、static)。</summary>
        static FontAsset _jpFont;

        /// <summary>派生クラス用Awake処理(BuildUI等を呼ぶ)</summary>
        protected virtual void Awake()
        {
            _doc = GetComponent<UIDocument>();
            if (_jpFont == null) _jpFont = Resources.Load<FontAsset>("NotoSansJP_SDF");
        }

        /// <summary>画面表示開始時刻(unscaledTime)。Update内で入力受付禁止期間判定用。</summary>
        protected float _enabledTime;

        /// <summary>表示直後 入力無効化時間(秒)。誤入力(キーカーソル残留)防止。</summary>
        protected const float InputLockDuration = 0.3f;

        /// <summary>OnEnableで _root取得+派生クラスのBindUI呼出+全Labelに日本語フォント強制適用+カーソルデフォルト解放</summary>
        protected virtual void OnEnable()
        {
            _enabledTime = Time.unscaledTime;
            // デフォルト: カーソル表示(メニュー類で操作可能)。HUD等は BindUI 内で上書き。
            UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            UnityEngine.Time.timeScale = 1f;

            if (_doc != null) _root = _doc.rootVisualElement;
            if (_root != null)
            {
                BindUI();
                ApplyJpFontRecursive(_root);
                // 動的生成された要素にも次フレームで再適用
                _root.schedule.Execute(() => ApplyJpFontRecursive(_root)).StartingIn(50);
            }
        }

        /// <summary>入力受付期間か(表示直後の InputLockDuration 秒間は false)。</summary>
        protected bool IsInputAllowed()
        {
            return Time.unscaledTime - _enabledTime >= InputLockDuration;
        }

        /// <summary>VisualElement 配下の全Label/Buttonに日本語FontAssetを適用する。</summary>
        protected void ApplyJpFontRecursive(VisualElement element)
        {
            if (_jpFont == null) return;
            ApplyFontToElement(element);
            foreach (var child in element.Children())
            {
                ApplyJpFontRecursive(child);
            }
        }

        /// <summary>個別要素にフォント適用。</summary>
        void ApplyFontToElement(VisualElement v)
        {
            if (v is Label || v is Button)
            {
                v.style.unityFontDefinition = new StyleFontDefinition(_jpFont);
            }
        }

        /// <summary>UI要素のQueryと初期化。派生クラスでoverride</summary>
        protected abstract void BindUI();

        /// <summary>遷移ヘルパー</summary>
        protected void GoTo(InkwaveScreenManager.Screen screen)
        {
            if (InkwaveScreenManager.Instance != null)
                InkwaveScreenManager.Instance.TransitionTo(screen);
        }

        /// <summary>自身が現在の表示画面か(派生クラスのUpdateで先頭ガードに使う)。</summary>
        protected bool IsActiveScreen(InkwaveScreenManager.Screen self)
        {
            return InkwaveScreenManager.Instance != null && InkwaveScreenManager.Instance.Current == self;
        }
    }
}
