using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Splatoon.Application;
using Splatoon.Domain;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Splatoon.Presentation
{
    /// <summary>
    /// ポーズメニュー兼モード/武器セレクター。ESCで開閉。
    /// ガチマッチ5モード切替 + 武器10種切替 + ステージ切替(現/マンタマリア号)。
    /// </summary>
    public class ModeSelectorMenu : MonoBehaviour
    {
        /// <summary>シングルトン</summary>
        public static ModeSelectorMenu Instance;

        /// <summary>メニュー本体パネル</summary>
        public GameObject MenuPanel;
        /// <summary>現在モード表示</summary>
        public TextMeshProUGUI CurrentModeText;
        /// <summary>現在武器表示</summary>
        public TextMeshProUGUI CurrentWeaponText;
        /// <summary>追跡対象のWeaponShooter(Player)</summary>
        public WeaponShooter LocalShooter;

        // 内部
        InputAction _escAction;
        bool _isOpen;
        // 武器プリセット
        WeaponData[] _weapons;
        int _weaponIndex;
        // モードリスト
        string[] _modeNames = { "Turf War", "Splat Zones", "Tower Control", "Rainmaker", "Clam Blitz" };
        int _modeIndex;

        void Awake()
        {
            Instance = this;
            _escAction = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
            _escAction.Enable();

            // 武器ロード(SO配列)
            _weapons = new WeaponData[]
            {
                LoadW("Weapon_Splattershot"),
                LoadW("Weapon_Splat_Roller"),
                LoadW("Weapon_Splat_Charger"),
                LoadW("Weapon_Slosher"),
                LoadW("Weapon_Splatling"),
                LoadW("Weapon_Dualies"),
                LoadW("Weapon_Brella"),
                LoadW("Weapon_Blaster"),
                LoadW("Weapon_Brush"),
                LoadW("Weapon_Stringer"),
                LoadW("Weapon_Splatana"),
            };
            if (MenuPanel != null) MenuPanel.SetActive(false);
        }

        WeaponData LoadW(string name)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<WeaponData>($"Assets/Splatoon/Data/{name}.asset");
#else
            return Resources.Load<WeaponData>(name);
#endif
        }

        void OnDestroy() { _escAction?.Dispose(); }

        void Update()
        {
            if (_escAction.WasPressedThisFrame())
            {
                _isOpen = !_isOpen;
                if (MenuPanel != null) MenuPanel.SetActive(_isOpen);
                Cursor.lockState = _isOpen ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = _isOpen;
                Time.timeScale = _isOpen ? 0f : 1f;
            }
            UpdateLabels();
        }

        void UpdateLabels()
        {
            if (CurrentModeText != null) CurrentModeText.text = $"MODE: {_modeNames[_modeIndex]}";
            if (CurrentWeaponText != null && _weapons != null && _weaponIndex < _weapons.Length && _weapons[_weaponIndex] != null)
                CurrentWeaponText.text = $"WEAPON: {_weapons[_weaponIndex].DisplayName}";
        }

        /// <summary>次のモードへ切替(UIボタンから呼び出し)</summary>
        public void NextMode() { _modeIndex = (_modeIndex + 1) % _modeNames.Length; ApplyMode(); }
        /// <summary>前のモードへ</summary>
        public void PrevMode() { _modeIndex = (_modeIndex - 1 + _modeNames.Length) % _modeNames.Length; ApplyMode(); }
        /// <summary>次の武器へ</summary>
        public void NextWeapon() { _weaponIndex = (_weaponIndex + 1) % _weapons.Length; ApplyWeapon(); }
        /// <summary>前の武器へ</summary>
        public void PrevWeapon() { _weaponIndex = (_weaponIndex - 1 + _weapons.Length) % _weapons.Length; ApplyWeapon(); }

        void ApplyMode()
        {
            // モード切替: 既存MatchManagerに通知(MVPは表示のみ、ロジック切替は将来)
            // 各モード用シーンオブジェクトのアクティブ切替
            ToggleModeObjects();
        }

        void ApplyWeapon()
        {
            if (_weapons[_weaponIndex] != null && LocalShooter != null)
            {
                LocalShooter.Weapon = _weapons[_weaponIndex];
            }
        }

        /// <summary>モード別シーンオブジェクトを切替</summary>
        void ToggleModeObjects()
        {
            // 各モード固有オブジェクトの有効/無効化
            var splatZone = GameObject.Find("Mode_SplatZone");
            var tower = GameObject.Find("Mode_Tower");
            var rainmaker = GameObject.Find("Mode_Rainmaker");
            if (splatZone != null) splatZone.SetActive(_modeIndex == 1);
            if (tower != null) tower.SetActive(_modeIndex == 2);
            if (rainmaker != null) rainmaker.SetActive(_modeIndex == 3);
        }
    }
}
