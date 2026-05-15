using UnityEngine;

namespace Splatoon.Presentation
{
    /// <summary>
    /// カメラ揺れエフェクト。WeaponShooter等から Shake(strength, duration) で発火。
    /// CinemachineCameraのアウトプット後にlocalPositionへ加算するため、子オブとして配置推奨。
    /// </summary>
    public class CameraShaker : MonoBehaviour
    {
        /// <summary>シングルトン参照</summary>
        public static CameraShaker Instance;

        // 内部
        float _remainingTime;
        float _strength;
        Vector3 _basePosition;

        void Awake()
        {
            Instance = this;
            _basePosition = transform.localPosition;
        }

        /// <summary>
        /// 揺れを発火する。
        /// </summary>
        /// <param name="strength">最大変位(m)</param>
        /// <param name="duration">持続時間(秒)</param>
        public void Shake(float strength, float duration)
        {
            _strength = Mathf.Max(_strength, strength);
            _remainingTime = Mathf.Max(_remainingTime, duration);
        }

        void LateUpdate()
        {
            if (_remainingTime > 0f)
            {
                _remainingTime -= Time.deltaTime;
                // ランダムオフセット(時間で減衰)
                float ratio = Mathf.Max(0f, _remainingTime / 0.2f);
                Vector3 offset = new Vector3(
                    (UnityEngine.Random.value - 0.5f) * 2f,
                    (UnityEngine.Random.value - 0.5f) * 2f,
                    0f
                ) * _strength * ratio;
                transform.localPosition = _basePosition + offset;
            }
            else
            {
                transform.localPosition = _basePosition;
            }
        }
    }
}
