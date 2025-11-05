using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TheRPG.AI;
using TheRPG.Content;
using TheRPG.Narrative;
using TheRPG.Save;
using TheRPG.UI;

namespace TheRPG.Dialogue
{
	public sealed class EventLoopRunner : MonoBehaviour
	{
		[SerializeField] DialogueUI _ui;
		[SerializeField] EventCategory _category = EventCategory.Town;

		private IAIAdapter _aiAdapter;
		private FeatureFlags _flags;
		private ISaveService _saveService;

		async void Start()
		{
			_flags = FeatureFlags.Instance;
			_flags.Load();
			_aiAdapter = _flags.EnableAi ? new AIAdapter(_flags) : new NullAIAdapter() as IAIAdapter;
			_saveService = new SaveService();

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

			if (_aiAdapter.IsEnabled() && !ev.IsCritical() && !string.IsNullOrEmpty(ev.Text?.AiStyle))
			{
				_ui.ShowLoading(StringTable.Get("ui.loading.init_model", "初始化 AI 模型..."));
				await _aiAdapter.EnsureModelReady((progress, msg) =>
				{
					_ui.ShowProgress(progress, msg);
				});
				_ui.HideLoading();

				_ui.ShowLoading(StringTable.Get("ui.loading.stylizing", "AI 润色中..."));
				var target = ParseAiTarget(ev.Text.AiTarget);
				var card = new CharacterCard { Id = "char_mc", Name = "游丝", Bio = "黑街散修，兼收旧谱与旧世固件，刀术入电。" };
				rendered = await _aiAdapter.Stylize(baseText, target, ev.Text.AiStyle, card, _flags.AiTokenBudget, _flags.AiTimeoutMs);
				_ui.HideLoading();
			}

			var choice = await _ui.Show(ev, rendered, ev.Choices as IReadOnlyList<Choice>, world);
			if (choice == null) return;
			var res = graph.ResolveChoice(ev.Id, choice.Id, world);
			graph.TickCooldowns();

			// Floating feedback for applied effects
			var canvas = FindObjectOfType<Canvas>();
			if (canvas != null && res.AppliedEffects != null)
			{
				foreach (var eff in res.AppliedEffects)
				{
					if (eff == null) continue;
					string msg = null;
					switch (eff.Type)
					{
						case "rep": msg = $"声望 {eff.Key} {(int) (int.Parse(eff.Delta.ToString()) >=0 ? "+" : "")}{eff.Delta}"; break;
						case "currency": msg = $"信用 {(int) (int.Parse(eff.Delta.ToString()) >=0 ? "+" : "")}{eff.Delta}"; break;
						case "stat": msg = $"{eff.Key} {(int) (int.Parse(eff.Delta.ToString()) >=0 ? "+" : "")}{eff.Delta}"; break;
						case "add_item": msg = $"获得 {eff.Key}"; break;
						case "gain_trait": msg = $"获得特性 {eff.Key}"; break;
					}
					if (!string.IsNullOrEmpty(msg)) FloatingText.Spawn(canvas, msg, Color.cyan);
				}
			}

			var save = new GameSave
			{
				Version = 1,
				Scene = _category.ToString(),
				WorldState = world,
				HistoryIds = new System.Collections.Generic.List<string> { ev.Id }
			};
			await _saveService.Save(1, save);

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


