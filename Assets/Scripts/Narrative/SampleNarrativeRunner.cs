using System.Threading.Tasks;
using UnityEngine;
using TheRPG.Content;

namespace TheRPG.Narrative
{
	public sealed class SampleNarrativeRunner : MonoBehaviour
	{
		[SerializeField] string _eventsRelativeFolder = "Assets/Content/events";

		async void Start()
		{
			await NarrativeRuntime.InitializeFromFolder(System.IO.Path.Combine(Application.dataPath, "Content/events"));
			var ev = NarrativeRuntime.Graph.PickEvent(EventCategory.Town, NarrativeRuntime.World);
			if (ev != null)
			{
				Debug.Log($"[THE-RPG] Picked event: {ev.Id} ({ev.Category})");
			}
			else
			{
				Debug.LogWarning("[THE-RPG] No event picked.");
			}
		}
	}
}


