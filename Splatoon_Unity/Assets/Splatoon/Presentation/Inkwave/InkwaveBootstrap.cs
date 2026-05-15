using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Splatoon.Presentation.Inkwave
{
    /// <summary>
    /// INKWAVE 13画面の自動起動セットアップ。
    /// シーンに本コンポーネントを1個置き、UXML資産・PanelSettingsをInspectorで配線するだけで起動する。
    /// </summary>
    public class InkwaveBootstrap : MonoBehaviour
    {
        /// <summary>全画面共通のPanelSettings。</summary>
        public PanelSettings SharedPanelSettings;

        /// <summary>13画面分のUXML(enum Screen順)。</summary>
        public VisualTreeAsset Title;
        public VisualTreeAsset Menu;
        public VisualTreeAsset Character;
        public VisualTreeAsset Weapon;
        public VisualTreeAsset Lobby;
        public VisualTreeAsset Loading;
        public VisualTreeAsset HUD;
        public VisualTreeAsset Map;
        public VisualTreeAsset Respawn;
        public VisualTreeAsset Pause;
        public VisualTreeAsset Results;
        public VisualTreeAsset Training;
        public VisualTreeAsset Settings;

        /// <summary>初期画面。</summary>
        public InkwaveScreenManager.Screen InitialScreen = InkwaveScreenManager.Screen.Title;

        /// <summary>既存UI(uGUI Canvas / 既存UIDocument)を起動時に無効化するか。</summary>
        public bool DisableLegacyUI = true;

        InkwaveScreenManager _mgr;

        /// <summary>Awake: InkwaveScreenManagerを生成し、13画面分のUIDocument+Controllerを子GameObjectで作る。</summary>
        void Awake()
        {
            // GameView 非フォーカス時もフレーム進行(Editor MCP操作中も止まらない)
            UnityEngine.Application.runInBackground = true;

            // 既存UI無効化(uGUI Canvas + 既存UIDocument)
            if (DisableLegacyUI)
            {
                foreach (var c in FindObjectsByType<UnityEngine.Canvas>(FindObjectsSortMode.None))
                {
                    c.gameObject.SetActive(false);
                }
                foreach (var d in FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
                {
                    // 自分自身/子孫は除外(まだ生成前なのでこの時点では既存物のみヒット)
                    d.gameObject.SetActive(false);
                }
            }

            // ScreenManager
            _mgr = gameObject.AddComponent<InkwaveScreenManager>();
            _mgr.InitialScreen = InitialScreen;

            // 13画面分の GameObject + UIDocument + Controller
            var defs = new (string name, VisualTreeAsset uxml, Type ctrl)[]
            {
                ("01_Title",     Title,     typeof(Screens.Screen_Title)),
                ("02_Menu",      Menu,      typeof(Screens.Screen_Menu)),
                ("03_Character", Character, typeof(Screens.Screen_Character)),
                ("04_Weapon",    Weapon,    typeof(Screens.Screen_Weapon)),
                ("05_Lobby",     Lobby,     typeof(Screens.Screen_Lobby)),
                ("06_Loading",   Loading,   typeof(Screens.Screen_Loading)),
                ("07_HUD",       HUD,       typeof(Screens.Screen_HUD)),
                ("08_Map",       Map,       typeof(Screens.Screen_Map)),
                ("09_Respawn",   Respawn,   typeof(Screens.Screen_Respawn)),
                ("10_Pause",     Pause,     typeof(Screens.Screen_Pause)),
                ("11_Results",   Results,   typeof(Screens.Screen_Results)),
                ("12_Training",  Training,  typeof(Screens.Screen_Training)),
                ("13_Settings",  Settings,  typeof(Screens.Screen_Settings))
            };

            _mgr.ScreenDocuments.Clear();
            UnityEngine.UIElements.UIDocument hudDoc = null;
            // Current を先に設定(Update内 IsActiveScreen ガード が初フレームから機能)
            _mgr.Current = InitialScreen;

            foreach (var (name, uxml, ctrl) in defs)
            {
                var go = new GameObject(name);
                // 初期非表示化(Active=false で OnEnable発火抑止 → Update も走らない)
                bool isInitial = name.EndsWith(InitialScreen.ToString());
                go.SetActive(false);
                go.transform.SetParent(transform);
                var doc = go.AddComponent<UIDocument>();
                doc.panelSettings = SharedPanelSettings;
                doc.visualTreeAsset = uxml;
                if (ctrl != null) go.AddComponent(ctrl);
                _mgr.ScreenDocuments.Add(doc);
                if (name == "07_HUD") hudDoc = doc;
                // 初期画面のみActive化
                if (isInitial) go.SetActive(true);
            }

            // ScreenManager に画面切替時のActive制御を委譲(Show時に該当GameObjectのみActive化する仕組みを使う)

            // HUDBridge を自動配線(同GameObject)
            if (hudDoc != null)
            {
                var bridge = GetComponent<InkwaveHUDBridge>();
                if (bridge == null) bridge = gameObject.AddComponent<InkwaveHUDBridge>();
                // Screen_HUD は HUD UIDocumentと同GameObject上にControllerとしてあるので参照取得
                bridge.HUD = hudDoc.GetComponent<Screens.Screen_HUD>();
            }
        }
    }
}
