using UnityEngine;
using UnityEngine.UIElements;

namespace Splatoon.Presentation.Inkwave.Screens
{
    /// <summary>
    /// INKWAVE 08 マップビュー画面。スーパージャンプ先選択+マップ表示+目標/ピン/勝率。
    /// </summary>
    public class Screen_Map : InkwaveScreenBase
    {
        struct Jumper { public string Name; public string Pos; public string Kd; public bool Dead, You; public string Key; }

        readonly Jumper[] _jumpers = new Jumper[]
        {
            new() { Name="マリィ", Pos="東フランク", Kd="3/1", Key="1" },
            new() { Name="オックスボウ", Pos="復活まで 4秒", Kd="5/2", Dead=true },
            new() { Name="ニンバス", Pos="中央", Kd="2/3", Key="3" },
            new() { Name="あなた", Pos="スタート地点", Kd="4/2", You=true }
        };

        readonly (string label, string key)[] _pins = new[]
        {
            ("こっち!", "E"), ("ナイス!", "F"), ("敵!", "V"),
            ("助けて", "G"), ("前へ!", "T"), ("下がる", "Y")
        };

        int _target = 0;

        /// <summary>UI構築。</summary>
        protected override void BindUI()
        {
            BuildSjList();
            BuildPinGrid();
            BuildMapView();
        }

        /// <summary>SJリスト構築。</summary>
        void BuildSjList()
        {
            var list = _root.Q<VisualElement>("sj-list");
            list.Clear();
            for (int i = 0; i < _jumpers.Length; i++)
            {
                int idx = i;
                var p = _jumpers[i];
                var row = new VisualElement();
                row.AddToClassList("iw-row");
                if (idx == _target) row.AddToClassList("iw-row-selected");
                row.style.position = Position.Relative;
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.paddingLeft = 10; row.style.paddingRight = 10;
                row.style.paddingTop = 10; row.style.paddingBottom = 10;
                row.style.marginBottom = 8;
                row.style.backgroundColor = idx == _target ? new Color(0.165f, 0.165f, 0.2f) : new Color(0.114f, 0.114f, 0.141f);
                row.style.borderLeftWidth = 3;
                row.style.borderLeftColor = idx == _target ? new Color(1f, 0.106f, 0.42f) : new Color(0.219f, 0.219f, 0.263f);
                row.style.opacity = p.Dead || p.You ? 0.5f : 1f;

                // 三角アイコン
                var tri = new VisualElement();
                tri.style.width = 28; tri.style.height = 28;
                tri.style.backgroundColor = new Color(1f, 0.106f, 0.42f);
                tri.style.marginRight = 10;
                row.Add(tri);

                var info = new VisualElement();
                info.style.flexGrow = 1;
                var name = new Label(p.Name);
                name.style.fontSize = 12; name.style.color = new Color(0.957f, 0.957f, 0.941f);
                name.style.unityFontStyleAndWeight = FontStyle.Bold;
                info.Add(name);
                var pos = new Label(p.Pos);
                pos.style.fontSize = 10; pos.style.color = new Color(0.541f, 0.541f, 0.572f);
                info.Add(pos);
                row.Add(info);

                var kd = new Label(p.Kd);
                kd.AddToClassList("iw-num");
                kd.style.fontSize = 11; kd.style.color = new Color(0.957f, 0.957f, 0.941f);
                kd.style.marginRight = 8;
                row.Add(kd);

                if (!string.IsNullOrEmpty(p.Key))
                {
                    var keyEl = new IwKey(); keyEl.SetText(p.Key);
                    row.Add(keyEl);
                }

                row.RegisterCallback<ClickEvent>(e => { if (!p.Dead && !p.You) { _target = idx; BuildSjList(); } });
                list.Add(row);
            }
        }

        /// <summary>ピングリッド構築。</summary>
        void BuildPinGrid()
        {
            var grid = _root.Q<VisualElement>("pin-grid");
            grid.Clear();
            foreach (var pin in _pins)
            {
                var cell = new VisualElement();
                cell.style.width = Length.Percent(48);
                cell.style.flexDirection = FlexDirection.Row;
                cell.style.justifyContent = Justify.SpaceBetween;
                cell.style.alignItems = Align.Center;
                cell.style.paddingLeft = 6; cell.style.paddingRight = 6;
                cell.style.paddingTop = 4; cell.style.paddingBottom = 4;
                cell.style.marginRight = 4; cell.style.marginBottom = 4;
                cell.style.backgroundColor = new Color(0.114f, 0.114f, 0.141f);
                var l = new Label(pin.label);
                l.style.fontSize = 10; l.style.color = new Color(0.957f, 0.957f, 0.941f);
                cell.Add(l);
                var k = new IwKey(); k.SetText(pin.key);
                cell.Add(k);
                grid.Add(cell);
            }
        }

        /// <summary>マップビュー描画(Painter2D)。</summary>
        void BuildMapView()
        {
            var mv = _root.Q<VisualElement>("map-view");
            mv.generateVisualContent += PaintMap;
        }

        /// <summary>マップPath描画。</summary>
        void PaintMap(MeshGenerationContext ctx)
        {
            var p = ctx.painter2D;
            var r = ctx.visualElement.contentRect;
            float sx = r.width / 600f;
            float sy = r.height / 380f;

            // アルファ陣
            p.fillColor = new Color(1f, 0.106f, 0.42f, 0.65f);
            p.BeginPath();
            p.MoveTo(new Vector2(40 * sx, 60 * sy));
            p.LineTo(new Vector2(240 * sx, 320 * sy));
            p.LineTo(new Vector2(260 * sx, 60 * sy));
            p.ClosePath();
            p.Fill();
            // ブラボー陣
            p.fillColor = new Color(0f, 0.898f, 1f, 0.65f);
            p.BeginPath();
            p.MoveTo(new Vector2(560 * sx, 60 * sy));
            p.LineTo(new Vector2(360 * sx, 320 * sy));
            p.LineTo(new Vector2(340 * sx, 60 * sy));
            p.ClosePath();
            p.Fill();

            // マップ枠
            p.strokeColor = new Color(0.957f, 0.957f, 0.941f, 0.4f);
            p.lineWidth = 1.5f;
            p.BeginPath();
            p.MoveTo(new Vector2(40 * sx, 60 * sy));
            p.LineTo(new Vector2(560 * sx, 60 * sy));
            p.LineTo(new Vector2(560 * sx, 320 * sy));
            p.LineTo(new Vector2(40 * sx, 320 * sy));
            p.ClosePath();
            p.Stroke();

            // 中央障害物
            p.fillColor = new Color(0.165f, 0.165f, 0.2f);
            p.BeginPath();
            p.MoveTo(new Vector2(240 * sx, 160 * sy));
            p.LineTo(new Vector2(360 * sx, 160 * sy));
            p.LineTo(new Vector2(360 * sx, 220 * sy));
            p.LineTo(new Vector2(240 * sx, 220 * sy));
            p.ClosePath();
            p.Fill();

            // 高台4箇所
            p.fillColor = new Color(0.165f, 0.165f, 0.2f);
            int[][] highs = new int[][] { new[] { 160, 80 }, new[] { 160, 260 }, new[] { 380, 80 }, new[] { 380, 260 } };
            foreach (var h in highs)
            {
                p.BeginPath();
                p.MoveTo(new Vector2(h[0] * sx, h[1] * sy));
                p.LineTo(new Vector2((h[0] + 60) * sx, h[1] * sy));
                p.LineTo(new Vector2((h[0] + 60) * sx, (h[1] + 40) * sy));
                p.LineTo(new Vector2(h[0] * sx, (h[1] + 40) * sy));
                p.ClosePath();
                p.Fill();
            }

            // 自分マーカー(三角)
            p.fillColor = new Color(0.957f, 0.957f, 0.941f);
            p.BeginPath();
            p.MoveTo(new Vector2(70 * sx, 190 * sy));
            p.LineTo(new Vector2(78 * sx, 202 * sy));
            p.LineTo(new Vector2(62 * sx, 202 * sy));
            p.ClosePath();
            p.Fill();

            // 味方マ
            p.fillColor = new Color(1f, 0.106f, 0.42f);
            p.BeginPath();
            p.Arc(new Vector2(180 * sx, 160 * sy), 9 * sx, 0f, 360f);
            p.Fill();

            // 味方ニ
            p.BeginPath();
            p.Arc(new Vector2(270 * sx, 190 * sy), 9 * sx, 0f, 360f);
            p.Fill();

            // 敵
            p.fillColor = new Color(0f, 0.898f, 1f);
            p.BeginPath();
            p.MoveTo(new Vector2(386 * sx, 156 * sy));
            p.LineTo(new Vector2(398 * sx, 156 * sy));
            p.LineTo(new Vector2(398 * sx, 168 * sy));
            p.LineTo(new Vector2(386 * sx, 168 * sy));
            p.ClosePath();
            p.Fill();

            // 警告円(右上)
            p.strokeColor = new Color(1f, 0.839f, 0f);
            p.lineWidth = 2f;
            p.BeginPath();
            p.Arc(new Vector2(430 * sx, 170 * sy), 14 * sx, 0f, 360f);
            p.Stroke();
        }

        /// <summary>キー入力。</summary>
        void Update()
        {
            if (_root == null) return;
            if (!IsActiveScreen(InkwaveScreenManager.Screen.Map)) return;
            if (!IsInputAllowed()) return;
            if (InkwaveInput.GetKeyDown(KeyCode.Tab)) GoTo(InkwaveScreenManager.Screen.HUD);
            else if (InkwaveInput.GetKeyDown(KeyCode.Alpha1)) { _target = 0; BuildSjList(); }
            else if (InkwaveInput.GetKeyDown(KeyCode.Alpha2)) { /* dead */ }
            else if (InkwaveInput.GetKeyDown(KeyCode.Alpha3)) { _target = 2; BuildSjList(); }
        }
    }
}
