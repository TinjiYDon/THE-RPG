import 'dart:math';
import 'models.dart';
import 'evaluator.dart';
import 'world_state.dart';

class NarrativeGraph {
	final Map<EventCategory, List<NarrativeEvent>> _byCat = {
		EventCategory.town: <NarrativeEvent>[],
		EventCategory.wild: <NarrativeEvent>[],
		EventCategory.dungeon: <NarrativeEvent>[],
		EventCategory.faction: <NarrativeEvent>[],
		EventCategory.story: <NarrativeEvent>[],
	};
	final Map<String, int> _cooldowns = <String, int>{};
	final _eval = ConditionEvaluator();
	final _rng = Random();

	void load(List<NarrativeEvent> events) {
		for (final list in _byCat.values) list.clear();
		for (final e in events) {
			_byCat[e.category]!.add(e);
		}
	}

	NarrativeEvent? pick(EventCategory cat, WorldState world) {
		final src = _byCat[cat] ?? const <NarrativeEvent>[];
		final pool = <NarrativeEvent>[];
		for (final e in src) {
			if (e.weight <= 0) continue;
			final cd = _cooldowns[e.id] ?? 0; if (cd > 0) continue;
			if (e.conditions.isNotEmpty && !_eval.evaluate(e.conditions, world)) continue;
			pool.add(e);
		}
		var total = 0; for (final e in pool) { total += e.weight; }
		if (total <= 0 || pool.isEmpty) return null;
		final r = _rng.nextInt(total) + 1;
		var acc = 0;
		for (final e in pool) { acc += e.weight; if (r <= acc) return e; }
		return pool.last;
	}

	void tickCooldowns() {
		final keys = List<String>.from(_cooldowns.keys);
		for (final k in keys) { _cooldowns[k] = (_cooldowns[k]! - 1).clamp(0, 9999); }
	}

	Resolution resolve(String eventId, String choiceId, WorldState world) {
		final e = _find(eventId);
		final ch = e?.choices.firstWhere((c) => c.id == choiceId, orElse: () => throw StateError('choice not found'));
		if (e == null || ch == null) return Resolution(false, null);
		_applyEffects(world, ch.effects);
		if (e.cooldown > 0) { _cooldowns[e.id] = e.cooldown; }
		return Resolution(true, ch.next ?? e.next);
	}

	NarrativeEvent? _find(String id) {
		for (final list in _byCat.values) {
			for (final e in list) { if (e.id == id) return e; }
		}
		return null;
	}

	void _applyEffects(WorldState world, List effects) {
		for (final eff in effects) {
			switch (eff.type) {
				case 'rep':
					_add(world, eff.key, _toInt(eff.delta));
					break;
				case 'stat':
					_add(world, eff.key, _toInt(eff.delta));
					break;
				case 'currency':
					_add(world, eff.key, _toInt(eff.delta));
					break;
				case 'add_item':
					_addToList(world, 'inventory', eff.key);
					break;
				case 'add_quest':
					_addToList(world, 'quests', eff.key);
					break;
				case 'gain_trait':
					_addToList(world, 'traits', eff.key);
					break;
				default:
					break;
			}
		}
	}

	int _toInt(dynamic v) {
		if (v is int) return v; if (v is num) return v.toInt();
		return int.tryParse(v?.toString() ?? '0') ?? 0;
	}

	void _add(WorldState world, String? key, int delta) {
		if (key == null || key.isEmpty) return;
		final cur = world.vars[key];
		final base = (cur is num) ? cur.toInt() : int.tryParse(cur?.toString() ?? '0') ?? 0;
		world.vars[key] = base + delta;
	}

	void _addToList(WorldState world, String key, String? value) {
		if (value == null) return;
		final list = (world.vars[key] as List?) ?? <String>[];
		if (!list.contains(value)) list.add(value);
		world.vars[key] = list;
	}
}

class Resolution {
	final bool ok;
	final String? nextId;
	Resolution(this.ok, this.nextId);
}


