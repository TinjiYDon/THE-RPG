using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TheRPG.Narrative;
using TheRPG.Content;

namespace TheRPG.EditorTools
{
	public static class ContentValidationMenu
	{
		[MenuItem("Tools/THE-RPG/Validate Events (Assets/Content/events)")]
		public static void ValidateDefault()
		{
			var eventsDir = Path.Combine(Application.dataPath, "Content/events");
			Validate(eventsDir);
		}

		[MenuItem("Tools/THE-RPG/Validate Events...")]
		public static void ValidatePickFolder()
		{
			var picked = EditorUtility.OpenFolderPanel("Pick events folder", Application.dataPath, "events");
			if (!string.IsNullOrEmpty(picked)) Validate(picked);
		}

		[MenuItem("Tools/THE-RPG/Load Events To Runtime Cache (Assets/Content/events)")]
		public static void LoadToRuntimeDefault()
		{
			var eventsDir = Path.Combine(Application.dataPath, "Content/events");
			LoadToRuntime(eventsDir);
		}

		[MenuItem("Tools/THE-RPG/Load Events To Runtime Cache...")]
		public static void LoadToRuntimePick()
		{
			var picked = EditorUtility.OpenFolderPanel("Pick events folder", Application.dataPath, "events");
			if (!string.IsNullOrEmpty(picked)) LoadToRuntime(picked);
		}

		private static void Validate(string eventsDir)
		{
			if (string.IsNullOrEmpty(eventsDir) || !Directory.Exists(eventsDir))
			{
				Debug.LogError($"Events directory not found: {eventsDir}");
				return;
			}

			var loader = new ContentLoader(new NewtonsoftJsonDeserializer());
			var list = loader.LoadEvents(eventsDir).GetAwaiter().GetResult() ?? new List<NarrativeEvent>();

			var ids = new HashSet<string>();
			var dupes = new HashSet<string>();
			var byCategory = new Dictionary<EventCategory, int>();
			foreach (EventCategory c in System.Enum.GetValues(typeof(EventCategory))) byCategory[c] = 0;

			foreach (var ev in list)
			{
				if (ev == null) continue;
				if (string.IsNullOrEmpty(ev.Id)) continue;
				if (!ids.Add(ev.Id)) dupes.Add(ev.Id);
				if (byCategory.ContainsKey(ev.Category)) byCategory[ev.Category]++;
			}

			var idSet = new HashSet<string>(ids);
			var orphanRefs = new List<string>();
			foreach (var ev in list)
			{
				if (ev == null) continue;
				if (!string.IsNullOrEmpty(ev.Next) && !idSet.Contains(ev.Next)) orphanRefs.Add($"{ev.Id} -> next:{ev.Next}");
				if (ev.Choices != null)
				{
					for (int i = 0; i < ev.Choices.Count; i++)
					{
						var ch = ev.Choices[i];
						if (!string.IsNullOrEmpty(ch?.Next) && !idSet.Contains(ch.Next))
						{
							orphanRefs.Add($"{ev.Id} choice:{ch.Id} -> next:{ch.Next}");
						}
					}
				}
			}

			// Report
			Debug.Log($"[THE-RPG] Events loaded: {list.Count}\n" +
				$"By Category: Town={byCategory[EventCategory.Town]}, Wild={byCategory[EventCategory.Wild]}, Dungeon={byCategory[EventCategory.Dungeon]}, Faction={byCategory[EventCategory.Faction]}, Story={byCategory[EventCategory.Story]}");

			if (dupes.Count > 0)
			{
				Debug.LogError($"Duplicate IDs ({dupes.Count}):\n" + string.Join("\n", dupes.Take(50)) + (dupes.Count > 50 ? "\n..." : ""));
			}
			else
			{
				Debug.Log("No duplicate IDs detected.");
			}

			if (orphanRefs.Count > 0)
			{
				Debug.LogError($"Orphan next references ({orphanRefs.Count}):\n" + string.Join("\n", orphanRefs.Take(50)) + (orphanRefs.Count > 50 ? "\n..." : ""));
			}
			else
			{
				Debug.Log("No orphan next references detected.");
			}
		}

		private static void LoadToRuntime(string eventsDir)
		{
			if (string.IsNullOrEmpty(eventsDir) || !Directory.Exists(eventsDir))
			{
				Debug.LogError($"Events directory not found: {eventsDir}");
				return;
			}
			NarrativeRuntime.InitializeFromFolder(eventsDir).GetAwaiter().GetResult();
			Debug.Log("[THE-RPG] Narrative runtime cache loaded. You can enter Play Mode now.");
		}
	}
}


