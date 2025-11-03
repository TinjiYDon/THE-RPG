using System;
using System.IO;
using UnityEngine;

namespace TheRPG.AI
{
	public sealed class FeatureFlags
	{
		public bool EnableAi { get; private set; } = true;
		public bool EnableModelDownload { get; private set; } = true;
		public int AiTokenBudget { get; private set; } = 96;
		public int AiTimeoutMs { get; private set; } = 1800;
		public bool FallbackToBaseText { get; private set; } = true;

		private static FeatureFlags _instance;
		public static FeatureFlags Instance
		{
			get
			{
				if (_instance == null) _instance = new FeatureFlags();
				return _instance;
			}
		}

		public void Load()
		{
			var path = Path.Combine(Application.dataPath, "Configs/feature_flags.json");
			if (!File.Exists(path)) return;
			try
			{
				var json = File.ReadAllText(path);
				var parsed = JsonUtility.FromJson<FeatureFlagsJson>(json);
				if (parsed != null)
				{
					EnableAi = parsed.enable_ai;
					EnableModelDownload = parsed.enable_model_download;
					AiTokenBudget = parsed.ai_token_budget;
					AiTimeoutMs = parsed.ai_timeout_ms;
					FallbackToBaseText = parsed.fallback_to_base_text;
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning($"[THE-RPG] Failed to load feature_flags.json: {e}");
			}
		}

		[Serializable]
		private sealed class FeatureFlagsJson
		{
			public bool enable_ai;
			public bool enable_model_download;
			public int ai_token_budget;
			public int ai_timeout_ms;
			public bool fallback_to_base_text;
		}
	}
}
