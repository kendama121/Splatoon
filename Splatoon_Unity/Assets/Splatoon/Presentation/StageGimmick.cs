using UnityEngine;

namespace Splatoon.Presentation
{
    /// <summary>ジャンプパッド: 触れたCharacterControllerを上方に跳ね飛ばす</summary>
    public class JumpPad : MonoBehaviour
    {
        public float Power = 18f;
        public Vector3 Direction = Vector3.up;
        void OnTriggerEnter(Collider other) { TryBoost(other); }
        void OnTriggerStay(Collider other) { TryBoost(other); }
        void TryBoost(Collider other)
        {
            var cc = other.GetComponent<CharacterController>();
            if (cc == null) return;
            // Player/BOT を強制ジャンプ
            var pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                var vel = (Vector3)typeof(PlayerController).GetField("_velocity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(pc);
                vel = Direction.normalized * Power;
                typeof(PlayerController).GetField("_velocity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(pc, vel);
            }
            if (ProceduralAudio.Instance != null) ProceduralAudio.Instance.PlayJump();
        }
    }

    /// <summary>移動床: 往復スライド</summary>
    public class MovingPlatform : MonoBehaviour
    {
        public Vector3 Distance = new Vector3(5f, 0, 0);
        public float Speed = 1f;
        Vector3 _origin;
        void Awake() { _origin = transform.position; }
        void Update()
        {
            float t = Mathf.Sin(Time.time * Speed) * 0.5f + 0.5f;
            transform.position = _origin + Distance * t;
        }
    }

    /// <summary>回転オブジェクト</summary>
    public class RotatingObject : MonoBehaviour
    {
        public Vector3 RotateAxis = Vector3.up;
        public float Speed = 50f;
        void Update() { transform.Rotate(RotateAxis * Speed * Time.deltaTime); }
    }

    /// <summary>キルゾーン: 触れたPlayerHealthに大ダメージ</summary>
    public class KillZone : MonoBehaviour
    {
        public float DamagePerSec = 100f;
        void OnTriggerStay(Collider other)
        {
            var ph = other.GetComponent<PlayerHealth>();
            if (ph == null) ph = other.GetComponentInParent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(DamagePerSec * Time.deltaTime, Splatoon.Domain.TeamId.Neutral);
        }
    }

    /// <summary>パルス発光: 時間でEmission強度が脈動</summary>
    public class PulseGlow : MonoBehaviour
    {
        public Color BaseColor = Color.white;
        public float MinIntensity = 1f;
        public float MaxIntensity = 4f;
        public float Speed = 2f;
        Material _mat;
        void Awake()
        {
            var mr = GetComponent<MeshRenderer>();
            if (mr != null) _mat = mr.material;
        }
        void Update()
        {
            if (_mat == null) return;
            float t = Mathf.Sin(Time.time * Speed) * 0.5f + 0.5f;
            float intensity = Mathf.Lerp(MinIntensity, MaxIntensity, t);
            _mat.SetColor("_EmissionColor", BaseColor * intensity);
        }
    }
}
