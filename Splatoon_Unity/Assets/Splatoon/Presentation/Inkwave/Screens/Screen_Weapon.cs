using UnityEngine;
using UnityEngine.UIElements;

namespace Splatoon.Presentation.Inkwave.Screens
{
    /// <summary>
    /// INKWAVE 04 ブキ選択画面。カテゴリ7+ブキ6 + 詳細パネル。
    /// </summary>
    public class Screen_Weapon : InkwaveScreenBase
    {
        struct WeaponDef
        {
            public string Name, Cat, Sub, Sp, Tag, Rarity, Ck;
            public int SpPt, Tier, Dmg, Fire, Range, Mob;
        }

        readonly string[] _cats = { "すべて", "シューター", "ローラー", "チャージャー", "デュアル", "スロッシャー", "ブレード" };

        readonly WeaponDef[] _weapons = new WeaponDef[]
        {
            new() { Name="タイド-08 スプレイヤー", Cat="シューター", Sub="スプラッシュボム", Sp="ウェーブバースト", SpPt=180, Tier=3, Rarity="RARE", Tag="バランス", Ck="shooter", Dmg=36, Fire=78, Range=52, Mob=70 },
            new() { Name="ケルプ ローラー", Cat="ローラー", Sub="カールボム", Sp="インクストーム", SpPt=210, Tier=2, Rarity="COMMON", Tag="近接強", Ck="roller", Dmg=90, Fire=22, Range=24, Mob=56 },
            new() { Name="ロングショア MK-2", Cat="チャージャー", Sub="スプラットマイン", Sp="ストームフロント", SpPt=200, Tier=4, Rarity="EPIC", Tag="射程最長", Ck="charger", Dmg=180, Fire=14, Range=96, Mob=32 },
            new() { Name="ツイン ハープーン", Cat="デュアル", Sub="オートボム", Sp="クラブウォーカー", SpPt=190, Tier=3, Rarity="RARE", Tag="機動最高", Ck="dualies", Dmg=28, Fire=88, Range=40, Mob=90 },
            new() { Name="スロッシュ バケット", Cat="スロッシャー", Sub="トーピード", Sp="インクジェット", SpPt=200, Tier=2, Rarity="COMMON", Tag="対地強", Ck="slosher", Dmg=70, Fire=38, Range=58, Mob=48 },
            new() { Name="グラナイト スプラタナ", Cat="ブレード", Sub="クイックボム", Sp="ウェーブバースト", SpPt=170, Tier=4, Rarity="EPIC", Tag="一撃必殺", Ck="splatana", Dmg=60, Fire=56, Range=36, Mob=80 }
        };

        int _catIdx = 0;
        int _active = 0;
        bool _equipped = false;

        /// <summary>UI構築。</summary>
        protected override void BindUI()
        {
            _root.Q<Button>("btn-back").clicked += () => GoTo(InkwaveScreenManager.Screen.Menu);
            _root.Q<Button>("btn-equip").clicked += () => { _equipped = true; BuildAll(); };
            BuildAll();
        }

        /// <summary>全体再構築。</summary>
        void BuildAll()
        {
            BuildCatBar();
            BuildWeaponList();
            BuildHero();
            BuildRightCards();
        }

        /// <summary>カテゴリタブ生成。</summary>
        void BuildCatBar()
        {
            var bar = _root.Q<VisualElement>("cat-bar");
            bar.Clear();
            for (int i = 0; i < _cats.Length; i++)
            {
                int idx = i;
                var b = new Button(() => { _catIdx = idx; BuildWeaponList(); });
                b.text = _cats[i];
                b.AddToClassList("iw-tab");
                if (i == _catIdx) b.AddToClassList("iw-tab-active");
                b.style.fontSize = 10;
                b.style.paddingLeft = 8; b.style.paddingRight = 8;
                b.style.paddingTop = 6; b.style.paddingBottom = 6;
                b.style.marginRight = 4; b.style.marginBottom = 4;
                bar.Add(b);
            }
        }

        /// <summary>ブキリスト生成。</summary>
        void BuildWeaponList()
        {
            var list = _root.Q<VisualElement>("weapon-list");
            list.Clear();
            var header = new Label($"ブキを選ぶ · {_weapons.Length}本");
            header.AddToClassList("iw-label");
            header.style.fontSize = 10; header.style.color = new Color(0.541f, 0.541f, 0.572f);
            header.style.paddingLeft = 8; header.style.paddingRight = 8;
            header.style.paddingTop = 4; header.style.paddingBottom = 4;
            list.Add(header);

            for (int i = 0; i < _weapons.Length; i++)
            {
                int idx = i;
                var w = _weapons[i];
                var row = new VisualElement();
                row.AddToClassList("iw-row");
                if (idx == _active) row.AddToClassList("iw-row-selected");
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.paddingLeft = 10; row.style.paddingRight = 10;
                row.style.paddingTop = 10; row.style.paddingBottom = 10;
                row.style.marginTop = 6;
                row.style.borderTopWidth = 2; row.style.borderBottomWidth = 2;
                row.style.borderLeftWidth = 2; row.style.borderRightWidth = 2;
                row.style.borderTopColor = new Color(0.051f, 0.051f, 0.063f);
                row.style.borderBottomColor = new Color(0.051f, 0.051f, 0.063f);
                row.style.borderLeftColor = new Color(0.051f, 0.051f, 0.063f);
                row.style.borderRightColor = new Color(0.051f, 0.051f, 0.063f);
                row.style.backgroundColor = idx == _active ? new Color(0.165f, 0.165f, 0.2f) : new Color(0.114f, 0.114f, 0.141f);

                // 武器サムネ(簡易)
                var thumb = new VisualElement();
                thumb.style.width = 52; thumb.style.height = 36;
                thumb.style.backgroundColor = new Color(0.051f, 0.051f, 0.063f);
                thumb.style.borderTopWidth = 2; thumb.style.borderBottomWidth = 2;
                thumb.style.borderLeftWidth = 2; thumb.style.borderRightWidth = 2;
                thumb.style.borderTopColor = new Color(0.051f, 0.051f, 0.063f);
                thumb.style.borderBottomColor = new Color(0.051f, 0.051f, 0.063f);
                thumb.style.borderLeftColor = new Color(0.051f, 0.051f, 0.063f);
                thumb.style.borderRightColor = new Color(0.051f, 0.051f, 0.063f);
                thumb.style.marginRight = 10;
                row.Add(thumb);

                // 名前+ラベル
                var info = new VisualElement();
                info.style.flexGrow = 1;
                var name = new Label(w.Name);
                name.style.fontSize = 12; name.style.unityFontStyleAndWeight = FontStyle.Bold;
                name.style.color = new Color(0.957f, 0.957f, 0.941f);
                info.Add(name);
                var rare = new VisualElement();
                rare.style.flexDirection = FlexDirection.Row;
                var raL = new Label(w.Rarity);
                raL.style.fontSize = 9; raL.style.color = RarityColor(w.Rarity); raL.style.marginRight = 8;
                rare.Add(raL);
                var cat = new Label(w.Cat);
                cat.AddToClassList("iw-label");
                cat.style.fontSize = 9; cat.style.color = new Color(0.541f, 0.541f, 0.572f);
                rare.Add(cat);
                info.Add(rare);
                row.Add(info);

                // ティアバー
                var tier = new VisualElement();
                tier.style.flexDirection = FlexDirection.ColumnReverse;
                for (int t = 0; t < w.Tier; t++)
                {
                    var bar = new VisualElement();
                    bar.style.width = 14; bar.style.height = 3;
                    bar.style.backgroundColor = new Color(1f, 0.839f, 0f);
                    bar.style.marginBottom = 2;
                    tier.Add(bar);
                }
                row.Add(tier);

                row.RegisterCallback<ClickEvent>(e => { _active = idx; _equipped = false; BuildAll(); });
                list.Add(row);
            }
        }

        /// <summary>中央ヒーロー生成。</summary>
        void BuildHero()
        {
            var hero = _root.Q<VisualElement>("hero");
            hero.Clear();
            var w = _weapons[_active];
            Color rc = RarityColor(w.Rarity);

            // タイトル帯
            var title = new VisualElement();
            title.style.position = Position.Absolute;
            title.style.top = 0; title.style.left = 0; title.style.right = 0;
            title.style.paddingLeft = 18; title.style.paddingRight = 18;
            title.style.paddingTop = 14; title.style.paddingBottom = 14;
            title.style.backgroundColor = new Color(0, 0, 0, 0.55f);
            title.style.flexDirection = FlexDirection.Row;
            title.style.justifyContent = Justify.SpaceBetween;

            var titleLeft = new VisualElement();
            var rare = new VisualElement();
            rare.style.flexDirection = FlexDirection.Row;
            var rareB = new Label(w.Rarity);
            rareB.style.fontSize = 10; rareB.style.color = new Color(0.957f, 0.957f, 0.941f);
            rareB.style.backgroundColor = rc; rareB.style.paddingLeft = 6; rareB.style.paddingRight = 6;
            rareB.style.paddingTop = 2; rareB.style.paddingBottom = 2;
            rareB.style.marginRight = 8; rareB.style.unityFontStyleAndWeight = FontStyle.Bold;
            rare.Add(rareB);
            var catL = new Label($"{w.Cat} · ティア {w.Tier}");
            catL.AddToClassList("iw-label");
            catL.style.fontSize = 10; catL.style.color = new Color(0.541f, 0.541f, 0.572f);
            catL.style.unityTextAlign = TextAnchor.MiddleLeft;
            rare.Add(catL);
            titleLeft.Add(rare);

            var name = new Label(w.Name);
            name.AddToClassList("iw-head");
            name.AddToClassList("iw-text-outline");
            name.style.fontSize = 38; name.style.color = new Color(0.957f, 0.957f, 0.941f);
            name.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLeft.Add(name);
            title.Add(titleLeft);

            if (_equipped)
            {
                var st = new IwSticker();
                st.SetSticker("装備しました!", new Color(0.255f, 0.878f, 0.478f), new Color(0.051f, 0.051f, 0.063f), -3f);
                title.Add(st);
            }
            else
            {
                var st = new IwSticker();
                st.SetSticker(w.Tag, new Color(1f, 0.106f, 0.42f), new Color(0.957f, 0.957f, 0.941f), -3f);
                title.Add(st);
            }
            hero.Add(title);

            // キャラ
            var ch = new IwInkChar();
            ch.AddToClassList("iw-anim-pop");
            ch.Configure("aim", "a", w.Ck, 280);
            ch.style.position = Position.Absolute;
            ch.style.left = 24; ch.style.bottom = 80;
            hero.Add(ch);

            // 射程ラベル
            var rangeSticker = new IwSticker();
            rangeSticker.SetSticker($"射程 {w.Range}m", new Color(0.957f, 0.957f, 0.941f), new Color(0.051f, 0.051f, 0.063f), 4f);
            rangeSticker.style.position = Position.Absolute;
            rangeSticker.style.right = 40; rangeSticker.style.top = 130;
            hero.Add(rangeSticker);

            // ステータスフッター
            var footer = new VisualElement();
            footer.style.position = Position.Absolute;
            footer.style.bottom = 0; footer.style.left = 0; footer.style.right = 0;
            footer.style.paddingLeft = 22; footer.style.paddingRight = 22;
            footer.style.paddingTop = 16; footer.style.paddingBottom = 16;
            footer.style.backgroundColor = new Color(0, 0, 0, 0.85f);
            footer.style.flexDirection = FlexDirection.Row;

            AddStat(footer, "ダメージ", w.Dmg, rc);
            AddStat(footer, "連射", w.Fire, rc);
            AddStat(footer, "射程", w.Range, rc);
            AddStat(footer, "機動", w.Mob, rc);
            hero.Add(footer);
        }

        /// <summary>ステータス1個追加。</summary>
        void AddStat(VisualElement parent, string label, int v, Color rc)
        {
            var s = new VisualElement();
            s.style.flexGrow = 1; s.style.marginRight = 8;
            var topR = new VisualElement();
            topR.style.flexDirection = FlexDirection.Row;
            topR.style.justifyContent = Justify.SpaceBetween;
            var l = new Label(label);
            l.AddToClassList("iw-label");
            l.style.fontSize = 10; l.style.color = new Color(0.541f, 0.541f, 0.572f);
            topR.Add(l);
            var vn = new Label(v.ToString());
            vn.AddToClassList("iw-num");
            vn.style.fontSize = 13; vn.style.color = new Color(0.957f, 0.957f, 0.941f);
            topR.Add(vn);
            s.Add(topR);

            var bar = new IwBar();
            bar.SetBar(Mathf.Min(100, v), rc, 8);
            bar.style.marginTop = 4;
            s.Add(bar);
            parent.Add(s);
        }

        /// <summary>右カラムカード生成。</summary>
        void BuildRightCards()
        {
            var w = _weapons[_active];

            var mc = _root.Q<VisualElement>("main-weapon-card");
            mc.Clear();
            var ml = new Label("メインブキ"); ml.AddToClassList("iw-label");
            ml.style.fontSize = 10; ml.style.color = new Color(0.541f, 0.541f, 0.572f);
            mc.Add(ml);
            var mrow = new VisualElement(); mrow.style.flexDirection = FlexDirection.Row;
            mrow.style.alignItems = Align.Center; mrow.style.marginTop = 6;
            var mthumb = new VisualElement();
            mthumb.style.width = 64; mthumb.style.height = 50;
            mthumb.style.backgroundColor = new Color(0.114f, 0.114f, 0.141f);
            mthumb.style.borderTopWidth = 2; mthumb.style.borderBottomWidth = 2;
            mthumb.style.borderLeftWidth = 2; mthumb.style.borderRightWidth = 2;
            mthumb.style.borderTopColor = new Color(0.051f, 0.051f, 0.063f);
            mthumb.style.borderBottomColor = new Color(0.051f, 0.051f, 0.063f);
            mthumb.style.borderLeftColor = new Color(0.051f, 0.051f, 0.063f);
            mthumb.style.borderRightColor = new Color(0.051f, 0.051f, 0.063f);
            mthumb.style.marginRight = 10;
            mrow.Add(mthumb);
            var mwrap = new VisualElement();
            var mn = new Label(w.Name); mn.AddToClassList("iw-num");
            mn.style.fontSize = 12; mn.style.color = new Color(0.957f, 0.957f, 0.941f);
            mwrap.Add(mn);
            var minkc = new Label("インク消費 · 0.9%/発");
            minkc.style.fontSize = 10; minkc.style.color = new Color(0.541f, 0.541f, 0.572f);
            mwrap.Add(minkc);
            mrow.Add(mwrap);
            mc.Add(mrow);

            // サブ
            var sc = _root.Q<VisualElement>("sub-card");
            sc.Clear();
            var sl = new Label("サブウェポン"); sl.AddToClassList("iw-label");
            sl.style.fontSize = 10; sl.style.color = new Color(0.541f, 0.541f, 0.572f);
            sc.Add(sl);
            var srow = new VisualElement(); srow.style.flexDirection = FlexDirection.Row;
            srow.style.alignItems = Align.Center; srow.style.marginTop = 6;
            var sbubble = new VisualElement();
            sbubble.style.width = 30; sbubble.style.height = 30;
            sbubble.style.backgroundColor = new Color(1f, 0.106f, 0.42f);
            sbubble.style.borderTopLeftRadius = Length.Percent(40);
            sbubble.style.borderTopRightRadius = Length.Percent(60);
            sbubble.style.borderBottomLeftRadius = Length.Percent(50);
            sbubble.style.borderBottomRightRadius = Length.Percent(50);
            sbubble.style.marginRight = 10;
            srow.Add(sbubble);
            var swrap = new VisualElement(); swrap.style.flexGrow = 1;
            var sn = new Label(w.Sub); sn.style.fontSize = 12;
            sn.style.color = new Color(0.957f, 0.957f, 0.941f);
            swrap.Add(sn);
            var sinkc = new Label("インク消費 · 70%");
            sinkc.style.fontSize = 10; sinkc.style.color = new Color(0.541f, 0.541f, 0.572f);
            swrap.Add(sinkc);
            var sb = new IwBar(); sb.SetBar(70, new Color(1f, 0.106f, 0.42f), 4); sb.style.marginTop = 4;
            swrap.Add(sb);
            srow.Add(swrap);
            sc.Add(srow);

            // SP
            var spc = _root.Q<VisualElement>("sp-card");
            spc.Clear();
            spc.style.backgroundColor = Color.Lerp(new Color(0.082f, 0.082f, 0.102f), new Color(0f, 0.898f, 1f), 0.13f);
            var spl = new Label("スペシャル"); spl.AddToClassList("iw-label");
            spl.style.fontSize = 10; spl.style.color = new Color(0f, 0.898f, 1f);
            spc.Add(spl);
            var sprow = new VisualElement(); sprow.style.flexDirection = FlexDirection.Row;
            sprow.style.alignItems = Align.Center; sprow.style.marginTop = 6;
            var spdia = new VisualElement();
            spdia.AddToClassList("iw-anim-glow");
            spdia.style.width = 26; spdia.style.height = 26;
            spdia.style.backgroundColor = new Color(0f, 0.898f, 1f);
            spdia.style.rotate = new StyleRotate(new Rotate(45));
            spdia.style.marginRight = 14;
            sprow.Add(spdia);
            var spwrap = new VisualElement(); spwrap.style.flexGrow = 1;
            var spn = new Label(w.Sp); spn.style.fontSize = 12;
            spn.style.color = new Color(0.957f, 0.957f, 0.941f);
            spwrap.Add(spn);
            var sppt = new Label($"必要pt · {w.SpPt}");
            sppt.style.fontSize = 10; sppt.style.color = new Color(0.541f, 0.541f, 0.572f);
            spwrap.Add(sppt);
            sprow.Add(spwrap);
            spc.Add(sprow);

            // 装備ボタンテキスト
            var btn = _root.Q<Button>("btn-equip");
            btn.text = _equipped ? "✓ 装備中" : "このブキを装備";
        }

        /// <summary>レアリティ色。</summary>
        Color RarityColor(string r) => r switch
        {
            "EPIC" => new Color(0.698f, 0.42f, 1f),
            "RARE" => new Color(0f, 0.898f, 1f),
            _ => new Color(0.541f, 0.541f, 0.572f)
        };

        /// <summary>キー入力。</summary>
        void Update()
        {
            if (_root == null) return;
            if (!IsActiveScreen(InkwaveScreenManager.Screen.Weapon)) return;
            if (!IsInputAllowed()) return;
            if (InkwaveInput.GetKeyDown(KeyCode.W))
            {
                _active = (_active - 1 + _weapons.Length) % _weapons.Length;
                _equipped = false; BuildAll();
            }
            else if (InkwaveInput.GetKeyDown(KeyCode.S))
            {
                _active = (_active + 1) % _weapons.Length;
                _equipped = false; BuildAll();
            }
            else if (InkwaveInput.GetKeyDown(KeyCode.Tab))
            {
                _catIdx = (_catIdx + 1) % _cats.Length; BuildCatBar();
            }
            else if (InkwaveInput.GetKeyDown(KeyCode.Return)) { _equipped = true; BuildAll(); }
            else if (InkwaveInput.GetKeyDown(KeyCode.Escape)) GoTo(InkwaveScreenManager.Screen.Menu);
        }
    }
}
