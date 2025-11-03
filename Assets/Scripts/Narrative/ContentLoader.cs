using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TheRPG.Content;

namespace TheRPG.Narrative
{
	public interface IContentLoader
	{
		Task<IReadOnlyList<NarrativeEvent>> LoadEvents(string directory);
	}

	public interface IJsonDeserializer
	{
		NarrativeEvent DeserializeEvent(string json);
	}

	public sealed class NullJsonDeserializer : IJsonDeserializer
	{
		public NarrativeEvent DeserializeEvent(string json) { return null; }
	}

	public sealed class ContentLoader : IContentLoader
	{
		private readonly IJsonDeserializer _deserializer;

		public ContentLoader() : this(new NullJsonDeserializer()) {}

		public ContentLoader(IJsonDeserializer deserializer)
		{
			_deserializer = deserializer ?? new NullJsonDeserializer();
		}

		public async Task<IReadOnlyList<NarrativeEvent>> LoadEvents(string directory)
		{
			var results = new List<NarrativeEvent>();
			if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
			{
				return await Task.FromResult(results);
			}

			var files = Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				var path = files[i];
				string json;
				try { json = File.ReadAllText(path); }
				catch { continue; }

				NarrativeEvent ev = null;
				try { ev = _deserializer.DeserializeEvent(json); }
				catch { ev = null; }

				if (ev != null) results.Add(ev);
			}

			return await Task.FromResult((IReadOnlyList<NarrativeEvent>)results);
		}
	}
}


