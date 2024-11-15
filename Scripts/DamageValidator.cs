using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KulibinSpace.DamageSystem {

	// Валидатор объекта устанавливается в DamagePart для проверки объекта, с которым произошла коллизия.
	// Конкретика валидатора - проектная.

	public abstract class DamageValidator {

		public abstract bool Valid (GameObject go);
		
		// public bool Valid (GameObject go) { return (Global.Impactor(go) || Global.Ground(go)) }

	}

}