using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheRPG.Content;
using TheRPG.World;

namespace TheRPG.Narrative
{
	public interface INarrativeGraph
	{
		Task LoadFromDirectory(string dir);
		NarrativeEvent PickEvent(EventCategory category, WorldState world);
		ResolutionResult ResolveChoice(string eventId, string choiceId, WorldState world);
		void TickCooldowns();
	}

	public sealed class NarrativeGraph : INarrativeGraph
	{
		private readonly Dictionary<EventCategory, List<NarrativeEvent>> _eventsByCategory = new Dictionary<EventCategory, List<NarrativeEvent>>();
		private readonly Dictionary<string, int> _cooldownTracker = new Dictionary<string, int>();
		private readonly IConditionEvaluator _evaluator;
		private readonly IContentLoader _loader;
		private readonly Random _rng = new Random();

		public NarrativeGraph()
		{
			_evaluator = new ConditionEvaluator();
			_loader = new ContentLoader();
			foreach (EventCategory c in Enum.GetValues(typeof(EventCategory)))
			{
				_eventsByCategory[c] = new List<NarrativeEvent>();
			}
		}

		public async Task LoadFromDirectory(string dir)
		{
			// Loader returns flat list; group by category
			var all = await _loader.LoadEvents(dir);
			foreach (var list in _eventsByCategory.Values) list.Clear();
			if (all != null)
			{
				foreach (var ev in all)
				{
					if (!_eventsByCategory.TryGetValue(ev.Category, out var bucket))
					{
						bucket = new List<NarrativeEvent>();
						_eventsByCategory[ev.Category] = bucket;
					}
					bucket.Add(ev);
				}
			}
		}

		public NarrativeEvent PickEvent(EventCategory category, WorldState world)
		{
			if (!_eventsByCategory.TryGetValue(category, out var events) || events.Count == 0) return null;
			var pool = new List<NarrativeEvent>();
			foreach (var ev in events)
			{
				if (ev == null) continue;
				if (ev.Weight <= 0) continue;
				if (_cooldownTracker.TryGetValue(ev.Id, out var cd) && cd > 0) continue;
				if (ev.Conditions != null && !_evaluator.Evaluate(ev.Conditions, world)) continue;
				pool.Add(ev);
			}

			var total = 0;
			for (int i = 0; i < pool.Count; i++) total += Math.Max(0, pool[i].Weight);
			if (total <= 0 || pool.Count == 0) return null;

			var r = _rng.Next(1, total + 1); // inclusive
			var acc = 0;
			for (int i = 0; i < pool.Count; i++)
			{
				acc += Math.Max(0, pool[i].Weight);
				if (r <= acc) return pool[i];
			}
			return pool[pool.Count - 1];
		}

		public void TickCooldowns()
		{
			var keys = _cooldownTracker.Keys.ToList();
			for (int i = 0; i < keys.Count; i++)
			{
				var k = keys[i];
				var v = _cooldownTracker[k];
				_cooldownTracker[k] = Math.Max(0, v - 1);
			}
		}

		private void ApplyCooldown(NarrativeEvent ev)
		{
			if (ev != null && ev.Cooldown > 0)
			{
				_cooldownTracker[ev.Id] = ev.Cooldown;
			}
		}

		public ResolutionResult ResolveChoice(string eventId, string choiceId, WorldState world)
		{
			var ev = FindEvent(eventId);
			var choice = ev?.Choices?.FirstOrDefault(c => c.Id == choiceId);
			if (ev == null || choice == null)
			{
				return new ResolutionResult { Ok = false, NextEventId = null, AppliedEffects = new List<Effect>() };
			}

			if (choice.Requirements != null && !_evaluator.Evaluate(choice.Requirements, world))
			{
				return new ResolutionResult { Ok = false, NextEventId = null, AppliedEffects = new List<Effect>() };
			}

			var applied = new List<Effect>();
			foreach (var eff in choice.Effects ?? Enumerable.Empty<Effect>())
			{
				ApplyEffect(world, eff);
				applied.Add(eff);
			}

			ApplyCooldown(ev);

			var next = !string.IsNullOrEmpty(choice.Next) ? choice.Next : ev.Next;
			return new ResolutionResult { Ok = true, NextEventId = next, AppliedEffects = applied };
		}

		private NarrativeEvent FindEvent(string eventId)
		{
			foreach (var kv in _eventsByCategory)
			{
				var found = kv.Value.FirstOrDefault(e => e.Id == eventId);
				if (found != null) return found;
			}
			return null;
		}

		private static int ToInt(object v)
		{
			if (v == null) return 0;
			if (v is int i) return i;
			if (v is long l) return (int)l;
			if (v is float f) return (int)f;
			if (v is double d) return (int)d;
			if (int.TryParse(v.ToString(), out var p)) return p;
			return 0;
		}

		private static void Add(Dictionary<string, object> vars, string key, int delta)
		{
			if (string.IsNullOrEmpty(key)) return;
			if (!vars.TryGetValue(key, out var cur))
			{
				vars[key] = delta;
				return;
			}
			vars[key] = ToInt(cur) + delta;
		}

		private static void AddToList(Dictionary<string, object> vars, string key, object value)
		{
			if (string.IsNullOrEmpty(key) || value == null) return;
			if (!vars.TryGetValue(key, out var cur) || !(cur is List<object> list))
			{
				list = new List<object>();
				vars[key] = list;
			}
			list.Add(value);
		}

		private static void ApplyEffect(WorldState world, Effect eff)
		{
			var vars = world?.Vars;
			if (vars == null || eff == null) return;
			switch (eff.Type)
			{
				case "rep":
					Add(vars, eff.Key, ToInt(eff.Delta));
					break;
				case "stat":
					Add(vars, eff.Key, ToInt(eff.Delta));
					break;
				case "currency":
					Add(vars, eff.Key, ToInt(eff.Delta));
					break;
				case "add_item":
					AddToList(vars, "inventory", eff.Key);
					break;
				case "add_quest":
					AddToList(vars, "quests", eff.Key);
					break;
				case "gain_trait":
					AddToList(vars, "traits", eff.Key);
					break;
				case "start_combat":
					vars["pendingCombat"] = eff.Key; // eff.Set 可在后续扩展
					break;
				default:
					break;
			}
		}
	}
}


