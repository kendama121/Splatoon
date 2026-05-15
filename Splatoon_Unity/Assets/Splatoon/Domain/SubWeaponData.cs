using UnityEngine;

namespace Splatoon.Domain
{
    /// <summary>サブウェポンカテゴリ</summary>
    public enum SubWeaponCategory
    {
        SplashBomb, CurlingBomb, QuickBomb, SuctionBomb, Sprinkler,
        SplashWall, AutoBomb, RoboBomb, FizzBomb, Torpedo,
        InkMine, PointSensor, BeakonJump, LineMarker, PoisonMist
    }

    /// <summary>サブウェポン設定</summary>
    [CreateAssetMenu(fileName = "SubWeaponData", menuName = "Splatoon/SubWeapon Data")]
    public class SubWeaponData : ScriptableObject
    {
        public SubWeaponCategory Category = SubWeaponCategory.SplashBomb;
        public string DisplayName = "Splash Bomb";
        public float InkCost = 70f;
        public float Damage = 180f;
        public float ExplosionRadius = 2.5f;
        public float FuseTime = 1.0f; // 起爆時間
        public float ThrowForce = 7f;
        public float Gravity = 9.81f;
    }
}
