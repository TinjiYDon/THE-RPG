using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TheRPG.AI;
using TheRPG.Content;
using TheRPG.Narrative;

namespace TheRPG.Dialogue
{
	public sealed class EventLoopRunner : MonoBehaviour
	{
		[SerializeField] DialogueUI _ui;
		[SerializeField] EventCategory _category = EventCategory.Town;

		private IAIAdapter _aiAdapter;
		private FeatureFlags _flags;

		async void Start()
		{
			_flags = FeatureFlags.Instance;
			_flags.Load();
			_aiAdapter = _flags.EnableAi ? new AIAdapter(_flags) : new NullAIAdapter() as IAIAdapter;

			if (NarrativeRuntime.Graph == null)
			{
				await NarrativeRuntime.InitializeFromFolder(System.IO.Path.Combine(Application.dataPath, "Content/events"));
			}
			if (_ui == null) _ui = FindObjectOfType<DialogueUI>() ?? new GameObject("DialogueUI").AddComponent<DialogueUI>();
			await RunOnce();
		}

		async Task RunOnce()
		{
			var world = NarrativeRuntime.World;
			var graph = NarrativeRuntime.Graph;
			var ev = graph.PickEvent(_category, world);
			if (ev == null) { Debug.LogWarning("[THE-RPG] No event available."); return; }

			var baseText = ev.Text?.Base ?? string.Empty;
			var rendered = baseText;

			// AI stylize if enabled and not critical
			if (_aiAdapter.IsEnabled() && !ev.IsCritical() && !string.IsNullOrEmpty(ev.Text?.AiStyle))
			{
				await _aiAdapter.EnsureModelReady();
				var target = ParseAiTarget(ev.Text.AiTarget);
				var card = new CharacterCard { Id = "char_mc", Name = "游丝", Bio = "黑街散修，兼收旧谱与旧世固件，刀术入电。" };
				rendered = await _aiAdapter.Stylize(baseText, target, ev.Text.AiStyle, card, _flags.AiTokenBudget, _flags.AiTimeoutMs);
			}

			var choice = await _ui.Show(ev, rendered, ev.Choices as IReadOnlyList<Choice>);
			if (choice == null) return;
			var res = graph.ResolveChoice(ev.Id, choice.Id, world);
			Debug.Log($"[THE-RPG] Choice: {choice.Id}, next: {res.NextEventId}");
		}

		private AiTarget ParseAiTarget(string s)
		{
			if (string.IsNullOrEmpty(s)) return AiTarget.Flavor;
			s = s.ToLowerInvariant();
			if (s == "summary") return AiTarget.Summary;
			if (s == "dialogue") return AiTarget.Dialogue;
			return AiTarget.Flavor;
		}
	}
}


