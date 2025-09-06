using UnityEngine;

namespace KulibinSpace.DamageSystem {

    public class MouseClickTest : MonoBehaviour {

        public float damage = 150f;
        public float heal = 150f;

        void Update () {
            if (Input.GetMouseButtonDown(0)) { // ЛКМ
                // Создаём луч из камеры в точку курсора
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit)) {
                    if (hit.collider.TryGetComponent(out DamageReceiver dr)) {
                        dr.CauseDamage(damage);
                    }
                }
            } else if (Input.GetMouseButtonDown(1)) { // ПКМ
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit)) {
                    if (hit.collider.TryGetComponent(out DamageReceiver dr)) {
                        dr.RestoreDurability(heal);
                    }
                }
            }
        }
    }

}
