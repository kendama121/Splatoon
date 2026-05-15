using UnityEngine;
using UnityEngine.UIElements;
using Splatoon.Application;
using Splatoon.Domain;

namespace Splatoon.Presentation
{
    /// <summary>
    /// UIToolkit版HUD。UIDocumentに付加し、UXML+USSでレイアウト。
    /// 旧uGUI HUDと並走または置き換え可能。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class SplatoonHUDToolkit : MonoBehaviour
    {
        public WeaponShooter LocalShooter;
        public SpecialAction LocalSpecial;

        // UI要素参照
        Label _timer;
        VisualElement _alphaBar;
        VisualElement _bravoBar;
        Label _alphaPct;
        Label _bravoPct;
        Label _leadText;
        VisualElement _inkFill;
        Label _inkPct;
        VisualElement _specialFill;
        Label _weaponName;
        Label _centerNotice;

        float _alphaDisp;
        float _bravoDisp;
        float _inkDisp = 1f;
        float _spDisp;
        float _noticeTime;

        void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            var root = doc.rootVisualElement;
            _timer = root.Q<Label>("Timer");
            _alphaBar = root.Q<VisualElement>("AlphaBar");
            _bravoBar = root.Q<VisualElement>("BravoBar");
            _alphaPct = root.Q<Label>("AlphaPct");
            _bravoPct = root.Q<Label>("BravoPct");
            _leadText = root.Q<Label>("LeadText");
            _inkFill = root.Q<VisualElement>("InkTankFill");
            _inkPct = root.Q<Label>("InkPct");
            _specialFill = root.Q<VisualElement>("SpecialFill");
            _weaponName = root.Q<Label>("WeaponName");
            _centerNotice = root.Q<Label>("CenterNotice");
        }

        void Update()
        {
            if (TurfWarMatchManager.Instance == null) return;
            var mgr = TurfWarMatchManager.Instance;

            // タイマー
            int sec = Mathf.Max(0, Mathf.CeilToInt(mgr.RemainingTime));
            int m = sec / 60; int s = sec % 60;
            if (_timer != null)
            {
                _timer.text = $"{m}:{s:00}";
                _timer.style.color = sec <= 30
                    ? new StyleColor(new Color(1f, 0.4f, 0.3f))
                    : new StyleColor(Color.white);
            }

            // 塗りバー(アニメ補間)
            float aRatio = mgr.Score.TeamRatios[0];
            float bRatio = mgr.Score.TeamRatios[1];
            _alphaDisp = Mathf.Lerp(_alphaDisp, aRatio, 5f * Time.deltaTime);
            _bravoDisp = Mathf.Lerp(_bravoDisp, bRatio, 5f * Time.deltaTime);
            if (_alphaBar != null) _alphaBar.style.scale = new StyleScale(new Scale(new Vector3(Mathf.Min(1f, _alphaDisp * 2f), 1, 1)));
            if (_bravoBar != null) _bravoBar.style.scale = new StyleScale(new Scale(new Vector3(Mathf.Min(1f, _bravoDisp * 2f), 1, 1)));
            if (_alphaPct != null) _alphaPct.text = $"{_alphaDisp * 100f:F1}%";
            if (_bravoPct != null) _bravoPct.text = $"{_bravoDisp * 100f:F1}%";

            // リード表示
            if (_leadText != null)
            {
                float diff = _alphaDisp - _bravoDisp;
                bool show = Mathf.Abs(diff) > 0.05f;
                _leadText.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
                if (show)
                {
                    _leadText.text = diff > 0 ? "ALPHA LEAD!" : "BRAVO LEAD!";
                    _leadText.style.color = diff > 0
                        ? new StyleColor(new Color(1f, 0.5f, 0f))
                        : new StyleColor(new Color(0.3f, 0.5f, 1f));
                }
            }

            // インクタンク
            if (LocalShooter != null)
            {
                float inkRatio = LocalShooter.CurrentInk / 100f;
                _inkDisp = Mathf.Lerp(_inkDisp, inkRatio, 8f * Time.deltaTime);
                if (_inkFill != null) _inkFill.style.height = new StyleLength(new Length(_inkDisp * 100f, LengthUnit.Percent));
                if (_inkPct != null) _inkPct.text = $"{LocalShooter.CurrentInk:F0}";
                if (_weaponName != null && LocalShooter.Weapon != null) _weaponName.text = LocalShooter.Weapon.DisplayName;
            }

            // スペシャルゲージ(円形scale)
            if (LocalSpecial != null && _specialFill != null)
            {
                _spDisp = Mathf.Lerp(_spDisp, LocalSpecial.Charge, 5f * Time.deltaTime);
                _specialFill.style.scale = new StyleScale(new Scale(Vector3.one * _spDisp));
            }

            // 中央通知(試合終了時)
            if (_centerNotice != null)
            {
                if (!mgr.IsMatchActive && mgr.RemainingTime <= 0f)
                {
                    bool isWin = mgr.Winner == TeamId.Alpha;
                    _centerNotice.text = isWin ? "YOU WIN!" : "YOU LOSE!";
                    _centerNotice.style.color = isWin
                        ? new StyleColor(new Color(1f, 0.85f, 0.1f))
                        : new StyleColor(new Color(0.6f, 0.6f, 0.6f));
                    _centerNotice.RemoveFromClassList("hidden");
                    _noticeTime += Time.unscaledDeltaTime;
                    float scale = 1f + Mathf.Max(0f, 1f - _noticeTime * 2f) * 0.5f;
                    _centerNotice.style.scale = new StyleScale(new Scale(Vector3.one * scale));
                }
                else
                {
                    _centerNotice.AddToClassList("hidden");
                    _noticeTime = 0f;
                }
            }
        }
    }
}
