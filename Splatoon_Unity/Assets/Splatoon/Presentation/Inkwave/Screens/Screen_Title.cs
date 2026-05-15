using UnityEngine;
using UnityEngine.UIElements;

namespace Splatoon.Presentation.Inkwave.Screens
{
    /// <summary>
    /// INKWAVE 01 タイトル画面コントローラ。
    /// PRESS START → Menu遷移、Esc → 終了、F1 → クレジット(未実装)。
    /// </summary>
    public class Screen_Title : InkwaveScreenBase
    {
        Button _pressStart;
        VisualElement _arrow;
        VisualElement _heroRight;
        VisualElement _tilt;

        // アニメ用時間
        float _tiltTimer;
        float _floatTimer;
        float _arrowTimer;

        /// <summary>UI要素のQueryとイベントバインド。</summary>
        protected override void BindUI()
        {
            _pressStart = _root.Q<Button>("press-start");
            _arrow = _root.Q<VisualElement>("arrow");
            _heroRight = _root.Q<VisualElement>("hero-right");
            _tilt = _pressStart;

            if (_pressStart != null)
            {
                _pressStart.clicked += OnPressStart;
                // hover時のscale変化はUSS側
            }

            // キー入力はUpdateで監視
        }

        /// <summary>PRESS STARTボタン押下 → Menuへ遷移。</summary>
        void OnPressStart()
        {
            GoTo(InkwaveScreenManager.Screen.Menu);
        }

        /// <summary>Update: キー入力監視+アニメ更新。</summary>
        void Update()
        {
            if (_root == null) return;
            if (!IsActiveScreen(InkwaveScreenManager.Screen.Title)) return;
            if (!IsInputAllowed()) return;

            // キー入力監視
            if (InkwaveInput.GetKeyDown(KeyCode.Return) || InkwaveInput.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnPressStart();
            }
            else if (InkwaveInput.GetKeyDown(KeyCode.Escape))
            {
                UnityEngine.Application.Quit();
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
            }

            // tilt アニメ (PRESS START ボタン)
            _tiltTimer += Time.unscaledDeltaTime;
            if (_tilt != null)
            {
                float t = Mathf.Sin(_tiltTimer * 2f) * 3f;
                _tilt.style.rotate = new StyleRotate(new Rotate(t));
            }

            // float アニメ (右ヒーロー)
            _floatTimer += Time.unscaledDeltaTime;
            if (_heroRight != null)
            {
                float y = Mathf.Sin(_floatTimer * 1.5f) * 8f;
                _heroRight.style.translate = new StyleTranslate(new Translate(0, y, 0));
            }

            // arrow アニメ
            _arrowTimer += Time.unscaledDeltaTime;
            if (_arrow != null)
            {
                float x = Mathf.Abs(Mathf.Sin(_arrowTimer * 3f)) * 6f;
                _arrow.style.translate = new StyleTranslate(new Translate(x, 0, 0));
            }
        }
    }
}
