using UnityEngine;

namespace TheRPG.Tutorial
{
	public static class TutorialManager
	{
		const string KeySeenPrefix = "tutorial_seen_";

		public static bool HasSeen(string id)
		{
			return PlayerPrefs.GetInt(KeySeenPrefix + id, 0) == 1;
		}

		public static void MarkSeen(string id)
		{
			PlayerPrefs.SetInt(KeySeenPrefix + id, 1);
			PlayerPrefs.Save();
		}
	}
}


