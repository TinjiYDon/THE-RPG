enum EventCategory { town, wild, dungeon, faction, story }

class Condition {
	final String? type;
	final String? op;
	final String? key;
	final dynamic value;
	Condition({this.type, this.op, this.key, this.value});
	factory Condition.fromJson(Map<String, dynamic> j) => Condition(
		type: j['type'] as String?,
		op: j['op'] as String?,
		key: j['key'] as String?,
		value: j['value'],
	);
}

class Effect {
	final String? type;
	final String? key;
	final dynamic delta;
	final dynamic set;
	Effect({this.type, this.key, this.delta, this.set});
	factory Effect.fromJson(Map<String, dynamic> j) => Effect(
		type: j['type'] as String?,
		key: j['key'] as String?,
		delta: j['delta'],
		set: j['set'],
	);
}

class Choice {
	final String id;
	final String label;
	final String? aiLabelHint;
	final List<Condition> requirements;
	final List<Effect> effects;
	final String? next;
	Choice({required this.id, required this.label, this.aiLabelHint, required this.requirements, required this.effects, this.next});
	factory Choice.fromJson(Map<String, dynamic> j) => Choice(
		id: j['id'] as String,
		label: j['label'] as String,
		aiLabelHint: j['ai_label_hint'] as String?,
		requirements: ((j['requirements'] as List?) ?? const []).map((e) => Condition.fromJson(e as Map<String, dynamic>)).toList(),
		effects: ((j['effects'] as List?) ?? const []).map((e) => Effect.fromJson(e as Map<String, dynamic>)).toList(),
		next: j['next'] as String?,
	);
}

class EventText {
	final String base;
	final String? aiStyle;
	final String? aiTarget;
	EventText({required this.base, this.aiStyle, this.aiTarget});
	factory EventText.fromJson(Map<String, dynamic> j) => EventText(
		base: j['base'] as String? ?? '',
		aiStyle: j['ai_style'] as String?,
		aiTarget: j['ai_target'] as String?,
	);
}

class NarrativeEvent {
	final String id;
	final EventCategory category;
	final List<String> tags;
	final int weight;
	final int cooldown;
	final List<Condition> conditions;
	final EventText text;
	final List<Choice> choices;
	final Map<String, dynamic> variables;
	final String? next;
	NarrativeEvent({required this.id, required this.category, required this.tags, required this.weight, required this.cooldown, required this.conditions, required this.text, required this.choices, required this.variables, this.next});
	factory NarrativeEvent.fromJson(Map<String, dynamic> j) => NarrativeEvent(
		id: j['id'] as String,
		category: _catFrom(j['category'] as String?),
		tags: ((j['tags'] as List?) ?? const []).cast<String>(),
		weight: (j['weight'] as num?)?.toInt() ?? 0,
		cooldown: (j['cooldown'] as num?)?.toInt() ?? 0,
		conditions: ((j['conditions'] as List?) ?? const []).map((e) => Condition.fromJson(e as Map<String, dynamic>)).toList(),
		text: EventText.fromJson((j['text'] as Map?)?.cast<String, dynamic>() ?? const {}),
		choices: ((j['choices'] as List?) ?? const []).map((e) => Choice.fromJson(e as Map<String, dynamic>)).toList(),
		variables: (j['variables'] as Map?)?.cast<String, dynamic>() ?? <String, dynamic>{},
		next: j['next'] as String?,
	);
}

EventCategory _catFrom(String? s) {
	switch (s) {
		case 'town': return EventCategory.town;
		case 'wild': return EventCategory.wild;
		case 'dungeon': return EventCategory.dungeon;
		case 'faction': return EventCategory.faction;
		case 'story': return EventCategory.story;
		default: return EventCategory.town;
	}
}


