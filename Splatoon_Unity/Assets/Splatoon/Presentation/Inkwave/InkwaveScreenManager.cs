using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Splatoon.Presentation.Inkwave
{
    /// <summary>
    /// INKWAVE 13画面の遷移管理。UIDocumentを各画面1個ずつ保持し、表示切替を行う。
    /// </summary>
    public class InkwaveScreenManager : MonoBehaviour
    {
        public enum Screen
        {
            Title, Menu, Character, Weapon, Lobby,
            Loading, HUD, Map, Respawn, Pause,
            Results, Training, Settings
        }

        /// <summary>シングルトン</summary>
        public static InkwaveScreenManager Instance;

        /// <summary>全画面のUIDocument(Inspectorから配線、enum順)</summary>
        public List<UIDocument> ScreenDocuments = new List<UIDocument>();

        /// <summary>初期画面</summary>
        public Screen InitialScreen = Screen.Title;

        /// <summary>現在表示中の画面</summary>
        public Screen Current { get; set; }

        /// <summary>遷移中フラグ(二重 GoTo 防止)</summary>
        public bool IsTransitioning { get; private set; }

        /// <summary>進行中の遷移コルーチン参照(Stop用)</summary>
        Coroutine _transitionCo;

        void Awake()
        {
            // 多重シングルトン防止
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            HideAll();
            Show(InitialScreen);
        }

        /// <summary>
        /// 全画面を非表示にする。GameObject SetActive(false) で Update も停止。
        /// </summary>
        public void HideAll()
        {
            foreach (var doc in ScreenDocuments)
            {
                if (doc == null) continue;
                doc.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 指定画面を表示。他は非表示にする。GameObject SetActive 制御 + display=Flex 設定。
        /// </summary>
        public void Show(Screen screen)
        {
            HideAll();
            int idx = (int)screen;
            if (idx < 0 || idx >= ScreenDocuments.Count) return;
            var doc = ScreenDocuments[idx];
            if (doc == null) return;
            doc.gameObject.SetActive(true);
            // SetActive直後の rootVisualElement は null かも → schedule で確実適用
            doc.rootVisualElement?.schedule.Execute(() =>
            {
                if (doc.rootVisualElement != null)
                    doc.rootVisualElement.style.display = DisplayStyle.Flex;
            }).StartingIn(0);
            Current = screen;
        }

        /// <summary>
        /// 画面遷移エフェクト付きで表示(フェード/ポップ)。
        /// 遷移中は再エントリを無視する。
        /// </summary>
        public void TransitionTo(Screen screen)
        {
            // 同画面への遷移、または遷移中の再要求は無視
            if (IsTransitioning) return;
            if (Current == screen) return;
            // 既存コルーチン停止
            if (_transitionCo != null) StopCoroutine(_transitionCo);
            _transitionCo = StartCoroutine(TransitionRoutine(screen));
        }

        System.Collections.IEnumerator TransitionRoutine(Screen screen)
        {
            IsTransitioning = true;
            // 簡易フェード: rootのopacityを0→1
            yield return new WaitForSecondsRealtime(0.1f);
            Show(screen);
            int idx = (int)screen;
            if (idx >= 0 && idx < ScreenDocuments.Count && ScreenDocuments[idx] != null)
            {
                var root = ScreenDocuments[idx].rootVisualElement;
                if (root != null)
                {
                    root.style.opacity = 0f;
                    float t = 0f;
                    while (t < 0.3f)
                    {
                        t += Time.unscaledDeltaTime;
                        root.style.opacity = Mathf.Clamp01(t / 0.3f);
                        yield return null;
                    }
                    root.style.opacity = 1f;
                }
            }
            IsTransitioning = false;
            _transitionCo = null;
        }

        /// <summary>無効化時にコルーチン安全停止。</summary>
        void OnDisable()
        {
            if (_transitionCo != null) { StopCoroutine(_transitionCo); _transitionCo = null; }
            IsTransitioning = false;
        }
    }
}
