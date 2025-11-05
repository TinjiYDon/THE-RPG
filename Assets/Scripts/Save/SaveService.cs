using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using TheRPG.World;
using TheRPG.Narrative;

namespace TheRPG.Save
{
	public interface ISaveService
	{
		Task<GameSave> Load(int slot);
		Task Save(int slot, GameSave data);
		Task Migrate(int fromVersion, int toVersion);
	}

	public sealed class SaveService : ISaveService
	{
		private readonly string _saveDir;

		public SaveService()
		{
			_saveDir = Path.Combine(Application.persistentDataPath, "Saves");
			Directory.CreateDirectory(_saveDir);
		}

		private string GetSavePath(int slot)
		{
			return Path.Combine(_saveDir, $"save_{slot}.json");
		}

		public async Task<GameSave> Load(int slot)
		{
			var path = GetSavePath(slot);
			if (!File.Exists(path))
			{
				return new GameSave
				{
					Version = 1,
					Scene = "Town",
					WorldState = WorldBootstrap.CreateDefault(),
					HistoryIds = new System.Collections.Generic.List<string>()
				};
			}

			try
			{
				var json = await File.ReadAllTextAsync(path);
				var save = JsonUtility.FromJson<GameSaveJson>(json);
				if (save == null) throw new Exception("Failed to parse save file");

				// Migrate if needed
				if (save.version < 1)
				{
					await Migrate(save.version, 1);
					return await Load(slot); // Reload after migration
				}

				return new GameSave
				{
					Version = save.version,
					Scene = save.scene ?? "Town",
					WorldState = DeserializeWorldState(save.worldState),
					HistoryIds = save.historyIds ?? new System.Collections.Generic.List<string>()
				};
			}
			catch (Exception e)
			{
				Debug.LogError($"[THE-RPG] Failed to load save slot {slot}: {e}");
				return new GameSave
				{
					Version = 1,
					Scene = "Town",
					WorldState = WorldBootstrap.CreateDefault(),
					HistoryIds = new System.Collections.Generic.List<string>()
				};
			}
		}

		public async Task Save(int slot, GameSave data)
		{
			var path = GetSavePath(slot);
			try
			{
				var json = new GameSaveJson
				{
					version = data.Version,
					scene = data.Scene,
					worldState = SerializeWorldState(data.WorldState),
					historyIds = data.HistoryIds ?? new System.Collections.Generic.List<string>()
				};

				var jsonString = JsonUtility.ToJson(json, true);
				await File.WriteAllTextAsync(path, jsonString);
				Debug.Log($"[THE-RPG] Saved to slot {slot}");
			}
			catch (Exception e)
			{
				Debug.LogError($"[THE-RPG] Failed to save slot {slot}: {e}");
				throw;
			}
		}

		public Task Migrate(int fromVersion, int toVersion)
		{
			// Placeholder: implement migration logic if needed
			Debug.Log($"[THE-RPG] Migrating save from v{fromVersion} to v{toVersion}");
			return Task.CompletedTask;
		}

		private WorldState DeserializeWorldState(string json)
		{
			var world = WorldBootstrap.CreateDefault();
			if (string.IsNullOrEmpty(json)) return world;
			try
			{
				var wrapper = JsonUtility.FromJson<WorldStateWrapper>(json);
				if (wrapper?.vars != null)
				{
					foreach (var entry in wrapper.vars)
					{
						if (entry != null && !string.IsNullOrEmpty(entry.key))
						{
							world.Vars[entry.key] = entry.value;
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning($"[THE-RPG] Failed to deserialize world state: {e}");
			}
			return world;
		}

		private string SerializeWorldState(WorldState world)
		{
			var wrapper = new WorldStateWrapper();
			if (world?.Vars != null)
			{
				wrapper.vars = new System.Collections.Generic.List<WorldStateEntry>();
				foreach (var kv in world.Vars)
				{
					wrapper.vars.Add(new WorldStateEntry { key = kv.Key, value = kv.Value?.ToString() ?? "" });
				}
			}
			return JsonUtility.ToJson(wrapper);
		}

		[Serializable]
		private sealed class GameSaveJson
		{
			public int version;
			public string scene;
			public string worldState;
			public System.Collections.Generic.List<string> historyIds;
		}

		[Serializable]
		private sealed class WorldStateWrapper
		{
			public System.Collections.Generic.List<WorldStateEntry> vars;
		}

		[Serializable]
		private sealed class WorldStateEntry
		{
			public string key;
			public string value;
		}
	}
}
