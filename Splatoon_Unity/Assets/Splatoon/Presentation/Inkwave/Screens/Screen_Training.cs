using UnityEngine;
using UnityEngine.UIElements;

namespace Splatoon.Presentation.Inkwave.Screens
{
    /// <summary>
    /// INKWAVE 12 トレーニング画面。ドリル選択+練習場+統計。
    /// </summary>
    public class Screen_Training : InkwaveScreenBase
    {
        readonly (string name, string desc)[] _drills = new[]
        {
            ("基本射撃", "標的を3秒以内に倒せ"),
            ("チャージ撃ち", "フルチャージで遠距離を撃ち抜け"),
            ("ボム投擲", "サブを投げて敵を倒せ"),
            ("スーパージャンプ", "目標地点へ着地せよ"),
            ("イカロール", "回避で攻撃をかわせ")
        };

        int _activeDrill = 0;

        /// <summary>UI構築。</summary>
        protected override void BindUI()
        {
            _root.Q<Button>("btn-start").clicked += () => GoTo(InkwaveScreenManager.Screen.HUD);
            BuildDrillList();
        }

        /// <summary>ドリル一覧構築。</summary>
        void BuildDrillList()
        {
            var list = _root.Q<VisualElement>("drill-list");
            list.Clear();
            for (int i = 0; i < _drills.Length; i++)
            {
                int idx = i;
                var d = _drills[i];
                var row = new VisualElement();
                row.AddToClassList("iw-row");
                if (idx == _activeDrill) row.AddToClassList("iw-row-selected");
                row.style.flexDirection = FlexDirection.Column;
                row.style.paddingLeft = 10; row.style.paddingRight = 10;
                row.style.paddingTop = 10; row.style.paddingBottom = 10;
                row.style.marginBottom = 6;
                row.style.backgroundColor = idx == _activeDrill ? new Color(0.165f, 0.165f, 0.2f) : new Color(0.114f, 0.114f, 0.141f);
                row.style.borderLeftWidth = 3;
                row.style.borderLeftColor = idx == _activeDrill ? new Color(1f, 0.106f, 0.42f) : new Color(0.219f, 0.219f, 0.263f);

                var n = new Label(d.name);
                n.style.fontSize = 12; n.style.color = new Color(0.957f, 0.957f, 0.941f);
                n.style.unityFontStyleAndWeight = FontStyle.Bold;
                row.Add(n);
                var dc = new Label(d.desc);
                dc.style.fontSize = 10; dc.style.color = new Color(0.541f, 0.541f, 0.572f);
                dc.style.whiteSpace = WhiteSpace.Normal;
                row.Add(dc);

                row.RegisterCallback<ClickEvent>(e => { _activeDrill = idx; BuildDrillList(); });
                list.Add(row);
            }
        }

        /// <summary>キー入力。</summary>
        void Update()
        {
            if (_root == null) return;
            if (!IsActiveScreen(InkwaveScreenManager.Screen.Training)) return;
            if (!IsInputAllowed()) return;
            if (InkwaveInput.GetKeyDown(KeyCode.Return)) GoTo(InkwaveScreenManager.Screen.HUD);
            else if (InkwaveInput.GetKeyDown(KeyCode.Escape)) GoTo(InkwaveScreenManager.Screen.Menu);
        }
    }
}
