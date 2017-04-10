#define BOLT
using UnityEngine;
using System;

namespace AssemblyCSharp {
#if BOLT
	public class EngineStrategy : MonoBehaviour {
#else
	public class EngineStrategy : MonoBehaviour {
#endif
#if BOLT
		public class DragonState : Bolt.EntityBehaviour<IDragonState> {}
		public class MonsterState : Bolt.EntityBehaviour<IMonsterState> {}
#else
		public class DragonState : MonoBehaviour {}
		public class MonsterState : MonoBehaviour {}
#endif
		public static float GetDeltaTime () {
#if BOLT
			return BoltNetwork.frameDeltaTime;
#else
			return Time.deltaTime;
#endif
		}
	}
}


