using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KulibinSpace.DamageSystem {

	// Наследники: HitsControl, ShieldDestroyOnHit, HitsCount01, ChildCollide
	public abstract class DamageReceiver : MonoBehaviour {

		public delegate void TakeHitEvent (Collider c, GameObject go, Vector3 velocity, Vector3 point, Vector3 normal, float impulse);
		// события
		public event TakeHitEvent OnTakeHit;

		//public float durability { get { return _durability; } set { SetDurability(value); } } //= 2000f; // 12:27 07.06.2019 Размерность совпадает с величиной получаемого импульса при ударе. уменьшается при попаданиях, при обнулении объект уничтожается
		public float durability = 2000f; // 12:27 07.06.2019 Размерность совпадает с величиной получаемого импульса при ударе. уменьшается при попаданиях, при обнулении объект уничтожается
		//private float _durability; // 21:16 08.08.2021 надо связать установку durability с durabilityMax
		[HideInInspector]
		public float durabilityMax;

		// 21:20 08.08.2021 вызывается при инициализации бота на спауне, т.е. однократно. Других применений не предвидится, да и не нужно.
		void SetDurability (float dur) {
			//_durability = dur;
			//durabilityMax = _durability;
		}

		// 11:08 05.10.2019 Защититься от переопределения этого метода в наследниках невозможно, c# не имеет способов это контролировать.
		// Поэтому надо следить самому. Для этого создаю фейковый абстрактный метод CheckAwake, который надо раскоментарить и проверить каждого наследника.
		//public abstract void CheckAwake();
		void Awake () {
			durabilityMax = durability;
		}

		// всё максимально просто - уменьшить стойкость на величину "удара"
		void ReduceDurability (float val) {
			if (durability > 0) {
				durability -= val;
				// Damage(); // а если слишком большой импульс, то durability становится меньше нуля и ломает контракт DamagePart
				if (durability <= 0) {
					durability = 0;
					Kaput();
				} else Damage();
			}
		}

		// обработка данных в наследниках
		public virtual void TakeHit (Collider c, GameObject go, Vector3 velocity, Vector3 point, Vector3 normal, float impulse) {
			ReduceDurability(impulse);
			if (OnTakeHit != null) OnTakeHit(c, go, velocity, point, normal, impulse); // лучше после расчёта durability, тогда подписчик сможет сам определить, капут или нет
		} 

		// Замена для SendMessage - скорость + много параметров.
		// нанесение повреждения без учёта точки попадания
		// 21:19 22.07.2019 плохо сделано, наследник может переопределить TakeHit, но забыть про CauseDamage
		public virtual void CauseDamage (float impulse) {
			ReduceDurability(impulse);
		}

		public abstract void Kaput ();
		public abstract void Damage ();

	}

}