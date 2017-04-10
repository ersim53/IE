using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;

namespace AssemblyCSharp {
	public static class Despawner {
		public static void Despawn (SpawnPool pool, GameObject unit) {
			if (unit.activeSelf) {
				pool.Despawn(unit.transform);
			}
		}
	}
}
