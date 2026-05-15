using UnityEngine;
using UnityEngine.UIElements;

namespace Splatoon.Presentation.Inkwave.Screens
{
    /// <summary>
    /// INKWAVE 06 ロード画面。VS構図+5段進捗バー+Tips循環+完了→HUD遷移。
    /// </summary>
    public class Screen_Loading : InkwaveScreenBase
    {
        readonly string[] _tips = new[]
        {
            "イカ状態で潜伏すると、自軍インクの上ではより速く移動できる。",
            "スーパージャンプ着地時は無防備。敵スポーンに注意。",
            "サブウェポンを混ぜると塗り効率がぐっと上がる。",
            "高所からの攻撃はダメージが伸びる。チャージャーで狙え。",
            "ブラボー陣営の動きはミニマップで確認、奇襲を回避。"
        };

        float _pct;
        float _tipTimer;
        int _tipIdx;

        IwBar _stage, _tex, _paint, _voice, _net;
        Label _pctLabel, _tipText;

        /// <summary>UI構築。</summary>
        protected override void BindUI()
        {
            _pct = 0; _tipTimer = 0; _tipIdx = 0;
            _stage = _root.Q<IwBar>("prog-stage");
            _tex = _root.Q<IwBar>("prog-tex");
            _paint = _root.Q<IwBar>("prog-paint");
            _voice = _root.Q<IwBar>("prog-voice");
            _net = _root.Q<IwBar>("prog-net");
            _pctLabel = _root.Q<Label>("pct-label");
            _tipText = _root.Q<Label>("tip-text");
            if (_tipText != null) _tipText.text = _tips[0];
        }

        /// <summary>Update: 進捗+Tips循環+完了でHUDへ。</summary>
        void Update()
        {
            if (_root == null) return;
            if (!IsActiveScreen(InkwaveScreenManager.Screen.Loading)) return;
            if (!IsInputAllowed()) return;
            if (InkwaveInput.GetKeyDown(KeyCode.Escape)) { GoTo(InkwaveScreenManager.Screen.Lobby); return; }

            // 進捗(0~100)
            _pct += Time.unscaledDeltaTime * 60f * 0.9f / 60f * 100f / 90f;
            _pct = Mathf.Min(100f, _pct);
            if (_pctLabel != null) _pctLabel.text = $"{Mathf.RoundToInt(_pct)}%";
            // 5段階バー、それぞれの進み方を変える
            if (_stage != null) _stage.SetPct(Mathf.Min(100, _pct * 1.4f));
            if (_tex != null) _tex.SetPct(Mathf.Min(100, _pct * 1.2f));
            if (_paint != null) _paint.SetPct(Mathf.Min(100, _pct * 1.0f));
            if (_voice != null) _voice.SetPct(Mathf.Min(100, _pct * 0.8f));
            if (_net != null) _net.SetPct(Mathf.Min(100, _pct * 0.7f));

            // Tipsローテ(3秒毎)
            _tipTimer += Time.unscaledDeltaTime;
            if (_tipTimer >= 3f)
            {
                _tipTimer = 0;
                _tipIdx = (_tipIdx + 1) % _tips.Length;
                if (_tipText != null) _tipText.text = _tips[_tipIdx];
            }

            // 完了
            if (_pct >= 100f)
            {
                GoTo(InkwaveScreenManager.Screen.HUD);
            }
        }
    }
}
