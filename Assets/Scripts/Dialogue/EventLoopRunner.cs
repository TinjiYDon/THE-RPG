using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TheRPG.Content;
using TheRPG.Narrative;

namespace TheRPG.Dialogue
{
	public sealed class EventLoopRunner : MonoBehaviour
	{
		[SerializeField] DialogueUI _ui;
		[SerializeField] EventCategory _category = EventCategory.Town;

		async void Start()
		{
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
			var rendered = ev.Text?.Base ?? string.Empty; // hook AI stylize here when integrated
			var choice = await _ui.Show(ev, rendered, ev.Choices as IReadOnlyList<Choice>);
			if (choice == null) return;
			var res = graph.ResolveChoice(ev.Id, choice.Id, world);
			Debug.Log($"[THE-RPG] Choice: {choice.Id}, next: {res.NextEventId}");
		}
	}
}


