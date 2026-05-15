using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Splatoon.Presentation.Inkwave.Screens
{
    /// <summary>
    /// INKWAVE 13 設定画面。5タブ(操作/映像/音響/ゲームプレイ/アカウント)+キーバインド。
    /// </summary>
    public class Screen_Settings : InkwaveScreenBase
    {
        enum CtrlType { Slider, Toggle, Select, Text, Action }
        struct SettingRow { public string Label; public CtrlType Ctrl; public object Value; public float Min, Max; public string[] Options; }

        readonly string[] _tabs = { "操作", "映像", "音響", "ゲームプレイ", "アカウント" };

        readonly SettingRow[][] _allRows;

        readonly (string label, string key)[] _keybinds = new[]
        {
            ("移動", "W A S D"), ("視点", "マウス"), ("メイン射撃", "左クリック"),
            ("サブ投擲", "右クリック"), ("イカ潜伏(押下)", "Shift"), ("ジャンプ", "Space"),
            ("スペシャル", "Q"), ("マップ(押下)", "Tab"), ("カメラリセット", "R"),
            ("合図 — こっち", "E"), ("合図 — ナイス", "F"), ("スコープ", "Ctrl")
        };

        int _tab = 0;
        readonly Dictionary<int, Dictionary<int, object>> _values = new();

        public Screen_Settings()
        {
            _allRows = new SettingRow[][]
            {
                // 操作
                new SettingRow[]
                {
                    new() { Label="マウス感度 · 横", Ctrl=CtrlType.Slider, Value=3.2f, Min=-5, Max=5 },
                    new() { Label="マウス感度 · 縦", Ctrl=CtrlType.Slider, Value=2.0f, Min=-5, Max=5 },
                    new() { Label="Y軸反転", Ctrl=CtrlType.Toggle, Value=false },
                    new() { Label="スムージング", Ctrl=CtrlType.Slider, Value=0.2f, Min=0, Max=1 },
                    new() { Label="イカ潜伏方式", Ctrl=CtrlType.Select, Value="押しっぱなし", Options=new[]{"押しっぱなし","トグル"} },
                    new() { Label="スコープ方式", Ctrl=CtrlType.Select, Value="押しっぱなし", Options=new[]{"押しっぱなし","トグル"} }
                },
                // 映像
                new SettingRow[]
                {
                    new() { Label="解像度", Ctrl=CtrlType.Select, Value="1920 × 1080", Options=new[]{"1280×720","1920×1080","2560×1440"} },
                    new() { Label="ウィンドウ", Ctrl=CtrlType.Select, Value="ボーダーレス", Options=new[]{"フル","ボーダーレス","ウィンドウ"} },
                    new() { Label="フレームレート上限", Ctrl=CtrlType.Select, Value="144 FPS", Options=new[]{"60 FPS","144 FPS","無制限"} },
                    new() { Label="垂直同期", Ctrl=CtrlType.Toggle, Value=false },
                    new() { Label="視野角 (FOV)", Ctrl=CtrlType.Slider, Value=84f, Min=60, Max=110 },
                    new() { Label="モーションブラー", Ctrl=CtrlType.Slider, Value=20f, Min=0, Max=100 },
                    new() { Label="ブルーム強度", Ctrl=CtrlType.Slider, Value=45f, Min=0, Max=100 },
                    new() { Label="色収差", Ctrl=CtrlType.Slider, Value=10f, Min=0, Max=100 }
                },
                // 音響
                new SettingRow[]
                {
                    new() { Label="マスター音量", Ctrl=CtrlType.Slider, Value=80f, Min=0, Max=100 },
                    new() { Label="BGM", Ctrl=CtrlType.Slider, Value=60f, Min=0, Max=100 },
                    new() { Label="効果音", Ctrl=CtrlType.Slider, Value=90f, Min=0, Max=100 },
                    new() { Label="ボイス · 自分", Ctrl=CtrlType.Slider, Value=75f, Min=0, Max=100 },
                    new() { Label="ボイス · 他人", Ctrl=CtrlType.Slider, Value=70f, Min=0, Max=100 },
                    new() { Label="イカ潜伏中ローパス", Ctrl=CtrlType.Toggle, Value=true }
                },
                // ゲームプレイ
                new SettingRow[]
                {
                    new() { Label="カラーUDサポート", Ctrl=CtrlType.Select, Value="シアン / オレンジ", Options=new[]{"オフ","シアン / オレンジ","赤 / 青"} },
                    new() { Label="ダメージ数値を表示", Ctrl=CtrlType.Toggle, Value=true },
                    new() { Label="自動拾得", Ctrl=CtrlType.Toggle, Value=true },
                    new() { Label="キルカム表示時間", Ctrl=CtrlType.Slider, Value=6f, Min=0, Max=10 },
                    new() { Label="PTT (Push-to-Talk)", Ctrl=CtrlType.Toggle, Value=false },
                    new() { Label="不適切ワードフィルタ", Ctrl=CtrlType.Toggle, Value=true }
                },
                // アカウント
                new SettingRow[]
                {
                    new() { Label="表示名", Ctrl=CtrlType.Text, Value="カイ-07" },
                    new() { Label="リージョン", Ctrl=CtrlType.Select, Value="アジア / 日本", Options=new[]{"アジア / 日本","北米","欧州"} },
                    new() { Label="チーム", Ctrl=CtrlType.Text, Value="リップタイド" },
                    new() { Label="クロスプレイ", Ctrl=CtrlType.Toggle, Value=true },
                    new() { Label="招待を許可", Ctrl=CtrlType.Select, Value="フレンドのみ", Options=new[]{"全員","フレンドのみ","オフ"} },
                    new() { Label="サインアウト", Ctrl=CtrlType.Action, Value="サインアウト" }
                }
            };
        }

        /// <summary>UI構築。</summary>
        protected override void BindUI()
        {
            BuildTabs();
            BuildContent();
            BuildKeybinds();
        }

        /// <summary>タブ生成。</summary>
        void BuildTabs()
        {
            var t = _root.Q<VisualElement>("tabs");
            t.Clear();
            for (int i = 0; i < _tabs.Length; i++)
            {
                int idx = i;
                var b = new Button(() => { _tab = idx; BuildTabs(); BuildContent(); });
                b.text = _tabs[i];
                b.AddToClassList("iw-tab");
                if (i == _tab) b.AddToClassList("iw-tab-active");
                b.style.fontSize = 12;
                b.style.marginRight = 6;
                t.Add(b);
            }
        }

        /// <summary>設定コンテンツ生成。</summary>
        void BuildContent()
        {
            var c = _root.Q<VisualElement>("settings-content");
            c.Clear();
            var lbl = new Label(_tabs[_tab]);
            lbl.AddToClassList("iw-head");
            lbl.style.fontSize = 18; lbl.style.color = new Color(0.957f, 0.957f, 0.941f);
            lbl.style.marginBottom = 12;
            c.Add(lbl);

            var rows = _allRows[_tab];
            for (int i = 0; i < rows.Length; i++)
            {
                int rowIdx = i;
                var s = rows[i];
                var row = new VisualElement();
                row.AddToClassList("iw-anim-slide");
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                row.style.alignItems = Align.Center;
                row.style.paddingLeft = 14; row.style.paddingRight = 14;
                row.style.paddingTop = 12; row.style.paddingBottom = 12;
                row.style.marginBottom = 6;
                row.style.backgroundColor = new Color(0.114f, 0.114f, 0.141f);

                var label = new Label(s.Label);
                label.style.fontSize = 12; label.style.color = new Color(0.957f, 0.957f, 0.941f);
                label.style.flexGrow = 1;
                row.Add(label);

                var ctrl = BuildControl(s, rowIdx);
                ctrl.style.width = 220;
                row.Add(ctrl);
                c.Add(row);
            }
        }

        /// <summary>コントロール生成(slider/toggle/select/text/action)。</summary>
        VisualElement BuildControl(SettingRow s, int rowIdx)
        {
            var wrap = new VisualElement();
            object val = GetValue(rowIdx, s.Value);
            switch (s.Ctrl)
            {
                case CtrlType.Slider:
                {
                    float fv = (float)val;
                    var top = new VisualElement();
                    top.style.flexDirection = FlexDirection.Row;
                    top.style.justifyContent = Justify.SpaceBetween;
                    var mn = new Label(s.Min.ToString());
                    mn.style.fontSize = 9; mn.style.color = new Color(0.541f, 0.541f, 0.572f);
                    var cur = new Label(fv.ToString("F1"));
                    cur.style.fontSize = 11; cur.style.color = new Color(0.957f, 0.957f, 0.941f);
                    cur.style.unityFontStyleAndWeight = FontStyle.Bold;
                    var mx = new Label(s.Max.ToString());
                    mx.style.fontSize = 9; mx.style.color = new Color(0.541f, 0.541f, 0.572f);
                    top.Add(mn); top.Add(cur); top.Add(mx);
                    wrap.Add(top);

                    var track = new VisualElement();
                    track.AddToClassList("iw-slider-track");
                    track.style.height = 4; track.style.backgroundColor = new Color(0.165f, 0.165f, 0.2f);
                    track.style.marginTop = 6;
                    var fill = new VisualElement();
                    fill.AddToClassList("iw-slider-fill");
                    float pct = (fv - s.Min) / (s.Max - s.Min);
                    fill.style.width = Length.Percent(pct * 100);
                    fill.style.height = 4;
                    fill.style.backgroundColor = new Color(1f, 0.106f, 0.42f);
                    track.Add(fill);
                    track.RegisterCallback<ClickEvent>(e =>
                    {
                        float p = e.localPosition.x / track.contentRect.width;
                        float nv = s.Min + p * (s.Max - s.Min);
                        nv = Mathf.Round(nv * 10) / 10f;
                        SetValue(rowIdx, nv);
                        BuildContent();
                    });
                    wrap.Add(track);
                    return wrap;
                }
                case CtrlType.Toggle:
                {
                    bool bv = (bool)val;
                    var toggle = new VisualElement();
                    toggle.AddToClassList("iw-toggle");
                    if (bv) toggle.AddToClassList("iw-toggle-on");
                    toggle.style.width = 44; toggle.style.height = 22;
                    toggle.style.backgroundColor = bv ? new Color(1f, 0.106f, 0.42f) : new Color(0.219f, 0.219f, 0.263f);
                    toggle.style.borderTopLeftRadius = 4; toggle.style.borderTopRightRadius = 4;
                    toggle.style.borderBottomLeftRadius = 4; toggle.style.borderBottomRightRadius = 4;
                    var knob = new VisualElement();
                    knob.AddToClassList("iw-toggle-knob");
                    knob.style.position = Position.Absolute;
                    knob.style.left = bv ? 24 : 2; knob.style.top = 2;
                    knob.style.width = 18; knob.style.height = 18;
                    knob.style.backgroundColor = new Color(0.957f, 0.957f, 0.941f);
                    toggle.Add(knob);
                    toggle.RegisterCallback<ClickEvent>(e => { SetValue(rowIdx, !bv); BuildContent(); });
                    wrap.style.alignItems = Align.FlexEnd;
                    wrap.Add(toggle);
                    return wrap;
                }
                case CtrlType.Select:
                {
                    string sv = (string)val;
                    var box = new VisualElement();
                    box.style.flexDirection = FlexDirection.Row;
                    box.style.alignItems = Align.Center;
                    box.style.justifyContent = Justify.SpaceBetween;
                    box.style.paddingLeft = 10; box.style.paddingRight = 10;
                    box.style.paddingTop = 6; box.style.paddingBottom = 6;
                    box.style.backgroundColor = new Color(0.165f, 0.165f, 0.2f);
                    var lt = new Label("‹"); lt.style.color = new Color(0.541f, 0.541f, 0.572f);
                    box.Add(lt);
                    var l = new Label(sv); l.style.fontSize = 11;
                    l.style.color = new Color(0.957f, 0.957f, 0.941f); l.style.unityFontStyleAndWeight = FontStyle.Bold;
                    box.Add(l);
                    var rt = new Label("›"); rt.style.color = new Color(0.541f, 0.541f, 0.572f);
                    box.Add(rt);
                    box.RegisterCallback<ClickEvent>(e =>
                    {
                        if (s.Options == null) return;
                        int curIdx = System.Array.IndexOf(s.Options, sv);
                        curIdx = (curIdx + 1) % s.Options.Length;
                        SetValue(rowIdx, s.Options[curIdx]);
                        BuildContent();
                    });
                    wrap.Add(box);
                    return wrap;
                }
                case CtrlType.Text:
                {
                    var box = new VisualElement();
                    box.style.flexDirection = FlexDirection.Row;
                    box.style.justifyContent = Justify.SpaceBetween;
                    box.style.paddingLeft = 10; box.style.paddingRight = 10;
                    box.style.paddingTop = 6; box.style.paddingBottom = 6;
                    box.style.backgroundColor = new Color(0.165f, 0.165f, 0.2f);
                    var l = new Label((string)val); l.style.fontSize = 11;
                    l.style.color = new Color(0.957f, 0.957f, 0.941f); l.style.unityFontStyleAndWeight = FontStyle.Bold;
                    box.Add(l);
                    var ed = new Label("✎"); ed.style.color = new Color(0.541f, 0.541f, 0.572f);
                    box.Add(ed);
                    wrap.Add(box);
                    return wrap;
                }
                case CtrlType.Action:
                {
                    var b = new Button(() => { });
                    b.text = (string)val;
                    b.AddToClassList("iw-btn"); b.AddToClassList("iw-btn-ghost");
                    b.style.height = 32;
                    wrap.Add(b);
                    return wrap;
                }
                default:
                {
                    // 未知の CtrlType: 空ラベル返却で安全側に倒す
                    wrap.Add(new Label(""));
                    return wrap;
                }
            }
        }

        /// <summary>キーバインドリスト構築。</summary>
        void BuildKeybinds()
        {
            var list = _root.Q<VisualElement>("keybind-list");
            list.Clear();
            foreach (var k in _keybinds)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                row.style.paddingTop = 4; row.style.paddingBottom = 4;
                var l = new Label(k.label);
                l.style.fontSize = 11; l.style.color = new Color(0.957f, 0.957f, 0.941f);
                row.Add(l);
                var key = new Label(k.key);
                key.AddToClassList("iw-num");
                key.style.fontSize = 11; key.style.color = new Color(0.541f, 0.541f, 0.572f);
                row.Add(key);
                list.Add(row);
            }
        }

        /// <summary>値取得(永続化済orデフォルト)。</summary>
        object GetValue(int rowIdx, object defaultVal)
        {
            if (_values.TryGetValue(_tab, out var d) && d.TryGetValue(rowIdx, out var v)) return v;
            return defaultVal;
        }

        /// <summary>値設定。</summary>
        void SetValue(int rowIdx, object v)
        {
            if (!_values.ContainsKey(_tab)) _values[_tab] = new Dictionary<int, object>();
            _values[_tab][rowIdx] = v;
        }

        /// <summary>キー入力。</summary>
        void Update()
        {
            if (_root == null) return;
            if (!IsActiveScreen(InkwaveScreenManager.Screen.Settings)) return;
            if (!IsInputAllowed()) return;
            if (InkwaveInput.GetKeyDown(KeyCode.Tab)) { _tab = (_tab + 1) % _tabs.Length; BuildTabs(); BuildContent(); }
            else if (InkwaveInput.GetKeyDown(KeyCode.Escape)) GoTo(InkwaveScreenManager.Screen.Menu);
        }
    }
}
