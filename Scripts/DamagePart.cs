using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 15:16 12.08.2019 переосмысление системы разрушения объекта, состоящего из повреждаемых частей. Это - здания, враги, оружие.
// Вместо связки Parent Collide - Child Collide, работающей на сигнале SnapOff, ввожу реализацию класса DamageReceiver и добавляю к нему события юнити.
// 21:54 15.08.2019 добавил wholeDurability, это концепция единого нанесения урона от частей Целому. Все части DamagePart составляют Целое и урон каждой рассчитывается относительно суммы жизнестойкостей всех частей. Пример: если Целое состоит из двух частей стойкостью по 5000 каждая, то сигнал Damage при уроне в 1000 ед. отошлёт значение 0.1, а не 0.2
// 20:20 19.08.2019 оказывается, есть ещё ветка Shatter
// 19:44 21.08.2019 складывается система DamagePart - DamageAccum - Wrecker
// 17:23 24.12.2020 добавил неуничтожимость solid (не уменьшать стойкость и постоянно передавать урон)
// 22:44 24.12.2020 а что, если добавить ступени уничтожения? Вместо одноступенчатого разрушения будет несколько шагов, на которые можно добавлять разную степень разрушенности. Надо ли такое? Может, тогда не ступени, а каскад DamagePart?

namespace KulibinSpace.DamageSystem {


	[System.Serializable]
	public class MyFloatEvent : UnityEvent<float> {}


	public class DamagePart : DamageReceiver {

		public DamageValidator dv;
		float prevDurability;
		[Tooltip("Стойкость не уменьшается")]
		public bool solid = false;
		[Tooltip("Автопоиск аккумулятора")]
		public bool autoConnectToAccum = false;
		[Tooltip("Сумма стойкости целого")]
		public float wholeDurability; // для расчёта повреждения относительно целого 
		[Tooltip("событие при получении повреждения")]
		public MyFloatEvent onDamageEvent;
		[Tooltip("событие при исчерпании жизнестойкости (durability)")]
		public UnityEvent onKaputEvent;
		[Tooltip("Куда бот прицеливаться будет. Устанавливать в редакторе, вручную")]
		public Transform center; // 13:05 20.01.2021 в силу невозможности предусмотреть все варианты центровки, надо указывать центр вручную (это надо для прицеливания)

		void Start () {
			prevDurability = durability;
			if (autoConnectToAccum) FindDamageAccum();
			if (!center) center = gameObject.transform.Find("Center");  // 13:16 20.01.2021 важная штука, охрана от забывчивости
		}

		// 19:07 25.12.2020 придумал автопоиск аккумулятора при инстанцировании префаба
		// надо ещё обдумать
		public void FindDamageAccum () {
			// первый же аккумулятор, обычная иерархия
			DamageAccum da = GetComponentInParent<DamageAccum>();
			wholeDurability = da.wholeDurability;
			// сразу поставить себе onDamageEvent в аккумулятор
			onDamageEvent = new MyFloatEvent();
	//		onDamageEvent.AddListener(delegate { da.CauseDamage(10.0f); } );
			onDamageEvent.AddListener(da.CauseDamage); // 20:34 25.12.2020 в редакторе не видно, но вроде бы работает!
		}

		// для отладки, можно вызвать из onDamageEvent
		public void PrintDamage (float value) {
			print(gameObject.name + " get damage " + value);
		}

		// 16:27 24.12.2020 для того, чтобы урон не уменьшался и Часть постоянно передавала урон в Аккумулятор, надо перекрыть расчёт урона
		public override void TakeHit (Collider c, GameObject go, Vector3 velocity, Vector3 point, Vector3 normal, float impulse) {
			base.TakeHit(c, go, velocity, point, normal, impulse); // просто для подсчёта durability, и последующего вызова Damage
			if (solid) durability = prevDurability;
		}

		// 17:13 24.12.2020 здесь тоже перекрываем, на всякий случай
		public override void CauseDamage (float impulse) {
			base.CauseDamage(impulse); // просто для подсчёта durability, и последующего вызова Damage
			if (solid) durability = prevDurability;
		}

		// при каждом повреждении сообщить куда-то
		// Значение урона приводится к диапазону 0, 1
		public override void Damage () {
			float uron = (prevDurability - durability) / durabilityMax; // урон, приведённый к 1, где 1 это целостность части.
			if (wholeDurability > 0) uron *= durabilityMax / wholeDurability; // Урон приводится к общей целостности. Слабая часть влияет слабее на общий урон
			//print("uron = " + uron + ", prevDurability = " + prevDurability + ", durability = " + durability + ", durabilityMax = " + durabilityMax);
			onDamageEvent.Invoke(uron);
			if (!solid) prevDurability = durability; // для solid не надо запоминать
		}

		// когда убили, тоже сообщить
		public override void Kaput () {
			// 10:52 11.01.2021 меняю порядок, теперь сперва себе капут, а потом наверх передаём повреждение
			//Damage(); 	// 13:36 23.11.2019 общая логика DamageReceiver такова: если durability > 0, то сообщаем только Damage, а если жизнестойкость исчерпана, то посылается только Kaput. Но в системе DamagePart-DamageAccum, надо наверх сообщать и про уровень повреждений тоже, т.к. там общая жизнестойкость больше, чем у отдельной части.
			onKaputEvent.Invoke();
			Damage(); // 10:53 11.01.2021 капут солнечной батареи сначала на этой панельке, а уже только потом на корневом объекте
		}

		bool Valid (GameObject gameObject) {
			if (dv != null) return dv.Valid(gameObject); else return true;
		}

		// добавляю универсальности, чтобы объект мог получить повреждения от случайных объектов
		void OnCollisionEnter(Collision collision) {
			//print(gameObject.name + ", hit by " + collision.gameObject.name);
			//if (Global.Impactor(collision.gameObject) || Global.Ground(collision.gameObject)) {
			if (Valid(collision.gameObject)) {
				// сам себе засчитывает повреждения
				float oImpulse = 0;
				if (collision.rigidbody) {
					//oImpulse = collision.rigidbody.mass * collision.rigidbody.velocity.magnitude; // 21:12 12.01.2021 это просто не нужно, ведь физика сама весь импульс рассчитает!
					//print(collision.gameObject.name + ", " + collision.rigidbody.mass + ", " + collision.rigidbody.velocity.magnitude);
				}
				//print(gameObject.name + ", " + oImpulse + ", " + collision.impulse.magnitude);
				CauseDamage(oImpulse + collision.impulse.magnitude);
			}
		}
	}

}
