using Newtonsoft.Json;
using TheRPG.Content;

namespace TheRPG.Narrative
{
	public sealed class NewtonsoftJsonDeserializer : IJsonDeserializer
	{
		private readonly JsonSerializerSettings _settings;

		public NewtonsoftJsonDeserializer(JsonSerializerSettings settings = null)
		{
			_settings = settings ?? new JsonSerializerSettings
			{
				MissingMemberHandling = MissingMemberHandling.Ignore,
				NullValueHandling = NullValueHandling.Include
			};
		}

		public NarrativeEvent DeserializeEvent(string json)
		{
			if (string.IsNullOrEmpty(json)) return null;
			try
			{
				return JsonConvert.DeserializeObject<NarrativeEvent>(json, _settings);
			}
			catch
			{
				return null;
			}
		}
	}
}


