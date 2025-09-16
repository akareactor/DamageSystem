using UnityEngine;

namespace KulibinSpace.DamageSystem {

    // https://www.stevestreeting.com/2019/02/22/enemy-health-bars-in-1-draw-call-in-unity/
    // 14:55 01.10.2019 полоска здоровья врага. На старте компонент рендерера должен быть выключен. 
    // Включается один раз, при первом попадании.

    public class HealthBar : MonoBehaviour {

        MaterialPropertyBlock matBlock;
        MeshRenderer meshRenderer;
        public DamageReceiver dr;
        bool wasEnabled; // чтоб не дёргать каждый раз свойство meshRenderer.enabled
        public float activationThreshold = 0.9f; // 13:40 14.08.2021 порог активации, чтоб включался не сразу, т.к. сначала включается щит
        public float minViewDistance = 45f;      // 2025-09-16 включение ближе этой дистанции
        public float maxViewDistance = 50f;      // 2025-09-16 выключение дальше этой дистанции
        Camera cam;

        void Awake () {
            meshRenderer = GetComponent<MeshRenderer>();
            matBlock = new MaterialPropertyBlock();
            wasEnabled = meshRenderer.enabled;
        }

        void OnEnable () {
            cam = Camera.main;
        }

        void Update () {
            if (!cam) {
                cam = Camera.main;
                return;
            }

            float distance = Vector3.Distance(transform.position, cam.transform.position);

            // Only display on partial health and within distance
            bool withinHealth = dr.Durability < dr.durabilityMax * activationThreshold;
            bool withinDistance = wasEnabled 
                ? distance <= maxViewDistance   // уже включена — держим до max
                : distance <= minViewDistance;  // выключена — включаем только ближе min

            if (withinHealth && withinDistance) {
                if (!wasEnabled) { meshRenderer.enabled = true; wasEnabled = true; }
                AlignCamera();
                UpdateParams();
                if (dr.Durability == 0) gameObject.SetActive(false); // 16:00 21.09.2021 почему-то при откачке щита это не срабатывает.
            } else { // 2025-09-06 11:49:14 если вдруг починили выше порога activationThreshold
                if (wasEnabled) { meshRenderer.enabled = false; wasEnabled = false; }
            }
        }

        void UpdateParams () {
            meshRenderer.GetPropertyBlock(matBlock);
            matBlock.SetFloat("_Fill", dr.Durability / dr.durabilityMax);
            meshRenderer.SetPropertyBlock(matBlock);
        }

        void AlignCamera () {
            if (cam) {
                var camXform = cam.transform;
                var forward = transform.position - camXform.position;
                forward.Normalize();
                var up = Vector3.Cross(forward, camXform.right);
                transform.rotation = Quaternion.LookRotation(forward, up);
            }
        }

        void OnValidate () {
            if (minViewDistance >= maxViewDistance) {
                minViewDistance = maxViewDistance - 1f;
                if (minViewDistance < 0f) minViewDistance = 0f;
            }
        }

//        void OnValidate () {
//            dr = GetComponentInParent<DamageReceiver>();
//        }

    }

}
