using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Splatoon.Application;
using Splatoon.Presentation;

namespace Splatoon.Presentation.Inkwave.Screens
{
    /// <summary>
    /// INKWAVE 07 ゲーム中HUD。
    /// 既存ゲーム本体(Player/Bot/InkPaint/TurfWarMatch)とリアルタイム連携。
    /// 死亡→Respawn 遷移、試合終了→Results 遷移。
    /// </summary>
    public class Screen_HUD : InkwaveScreenBase
    {
        // 既存ゲーム参照キャッシュ
        PlayerHealth _playerHealth;
        WeaponShooter _shooter;
        SpecialAction _special;
        PlayerHealth _botHealth;

        // UI要素
        Label _inkNum, _spNum, _scoreA, _scoreB, _timer, _finalWarn, _hitMarker;
        VisualElement _inkFill, _killStreak;
        VisualElement _allyListContainer, _enemyListContainer, _killFeedContainer;
        IwTurfBar _turfBar;
        Label _playerHpLabel, _botHpLabel;
        IwBar _playerHpBar, _botHpBar;

        // 状態管理
        float _hitTimer;
        bool _wasSplatted;
        bool _matchEnded;

        // キルフィード履歴(最新3件)
        readonly Queue<string> _killFeedQueue = new Queue<string>();

        /// <summary>UI構築。表示時に試合開始+カーソルロック+既存ゲーム参照取得。</summary>
        protected override void BindUI()
        {
            // ── UI要素取得 ──
            _inkNum = _root.Q<Label>("ink-num");
            _spNum = _root.Q<Label>("sp-num");
            _scoreA = _root.Q<Label>("score-a");
            _scoreB = _root.Q<Label>("score-b");
            _timer = _root.Q<Label>("timer");
            _finalWarn = _root.Q<Label>("final-warn");
            _hitMarker = _root.Q<Label>("hit-marker");
            _inkFill = _root.Q<VisualElement>("ink-fill");
            _turfBar = _root.Q<IwTurfBar>("turf-bar");
            _killStreak = _root.Q<VisualElement>("kill-streak");
            _allyListContainer = _root.Q<VisualElement>("ally-list");
            _enemyListContainer = _root.Q<VisualElement>("enemy-list");
            _killFeedContainer = _root.Q<VisualElement>("kill-feed");

            // ── 既存ゲーム参照取得 ──
            var playerGo = GameObject.Find("Player");
            if (playerGo != null)
            {
                _playerHealth = playerGo.GetComponent<PlayerHealth>();
                _shooter = playerGo.GetComponent<WeaponShooter>();
                _special = playerGo.GetComponent<SpecialAction>();
            }
            var botGo = GameObject.Find("BotPlayer");
            if (botGo != null) _botHealth = botGo.GetComponent<PlayerHealth>();

            // ── 試合開始 ──
            var mgr = TurfWarMatchManager.Instance;
            if (mgr != null && !mgr.IsMatchActive) mgr.StartMatch();
            _matchEnded = false;
            _wasSplatted = false;
            _killFeedQueue.Clear();

            // ── カーソルロック(FPS操作モード) ──
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            _root.pickingMode = PickingMode.Ignore;
            Time.timeScale = 1f;

            // ── 不要要素を初期非表示 ──
            if (_killStreak != null) _killStreak.style.display = DisplayStyle.None;
            if (_finalWarn != null) _finalWarn.style.display = DisplayStyle.None;

            // ── 味方/敵リスト構築(プレイヤー+Bot 実体ベース) ──
            BuildAllyList();
            BuildEnemyList();
            // キルフィードは試合中の戦闘ログを蓄積(初期は空)
            if (_killFeedContainer != null) _killFeedContainer.Clear();
        }

        /// <summary>味方リスト = プレイヤー1人のみ表示(実HP連動)。</summary>
        void BuildAllyList()
        {
            if (_allyListContainer == null) return;
            _allyListContainer.Clear();

            var hdr = new Label("味方 · アルファ");
            hdr.AddToClassList("iw-label");
            hdr.style.fontSize = 10; hdr.style.color = new Color(0.541f, 0.541f, 0.572f);
            hdr.style.marginBottom = 4;
            _allyListContainer.Add(hdr);

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingLeft = 10; row.style.paddingRight = 10;
            row.style.paddingTop = 6; row.style.paddingBottom = 6;
            row.style.backgroundColor = new Color(0.051f, 0.051f, 0.063f, 0.85f);
            row.style.borderLeftWidth = 5;
            row.style.borderLeftColor = new Color(1f, 0.106f, 0.42f);
            row.style.minWidth = 240;

            var dot = new VisualElement();
            dot.style.width = 8; dot.style.height = 8; dot.style.marginRight = 8;
            dot.style.backgroundColor = new Color(1f, 0.106f, 0.42f);
            row.Add(dot);

            var name = new Label("カイ-07 (あなた)");
            name.style.fontSize = 11;
            name.style.color = new Color(0.957f, 0.957f, 0.941f);
            name.style.flexGrow = 1;
            row.Add(name);

            _playerHpLabel = new Label("100");
            _playerHpLabel.style.fontSize = 11;
            _playerHpLabel.style.color = new Color(0.255f, 0.878f, 0.478f);
            _playerHpLabel.style.marginRight = 6;
            _playerHpLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            row.Add(_playerHpLabel);

            _playerHpBar = new IwBar();
            _playerHpBar.SetBar(100, new Color(0.255f, 0.878f, 0.478f), 6);
            _playerHpBar.style.width = 60;
            row.Add(_playerHpBar);

            _allyListContainer.Add(row);
        }

        /// <summary>敵リスト = Bot 1体のみ表示(実HP連動)。</summary>
        void BuildEnemyList()
        {
            if (_enemyListContainer == null) return;
            _enemyListContainer.Clear();

            var hdr = new Label("敵 · ブラボー");
            hdr.AddToClassList("iw-label");
            hdr.style.fontSize = 10; hdr.style.color = new Color(0.541f, 0.541f, 0.572f);
            hdr.style.marginBottom = 4;
            hdr.style.unityTextAlign = TextAnchor.MiddleRight;
            _enemyListContainer.Add(hdr);

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.justifyContent = Justify.FlexEnd;
            row.style.paddingLeft = 10; row.style.paddingRight = 10;
            row.style.paddingTop = 6; row.style.paddingBottom = 6;
            row.style.backgroundColor = new Color(0.051f, 0.051f, 0.063f, 0.85f);
            row.style.borderRightWidth = 5;
            row.style.borderRightColor = new Color(0f, 0.898f, 1f);
            row.style.minWidth = 240;

            _botHpBar = new IwBar();
            _botHpBar.SetBar(100, new Color(0.255f, 0.878f, 0.478f), 6);
            _botHpBar.style.width = 60;
            _botHpBar.style.marginRight = 8;
            row.Add(_botHpBar);

            _botHpLabel = new Label("100");
            _botHpLabel.style.fontSize = 11;
            _botHpLabel.style.color = new Color(0.255f, 0.878f, 0.478f);
            _botHpLabel.style.marginRight = 8;
            _botHpLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            row.Add(_botHpLabel);

            var name = new Label("ブラボーBOT");
            name.style.fontSize = 11;
            name.style.color = new Color(0.957f, 0.957f, 0.941f);
            name.style.marginRight = 8;
            row.Add(name);

            var dot = new VisualElement();
            dot.style.width = 8; dot.style.height = 8;
            dot.style.backgroundColor = new Color(0f, 0.898f, 1f);
            row.Add(dot);

            _enemyListContainer.Add(row);
        }

        /// <summary>キルログを HUD に追加(末尾追記、最新3件保持)。</summary>
        public void AddKillFeed(string text)
        {
            if (_killFeedContainer == null) return;
            _killFeedQueue.Enqueue(text);
            while (_killFeedQueue.Count > 3) _killFeedQueue.Dequeue();

            _killFeedContainer.Clear();
            foreach (var k in _killFeedQueue)
            {
                var row = new Label(k);
                row.style.fontSize = 11;
                row.style.color = new Color(0.957f, 0.957f, 0.941f);
                row.style.backgroundColor = new Color(0.051f, 0.051f, 0.063f, 0.75f);
                row.style.paddingLeft = 8; row.style.paddingRight = 8;
                row.style.paddingTop = 4; row.style.paddingBottom = 4;
                row.style.marginBottom = 4;
                _killFeedContainer.Add(row);
            }
        }

        /// <summary>ヒットマーカー表示。</summary>
        public void ShowHitMarker(int dmg)
        {
            if (_hitMarker == null) return;
            _hitMarker.text = $"-{dmg}";
            _hitTimer = 0.6f;
        }

        /// <summary>外部API:インク量(0-100)。</summary>
        public void SetInkAmount(float pct)
        {
            float v = Mathf.Clamp(pct, 0f, 100f);
            if (_inkNum != null) _inkNum.text = Mathf.RoundToInt(v).ToString();
            if (_inkFill != null) _inkFill.style.height = Length.Percent(v);
        }

        /// <summary>外部API:SPチャージ(0-100)。</summary>
        public void SetSpecial(float pct) { if (_spNum != null) _spNum.text = Mathf.RoundToInt(pct).ToString(); }

        /// <summary>外部API:塗り率(0-100, 0-100)。</summary>
        public void SetTurf(float a, float b)
        {
            if (_scoreA != null) _scoreA.text = $"{a:F1}%";
            if (_scoreB != null) _scoreB.text = $"{b:F1}%";
            if (_turfBar != null) _turfBar.SetTurf(a, b);
        }

        /// <summary>外部API:タイマー秒。</summary>
        public void SetTimer(float secLeft)
        {
            int m = Mathf.FloorToInt(Mathf.Max(0, secLeft) / 60f);
            int s = Mathf.FloorToInt(Mathf.Max(0, secLeft) % 60f);
            if (_timer != null) _timer.text = $"{m}:{s:D2}";
            // 60秒以下で FINAL MINUTE 表示
            if (_finalWarn != null) _finalWarn.style.display = (secLeft <= 60f && secLeft > 0f) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>Update: ゲーム状態をHUDに反映+死亡/終了監視+Esc/Tab入力。</summary>
        void Update()
        {
            if (_root == null) return;
            if (!IsActiveScreen(InkwaveScreenManager.Screen.HUD)) return;

            // ── ゲーム状態 → HUD 反映 ──
            if (_shooter != null) SetInkAmount(_shooter.CurrentInk);
            if (_special != null) SetSpecial(_special.Charge * 100f);

            var mgr = TurfWarMatchManager.Instance;
            if (mgr != null)
            {
                if (mgr.Score != null && mgr.Score.TeamRatios != null && mgr.Score.TeamRatios.Length >= 2)
                {
                    SetTurf(mgr.Score.TeamRatios[0] * 100f, mgr.Score.TeamRatios[1] * 100f);
                }
                SetTimer(mgr.RemainingTime);

                // 試合終了 → Results
                if (!_matchEnded && !mgr.IsMatchActive && mgr.RemainingTime <= 0f)
                {
                    _matchEnded = true;
                    GoTo(InkwaveScreenManager.Screen.Results);
                    return;
                }
            }

            // ── プレイヤーHP 反映 + 死亡監視 ──
            if (_playerHealth != null)
            {
                float hp = _playerHealth.CurrentHP;
                if (_playerHpLabel != null)
                {
                    _playerHpLabel.text = Mathf.RoundToInt(hp).ToString();
                    _playerHpLabel.style.color = hp > 60 ? new Color(0.255f, 0.878f, 0.478f)
                        : hp > 30 ? new Color(1f, 0.839f, 0f) : new Color(1f, 0.333f, 0.333f);
                }
                if (_playerHpBar != null)
                {
                    Color hc = hp > 60 ? new Color(0.255f, 0.878f, 0.478f)
                        : hp > 30 ? new Color(1f, 0.839f, 0f) : new Color(1f, 0.333f, 0.333f);
                    _playerHpBar.SetBar(hp, hc, 6);
                }
                // 死亡検知 → Respawn遷移
                if (_playerHealth.IsSplatted && !_wasSplatted)
                {
                    _wasSplatted = true;
                    AddKillFeed("ブラボーBOT ▸ あなた");
                    GoTo(InkwaveScreenManager.Screen.Respawn);
                    return;
                }
                if (!_playerHealth.IsSplatted) _wasSplatted = false;
            }

            // ── ボットHP 反映 ──
            if (_botHealth != null && _botHpLabel != null && _botHpBar != null)
            {
                float hp = _botHealth.CurrentHP;
                _botHpLabel.text = Mathf.RoundToInt(hp).ToString();
                Color hc = hp > 60 ? new Color(0.255f, 0.878f, 0.478f)
                    : hp > 30 ? new Color(1f, 0.839f, 0f) : new Color(1f, 0.333f, 0.333f);
                _botHpLabel.style.color = hc;
                _botHpBar.SetBar(hp, hc, 6);
            }

            // ── ヒットマーカー消失 ──
            if (_hitTimer > 0)
            {
                _hitTimer -= Time.unscaledDeltaTime;
                if (_hitTimer <= 0 && _hitMarker != null) _hitMarker.text = "";
            }

            // ── キー入力(IsInputAllowed 経由) ──
            if (IsInputAllowed())
            {
                if (InkwaveInput.GetKeyDown(KeyCode.Escape)) GoTo(InkwaveScreenManager.Screen.Pause);
                else if (InkwaveInput.GetKeyDown(KeyCode.Tab)) GoTo(InkwaveScreenManager.Screen.Map);
            }
        }
    }
}
