using System;
using System.Collections.Generic;

namespace TheRPG.AI
{
	public enum AiTarget { Summary, Dialogue, Flavor }

	public sealed class CharacterCard
	{
		public string Id;
		public string Name;
		public string Bio;
		public CharacterSpeech Speech;
		public CharacterStyle Style;
	}

	public sealed class CharacterSpeech
	{
		public string Tone;
		public List<string> Taboos;
	}

	public sealed class CharacterStyle
	{
		public List<string> AllowTerms;
		public List<string> BanTerms;
	}
}

namespace TheRPG.Content
{
	public enum EventCategory { Town, Wild, Dungeon, Faction, Story }

	public sealed class Condition
	{
		public string Type;
		public string Op;
		public string Key;
		public object Value;
	}

	public sealed class Effect
	{
		public string Type;
		public string Key;
		public object Delta;
		public object Set;
	}

	public sealed class Choice
	{
		public string Id;
		public string Label;
		public string AiLabelHint;
		public List<Condition> Requirements;
		public List<Effect> Effects;
		public string Next;
	}

	public sealed class EventText
	{
		public string Base;
		public string AiStyle;
		public string AiTarget; // "summary"|"dialogue"|"flavor"
	}

	public sealed class NarrativeEvent
	{
		public string Id;
		public EventCategory Category;
		public List<string> Tags;
		public int Weight;
		public int Cooldown;
		public List<Condition> Conditions;
		public EventText Text;
		public List<Choice> Choices;
		public Dictionary<string, object> Variables;
		public string Next;

		public bool IsCritical()
		{
			if (Variables == null) return false;
			return Variables.TryGetValue("critical", out var v) && v is bool b && b;
		}
	}

	public sealed class ResolutionResult
	{
		public bool Ok;
		public string NextEventId;
		public List<Effect> AppliedEffects;
	}
}

namespace TheRPG.World
{
	public sealed class WorldState
	{
		public Dictionary<string, object> Vars = new Dictionary<string, object>();
	}
}

namespace TheRPG.Save
{
	public sealed class GameSave
	{
		public int Version;
		public string Scene;
		public TheRPG.World.WorldState WorldState;
		public List<string> HistoryIds;
	}
}


