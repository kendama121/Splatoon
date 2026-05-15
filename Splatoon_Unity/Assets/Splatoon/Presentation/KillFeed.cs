using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Splatoon.Domain;

namespace Splatoon.Presentation
{
    /// <summary>
    /// キル/デス通知。右上にテキストでログを蓄積、数秒で消える。
    /// </summary>
    public class KillFeed : MonoBehaviour
    {
        public static KillFeed Instance;
        /// <summary>ログTextテンプレート(クローンして使用)</summary>
        public TextMeshProUGUI EntryTemplate;
        /// <summary>ログ最大表示数</summary>
        public int MaxEntries = 5;
        /// <summary>ログ表示時間(秒)</summary>
        public float EntryDuration = 4f;

        List<(TextMeshProUGUI text, float spawnTime)> _entries = new List<(TextMeshProUGUI, float)>();

        void Awake() { Instance = this; if (EntryTemplate != null) EntryTemplate.gameObject.SetActive(false); }

        /// <summary>キル/デスログを追加</summary>
        public void AddLog(string attackerName, string victimName, TeamId attackerTeam)
        {
            if (EntryTemplate == null) return;
            var clone = Instantiate(EntryTemplate, EntryTemplate.transform.parent);
            clone.gameObject.SetActive(true);
            Color teamCol = Splatoon.Infrastructure.InkPaintManager.Instance != null
                ? Splatoon.Infrastructure.InkPaintManager.Instance.GetTeamColor(attackerTeam)
                : Color.white;
            clone.text = $"<color=#{ColorUtility.ToHtmlStringRGB(teamCol)}>{attackerName}</color> > {victimName}";
            _entries.Add((clone, Time.time));

            // 古いログ削除
            while (_entries.Count > MaxEntries)
            {
                if (_entries[0].text != null) Destroy(_entries[0].text.gameObject);
                _entries.RemoveAt(0);
            }
        }

        void Update()
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                if (_entries[i].text == null) { _entries.RemoveAt(i); continue; }
                float age = Time.time - _entries[i].spawnTime;
                if (age > EntryDuration)
                {
                    Destroy(_entries[i].text.gameObject);
                    _entries.RemoveAt(i);
                }
                else
                {
                    // フェードアウト
                    float fade = Mathf.Clamp01(1f - (age - EntryDuration + 1f));
                    var c = _entries[i].text.color;
                    c.a = fade;
                    _entries[i].text.color = c;
                }
            }
        }
    }
}
