using UnityEngine;

namespace Splatoon.Presentation
{
    /// <summary>
    /// プロシージャル歩行・走行アニメーション。
    /// CharacterControllerの速度を読み取り、腕・脚・胴体を上下動させ「動いてる感」を演出。
    /// </summary>
    public class ProceduralLocomotion : MonoBehaviour
    {
        /// <summary>歩行サイクル速度(Hz)。本家っぽい速さ</summary>
        public float CycleSpeed = 4.5f;
        /// <summary>腕の振り角度(度)</summary>
        public float ArmSwingDegrees = 35f;
        /// <summary>脚の振り角度(度)</summary>
        public float LegSwingDegrees = 30f;
        /// <summary>胴の上下バウンス幅(m)</summary>
        public float BodyBobHeight = 0.05f;
        /// <summary>停止時の腕初期Z回転(度)</summary>
        public float ArmIdleZ = 20f;

        // ボーン参照(子オブジェクト名で自動取得)
        Transform _torso, _armL, _armR, _legL, _legR, _head;
        Vector3 _torsoBasePos, _headBasePos;
        Vector3 _armLBaseRot, _armRBaseRot, _legLBaseRot, _legRBaseRot;
        CharacterController _cc;
        float _phase;

        void Awake()
        {
            _cc = GetComponentInParent<CharacterController>();
            var humanModel = transform; // この MonoBehaviour は HumanModel に直接アタッチ
            _torso = humanModel.Find("Torso");
            _armL = humanModel.Find("ArmL");
            _armR = humanModel.Find("ArmR");
            _legL = humanModel.Find("LegL");
            _legR = humanModel.Find("LegR");
            _head = humanModel.Find("Head");

            if (_torso != null) _torsoBasePos = _torso.localPosition;
            if (_head != null) _headBasePos = _head.localPosition;
            if (_armL != null) _armLBaseRot = _armL.localEulerAngles;
            if (_armR != null) _armRBaseRot = _armR.localEulerAngles;
            if (_legL != null) _legLBaseRot = _legL.localEulerAngles;
            if (_legR != null) _legRBaseRot = _legR.localEulerAngles;
        }

        void Update()
        {
            // 速さを計算(CharacterControllerなければ常に走行)
            float speedScale = 0f;
            if (_cc != null)
            {
                Vector3 horiz = _cc.velocity;
                horiz.y = 0;
                speedScale = Mathf.Clamp01(horiz.magnitude / 4f);
            }

            // 動いてないなら待機ポーズ(微弱上下動)
            if (speedScale < 0.05f)
            {
                _phase += Time.deltaTime * 1.5f;
                float idleBob = Mathf.Sin(_phase) * 0.02f;
                if (_torso != null) _torso.localPosition = _torsoBasePos + Vector3.up * idleBob;
                if (_head != null) _head.localPosition = _headBasePos + Vector3.up * idleBob * 0.5f;
                // 腕は初期姿勢に滑らかに戻す
                if (_armL != null) _armL.localEulerAngles = Vector3.Lerp(_armL.localEulerAngles, _armLBaseRot, Time.deltaTime * 6f);
                if (_armR != null) _armR.localEulerAngles = Vector3.Lerp(_armR.localEulerAngles, _armRBaseRot, Time.deltaTime * 6f);
                if (_legL != null) _legL.localEulerAngles = Vector3.Lerp(_legL.localEulerAngles, _legLBaseRot, Time.deltaTime * 6f);
                if (_legR != null) _legR.localEulerAngles = Vector3.Lerp(_legR.localEulerAngles, _legRBaseRot, Time.deltaTime * 6f);
                return;
            }

            // 走行サイクル進行
            _phase += Time.deltaTime * CycleSpeed * speedScale;

            float sinPhase = Mathf.Sin(_phase);
            float sinPhase2 = Mathf.Sin(_phase * 2f); // 2倍周波で上下動

            // 胴体上下バウンス(両足着地で2倍周波)
            if (_torso != null)
            {
                Vector3 newPos = _torsoBasePos + Vector3.up * Mathf.Abs(sinPhase2) * BodyBobHeight * speedScale;
                _torso.localPosition = newPos;
            }
            if (_head != null)
            {
                Vector3 newPos = _headBasePos + Vector3.up * Mathf.Abs(sinPhase2) * BodyBobHeight * 0.7f * speedScale;
                _head.localPosition = newPos;
            }

            // 腕振り(左右逆位相)
            if (_armL != null)
            {
                Vector3 rot = _armLBaseRot;
                rot.x = sinPhase * ArmSwingDegrees * speedScale;
                _armL.localEulerAngles = rot;
            }
            if (_armR != null)
            {
                Vector3 rot = _armRBaseRot;
                // 右腕は武器持ってるので振り抑え目+逆位相
                rot.x += -sinPhase * ArmSwingDegrees * 0.4f * speedScale;
                _armR.localEulerAngles = rot;
            }

            // 脚振り(左右逆位相)
            if (_legL != null)
            {
                Vector3 rot = _legLBaseRot;
                rot.x = -sinPhase * LegSwingDegrees * speedScale;
                _legL.localEulerAngles = rot;
            }
            if (_legR != null)
            {
                Vector3 rot = _legRBaseRot;
                rot.x = sinPhase * LegSwingDegrees * speedScale;
                _legR.localEulerAngles = rot;
            }
        }
    }
}
