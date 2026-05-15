using UnityEngine;
using UnityEngine.UIElements;

namespace Splatoon.Presentation.Inkwave.Screens
{
    /// <summary>
    /// INKWAVE 11 リザルト画面。CountUp付きヒーローバナー+両チームテーブル+MVPカード。
    /// </summary>
    public class Screen_Results : InkwaveScreenBase
    {
        struct PlayerRow { public string Name; public int K, D, Paint, Sp; public bool Mvp, You; }

        readonly PlayerRow[] _alpha = new[]
        {
            new PlayerRow { Name="カイ-07", K=8, D=4, Paint=1242, Sp=3, Mvp=true, You=true },
            new PlayerRow { Name="マリィ", K=5, D=6, Paint=980, Sp=2 },
            new PlayerRow { Name="オックスボウ", K=9, D=3, Paint=740, Sp=2 },
            new PlayerRow { Name="ニンバス", K=4, D=5, Paint=1112, Sp=4 }
        };

        readonly PlayerRow[] _bravo = new[]
        {
            new PlayerRow { Name="リフト", K=7, D=6, Paint=1018, Sp=3 },
            new PlayerRow { Name="エコー", K=4, D=7, Paint=822, Sp=2 },
            new PlayerRow { Name="ヘイロー", K=6, D=6, Paint=956, Sp=3 },
            new PlayerRow { Name="クロウ", K=5, D=7, Paint=740, Sp=2 }
        };

        float _aTarget = 52.4f, _bTarget = 48.0f;
        float _aCur, _bCur;
        Label _countA, _countB;

        /// <summary>UI構築。</summary>
        protected override void BindUI()
        {
            _aCur = 0; _bCur = 0;
            _countA = _root.Q<Label>("count-a");
            _countB = _root.Q<Label>("count-b");
            BuildTable("table-a", "アルファ", new Color(1f, 0.106f, 0.42f), _alpha);
            BuildTable("table-b", "ブラボー", new Color(0f, 0.898f, 1f), _bravo);
            BuildMvp();
        }

        /// <summary>チームテーブル構築。</summary>
        void BuildTable(string name, string title, Color tc, PlayerRow[] rows)
        {
            var c = _root.Q<VisualElement>(name);
            c.Clear();
            var head = new VisualElement();
            head.style.flexDirection = FlexDirection.Row;
            head.style.justifyContent = Justify.SpaceBetween;
            head.style.alignItems = Align.FlexEnd;
            var ti = new Label(title);
            ti.AddToClassList("iw-head");
            ti.style.fontSize = 22; ti.style.color = tc;
            head.Add(ti);
            var sub = new Label("プレイヤー · K/D · 塗り · SP");
            sub.AddToClassList("iw-label");
            sub.style.fontSize = 9; sub.style.color = new Color(0.541f, 0.541f, 0.572f);
            head.Add(sub);
            c.Add(head);

            for (int i = 0; i < rows.Length; i++)
            {
                var r = rows[i];
                var row = new VisualElement();
                row.AddToClassList("iw-anim-slide");
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.paddingLeft = 10; row.style.paddingRight = 10;
                row.style.paddingTop = 8; row.style.paddingBottom = 8;
                row.style.marginTop = 6;
                row.style.backgroundColor = r.You ? new Color(0.165f, 0.165f, 0.2f) : new Color(0.114f, 0.114f, 0.141f);
                row.style.borderLeftWidth = 3; row.style.borderLeftColor = tc;

                var av = new IwAvatar();
                av.Configure(name == "table-a" ? "a" : "b", 28, "", false);
                av.style.marginRight = 8;
                row.Add(av);

                var nameWrap = new VisualElement(); nameWrap.style.flexGrow = 1;
                var nameLbl = new Label(r.Name);
                nameLbl.style.fontSize = 12; nameLbl.style.color = new Color(0.957f, 0.957f, 0.941f);
                nameLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
                if (r.Mvp) nameLbl.text = $"{r.Name} ★MVP";
                nameWrap.Add(nameLbl);
                var bar = new IwBar();
                bar.SetBar(Mathf.Min(100, r.Paint / 14), tc, 4);
                bar.style.marginTop = 2;
                nameWrap.Add(bar);
                row.Add(nameWrap);

                var kd = new Label($"{r.K}/{r.D}");
                kd.AddToClassList("iw-num");
                kd.style.fontSize = 11; kd.style.color = new Color(0.957f, 0.957f, 0.941f);
                kd.style.width = 50; kd.style.unityTextAlign = TextAnchor.MiddleCenter;
                row.Add(kd);

                var paint = new Label(r.Paint.ToString("N0"));
                paint.AddToClassList("iw-num");
                paint.style.fontSize = 11; paint.style.color = new Color(0.957f, 0.957f, 0.941f);
                paint.style.width = 70; paint.style.unityTextAlign = TextAnchor.MiddleCenter;
                row.Add(paint);

                var sp = new Label($"×{r.Sp}");
                sp.AddToClassList("iw-num");
                sp.style.fontSize = 11; sp.style.color = new Color(0.541f, 0.541f, 0.572f);
                sp.style.width = 30; sp.style.unityTextAlign = TextAnchor.MiddleCenter;
                row.Add(sp);
                c.Add(row);
            }
        }

        /// <summary>MVPカード構築。</summary>
        void BuildMvp()
        {
            var c = _root.Q<VisualElement>("mvp-card");
            c.Clear();
            var l = new Label("あなたの試合 · MVP");
            l.AddToClassList("iw-label");
            l.style.fontSize = 10; l.style.color = new Color(0.541f, 0.541f, 0.572f);
            c.Add(l);

            var top = new VisualElement();
            top.style.flexDirection = FlexDirection.Row;
            top.style.alignItems = Align.Center;
            top.style.marginTop = 8;
            var av = new IwAvatar();
            av.Configure("a", 70, "", false);
            top.Add(av);
            var info = new VisualElement(); info.style.marginLeft = 12;
            var n = new Label("カイ-07"); n.style.fontSize = 18;
            n.style.color = new Color(0.957f, 0.957f, 0.941f);
            n.style.unityFontStyleAndWeight = FontStyle.Bold;
            info.Add(n);
            var w = new Label("タイド-08 スプレイヤー");
            w.style.fontSize = 11; w.style.color = new Color(0.541f, 0.541f, 0.572f);
            info.Add(w);
            top.Add(info);
            c.Add(top);

            var stats = new VisualElement();
            stats.style.flexDirection = FlexDirection.Row;
            stats.style.flexWrap = Wrap.Wrap;
            stats.style.marginTop = 12;
            string[] keys = { "撃破", "撃破される", "塗り(pt)", "SP発動" };
            string[] vals = { "8", "4", "1,242", "3" };
            for (int i = 0; i < keys.Length; i++)
            {
                var cell = new VisualElement();
                cell.style.width = Length.Percent(48);
                cell.style.paddingLeft = 10; cell.style.paddingRight = 10;
                cell.style.paddingTop = 10; cell.style.paddingBottom = 10;
                cell.style.backgroundColor = new Color(0.114f, 0.114f, 0.141f);
                cell.style.marginRight = 8; cell.style.marginBottom = 8;
                var k = new Label(keys[i]);
                k.AddToClassList("iw-label");
                k.style.fontSize = 9; k.style.color = new Color(0.541f, 0.541f, 0.572f);
                cell.Add(k);
                var v = new Label(vals[i]);
                v.AddToClassList("iw-num");
                v.style.fontSize = 20; v.style.color = new Color(0.957f, 0.957f, 0.941f);
                cell.Add(v);
                stats.Add(cell);
            }
            c.Add(stats);

            var rew = new Label("獲得報酬");
            rew.AddToClassList("iw-label");
            rew.style.fontSize = 10; rew.style.color = new Color(0.541f, 0.541f, 0.572f);
            rew.style.marginTop = 8;
            c.Add(rew);
            var rewV = new Label("+580 EXP · +◆ 920 · +1 パスティア");
            rewV.style.fontSize = 12; rewV.style.color = new Color(1f, 0.839f, 0f);
            rewV.style.marginTop = 4;
            c.Add(rewV);

            var btns = new VisualElement();
            btns.style.flexDirection = FlexDirection.Row;
            btns.style.marginTop = 12;
            var b1 = new Button(() => GoTo(InkwaveScreenManager.Screen.Loading));
            b1.text = "次の試合へ";
            b1.AddToClassList("iw-btn"); b1.AddToClassList("iw-btn-primary");
            b1.style.flexGrow = 1; b1.style.marginRight = 6;
            btns.Add(b1);
            var b2 = new Button(() => GoTo(InkwaveScreenManager.Screen.Lobby));
            b2.text = "ロビーへ";
            b2.AddToClassList("iw-btn"); b2.AddToClassList("iw-btn-ghost");
            b2.style.flexGrow = 1;
            btns.Add(b2);
            c.Add(btns);
        }

        /// <summary>Update: CountUp+キー入力。</summary>
        void Update()
        {
            if (_root == null) return;
            if (!IsActiveScreen(InkwaveScreenManager.Screen.Results)) return;
            if (!IsInputAllowed()) return;
            if (InkwaveInput.GetKeyDown(KeyCode.Return)) GoTo(InkwaveScreenManager.Screen.Loading);
            else if (InkwaveInput.GetKeyDown(KeyCode.L)) GoTo(InkwaveScreenManager.Screen.Lobby);
            else if (InkwaveInput.GetKeyDown(KeyCode.Escape)) GoTo(InkwaveScreenManager.Screen.Menu);

            _aCur = Mathf.MoveTowards(_aCur, _aTarget, Time.unscaledDeltaTime * 40f);
            _bCur = Mathf.MoveTowards(_bCur, _bTarget, Time.unscaledDeltaTime * 40f);
            if (_countA != null) _countA.text = $"{_aCur:F1}%";
            if (_countB != null) _countB.text = $"{_bCur:F1}%";
        }
    }
}
