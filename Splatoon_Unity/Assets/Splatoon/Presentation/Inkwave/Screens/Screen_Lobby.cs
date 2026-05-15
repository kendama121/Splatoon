using UnityEngine;
using UnityEngine.UIElements;

namespace Splatoon.Presentation.Inkwave.Screens
{
    /// <summary>
    /// INKWAVE 05 ロビー画面。
    /// 2チーム x 4人タイル、ステージプレビュー、チャット、準備ボタン。
    /// </summary>
    public class Screen_Lobby : InkwaveScreenBase
    {
        struct Player
        {
            public string Name, Weapon;
            public int Lv, Ping;
            public bool You, Searching;
        }

        readonly Player[] _teamA = new Player[]
        {
            new() { Name="カイ-07", Weapon="タイド-08 スプレイヤー", Lv=28, Ping=22, You=true },
            new() { Name="マリィ", Weapon="ケルプ ローラー", Lv=41, Ping=18 },
            new() { Name="オックスボウ", Weapon="ロングショア MK-2", Lv=33, Ping=31 },
            new() { Name="ニンバス", Weapon="—", Lv=0, Ping=0, Searching=true }
        };

        bool _ready = false;

        /// <summary>UI構築。再表示時に _ready をリセットする。ボタン1クリックで即マッチ開始(Loading遷移)。</summary>
        protected override void BindUI()
        {
            _ready = false;
            BuildTeamA();
            BuildTeamB();
            BuildStageCard();
            BuildChatCard();
            var btn = _root.Q<Button>("btn-ready");
            if (btn != null)
            {
                // 強制テキスト + スタイル設定(UXMLキャッシュ問題回避)
                btn.text = "▶ 試合開始 (クリック)";
                btn.style.height = 70;
                btn.style.fontSize = 22;
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;
                btn.style.paddingLeft = 28; btn.style.paddingRight = 28;
                btn.style.backgroundColor = new Color(1f, 0.106f, 0.42f);
                btn.style.color = new Color(0.957f, 0.957f, 0.941f);
                btn.style.borderTopWidth = 0; btn.style.borderBottomWidth = 0;
                btn.style.borderLeftWidth = 0; btn.style.borderRightWidth = 0;
                btn.style.borderTopLeftRadius = 8; btn.style.borderTopRightRadius = 8;
                btn.style.borderBottomLeftRadius = 8; btn.style.borderBottomRightRadius = 8;
                btn.style.unityTextAlign = TextAnchor.MiddleCenter;

                // 1クリックで即 Loading 画面へ遷移(=試合開始)
                btn.clicked += () => GoTo(InkwaveScreenManager.Screen.Loading);
            }
            UpdateReadyButton();
        }

        /// <summary>チームAビルド。</summary>
        void BuildTeamA()
        {
            var c = _root.Q<VisualElement>("team-a");
            c.Clear();
            var head = new VisualElement(); head.style.flexDirection = FlexDirection.Row;
            head.style.justifyContent = Justify.SpaceBetween; head.style.alignItems = Align.FlexEnd;
            var ti = new Label("チーム アルファ");
            ti.AddToClassList("iw-head");
            ti.style.fontSize = 22; ti.style.color = new Color(1f, 0.106f, 0.42f);
            head.Add(ti);
            var sub = new Label("4 / 4 準備 · 残り 3名 マッチング中");
            sub.AddToClassList("iw-label"); sub.style.fontSize = 10;
            sub.style.color = new Color(0.541f, 0.541f, 0.572f);
            head.Add(sub);
            c.Add(head);

            var grid = new VisualElement(); grid.style.flexDirection = FlexDirection.Row;
            grid.style.marginTop = 16;
            for (int i = 0; i < _teamA.Length; i++)
            {
                var p = _teamA[i];
                var tile = BuildPlayerTile(p, true);
                tile.style.flexGrow = 1; tile.style.marginRight = 12;
                if (i == _teamA.Length - 1) tile.style.marginRight = 0;
                grid.Add(tile);
            }
            c.Add(grid);
        }

        /// <summary>プレイヤータイル生成。</summary>
        VisualElement BuildPlayerTile(Player p, bool teamA)
        {
            var tile = new VisualElement();
            tile.AddToClassList("iw-tile");
            tile.style.paddingLeft = 12; tile.style.paddingRight = 12;
            tile.style.paddingTop = 12; tile.style.paddingBottom = 12;
            tile.style.backgroundColor = p.You ? new Color(0.165f, 0.165f, 0.2f) : new Color(0.114f, 0.114f, 0.141f);
            tile.style.borderTopWidth = 3;
            Color tc = teamA ? new Color(1f, 0.106f, 0.42f) : new Color(0f, 0.898f, 1f);
            tile.style.borderTopColor = tc;
            tile.style.borderBottomColor = new Color(0.204f, 0.204f, 0.247f);
            tile.style.borderLeftColor = new Color(0.204f, 0.204f, 0.247f);
            tile.style.borderRightColor = new Color(0.204f, 0.204f, 0.247f);
            tile.style.borderBottomWidth = 1; tile.style.borderLeftWidth = 1; tile.style.borderRightWidth = 1;
            tile.style.position = Position.Relative;
            tile.style.opacity = p.Searching ? 0.55f : 1f;

            var top = new VisualElement();
            top.style.flexDirection = FlexDirection.Row;
            top.style.justifyContent = Justify.SpaceBetween;
            var av = new IwAvatar(); av.Configure(teamA ? "a" : "b", 56, "", false);
            top.Add(av);
            if (p.You)
            {
                var youBadge = new Label("YOU");
                youBadge.AddToClassList("iw-chip");
                youBadge.style.backgroundColor = tc;
                youBadge.style.color = new Color(0.957f, 0.957f, 0.941f);
                youBadge.style.paddingLeft = 6; youBadge.style.paddingRight = 6;
                youBadge.style.fontSize = 10;
                top.Add(youBadge);
            }
            tile.Add(top);

            var name = new Label(p.Name);
            name.AddToClassList("iw-num");
            name.style.fontSize = 13; name.style.color = new Color(0.957f, 0.957f, 0.941f);
            name.style.marginTop = 8;
            tile.Add(name);

            var lvl = new Label(p.Lv > 0 ? $"Lv.{p.Lv} · {p.Ping}ms" : "— · —");
            lvl.AddToClassList("iw-label");
            lvl.style.fontSize = 10; lvl.style.color = new Color(0.541f, 0.541f, 0.572f);
            tile.Add(lvl);

            var w = new Label(p.Weapon);
            w.AddToClassList("iw-num");
            w.style.fontSize = 10; w.style.color = new Color(0.541f, 0.541f, 0.572f);
            w.style.marginTop = 10;
            tile.Add(w);

            if (p.Searching)
            {
                var overlay = new Label("マッチング中…");
                overlay.AddToClassList("iw-anim-blink");
                overlay.style.position = Position.Absolute;
                overlay.style.top = 0; overlay.style.left = 0; overlay.style.right = 0; overlay.style.bottom = 0;
                overlay.style.unityTextAlign = TextAnchor.MiddleCenter;
                overlay.style.fontSize = 10; overlay.style.color = new Color(0.541f, 0.541f, 0.572f);
                overlay.style.letterSpacing = 2;
                tile.Add(overlay);
            }
            return tile;
        }

        /// <summary>チームB(HIDDEN表示)ビルド。</summary>
        void BuildTeamB()
        {
            var c = _root.Q<VisualElement>("team-b");
            c.Clear();
            var head = new VisualElement(); head.style.flexDirection = FlexDirection.Row;
            head.style.justifyContent = Justify.SpaceBetween; head.style.alignItems = Align.FlexEnd;
            var ti = new Label("チーム ブラボー");
            ti.AddToClassList("iw-head");
            ti.style.fontSize = 22; ti.style.color = new Color(0f, 0.898f, 1f);
            head.Add(ti);
            var sub = new Label("試合開始まで非公開");
            sub.AddToClassList("iw-label"); sub.style.fontSize = 10;
            sub.style.color = new Color(0.541f, 0.541f, 0.572f);
            head.Add(sub);
            c.Add(head);

            var grid = new VisualElement(); grid.style.flexDirection = FlexDirection.Row;
            grid.style.marginTop = 16;
            for (int i = 0; i < 4; i++)
            {
                var tile = new VisualElement();
                tile.style.flexGrow = 1;
                tile.style.paddingLeft = 12; tile.style.paddingRight = 12;
                tile.style.paddingTop = 12; tile.style.paddingBottom = 12;
                tile.style.backgroundColor = new Color(0.114f, 0.114f, 0.141f);
                tile.style.borderTopWidth = 3; tile.style.borderTopColor = new Color(0f, 0.898f, 1f);
                tile.style.borderBottomWidth = 1; tile.style.borderLeftWidth = 1; tile.style.borderRightWidth = 1;
                tile.style.borderBottomColor = new Color(0.204f, 0.204f, 0.247f);
                tile.style.borderLeftColor = new Color(0.204f, 0.204f, 0.247f);
                tile.style.borderRightColor = new Color(0.204f, 0.204f, 0.247f);
                tile.style.marginRight = i < 3 ? 12 : 0;

                var portrait = new VisualElement();
                portrait.style.width = 56; portrait.style.height = 56;
                portrait.style.backgroundColor = new Color(0.165f, 0.165f, 0.2f);
                portrait.style.alignItems = Align.Center; portrait.style.justifyContent = Justify.Center;
                var q = new Label("?");
                q.AddToClassList("iw-num");
                q.style.fontSize = 22; q.style.color = new Color(0f, 0.898f, 1f);
                portrait.Add(q);
                tile.Add(portrait);

                var na = new Label("— · — · —");
                na.AddToClassList("iw-num");
                na.style.fontSize = 13; na.style.color = new Color(0.541f, 0.541f, 0.572f);
                na.style.marginTop = 8;
                tile.Add(na);
                var h = new Label("HIDDEN");
                h.AddToClassList("iw-label");
                h.style.fontSize = 10; h.style.color = new Color(0.541f, 0.541f, 0.572f);
                tile.Add(h);
                grid.Add(tile);
            }
            c.Add(grid);
        }

        /// <summary>ステージカード。</summary>
        void BuildStageCard()
        {
            var c = _root.Q<VisualElement>("stage-card");
            c.Clear();
            var l = new Label("次のステージ");
            l.AddToClassList("iw-label");
            l.style.fontSize = 10; l.style.color = new Color(0.541f, 0.541f, 0.572f);
            c.Add(l);
            var slot = new IwImgSlot();
            slot.SetLabel("ステージプレビュー");
            slot.style.height = 120; slot.style.marginTop = 6;
            c.Add(slot);
            var name = new Label("WAVELINE ターミナル");
            name.AddToClassList("iw-head");
            name.style.fontSize = 18; name.style.color = new Color(0.957f, 0.957f, 0.941f);
            name.style.marginTop = 10;
            c.Add(name);
            var sub = new Label("ハーバー地区 · 22m × 14m");
            sub.AddToClassList("iw-label");
            sub.style.fontSize = 10; sub.style.color = new Color(0.541f, 0.541f, 0.572f);
            sub.style.marginTop = 4;
            c.Add(sub);
            var tags = new VisualElement(); tags.style.flexDirection = FlexDirection.Row;
            tags.style.flexWrap = Wrap.Wrap; tags.style.marginTop = 10;
            foreach (var t in new[] { "見通し良好", "狭路多め", "金網あり" })
            {
                var chip = new IwChip(); chip.SetChip(t, false);
                chip.style.marginRight = 6; chip.style.marginBottom = 4;
                tags.Add(chip);
            }
            c.Add(tags);
        }

        /// <summary>チャットカード。</summary>
        void BuildChatCard()
        {
            var c = _root.Q<VisualElement>("chat-card");
            c.Clear();
            var l = new Label("チームチャット");
            l.AddToClassList("iw-label");
            l.style.fontSize = 10; l.style.color = new Color(0.541f, 0.541f, 0.572f);
            c.Add(l);
            var msgs = new (string name, string time, string msg)[]
            {
                ("マリィ", "19:42", "左フランク行く、金網に注意"),
                ("オックスボウ", "19:42", "チャージャー高台もらった"),
                ("カイ-07", "19:43", "了解、1:30 までにバースト溜める")
            };
            var msgArea = new VisualElement(); msgArea.style.flexGrow = 1; msgArea.style.marginTop = 10;
            foreach (var m in msgs)
            {
                var line = new VisualElement(); line.style.marginBottom = 8;
                var hdr = new VisualElement(); hdr.style.flexDirection = FlexDirection.Row;
                var nm = new Label(m.name);
                nm.style.color = new Color(1f, 0.106f, 0.42f);
                nm.style.unityFontStyleAndWeight = FontStyle.Bold;
                nm.style.fontSize = 11;
                hdr.Add(nm);
                var tm = new Label($" · {m.time}");
                tm.style.color = new Color(0.541f, 0.541f, 0.572f);
                tm.style.fontSize = 11;
                hdr.Add(tm);
                line.Add(hdr);
                var body = new Label(m.msg);
                body.style.fontSize = 11; body.style.color = new Color(0.957f, 0.957f, 0.941f);
                line.Add(body);
                msgArea.Add(line);
            }
            c.Add(msgArea);

            var input = new VisualElement();
            input.style.flexDirection = FlexDirection.Row;
            input.style.paddingLeft = 10; input.style.paddingRight = 10;
            input.style.paddingTop = 8; input.style.paddingBottom = 8;
            input.style.backgroundColor = new Color(0.114f, 0.114f, 0.141f);
            input.style.marginTop = 10;
            var prompt = new Label("›");
            prompt.style.color = new Color(0.541f, 0.541f, 0.572f); prompt.style.fontSize = 11;
            prompt.style.marginRight = 8;
            input.Add(prompt);
            var ph = new Label("メッセージ または 定型句…");
            ph.style.color = new Color(0.541f, 0.541f, 0.572f); ph.style.fontSize = 11;
            input.Add(ph);
            c.Add(input);
        }

        /// <summary>準備ボタン更新。本実装ではクリック即遷移のためテキストは固定(「▶ 試合開始」)。</summary>
        void UpdateReadyButton()
        {
            // UXML 側で text/class 指定済み(▶ 試合開始 + iw-btn-primary + iw-anim-glow)。何もしない。
        }

        /// <summary>キー入力。</summary>
        void Update()
        {
            if (_root == null) return;
            if (!IsActiveScreen(InkwaveScreenManager.Screen.Lobby)) return;
            if (!IsInputAllowed()) return;
            if (InkwaveInput.GetKeyDown(KeyCode.R)) { _ready = !_ready; UpdateReadyButton(); }
            else if (InkwaveInput.GetKeyDown(KeyCode.L)) GoTo(InkwaveScreenManager.Screen.Weapon);
            else if (InkwaveInput.GetKeyDown(KeyCode.Escape)) GoTo(InkwaveScreenManager.Screen.Menu);
            else if (InkwaveInput.GetKeyDown(KeyCode.Return) && _ready) GoTo(InkwaveScreenManager.Screen.Loading);
        }
    }
}
