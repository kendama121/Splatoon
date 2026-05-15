using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Splatoon.Presentation.Inkwave.Screens
{
    /// <summary>
    /// INKWAVE 02 メインメニュー画面。
    /// 4モード(TURF/ZONE/TOWER/SHELL)選択+プロフィール+ローテ+クイックメニュー。
    /// </summary>
    public class Screen_Menu : InkwaveScreenBase
    {
        /// <summary>モード定義データ。</summary>
        struct ModeDef
        {
            public string En;       // 英大文字表記
            public string Jp;       // 日本語
            public string EnTitle;  // 英タイトル
            public string Desc;     // 説明
            public int Live;        // 待機人数
            public Color Accent;    // アクセント色
            public string Key;      // ショートカット
        }

        readonly ModeDef[] _modes = new ModeDef[]
        {
            new() { En="TURF", Jp="塗り合戦", EnTitle="TURF WAR", Desc="3分間で多く塗ったほうが勝ち", Live=1824, Accent=new Color(1f,0.106f,0.42f), Key="1" },
            new() { En="ZONE", Jp="ゾーン制圧", EnTitle="ZONE CONTROL", Desc="中央エリアを長く保持しろ", Live=942, Accent=new Color(0f,0.898f,1f), Key="2" },
            new() { En="TOWER", Jp="タワー奪取", EnTitle="TOWER PUSH", Desc="タワーに乗って敵陣へ運べ", Live=610, Accent=new Color(0.698f,0.42f,1f), Key="3" },
            new() { En="SHELL", Jp="貝アタック", EnTitle="SHELL RUSH", Desc="貝を集めてゴールに打ち込め", Live=388, Accent=new Color(1f,0.541f,0.122f), Key="4" }
        };

        readonly (string, string)[] _quickItems = new[]
        {
            ("ロードアウト", "L"), ("キャラ編集", "C"), ("トレーニング", "T"),
            ("設定", "O"), ("プライベート", "P"), ("フレンド", "F")
        };

        int _active = 0;
        readonly List<VisualElement> _modeCards = new();

        /// <summary>UI構築。</summary>
        protected override void BindUI()
        {
            var grid = _root.Q<VisualElement>("mode-grid");
            var quick = _root.Q<VisualElement>("quick-grid");
            _modeCards.Clear();

            // 「マッチング開始」大ボタン → Lobby遷移
            var startBtn = _root.Q<Button>("btn-start-match");
            if (startBtn != null) startBtn.clicked += () => GoTo(InkwaveScreenManager.Screen.Lobby);

            // モードカード4枚生成
            for (int i = 0; i < _modes.Length; i++)
            {
                int idx = i;
                var m = _modes[i];
                var card = BuildModeCard(m, idx == _active);
                card.RegisterCallback<ClickEvent>(e => SelectMode(idx));
                grid.Add(card);
                _modeCards.Add(card);
            }

            // クイックメニュー6個
            foreach (var (label, key) in _quickItems)
            {
                var row = new VisualElement();
                row.AddToClassList("iw-row");
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                row.style.alignItems = Align.Center;
                row.style.paddingLeft = 12;
                row.style.paddingRight = 12;
                row.style.paddingTop = 10;
                row.style.paddingBottom = 10;
                row.style.marginRight = 6;
                row.style.marginBottom = 6;
                row.style.width = Length.Percent(46);
                row.style.backgroundColor = new Color(0.114f, 0.114f, 0.141f);
                row.style.borderTopColor = new Color(0.051f, 0.051f, 0.063f);
                row.style.borderBottomColor = new Color(0.051f, 0.051f, 0.063f);
                row.style.borderLeftColor = new Color(0.051f, 0.051f, 0.063f);
                row.style.borderRightColor = new Color(0.051f, 0.051f, 0.063f);
                row.style.borderTopWidth = 2;
                row.style.borderBottomWidth = 2;
                row.style.borderLeftWidth = 2;
                row.style.borderRightWidth = 2;

                var lbl = new Label(label);
                lbl.style.fontSize = 11;
                lbl.style.color = new Color(0.957f, 0.957f, 0.941f);
                row.Add(lbl);

                var keyEl = new IwKey();
                keyEl.SetText(key);
                row.Add(keyEl);

                quick.Add(row);
            }
        }

        /// <summary>モードカードを構築する。</summary>
        VisualElement BuildModeCard(ModeDef m, bool isSelected)
        {
            var card = new VisualElement();
            card.AddToClassList("iw-poster");
            card.style.width = Length.Percent(48);
            card.style.height = 196;
            card.style.marginRight = 14;
            card.style.marginBottom = 14;
            card.style.overflow = Overflow.Hidden;
            card.style.flexDirection = FlexDirection.Row;
            card.style.backgroundColor = new Color(0.082f, 0.082f, 0.102f);
            card.style.borderTopWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderTopColor = new Color(0.204f, 0.204f, 0.247f);
            card.style.borderBottomColor = new Color(0.204f, 0.204f, 0.247f);
            card.style.borderLeftColor = new Color(0.204f, 0.204f, 0.247f);
            card.style.borderRightColor = new Color(0.204f, 0.204f, 0.247f);

            if (isSelected)
            {
                card.style.backgroundColor = Color.Lerp(new Color(0.082f, 0.082f, 0.102f), m.Accent, 0.15f);
                card.AddToClassList("iw-anim-tilt");
            }

            // 左キーアート部
            var artPanel = new VisualElement();
            artPanel.style.width = 140;
            artPanel.style.backgroundColor = m.Accent;
            artPanel.style.alignItems = Align.Center;
            artPanel.style.justifyContent = Justify.Center;
            var enTitle = new Label(m.EnTitle);
            enTitle.style.position = Position.Absolute;
            enTitle.style.top = 12;
            enTitle.style.left = 12;
            enTitle.style.fontSize = 9;
            enTitle.style.color = new Color(0.957f, 0.957f, 0.941f);
            enTitle.style.letterSpacing = 2;
            artPanel.Add(enTitle);
            var enBig = new Label(m.En);
            enBig.style.fontSize = 36;
            enBig.style.color = new Color(0.957f, 0.957f, 0.941f);
            enBig.style.unityFontStyleAndWeight = FontStyle.Bold;
            artPanel.Add(enBig);
            card.Add(artPanel);

            // 右情報部
            var info = new VisualElement();
            info.style.paddingLeft = 18;
            info.style.paddingRight = 18;
            info.style.paddingTop = 18;
            info.style.paddingBottom = 18;
            info.style.flexGrow = 1;
            info.style.flexDirection = FlexDirection.Column;
            info.style.justifyContent = Justify.SpaceBetween;
            card.Add(info);

            // 上段
            var top = new VisualElement();
            top.style.flexDirection = FlexDirection.Row;
            top.style.justifyContent = Justify.SpaceBetween;

            var nameWrap = new VisualElement();
            var jpName = new Label(m.Jp);
            jpName.style.fontSize = 30;
            jpName.style.color = new Color(0.957f, 0.957f, 0.941f);
            jpName.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameWrap.Add(jpName);
            var sub = new Label("4対4 · 3分");
            sub.style.fontSize = 10;
            sub.style.color = new Color(0.541f, 0.541f, 0.572f);
            sub.style.marginTop = 4;
            sub.style.letterSpacing = 2;
            nameWrap.Add(sub);
            top.Add(nameWrap);

            var keyCap = new IwKey();
            keyCap.SetText(m.Key);
            top.Add(keyCap);
            info.Add(top);

            // 説明
            var desc = new Label(m.Desc);
            desc.style.fontSize = 12;
            desc.style.color = new Color(0.541f, 0.541f, 0.572f);
            desc.style.marginTop = 12;
            desc.style.whiteSpace = WhiteSpace.Normal;
            info.Add(desc);

            // 下段
            var bottom = new VisualElement();
            bottom.style.flexDirection = FlexDirection.Row;
            bottom.style.justifyContent = Justify.SpaceBetween;
            bottom.style.alignItems = Align.Center;
            var live = new Label($"{m.Live:N0}人 待機中");
            live.style.fontSize = 11;
            live.style.color = m.Accent;
            bottom.Add(live);
            if (isSelected)
            {
                var stick = new IwSticker();
                stick.SetSticker("選択中", m.Accent, new Color(0.957f, 0.957f, 0.941f), -6f);
                bottom.Add(stick);
            }
            info.Add(bottom);

            return card;
        }

        /// <summary>モード選択。</summary>
        void SelectMode(int idx)
        {
            _active = idx;
            // 再構築
            var grid = _root.Q<VisualElement>("mode-grid");
            grid.Clear();
            _modeCards.Clear();
            for (int i = 0; i < _modes.Length; i++)
            {
                int local = i;
                var card = BuildModeCard(_modes[i], i == _active);
                card.RegisterCallback<ClickEvent>(e => SelectMode(local));
                grid.Add(card);
                _modeCards.Add(card);
            }
        }

        /// <summary>Update: キー入力監視。</summary>
        void Update()
        {
            if (_root == null) return;
            if (!IsActiveScreen(InkwaveScreenManager.Screen.Menu)) return;
            if (!IsInputAllowed()) return;
            if (InkwaveInput.GetKeyDown(KeyCode.Alpha1)) SelectMode(0);
            else if (InkwaveInput.GetKeyDown(KeyCode.Alpha2)) SelectMode(1);
            else if (InkwaveInput.GetKeyDown(KeyCode.Alpha3)) SelectMode(2);
            else if (InkwaveInput.GetKeyDown(KeyCode.Alpha4)) SelectMode(3);
            else if (InkwaveInput.GetKeyDown(KeyCode.Return)) GoTo(InkwaveScreenManager.Screen.Lobby);
            else if (InkwaveInput.GetKeyDown(KeyCode.Escape)) GoTo(InkwaveScreenManager.Screen.Title);
        }
    }
}
