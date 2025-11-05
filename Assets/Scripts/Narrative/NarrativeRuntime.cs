using System.Threading.Tasks;
using UnityEngine;
using TheRPG.World;

namespace TheRPG.Narrative
{
	public static class NarrativeRuntime
	{
		public static INarrativeGraph Graph { get; private set; }
		public static WorldState World { get; private set; }

		public static async Task InitializeFromFolder(string eventsDir)
		{
			// Load localization first
			StringTable.Load("zh_CN");

			var graph = new NarrativeGraph();
			await graph.LoadFromDirectory(eventsDir);
			Graph = graph;
			if (World == null)
			{
				World = TheRPG.World.WorldBootstrap.CreateDefault();
			}
			Debug.Log($"[THE-RPG] Narrative runtime initialized from: {eventsDir}");
		}

		public static void ResetWorld()
		{
			World = TheRPG.World.WorldBootstrap.CreateDefault();
		}
	}
}


