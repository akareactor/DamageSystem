using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 28.01.2017
// Решил написать простейший скрипт учёта попаданий.
// Ибо разных скриптов для этого уже развелось несколько штук, а ни один применить толком нельзя, все конкретные.
// Этот скрипт ловит попадания и при превышении заданного числа броадкастит об этом вдоль собственного объекта.
// 10:11 31.07.2018 добавление сообщения о прекращении ударов
// 7:57 26.10.2019 Похоже, этот скрипт юзается только на VirtualTarget. Перехожу на BuilderCursorTrigger
// Считаю объекты, на которых он висит 
// - Префабы/Враги/Монолиты/Монолит/Monolith/Body
// 10:52 02.07.2020 HitsControl считает количество попаданий в коллайдер. Это его основная задача, которая иногда встречается.

namespace KulibinSpace.DamageSystem {

	public class HitsControl : DamageReceiver {

		public string tagHit = "Shot"; // отвязано от любой системы тагов проекта, если нужно - пусть автоматика проставит нужный тег
		[Header("События после исчерпания счётчика hits")]
		public UnityEvent actions; // 16:21 04.10.2018 они тоже могут броадкастить, так что можно избавляться от своего броадкаста
		public bool repeatTrigger = false; // не обращать внимания на счётчик попаданий
		public int hits = 1; // счётчик попаданий, когда обращается в 0, то сразу рапортует 
		public bool countShotsOnly = true;
		//
		public float maxImpulse = 0.0f; // 15:35 30.09.2018 макс. импульс, который засчитывается в хит. Если = 0, то проходят все импульсы. Для триггера не работает.
		public float currentImpulse = 0.0f; // отладка
		public float period = 0f; // максимальная периодичность отправки сигналов по хиту, сек.
		float startTime;
		public bool debug = false;
	
		void OnEnable () {
			startTime = Time.realtimeSinceStartup;
		}

		void CountHit (GameObject go) {
			if ((period > 0) && (Time.realtimeSinceStartup - startTime < period)) return;
			startTime = Time.realtimeSinceStartup;
			if (debug) print("CountHit: " + go.name + ", " + go.tag);
			bool tagEmpty = System.String.IsNullOrEmpty(tagHit);
			if ((countShotsOnly && go && ((!tagEmpty && go.CompareTag(tagHit)) || tagEmpty) || !countShotsOnly)) {
				// 9:52 31.07.2018 нужно сохранить логику посылки сообщения при обнулении счётчика и добавить к ней логику постоянной посылки при установленном флаге repeatTrigger
				if (repeatTrigger) {
					if (hits > 0) hits -= 1; // ниже ноля не считаем, пусть остаётся ноль.
					actions.Invoke();
				} else {
					if (hits > 0) {
						hits -= 1; // ниже ноля не считаем, пусть остаётся ноль.
						if (hits == 0) actions.Invoke();
					}
				}
			}
		}
	
		public override void TakeHit (Collider c, GameObject go, Vector3 velocity, Vector3 point, Vector3 normal, float impulse) {
			CountHit(go);
		}

		public override void Damage () {
			// ничего тут не надо
		}

		public override void Kaput () {
		}
	
		void OnCollisionEnter (Collision other) {
			//print(other.impulse.magnitude);
			if (other.impulse.magnitude > currentImpulse) currentImpulse = other.impulse.magnitude;
			if ((maxImpulse > 0.0f && other.impulse.magnitude > maxImpulse) || (maxImpulse == 0.0f)) CountHit(other.gameObject);
		}

		void NotifyOnCease (GameObject g) {
	//		if (notifyOnCease && (!keepContact || (keepContact && ((g == contact) || (contact == null))))) {
	//			BroadcastTo(onCeaseMessage, g);
	//			if (keepContact) contact = null;
	//		}
		}

		void OnCollisionExit (Collision other) {
			NotifyOnCease(other.gameObject);
		}

		void OnTriggerEnter (Collider other) {
			if (debug) print("В триггер зашёл " + other.gameObject.name + ", " + other.gameObject.tag);
			CountHit(other.gameObject);
		}

		void OnTriggerExit (Collider other) {
			if (debug) print("Из триггера вышел " + other.gameObject.name + ", " + other.gameObject.tag);
			NotifyOnCease(other.gameObject);
		}
	
	}

}