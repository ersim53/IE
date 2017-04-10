using UnityEngine;
using System;
using System.Collections;

namespace AssemblyCSharp {
	public class CurrentLevel {
		private static string pool_name;
		private static bool initialized = false;

		private const string GENERAL_POOL_NAME = "General Pool";

		public static void SetCurrentLevelPoolName (string name) {
			pool_name = name;
		}

		public static string GetCurrentLevelPoolName () {
			return pool_name;
		}

		public static string GetGeneralPoolName () {
			return GENERAL_POOL_NAME;
		}

		public static bool LevelInitialized () {
			return initialized;
		}

		public static void InitializeLevel (bool init) {
			initialized = init;
		}
	}
}



