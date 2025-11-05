using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TheRPG.AI;
using TheRPG.Content;
using TheRPG.Narrative;
using TheRPG.Save;
using TheRPG.UI;
using TheRPG.Tutorial;

namespace TheRPG.Dialogue
{
	public sealed class EventLoopRunner : MonoBehaviour
	{
		[SerializeField] DialogueUI _ui;
		[SerializeField] EventCategory _category = EventCategory.Town;

		private IAIAdapter _aiAdapter;
		private FeatureFlags _flags;
		private ISaveService _saveService;
		private GuidanceBanner _banner;

		async void Start()
		{
			_flags = FeatureFlags.Instance;
			_flags.Load();
			_aiAdapter = _flags.EnableAi ? new AIAdapter(_flags) : new NullAIAdapter() as IAIAdapter;
			_saveService = new SaveService();
			_banner = FindObjectOfType<GuidanceBanner>() ?? new GameObject("GuidanceBanner").AddComponent<GuidanceBanner>();

			if (NarrativeRuntime.Graph == null)
			{
				await NarrativeRuntime.InitializeFromFolder(System.IO.Path.Combine(Application.dataPath, "Content/events"));
			}
			if (_ui == null) _ui = FindObjectOfType<DialogueUI>() ?? new GameObject("DialogueUI").AddComponent<DialogueUI>();

			if (!TutorialManager.HasSeen("intro_loop"))
			{
				_banner.Show("guide.intro.loop");
				TutorialManager.MarkSeen("intro_loop");
			}

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
				var style = ResolveStyle(ev.Text.AiStyle);
				var card = new CharacterCard { Id = "char_mc", Name = "游丝", Bio = "黑街散修，兼收旧谱与旧世固件，刀术入电。" };
				rendered = await _aiAdapter.Stylize(baseText, target, style, card, _flags.AiTokenBudget, _flags.AiTimeoutMs);
				_ui.HideLoading();
			}

			var combined = new List<Choice>();
			if (ev.Choices != null) combined.AddRange(ev.Choices);
			if (_aiAdapter.IsEnabled() && ev.AiGenerated != null)
			{
				foreach (var t in ev.AiGenerated)
				{
					var labelHint = !string.IsNullOrEmpty(t.AiLabelHint) ? t.AiLabelHint : t.Label;
					var style2 = ResolveStyle(ev.Text?.AiStyle);
					var label = await _aiAdapter.Stylize(labelHint ?? "选择", AiTarget.Dialogue, style2, new CharacterCard { Id = "char_mc", Name = "游丝", Bio = "" }, _flags.AiTokenBudget, _flags.AiTimeoutMs);
					combined.Add(new Choice { Id = string.IsNullOrEmpty(t.Id) ? ($"ai_{combined.Count}") : t.Id, Label = label, AiLabelHint = t.AiLabelHint, Requirements = t.Requirements, Effects = t.Effects, Next = t.Next });
				}
			}

			if (!TutorialManager.HasSeen("choice_hints"))
			{
				_banner.Show("guide.choice.hint");
				TutorialManager.MarkSeen("choice_hints");
			}

			var evaluator = new ConditionEvaluator();
			var filtered = new List<Choice>();
			foreach (var ch in combined)
			{
				if (ch.Requirements == null || evaluator.Evaluate(ch.Requirements, world))
				{
					filtered.Add(ch);
				}
			}

			var choice = await _ui.Show(ev, rendered, filtered, world);
			if (choice == null) return;
			var res = graph.ResolveChoice(ev.Id, choice.Id, world);
			graph.TickCooldowns();

			if (!TutorialManager.HasSeen("feedback"))
			{
				_banner.Show("guide.feedback");
				TutorialManager.MarkSeen("feedback");
			}

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

			// Causal guidance
			_banner.Show("guide.feedback");

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

		private string ResolveStyle(string eventStyle)
		{
			switch (_flags.StylePreset)
			{
				case "cold": return "冷峻、短句、江湖俚语+企业黑话混用";
				case "bold": return "豪放、俚语更重、句式更有力";
				case "minimal": return "极简、留白、信息密度低";
				default: return eventStyle ?? "";
			}
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


