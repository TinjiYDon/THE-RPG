using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using TheRPG.Content;

namespace TheRPG.AI
{
	public interface IAIAdapter
	{
		bool IsEnabled();
		Task EnsureModelReady();
		Task<string> Stylize(string @base, AiTarget target, string style, CharacterCard card, int budgetTokens, int timeoutMs);
	}

	public sealed class NullAIAdapter : IAIAdapter
	{
		public bool IsEnabled() { return false; }
		public Task EnsureModelReady() { return Task.CompletedTask; }
		public Task<string> Stylize(string @base, AiTarget target, string style, CharacterCard card, int budgetTokens, int timeoutMs)
		{
			return Task.FromResult(@base);
		}
	}

	public sealed class AIAdapter : IAIAdapter
	{
		private readonly FeatureFlags _flags;
		private bool _modelReady;

		public AIAdapter(FeatureFlags flags = null)
		{
			_flags = flags ?? FeatureFlags.Instance;
		}

		public bool IsEnabled()
		{
			return _flags.EnableAi;
		}

		public async Task EnsureModelReady()
		{
			if (_modelReady) return;
			if (!_flags.EnableModelDownload)
			{
				Debug.LogWarning("[THE-RPG] Model download disabled in feature flags");
				return;
			}
			// Placeholder: actual model download/init would go here
			await Task.Delay(100);
			_modelReady = true;
		}

		public async Task<string> Stylize(string @base, AiTarget target, string style, CharacterCard card, int budgetTokens, int timeoutMs)
		{
			if (!IsEnabled() || string.IsNullOrEmpty(@base))
			{
				return @base;
			}

			var cts = new CancellationTokenSource(timeoutMs);
			try
			{
				var result = await Task.Run(async () =>
				{
					// Placeholder: actual LLM call would go here
					// For now, simulate a delay and return base text
					await Task.Delay(100, cts.Token);
					return @base;
				}, cts.Token);

				return result;
			}
			catch (OperationCanceledException)
			{
				Debug.LogWarning($"[THE-RPG] AI stylize timeout ({timeoutMs}ms), falling back to base text");
				return _flags.FallbackToBaseText ? @base : string.Empty;
			}
			catch (Exception e)
			{
				Debug.LogError($"[THE-RPG] AI stylize error: {e}, falling back to base text");
				return _flags.FallbackToBaseText ? @base : string.Empty;
			}
			finally
			{
				cts.Dispose();
			}
		}
	}
}
