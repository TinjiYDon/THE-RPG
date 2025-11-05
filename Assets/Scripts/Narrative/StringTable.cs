using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TheRPG.Narrative
{
	public static class StringTable
	{
		private static readonly Dictionary<string, string> _map = new Dictionary<string, string>();

		public static void Load(string locale = "zh_CN")
		{
			_map.Clear();
			var path = Path.Combine(Application.dataPath, "Content/strings/" + locale + ".json");
			if (!File.Exists(path)) return;
			try
			{
				var json = File.ReadAllText(path);
				var wrapper = JsonUtility.FromJson<StringMapWrapper>(ToWrapper(json));
				if (wrapper?.entries != null)
				{
					foreach (var e in wrapper.entries)
					{
						_map[e.k] = e.v;
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning($"[THE-RPG] Failed to load string table: {e}");
			}
		}

		public static string Get(string key, string fallback = null)
		{
			if (string.IsNullOrEmpty(key)) return fallback ?? key;
			return _map.TryGetValue(key, out var v) ? v : (fallback ?? key);
		}

		// JsonUtility can't parse dictionary directly; wrap to array
		[Serializable]
		private sealed class StringMapWrapper { public Entry[] entries; }
		[Serializable]
		private sealed class Entry { public string k; public string v; }

		private static string ToWrapper(string json)
		{
			// very small converter: assumes flat object of string:string
			var dict = MiniJson.Deserialize(json) as Dictionary<string, object>;
			var list = new List<Entry>();
			if (dict != null)
			{
				foreach (var kv in dict) list.Add(new Entry { k = kv.Key, v = kv.Value?.ToString() ?? string.Empty });
			}
			var wrapper = new StringMapWrapper { entries = list.ToArray() };
			return JsonUtility.ToJson(wrapper);
		}
	}
}


