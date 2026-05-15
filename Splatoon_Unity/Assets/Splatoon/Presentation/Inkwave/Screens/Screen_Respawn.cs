using UnityEngine;
using UnityEngine.UIElements;

namespace Splatoon.Presentation.Inkwave.Screens
{
    /// <summary>
    /// INKWAVE 09 リスポーン画面。7秒カウントダウン→HUDに復帰。
    /// </summary>
    public class Screen_Respawn : InkwaveScreenBase
    {
        float _cd = 7f;
        Label _cdNum;

        /// <summary>UI構築。</summary>
        protected override void BindUI()
        {
            _cd = 7f;
            _cdNum = _root.Q<Label>("cd-num");
        }

        /// <summary>Update: カウントダウン+復帰。</summary>
        void Update()
        {
            if (_root == null) return;
            if (!IsActiveScreen(InkwaveScreenManager.Screen.Respawn)) return;
            if (!IsInputAllowed()) return;
            if (InkwaveInput.GetKeyDown(KeyCode.Tab)) GoTo(InkwaveScreenManager.Screen.Map);

            _cd -= Time.unscaledDeltaTime;
            if (_cdNum != null) _cdNum.text = Mathf.CeilToInt(Mathf.Max(0, _cd)).ToString();
            if (_cd <= 0) GoTo(InkwaveScreenManager.Screen.HUD);
        }
    }
}
