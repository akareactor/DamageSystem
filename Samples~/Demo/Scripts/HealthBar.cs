using UnityEngine;

namespace KulibinSpace.DamageSystem {

    // https://www.stevestreeting.com/2019/02/22/enemy-health-bars-in-1-draw-call-in-unity/
    // 14:55 01.10.2019 полоска здоровья врага. На старте компонент рендерера должен быть выключен. Включается один раз, при первом попадании.

    public class HealthBar : MonoBehaviour {

        MaterialPropertyBlock matBlock;
        MeshRenderer meshRenderer;
        public DamageReceiver dr;
        bool first = true;
        public float activationThreshold = 0.9f; // 13:40 14.08.2021 порог активации, чтоб включался не сразу, т.к. сначала включается щит

        void Awake () {
            meshRenderer = GetComponent<MeshRenderer>();
            matBlock = new MaterialPropertyBlock();
        }

        void Update () {
            // Only display on partial health
            if (dr.durability < dr.durabilityMax * activationThreshold) {
                //print(dr.durability + ", " + dr.durabilityMax);
                if (first) { meshRenderer.enabled = true; first = false; }
                AlignCamera();
                UpdateParams();
                if (dr.durability == 0) gameObject.SetActive(false); // 16:00 21.09.2021 почему-то при откачке щита это не срабатывает.
            }
        }

        void UpdateParams () {
            meshRenderer.GetPropertyBlock(matBlock);
            matBlock.SetFloat("_Fill", dr.durability / dr.durabilityMax);
            meshRenderer.SetPropertyBlock(matBlock);
        }

        void AlignCamera () {
            var camXform = Camera.main.transform;
            var forward = transform.position - camXform.position;
            forward.Normalize();
            var up = Vector3.Cross(forward, camXform.right);
            transform.rotation = Quaternion.LookRotation(forward, up);
        }

//        void OnValidate () {
//            dr = GetComponentInParent<DamageReceiver>();
//        }

    }

}
