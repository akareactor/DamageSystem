using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 19:19 21.08.2019 часть системы DamagePart - Wrecker - DamageAccum
// DamagePart + Wrecker могут работать независимо, посылая 
// DamagePart + DamageAccum точно так же независимо могут накапливать количество повреждений
// Аккумулятор ищет все DamagePart в потомках, суммирует жизнестойкость и получает сигналы урона от них. При достижении указанного уровня урона, рассылаются события

namespace KulibinSpace.DamageSystem {

	public class DamageAccum : MonoBehaviour {

		[Tooltip("Порог общего урона, oт 0 до 1")]
		public float threshold = 0.9999F;
		[Tooltip("событие при получении повреждения")]
		public MyFloatEvent onDamageEvent; // событие при получении повреждения
		[Tooltip("Событие при достижении порога урона")]
		public UnityEvent onKaputEvent; // событие при исчерпании жизнестойкости
		[Tooltip("Контейнер частей DamagePart")]
		public Transform root;
		public float stamina { get { return CheckStamina(); } }
		public float wholeDurability; // 20:38 25.12.2020 для автоподключения DamagePart
		public float currentDamage; // диапазон значений от 0 до 1 // 18:35 17.01.2021 опубликовал для отладки
		bool kaput = false;

		// 22:26 19.10.2020 не всегда доходит до 0, поэтому вручную проверяем погрешность (допустим, 0.001 должно хватить). Лучше иметь предсказуемый 0
		float CheckStamina () {
			float ret = 1 - currentDamage;
			if (ret < 0.001f) ret = 0;
		return ret;
		}

		// 11:41 31.08.2019 счётчик повреждений должен сбрасываться после ремонта.
		public void Reset () {
			currentDamage = 0;
		}

		// похоже, просуммировать жизнестойкость удобнее именно в Старте
		void Start () {
			if (!root) root = transform;
			DamagePart[] damageParts = root.GetComponentsInChildren<DamagePart>(true); // отыскивает рекурсивно!
			wholeDurability = 0; // значение wholeDurability соответствует сумме damageParts, там значения тысячи и десятки тысяч - для физики
			foreach (DamagePart dp in damageParts) wholeDurability += dp.durability;
			foreach (DamagePart dp in damageParts) dp.wholeDurability = wholeDurability;
			Reset();
		}

		// 10:16 24.06.2021 перенёс сигнал о повреждении под проверку превышения порога, иначе в момент превышения приходят сразу два сигнала - Damage и Kaput. Я не помню, используется ли где-то у меня такое, чтоб сразу два сигнала работали. Но это точно мешает для выдачи сообщения о поломке турели.
		public void CauseDamage (float damage) { // внести урон на величину damage (от 0 до 1)
			if (!kaput) {
				currentDamage = Mathf.Clamp01(currentDamage + damage);
				//onDamageEvent.Invoke(damage); // 10:15 24.06.2021 надо переносить под охрану от превышения threshold
				// 21:44 21.08.2019 пока что никакой охраны от многократного исполнения
				if (currentDamage >= threshold) {
					kaput = true;
					onKaputEvent.Invoke();
				} else
					onDamageEvent.Invoke(damage); // 10:15 24.06.2021 перенёс под охрану от превышения threshold
			}
		}

	}
}