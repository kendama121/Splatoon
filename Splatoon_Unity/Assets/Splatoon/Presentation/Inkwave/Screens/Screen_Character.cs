using UnityEngine;
using UnityEngine.UIElements;

namespace Splatoon.Presentation.Inkwave.Screens
{
    /// <summary>
    /// INKWAVE 03 キャラ編集画面。
    /// 装備スロット選択+チームトグル+回転+ポーズ+タブ(ギア/スキン/パワー)。
    /// </summary>
    public class Screen_Character : InkwaveScreenBase
    {
        struct SlotData
        {
            public string Key, Label, Value, Rarity, Ability;
        }

        readonly SlotData[] _slots = new SlotData[]
        {
            new() { Key="head", Label="あたま", Value="ハイドロバイザー", Rarity="EPIC", Ability="インク効率(メイン)" },
            new() { Key="top", Label="ふく", Value="ストリートパネル パーカー", Rarity="RARE", Ability="イカダッシュ速度" },
            new() { Key="bot", Label="くつした", Value="タイドカーゴ", Rarity="COMMON", Ability="ジャンプ高さ" },
            new() { Key="shoes", Label="くつ", Value="グリップチェイサーX", Rarity="RARE", Ability="復活時間短縮" }
        };

        readonly Color[] _inkColors = new[]
        {
            new Color(1f,0.106f,0.42f), new Color(1f,0.42f,0.106f), new Color(1f,0.839f,0f),
            new Color(0.255f,0.878f,0.478f), new Color(0f,0.898f,1f), new Color(0.106f,0.42f,1f),
            new Color(0.698f,0.42f,1f), new Color(1f,0.106f,0.878f)
        };

        readonly string[] _abilities = new[] { "インク効率(メイン)", "イカダッシュ速度", "復活時間短縮", "スーパージャンプ", "— 未開放 —" };
        readonly int[] _abilityStars = new[] { 3, 2, 1, 2, 0 };

        string _team = "a";
        int _colorIdx = 0;
        int _slotIdx = 0;
        string _tab = "gear";
        float _rotateY = 0;
        string _pose = "idle";

        IwInkChar _inkChar;

        /// <summary>UI構築。</summary>
        protected override void BindUI()
        {
            _inkChar = _root.Q<IwInkChar>("ink-char");

            BuildSlotList();
            BuildSprayGrid();
            BuildTabContent();

            _root.Q<Button>("btn-back").clicked += () => GoTo(InkwaveScreenManager.Screen.Menu);
            _root.Q<Button>("btn-team-a").clicked += () => SetTeam("a");
            _root.Q<Button>("btn-team-b").clicked += () => SetTeam("b");
            _root.Q<Button>("btn-rot-l").clicked += () => Rotate(-30);
            _root.Q<Button>("btn-rot-r").clicked += () => Rotate(30);
            _root.Q<Button>("pose-idle").clicked += () => SetPose("idle");
            _root.Q<Button>("pose-aim").clicked += () => SetPose("aim");
            _root.Q<Button>("pose-victory").clicked += () => SetPose("victory");
            _root.Q<Button>("tab-gear").clicked += () => SetTab("gear");
            _root.Q<Button>("tab-skin").clicked += () => SetTab("skin");
            _root.Q<Button>("tab-abilities").clicked += () => SetTab("abilities");
        }

        /// <summary>装備スロットリスト構築。</summary>
        void BuildSlotList()
        {
            var list = _root.Q<VisualElement>("slot-list");
            list.Clear();
            for (int i = 0; i < _slots.Length; i++)
            {
                int idx = i;
                var s = _slots[i];
                var row = new VisualElement();
                row.AddToClassList("iw-row");
                if (idx == _slotIdx) row.AddToClassList("iw-row-selected");
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.paddingLeft = 10; row.style.paddingRight = 10;
                row.style.paddingTop = 10; row.style.paddingBottom = 10;
                row.style.marginBottom = 8;
                row.style.borderTopWidth = 2; row.style.borderBottomWidth = 2;
                row.style.borderLeftWidth = 2; row.style.borderRightWidth = 2;
                row.style.borderTopColor = new Color(0.051f, 0.051f, 0.063f);
                row.style.borderBottomColor = new Color(0.051f, 0.051f, 0.063f);
                row.style.borderLeftColor = new Color(0.051f, 0.051f, 0.063f);
                row.style.borderRightColor = new Color(0.051f, 0.051f, 0.063f);
                row.style.backgroundColor = new Color(0.114f, 0.114f, 0.141f);

                // アイコン
                var icon = new VisualElement();
                icon.style.width = 44; icon.style.height = 44;
                icon.style.backgroundColor = new Color(0.165f, 0.165f, 0.2f);
                icon.style.alignItems = Align.Center; icon.style.justifyContent = Justify.Center;
                icon.style.marginRight = 10;
                var iconLabel = new Label(s.Label.Substring(0, System.Math.Min(2, s.Label.Length)));
                iconLabel.style.fontSize = 11;
                iconLabel.style.color = _inkColors[_colorIdx];
                icon.Add(iconLabel);
                row.Add(icon);

                // ラベル
                var info = new VisualElement();
                info.style.flexGrow = 1;
                var lbl = new Label(s.Label);
                lbl.style.fontSize = 10; lbl.style.color = new Color(0.541f, 0.541f, 0.572f);
                info.Add(lbl);
                var val = new Label(s.Value);
                val.style.fontSize = 11; val.style.color = new Color(0.957f, 0.957f, 0.941f);
                info.Add(val);
                row.Add(info);

                // レアリティ
                var rare = new Label(s.Rarity);
                rare.style.fontSize = 9;
                rare.style.color = new Color(0.957f, 0.957f, 0.941f);
                rare.style.paddingLeft = 6; rare.style.paddingRight = 6;
                rare.style.paddingTop = 2; rare.style.paddingBottom = 2;
                rare.style.backgroundColor = RarityColor(s.Rarity);
                row.Add(rare);

                row.RegisterCallback<ClickEvent>(e => SetSlot(idx));
                list.Add(row);
            }
        }

        /// <summary>スプレーグリッド構築(4個)。</summary>
        void BuildSprayGrid()
        {
            var grid = _root.Q<VisualElement>("spray-grid");
            grid.Clear();
            string[] icons = { "◆", "▲", "●", "★" };
            foreach (var s in icons)
            {
                var tile = new VisualElement();
                tile.style.width = Length.Percent(22);
                tile.style.height = 48;
                tile.style.marginRight = 4;
                tile.style.backgroundColor = new Color(0.114f, 0.114f, 0.141f);
                tile.style.borderTopWidth = 2; tile.style.borderBottomWidth = 2;
                tile.style.borderLeftWidth = 2; tile.style.borderRightWidth = 2;
                tile.style.borderTopColor = new Color(0.051f, 0.051f, 0.063f);
                tile.style.borderBottomColor = new Color(0.051f, 0.051f, 0.063f);
                tile.style.borderLeftColor = new Color(0.051f, 0.051f, 0.063f);
                tile.style.borderRightColor = new Color(0.051f, 0.051f, 0.063f);
                tile.style.alignItems = Align.Center;
                tile.style.justifyContent = Justify.Center;
                var lbl = new Label(s);
                lbl.style.fontSize = 18;
                lbl.style.color = _inkColors[_colorIdx];
                tile.Add(lbl);
                grid.Add(tile);
            }
        }

        /// <summary>タブコンテンツ構築。</summary>
        void BuildTabContent()
        {
            var content = _root.Q<VisualElement>("tab-content");
            content.Clear();
            if (_tab == "gear") BuildGearTab(content);
            else if (_tab == "skin") BuildSkinTab(content);
            else BuildAbilitiesTab(content);
        }

        /// <summary>ギアタブ。</summary>
        void BuildGearTab(VisualElement c)
        {
            var s = _slots[_slotIdx];
            var label = new Label($"{s.Rarity} · {s.Label}");
            label.AddToClassList("iw-label");
            label.style.fontSize = 10; label.style.color = RarityColor(s.Rarity);
            c.Add(label);
            var v = new Label(s.Value);
            v.style.fontSize = 20; v.style.color = new Color(0.957f, 0.957f, 0.941f);
            v.style.unityFontStyleAndWeight = FontStyle.Bold;
            v.style.marginTop = 4;
            c.Add(v);

            var preview = new VisualElement();
            preview.style.height = 130;
            preview.style.marginTop = 12;
            preview.style.backgroundColor = new Color(0.114f, 0.114f, 0.141f);
            preview.style.borderTopWidth = 2; preview.style.borderBottomWidth = 2;
            preview.style.borderLeftWidth = 2; preview.style.borderRightWidth = 2;
            preview.style.borderTopColor = new Color(0.051f, 0.051f, 0.063f);
            preview.style.borderBottomColor = new Color(0.051f, 0.051f, 0.063f);
            preview.style.borderLeftColor = new Color(0.051f, 0.051f, 0.063f);
            preview.style.borderRightColor = new Color(0.051f, 0.051f, 0.063f);
            preview.style.alignItems = Align.Center;
            preview.style.justifyContent = Justify.Center;
            var pLabel = new Label("3D プレビュー");
            pLabel.style.color = new Color(0.541f, 0.541f, 0.572f);
            pLabel.style.fontSize = 10;
            preview.Add(pLabel);
            c.Add(preview);

            var mainPow = new Label("メインパワー");
            mainPow.AddToClassList("iw-label");
            mainPow.style.fontSize = 10; mainPow.style.color = new Color(0.541f, 0.541f, 0.572f);
            mainPow.style.marginTop = 12;
            c.Add(mainPow);

            var powRow = new VisualElement();
            powRow.style.flexDirection = FlexDirection.Row;
            powRow.style.alignItems = Align.Center;
            powRow.style.paddingLeft = 10; powRow.style.paddingRight = 10;
            powRow.style.paddingTop = 8; powRow.style.paddingBottom = 8;
            powRow.style.backgroundColor = new Color(0.114f, 0.114f, 0.141f);
            powRow.style.marginTop = 6;
            var dia = new VisualElement();
            dia.style.width = 16; dia.style.height = 16;
            dia.style.backgroundColor = _inkColors[_colorIdx];
            dia.style.rotate = new StyleRotate(new Rotate(45));
            dia.style.marginRight = 12;
            powRow.Add(dia);
            var pa = new Label(s.Ability);
            pa.style.fontSize = 12; pa.style.color = new Color(0.957f, 0.957f, 0.941f);
            powRow.Add(pa);
            c.Add(powRow);
        }

        /// <summary>スキンタブ(色8+髪型6+瞳3)。</summary>
        void BuildSkinTab(VisualElement c)
        {
            var inkLbl = new Label("インクカラー");
            inkLbl.AddToClassList("iw-label");
            inkLbl.style.fontSize = 10; inkLbl.style.color = new Color(0.541f, 0.541f, 0.572f);
            c.Add(inkLbl);

            var colorGrid = new VisualElement();
            colorGrid.style.flexDirection = FlexDirection.Row;
            colorGrid.style.flexWrap = Wrap.Wrap;
            colorGrid.style.marginTop = 10;
            for (int i = 0; i < _inkColors.Length; i++)
            {
                int idx = i;
                var sw = new VisualElement();
                sw.AddToClassList("iw-swatch");
                if (idx == _colorIdx) sw.AddToClassList("iw-swatch-selected");
                sw.style.width = Length.Percent(22);
                sw.style.height = 40;
                sw.style.marginRight = 6; sw.style.marginBottom = 6;
                sw.style.backgroundColor = _inkColors[i];
                sw.style.borderTopWidth = 2; sw.style.borderBottomWidth = 2;
                sw.style.borderLeftWidth = 2; sw.style.borderRightWidth = 2;
                sw.style.borderTopColor = idx == _colorIdx ? Color.white : new Color(0.051f, 0.051f, 0.063f);
                sw.style.borderBottomColor = idx == _colorIdx ? Color.white : new Color(0.051f, 0.051f, 0.063f);
                sw.style.borderLeftColor = idx == _colorIdx ? Color.white : new Color(0.051f, 0.051f, 0.063f);
                sw.style.borderRightColor = idx == _colorIdx ? Color.white : new Color(0.051f, 0.051f, 0.063f);
                sw.RegisterCallback<ClickEvent>(e => { _colorIdx = idx; BuildSlotList(); BuildSprayGrid(); BuildTabContent(); });
                colorGrid.Add(sw);
            }
            c.Add(colorGrid);

            BuildOptionGrid(c, "髪型 (触手)", new[] { "ウェーブ", "スリック", "ロング", "ショート", "カール", "モヒカン" });
            BuildOptionGrid(c, "瞳", new[] { "ネオン", "ラウンド", "シャープ" });
        }

        /// <summary>オプション選択肢グリッド。</summary>
        void BuildOptionGrid(VisualElement c, string title, string[] opts)
        {
            var lbl = new Label(title);
            lbl.AddToClassList("iw-label");
            lbl.style.fontSize = 10; lbl.style.color = new Color(0.541f, 0.541f, 0.572f);
            lbl.style.marginTop = 14;
            c.Add(lbl);
            var g = new VisualElement();
            g.style.flexDirection = FlexDirection.Row;
            g.style.flexWrap = Wrap.Wrap;
            g.style.marginTop = 8;
            for (int i = 0; i < opts.Length; i++)
            {
                var b = new Button(() => { });
                b.text = opts[i];
                b.AddToClassList("iw-tab");
                if (i == 0) b.AddToClassList("iw-tab-active");
                b.style.fontSize = 10;
                b.style.width = Length.Percent(31);
                b.style.marginRight = 6; b.style.marginBottom = 6;
                g.Add(b);
            }
            c.Add(g);
        }

        /// <summary>パワータブ(ギアパワー一覧)。</summary>
        void BuildAbilitiesTab(VisualElement c)
        {
            var lbl = new Label("ギアパワー一覧");
            lbl.AddToClassList("iw-label");
            lbl.style.fontSize = 10; lbl.style.color = new Color(0.541f, 0.541f, 0.572f);
            c.Add(lbl);
            for (int i = 0; i < _abilities.Length; i++)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.paddingLeft = 10; row.style.paddingRight = 10;
                row.style.paddingTop = 8; row.style.paddingBottom = 8;
                row.style.backgroundColor = new Color(0.114f, 0.114f, 0.141f);
                row.style.marginTop = 8;

                var dia = new VisualElement();
                dia.style.width = 16; dia.style.height = 16;
                dia.style.backgroundColor = i == 0 ? _inkColors[_colorIdx] : new Color(0.219f, 0.219f, 0.263f);
                dia.style.rotate = new StyleRotate(new Rotate(45));
                dia.style.marginRight = 10;
                row.Add(dia);

                var name = new Label(_abilities[i]);
                name.style.fontSize = 11; name.style.unityFontStyleAndWeight = FontStyle.Bold;
                name.style.color = new Color(0.957f, 0.957f, 0.941f);
                name.style.flexGrow = 1;
                row.Add(name);

                var stars = new VisualElement();
                stars.style.flexDirection = FlexDirection.Row;
                for (int s = 0; s < 3; s++)
                {
                    var dot = new VisualElement();
                    dot.style.width = 10; dot.style.height = 10;
                    dot.style.marginLeft = 2;
                    dot.style.backgroundColor = s < _abilityStars[i] ? new Color(1f, 0.839f, 0f) : new Color(0.165f, 0.165f, 0.2f);
                    stars.Add(dot);
                }
                row.Add(stars);
                c.Add(row);
            }
        }

        /// <summary>レアリティ色。</summary>
        Color RarityColor(string r) => r switch
        {
            "EPIC" => new Color(0.698f, 0.42f, 1f),
            "RARE" => new Color(0f, 0.898f, 1f),
            _ => new Color(0.219f, 0.219f, 0.263f)
        };

        /// <summary>チーム切替。</summary>
        void SetTeam(string t)
        {
            _team = t;
            if (_inkChar != null) _inkChar.Configure(_pose, _team, "shooter", 280);
        }

        /// <summary>回転加算。</summary>
        void Rotate(int delta)
        {
            _rotateY += delta;
            if (_inkChar != null) _inkChar.style.rotate = new StyleRotate(new Rotate(_rotateY));
        }

        /// <summary>ポーズ切替。</summary>
        void SetPose(string p)
        {
            _pose = p;
            if (_inkChar != null) _inkChar.Configure(_pose, _team, "shooter", 280);
        }

        /// <summary>タブ切替。</summary>
        void SetTab(string t)
        {
            _tab = t;
            BuildTabContent();
        }

        /// <summary>スロット選択。</summary>
        void SetSlot(int idx)
        {
            _slotIdx = idx;
            BuildSlotList();
            if (_tab == "gear") BuildTabContent();
        }

        /// <summary>キー入力監視。</summary>
        void Update()
        {
            if (_root == null) return;
            if (!IsActiveScreen(InkwaveScreenManager.Screen.Character)) return;
            if (!IsInputAllowed()) return;
            if (InkwaveInput.GetKeyDown(KeyCode.Q)) Rotate(-30);
            else if (InkwaveInput.GetKeyDown(KeyCode.E)) Rotate(30);
            else if (InkwaveInput.GetKeyDown(KeyCode.Tab))
            {
                _tab = _tab == "gear" ? "skin" : _tab == "skin" ? "abilities" : "gear";
                BuildTabContent();
            }
            else if (InkwaveInput.GetKeyDown(KeyCode.Return)) GoTo(InkwaveScreenManager.Screen.Menu);
            else if (InkwaveInput.GetKeyDown(KeyCode.Escape)) GoTo(InkwaveScreenManager.Screen.Menu);
        }
    }
}
