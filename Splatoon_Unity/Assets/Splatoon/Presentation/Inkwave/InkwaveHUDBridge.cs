using System.Reflection;
using UnityEngine;
using Splatoon.Application;
using Splatoon.Presentation;

namespace Splatoon.Presentation.Inkwave
{
    /// <summary>
    /// INKWAVE Screen_HUD と既存ゲームロジック(WeaponShooter/SpecialAction/TurfWarMatchManager)の橋渡し。
    /// Reflection 1回キャッシュ + 毎フレーム値転送。
    /// </summary>
    public class InkwaveHUDBridge : MonoBehaviour
    {
        /// <summary>HUD画面コントローラ参照(Bootstrap実行後または手動配線)。</summary>
        public Screens.Screen_HUD HUD;

        /// <summary>武器シューター(CurrentInk取得元、0-100)。</summary>
        public WeaponShooter Shooter;

        /// <summary>スペシャル動作(Charge取得元、0-1)。</summary>
        public SpecialAction Special;

        /// <summary>Reflection キャッシュ: TurfWarMatchManager.RemainingTime プロパティ。</summary>
        PropertyInfo _propRemain;
        /// <summary>Reflection キャッシュ: TurfWarMatchManager.RemainingTime フィールド。</summary>
        FieldInfo _fieldRemain;
        /// <summary>キャッシュ初期化済フラグ。</summary>
        bool _reflectionResolved;

        /// <summary>Awake: 同GameObject or 親から HUD/Shooter/Special を自動取得(未配線時のフォールバック)。</summary>
        void Awake()
        {
            if (HUD == null) HUD = FindFirstObjectByType<Screens.Screen_HUD>();
            if (Shooter == null) Shooter = FindFirstObjectByType<WeaponShooter>();
            if (Special == null) Special = FindFirstObjectByType<SpecialAction>();
        }

        /// <summary>Reflection を1回だけ解決する(GetType()結果をキャッシュ)。</summary>
        void ResolveReflectionOnce()
        {
            if (_reflectionResolved) return;
            _reflectionResolved = true;
            var mgr = TurfWarMatchManager.Instance;
            if (mgr == null) return;
            var t = mgr.GetType();
            _propRemain = t.GetProperty("RemainingTime");
            if (_propRemain == null) _fieldRemain = t.GetField("RemainingTime");
        }

        /// <summary>Update: HUDに値を反映。</summary>
        void Update()
        {
            if (HUD == null) return;

            // インク量(WeaponShooter.CurrentInk は 0-100)
            if (Shooter != null)
            {
                HUD.SetInkAmount(Shooter.CurrentInk);
            }
            // SP充電(SpecialAction.Charge は 0-1)
            if (Special != null)
            {
                HUD.SetSpecial(Special.Charge * 100f);
            }
            // 塗り率
            var mgr = TurfWarMatchManager.Instance;
            if (mgr != null && mgr.Score != null
                && mgr.Score.TeamRatios != null && mgr.Score.TeamRatios.Length >= 2)
            {
                HUD.SetTurf(mgr.Score.TeamRatios[0] * 100f, mgr.Score.TeamRatios[1] * 100f);
            }
            // タイマー
            HUD.SetTimer(GetRemainingTime());
        }

        /// <summary>残り時間を取得(プロパティ/フィールド/0 の優先順)。</summary>
        float GetRemainingTime()
        {
            var mgr = TurfWarMatchManager.Instance;
            if (mgr == null) return 0f;
            ResolveReflectionOnce();
            if (_propRemain != null)
            {
                var v = _propRemain.GetValue(mgr);
                if (v is float f) return f;
            }
            if (_fieldRemain != null)
            {
                var v = _fieldRemain.GetValue(mgr);
                if (v is float f) return f;
            }
            return 0f;
        }
    }
}
