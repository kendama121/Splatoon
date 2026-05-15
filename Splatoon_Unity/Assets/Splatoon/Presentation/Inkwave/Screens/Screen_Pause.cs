using UnityEngine;
using UnityEngine.UIElements;

namespace Splatoon.Presentation.Inkwave.Screens
{
    /// <summary>
    /// INKWAVE 10 ポーズ画面。5メニュー(再開/ロードアウト/操作設定/降参に投票/試合を抜ける)。
    /// </summary>
    public class Screen_Pause : InkwaveScreenBase
    {
        struct MenuItem { public string Label; public string Key; public string Variant; public InkwaveScreenManager.Screen? GoToScreen; public System.Action Action; }

        MenuItem[] _items;

        /// <summary>UI構築。</summary>
        protected override void BindUI()
        {
            _items = new MenuItem[]
            {
                new() { Label="再開する", Key="Esc", Variant="primary", GoToScreen=InkwaveScreenManager.Screen.HUD },
                new() { Label="ロードアウト", Key="L", GoToScreen=InkwaveScreenManager.Screen.Weapon },
                new() { Label="操作設定", Key="K", GoToScreen=InkwaveScreenManager.Screen.Settings },
                new() { Label="降参に投票", Key="V", Variant="warn", Action = () => { } },
                new() { Label="試合を抜ける", Key="Q", Variant="danger", GoToScreen=InkwaveScreenManager.Screen.Title }
            };

            var menu = _root.Q<VisualElement>("pause-menu");
            menu.Clear();
            for (int i = 0; i < _items.Length; i++)
            {
                int idx = i;
                var it = _items[i];
                var btn = new Button(() =>
                {
                    if (_items[idx].GoToScreen.HasValue) GoTo(_items[idx].GoToScreen.Value);
                    _items[idx].Action?.Invoke();
                });
                btn.style.flexDirection = FlexDirection.Row;
                btn.style.justifyContent = Justify.SpaceBetween;
                btn.style.alignItems = Align.Center;
                btn.style.paddingLeft = 18; btn.style.paddingRight = 18;
                btn.style.paddingTop = 14; btn.style.paddingBottom = 14;
                btn.style.marginBottom = 8;
                btn.style.borderTopWidth = 1; btn.style.borderBottomWidth = 1;
                btn.style.borderLeftWidth = 1; btn.style.borderRightWidth = 1;

                Color bg = new Color(0.051f, 0.051f, 0.063f, 0.85f);
                Color border = new Color(0.204f, 0.204f, 0.247f);
                Color text = new Color(0.957f, 0.957f, 0.941f);
                if (it.Variant == "primary") { bg = new Color(1f, 0.106f, 0.42f); border = new Color(1f, 0.106f, 0.42f); }
                else if (it.Variant == "warn") { text = new Color(1f, 0.839f, 0f); }
                else if (it.Variant == "danger") { text = new Color(1f, 0.333f, 0.333f); }

                btn.style.backgroundColor = bg;
                btn.style.borderTopColor = border; btn.style.borderBottomColor = border;
                btn.style.borderLeftColor = border; btn.style.borderRightColor = border;
                btn.style.color = text;
                btn.text = "";

                var label = new Label(it.Label);
                label.style.fontSize = 16; label.style.color = text;
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
                btn.Add(label);

                var key = new IwKey(); key.SetText(it.Key);
                btn.Add(key);

                // ホバー時 translateX 6px
                btn.RegisterCallback<MouseEnterEvent>(e => btn.style.translate = new StyleTranslate(new Translate(6, 0, 0)));
                btn.RegisterCallback<MouseLeaveEvent>(e => btn.style.translate = new StyleTranslate(new Translate(0, 0, 0)));

                menu.Add(btn);
            }
        }

        /// <summary>キー入力。</summary>
        void Update()
        {
            if (_root == null) return;
            if (!IsActiveScreen(InkwaveScreenManager.Screen.Pause)) return;
            if (!IsInputAllowed()) return;
            if (InkwaveInput.GetKeyDown(KeyCode.Escape)) GoTo(InkwaveScreenManager.Screen.HUD);
            else if (InkwaveInput.GetKeyDown(KeyCode.L)) GoTo(InkwaveScreenManager.Screen.Weapon);
            else if (InkwaveInput.GetKeyDown(KeyCode.K)) GoTo(InkwaveScreenManager.Screen.Settings);
            else if (InkwaveInput.GetKeyDown(KeyCode.Q)) GoTo(InkwaveScreenManager.Screen.Title);
        }
    }
}
