using UnityEngine;
using Splatoon.Domain;
using Splatoon.Infrastructure;
using Splatoon.Application;

namespace Splatoon.Presentation
{
    /// <summary>
    /// 武器カテゴリ別の発射ロジック集。WeaponShooterから呼び出される。
    /// 各カテゴリ固有の挙動を関数群で提供。本家Splatoonの11カテゴリ準拠。
    /// </summary>
    public static class WeaponCategoryFire
    {
        /// <summary>
        /// カテゴリに応じた発射処理を実行。1回呼び出しで該当の弾/エフェクト群を生成。
        /// </summary>
        public static void Execute(WeaponData weapon, Vector3 origin, Vector3 dir, TeamId team, GameObject owner, GameObject bulletPrefab)
        {
            switch (weapon.Category)
            {
                case WeaponCategory.Shooter:
                    FireShooter(weapon, origin, dir, team, owner, bulletPrefab);
                    break;
                case WeaponCategory.Roller:
                    FireRoller(weapon, origin, dir, team);
                    break;
                case WeaponCategory.Charger:
                    FireCharger(weapon, origin, dir, team, owner, bulletPrefab);
                    break;
                case WeaponCategory.Slosher:
                    FireSlosher(weapon, origin, dir, team, owner, bulletPrefab);
                    break;
                case WeaponCategory.Splatling:
                    FireSplatling(weapon, origin, dir, team, owner, bulletPrefab);
                    break;
                case WeaponCategory.Dualies:
                    FireDualies(weapon, origin, dir, team, owner, bulletPrefab);
                    break;
                case WeaponCategory.Brella:
                    FireBrella(weapon, origin, dir, team, owner, bulletPrefab);
                    break;
                case WeaponCategory.Blaster:
                    FireBlaster(weapon, origin, dir, team, owner, bulletPrefab);
                    break;
                case WeaponCategory.Brush:
                    FireBrush(weapon, origin, dir, team);
                    break;
                case WeaponCategory.Stringer:
                    FireStringer(weapon, origin, dir, team, owner, bulletPrefab);
                    break;
                case WeaponCategory.Splatana:
                    FireSplatana(weapon, origin, dir, team);
                    break;
                default:
                    FireShooter(weapon, origin, dir, team, owner, bulletPrefab);
                    break;
            }
        }

        // ============ シューター: 主弾+飛沫5発 ============
        static void FireShooter(WeaponData w, Vector3 o, Vector3 d, TeamId t, GameObject owner, GameObject prefab)
        {
            SpawnBullet(o, d, w, t, owner, prefab);
            for (int i = 0; i < 5; i++)
            {
                Vector3 spread = (d + Random.insideUnitSphere * 0.15f).normalized;
                SpawnBullet(o, spread, w, t, owner, prefab);
            }
        }

        // ============ ローラー: 前方扇状塗装(投擲なし、即時) ============
        static void FireRoller(WeaponData w, Vector3 o, Vector3 d, TeamId t)
        {
            Vector3 forward = d; forward.y = 0; forward.Normalize();
            // 前方扇形に8地点塗装
            for (int i = 0; i < 8; i++)
            {
                float angle = (i - 3.5f) * 7f; // ±25度
                Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
                Vector3 hitPos = o + dir * 1.8f;
                hitPos.y = 0.3f;
                InkPaintService.PaintAt(hitPos, w.PaintRadius * 1.3f, t);
            }
        }

        // ============ チャージャー: 高速直線弾(貫通風) ============
        static void FireCharger(WeaponData w, Vector3 o, Vector3 d, TeamId t, GameObject owner, GameObject prefab)
        {
            var go = Object.Instantiate(prefab, o, Quaternion.LookRotation(d));
            var b = go.GetComponent<InkBullet>();
            b.Initialize(w, o, d, t, owner);
            // チャージャーは速度を3倍にする(InkBulletのVelocityを上書き)
            b.Velocity = d.normalized * w.MuzzleVelocity * 3f;
            // 弾の見た目を細長く
            go.transform.localScale = new Vector3(0.08f, 0.08f, 0.6f);
        }

        // ============ スロッシャー: 放物線弾(上向き初速) ============
        static void FireSlosher(WeaponData w, Vector3 o, Vector3 d, TeamId t, GameObject owner, GameObject prefab)
        {
            Vector3 lobDir = (d + Vector3.up * 0.5f).normalized;
            var go = Object.Instantiate(prefab, o, Quaternion.LookRotation(lobDir));
            var b = go.GetComponent<InkBullet>();
            b.Initialize(w, o, lobDir, t, owner);
            b.Velocity = lobDir * w.MuzzleVelocity * 1.2f;
            go.transform.localScale = Vector3.one * 0.3f; // 大きめのバケツ弾
        }

        // ============ スピナー: 弾幕(6発バースト) ============
        static void FireSplatling(WeaponData w, Vector3 o, Vector3 d, TeamId t, GameObject owner, GameObject prefab)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3 spread = (d + Random.insideUnitSphere * 0.08f).normalized;
                SpawnBullet(o, spread, w, t, owner, prefab);
            }
        }

        // ============ マニューバー: 2連射 ============
        static void FireDualies(WeaponData w, Vector3 o, Vector3 d, TeamId t, GameObject owner, GameObject prefab)
        {
            SpawnBullet(o + Vector3.right * 0.1f, d, w, t, owner, prefab);
            SpawnBullet(o - Vector3.right * 0.1f, d, w, t, owner, prefab);
        }

        // ============ シェルター: 散弾7発 ============
        static void FireBrella(WeaponData w, Vector3 o, Vector3 d, TeamId t, GameObject owner, GameObject prefab)
        {
            for (int i = 0; i < 7; i++)
            {
                Vector3 spread = (d + Random.insideUnitSphere * 0.2f).normalized;
                SpawnBullet(o, spread, w, t, owner, prefab);
            }
        }

        // ============ ブラスター: 時限爆発弾 ============
        static void FireBlaster(WeaponData w, Vector3 o, Vector3 d, TeamId t, GameObject owner, GameObject prefab)
        {
            var go = Object.Instantiate(prefab, o, Quaternion.LookRotation(d));
            var b = go.GetComponent<InkBullet>();
            b.Initialize(w, o, d, t, owner);
            go.transform.localScale = Vector3.one * 0.25f;
            // ブラスターは0.3秒後に空中爆発(着弾しなくても爆発)
            var go2 = go;
            var t2 = t;
            var w2 = w;
            DelayedExplode(go2, w2, t2, 0.3f);
        }

        static async void DelayedExplode(GameObject go, WeaponData w, TeamId t, float delay)
        {
            await System.Threading.Tasks.Task.Delay((int)(delay * 1000));
            if (go == null) return;
            InkPaintService.PaintAt(go.transform.position, w.PaintRadius * 1.5f, t);
            Object.Destroy(go);
        }

        // ============ フデ: 前方塗装(連射軽め) ============
        static void FireBrush(WeaponData w, Vector3 o, Vector3 d, TeamId t)
        {
            Vector3 forward = d; forward.y = 0; forward.Normalize();
            for (int i = 1; i <= 4; i++)
            {
                Vector3 pos = o + forward * i * 0.4f;
                pos.y = 0.3f;
                InkPaintService.PaintAt(pos, w.PaintRadius, t);
            }
        }

        // ============ ストリンガー: 3弦同時発射 ============
        static void FireStringer(WeaponData w, Vector3 o, Vector3 d, TeamId t, GameObject owner, GameObject prefab)
        {
            for (int i = 0; i < 3; i++)
            {
                float yOffset = (i - 1) * 0.15f;
                Vector3 dir = (d + Vector3.up * yOffset).normalized;
                SpawnBullet(o, dir, w, t, owner, prefab);
            }
        }

        // ============ ワイパー: 前方衝撃波(扇形塗装) ============
        static void FireSplatana(WeaponData w, Vector3 o, Vector3 d, TeamId t)
        {
            Vector3 forward = d; forward.y = 0; forward.Normalize();
            for (int i = 0; i < 5; i++)
            {
                float angle = (i - 2f) * 8f;
                Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
                Vector3 pos = o + dir * 2.5f;
                pos.y = 0.3f;
                InkPaintService.PaintAt(pos, w.PaintRadius, t);
            }
        }

        // ============ ヘルパー: 1発弾生成 ============
        static void SpawnBullet(Vector3 o, Vector3 d, WeaponData w, TeamId t, GameObject owner, GameObject prefab)
        {
            var go = Object.Instantiate(prefab, o, Quaternion.LookRotation(d));
            var b = go.GetComponent<InkBullet>();
            b.Initialize(w, o, d, t, owner);
        }
    }
}
