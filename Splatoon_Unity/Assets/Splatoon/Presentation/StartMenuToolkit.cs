using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace Splatoon.Presentation
{
    /// <summary>
    /// UIToolkitスタートメニュー。タイトル+START/HOWTO/SETTINGS/QUITボタン。
    /// STARTでバトル開始(StartRoot非表示+試合活性化)。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class StartMenuToolkit : MonoBehaviour
    {
        /// <summary>HUD UIToolkit(START後に表示)</summary>
        public UIDocument HudDocument;
        /// <summary>uGUI HUDCanvas(両用、START後活性化)</summary>
        public GameObject UGuiHudCanvas;
        /// <summary>PlayerController(START前は操作無効化)</summary>
        public PlayerController LocalPlayer;

        VisualElement _root;
        Label _titleMain;
        bool _started;

        void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            _root = doc.rootVisualElement.Q<VisualElement>("StartRoot");
            _titleMain = doc.rootVisualElement.Q<Label>("TitleMain");

            doc.rootVisualElement.Q<Button>("BtnStart").clicked += OnStart;
            doc.rootVisualElement.Q<Button>("BtnHowTo").clicked += OnHowTo;
            doc.rootVisualElement.Q<Button>("BtnSettings").clicked += OnSettings;
            doc.rootVisualElement.Q<Button>("BtnQuit").clicked += OnQuit;

            // 試合一時停止+操作無効化
            if (Splatoon.Application.TurfWarMatchManager.Instance != null)
                Splatoon.Application.TurfWarMatchManager.Instance.IsMatchActive = false;
            if (LocalPlayer != null) LocalPlayer.enabled = false;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }

        void Update()
        {
            // タイトル文字をパルスアニメ(scale 1.0〜1.05)
            if (_titleMain != null && !_started)
            {
                float s = 1.0f + Mathf.Sin(Time.time * 2f) * 0.04f;
                _titleMain.style.scale = new Scale(new Vector3(s, s, 1));
            }
        }

        void OnStart()
        {
            _started = true;
            if (_root != null) _root.style.display = DisplayStyle.None;

            // 試合開始
            if (Splatoon.Application.TurfWarMatchManager.Instance != null)
            {
                Splatoon.Application.TurfWarMatchManager.Instance.IsMatchActive = true;
                Splatoon.Application.TurfWarMatchManager.Instance.RemainingTime = 180f;
            }
            if (LocalPlayer != null) LocalPlayer.enabled = true;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;

            // HUD表示
            if (HudDocument != null) HudDocument.gameObject.SetActive(true);
            if (UGuiHudCanvas != null) UGuiHudCanvas.SetActive(true);
        }

        void OnHowTo()
        {
            Debug.Log("[StartMenu] How to play clicked");
            // 簡易: ヒントテキスト更新
            var hint = GetComponent<UIDocument>().rootVisualElement.Q<Label>("Hint");
            if (hint != null) hint.text = "WASD Move | LMB Fire | RMB Sub | Shift Squid | Space Jump | Q Special | Tab Map";
        }

        void OnSettings()
        {
            Debug.Log("[StartMenu] Settings clicked");
            var hint = GetComponent<UIDocument>().rootVisualElement.Q<Label>("Hint");
            if (hint != null) hint.text = "Settings: not yet implemented. ESC mid-battle to change mode/weapon.";
        }

        void OnQuit()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            UnityEngine.Application.Quit();
            #endif
        }
    }
}
