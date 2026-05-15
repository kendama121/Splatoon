using UnityEngine;
using UnityEngine.UIElements;

namespace Splatoon.Presentation.Inkwave.Screens
{
    /// <summary>
    /// INKWAVE 07 ゲーム中HUD。
    /// 既存ゲーム連携: InkwaveHUDBridge.cs が WeaponShooter/PlayerHealth/InkPaint等から値を取得し このScreenに流す。
    /// </summary>
    public class Screen_HUD : InkwaveScreenBase
    {
        struct Ally { public string Name; public int HP; public bool You, Dead, SpReady; public int Respawn; public string Note; }
        struct Enemy { public string Name; public int HP; public bool Dead; public int Respawn; }

        Ally[] _allies = new Ally[]
        {
            new() { Name="カイ-07", HP=82, You=true, Note="◀" },
            new() { Name="マリィ", HP=64 },
            new() { Name="オックスボウ", HP=0, Dead=true, Respawn=4, Note="復活" },
            new() { Name="ニンバス", HP=92, SpReady=true }
        };

        Enemy[] _enemies = new Enemy[]
        {
            new() { Name="リフト", HP=58 },
            new() { Name="エコー", HP=0, Dead=true, Respawn=6 },
            new() { Name="ヘイロー", HP=22 },
            new() { Name="クロウ", HP=76 }
        };

        readonly string[] _killFeed = new[]
        {
            "マリィ ▸ ROLLER ▸ エコー",
            "クロウ ▸ SLOSHER ▸ オックスボウ",
            "カイ-07 ▸ SHOOTER ▸ ヘイロー"
        };

        float _inkAmount = 62f;
        float _inkTimer;
        Label _inkNum, _spNum, _scoreA, _scoreB, _timer, _hitMarker;
        VisualElement _inkFill, _killStreak;
        IwTurfBar _turfBar;
        float _hitTimer;

        /// <summary>UI構築。</summary>
        protected override void BindUI()
        {
            _inkNum = _root.Q<Label>("ink-num");
            _spNum = _root.Q<Label>("sp-num");
            _scoreA = _root.Q<Label>("score-a");
            _scoreB = _root.Q<Label>("score-b");
            _timer = _root.Q<Label>("timer");
            _hitMarker = _root.Q<Label>("hit-marker");
            _inkFill = _root.Q<VisualElement>("ink-fill");
            _turfBar = _root.Q<IwTurfBar>("turf-bar");
            _killStreak = _root.Q<VisualElement>("kill-streak");

            BuildAllyList();
            BuildEnemyList();
            BuildKillFeed();
        }

        /// <summary>味方リスト構築。</summary>
        void BuildAllyList()
        {
            var list = _root.Q<VisualElement>("ally-list");
            list.Clear();
            var hdr = new Label("味方 · アルファ");
            hdr.AddToClassList("iw-label");
            hdr.style.fontSize = 10; hdr.style.color = new Color(0.541f, 0.541f, 0.572f);
            hdr.style.marginBottom = 4;
            list.Add(hdr);
            foreach (var a in _allies)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row; row.style.alignItems = Align.Center;
                row.style.paddingLeft = 10; row.style.paddingRight = 10;
                row.style.paddingTop = 6; row.style.paddingBottom = 6;
                row.style.backgroundColor = new Color(0.051f, 0.051f, 0.063f, 0.85f);
                row.style.borderLeftWidth = 5;
                row.style.borderLeftColor = a.Dead ? new Color(0.219f, 0.219f, 0.263f) : new Color(1f, 0.106f, 0.42f);
                row.style.borderTopWidth = 2; row.style.borderRightWidth = 2; row.style.borderBottomWidth = 2;
                row.style.borderTopColor = new Color(0.051f, 0.051f, 0.063f);
                row.style.borderRightColor = new Color(0.051f, 0.051f, 0.063f);
                row.style.borderBottomColor = new Color(0.051f, 0.051f, 0.063f);
                row.style.marginBottom = 6;
                row.style.minWidth = 240;
                row.style.opacity = a.Dead ? 0.55f : 1f;

                var dot = new VisualElement();
                dot.style.width = 8; dot.style.height = 8; dot.style.marginRight = 8;
                dot.style.backgroundColor = a.SpReady ? new Color(0f, 0.898f, 1f) : new Color(1f, 0.106f, 0.42f);
                row.Add(dot);

                var name = new Label(a.Name);
                name.style.fontSize = 11; name.style.color = new Color(0.957f, 0.957f, 0.941f);
                name.style.flexGrow = 1;
                row.Add(name);

                if (a.SpReady)
                {
                    var sp = new Label("SP!");
                    sp.AddToClassList("iw-anim-blink");
                    sp.style.fontSize = 9; sp.style.color = new Color(0f, 0.898f, 1f);
                    sp.style.unityFontStyleAndWeight = FontStyle.Bold;
                    sp.style.marginRight = 6;
                    row.Add(sp);
                }

                if (a.Dead)
                {
                    var d = new Label($"復活 {a.Respawn}秒");
                    d.style.fontSize = 9; d.style.color = new Color(1f, 0.214f, 0f);
                    row.Add(d);
                }
                else
                {
                    var bar = new IwBar();
                    Color hpC = a.HP > 60 ? new Color(0.255f, 0.878f, 0.478f) : a.HP > 30 ? new Color(1f, 0.839f, 0f) : new Color(1f, 0.333f, 0.333f);
                    bar.SetBar(a.HP, hpC, 4);
                    bar.style.width = 40;
                    row.Add(bar);
                }

                if (a.You)
                {
                    var you = new Label(a.Note);
                    you.style.fontSize = 11; you.style.color = new Color(1f, 0.106f, 0.42f);
                    you.style.marginLeft = 6;
                    row.Add(you);
                }
                list.Add(row);
            }
        }

        /// <summary>敵リスト構築。</summary>
        void BuildEnemyList()
        {
            var list = _root.Q<VisualElement>("enemy-list");
            list.Clear();
            var hdr = new Label("敵 · ブラボー");
            hdr.AddToClassList("iw-label");
            hdr.style.fontSize = 10; hdr.style.color = new Color(0.541f, 0.541f, 0.572f);
            hdr.style.marginBottom = 4;
            list.Add(hdr);
            foreach (var e in _enemies)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row; row.style.alignItems = Align.Center;
                row.style.justifyContent = Justify.FlexEnd;
                row.style.paddingLeft = 10; row.style.paddingRight = 10;
                row.style.paddingTop = 6; row.style.paddingBottom = 6;
                row.style.backgroundColor = new Color(0.051f, 0.051f, 0.063f, 0.85f);
                row.style.borderRightWidth = 5;
                row.style.borderRightColor = e.Dead ? new Color(0.219f, 0.219f, 0.263f) : new Color(0f, 0.898f, 1f);
                row.style.marginBottom = 6;
                row.style.minWidth = 200;
                row.style.opacity = e.Dead ? 0.55f : 1f;

                if (e.Dead)
                {
                    var d = new Label($"撃破 {e.Respawn}秒");
                    d.style.fontSize = 9; d.style.color = new Color(1f, 0.214f, 0f);
                    row.Add(d);
                }
                else
                {
                    var bar = new IwBar();
                    Color hpC = e.HP > 60 ? new Color(0.255f, 0.878f, 0.478f) : e.HP > 30 ? new Color(1f, 0.839f, 0f) : new Color(1f, 0.333f, 0.333f);
                    bar.SetBar(e.HP, hpC, 4);
                    bar.style.width = 40; bar.style.marginRight = 8;
                    row.Add(bar);
                }

                var name = new Label(e.Name);
                name.style.fontSize = 11; name.style.color = new Color(0.957f, 0.957f, 0.941f);
                name.style.marginRight = 8;
                row.Add(name);

                var dot = new VisualElement();
                dot.style.width = 8; dot.style.height = 8;
                dot.style.backgroundColor = new Color(0f, 0.898f, 1f);
                row.Add(dot);
                list.Add(row);
            }
        }

        /// <summary>キルフィード構築。</summary>
        void BuildKillFeed()
        {
            var list = _root.Q<VisualElement>("kill-feed");
            list.Clear();
            foreach (var k in _killFeed)
            {
                var row = new Label(k);
                row.AddToClassList("iw-anim-slide");
                row.style.fontSize = 11;
                row.style.color = new Color(0.957f, 0.957f, 0.941f);
                row.style.backgroundColor = new Color(0.051f, 0.051f, 0.063f, 0.75f);
                row.style.paddingLeft = 8; row.style.paddingRight = 8;
                row.style.paddingTop = 4; row.style.paddingBottom = 4;
                row.style.marginBottom = 4;
                list.Add(row);
            }
        }

        /// <summary>外部APIからインク量を更新する。</summary>
        public void SetInkAmount(float pct)
        {
            _inkAmount = Mathf.Clamp(pct, 0f, 100f);
            if (_inkNum != null) _inkNum.text = Mathf.RoundToInt(_inkAmount).ToString();
            if (_inkFill != null) _inkFill.style.height = Length.Percent(_inkAmount);
        }

        /// <summary>外部APIからSPチャージを更新する。</summary>
        public void SetSpecial(float pct)
        {
            if (_spNum != null) _spNum.text = Mathf.RoundToInt(pct).ToString();
        }

        /// <summary>外部APIから塗り率を更新する。</summary>
        public void SetTurf(float a, float b)
        {
            if (_scoreA != null) _scoreA.text = $"{a:F1}%";
            if (_scoreB != null) _scoreB.text = $"{b:F1}%";
            if (_turfBar != null) _turfBar.SetTurf(a, b);
        }

        /// <summary>外部APIからタイマー更新。</summary>
        public void SetTimer(float secLeft)
        {
            int m = Mathf.FloorToInt(secLeft / 60f);
            int s = Mathf.FloorToInt(secLeft % 60f);
            if (_timer != null) _timer.text = $"{m}:{s:D2}";
        }

        /// <summary>ヒットマーカー表示。</summary>
        public void ShowHitMarker(int dmg)
        {
            if (_hitMarker == null) return;
            _hitMarker.text = $"-{dmg}";
            _hitTimer = 0.6f;
        }

        /// <summary>Update: インク自然変動デモ+ヒットマーカー消失。</summary>
        void Update()
        {
            if (_root == null) return;
            if (!IsActiveScreen(InkwaveScreenManager.Screen.HUD)) return;
            if (!IsInputAllowed()) return;
            // M=マップへ、Q=スペシャル、Esc=ポーズへ
            if (InkwaveInput.GetKeyDown(KeyCode.Tab)) GoTo(InkwaveScreenManager.Screen.Map);
            else if (InkwaveInput.GetKeyDown(KeyCode.Escape)) GoTo(InkwaveScreenManager.Screen.Pause);

            _inkTimer += Time.unscaledDeltaTime;
            // デモ: サイン波で揺らす(外部API未接続時のみ)
            float demo = 40f + Mathf.Sin(_inkTimer * 0.8f) * 30f + 30f;
            SetInkAmount(demo);

            // ヒットマーカー
            if (_hitTimer > 0)
            {
                _hitTimer -= Time.unscaledDeltaTime;
                if (_hitTimer <= 0 && _hitMarker != null) _hitMarker.text = "";
            }
        }
    }
}
