import 'models.dart';
import 'world_state.dart';

class ConditionEvaluator {
	bool evaluate(List<Condition> conditions, WorldState world) {
		if (conditions.isEmpty) return true;
		for (final c in conditions) {
			if (!_eval(c, world)) return false;
		}
		return true;
	}

	dynamic _getByPath(Map<String, dynamic> root, String? path) {
		if (root.isEmpty || path == null || path.isEmpty) return null;
		var cur = root as dynamic;
		for (final p in path.split('.')) {
			if (cur is Map<String, dynamic>) {
				if (!cur.containsKey(p)) return null;
				cur = cur[p];
			} else {
				return null;
			}
		}
		return cur;
	}

	double? _asNum(dynamic v) {
		if (v == null) return null;
		if (v is num) return v.toDouble();
		final s = v.toString();
		return double.tryParse(s);
	}

	bool _eval(Condition c, WorldState world) {
		final lhs = _getByPath(world.vars, c.key);
		final op = (c.op ?? '==').toLowerCase();
		switch (op) {
			case 'exists':
				return lhs != null && lhs.toString().isNotEmpty;
			case '!exists':
				return lhs == null || lhs.toString().isEmpty;
			case '==':
				return (lhs?.toString() ?? '') == (c.value?.toString() ?? '');
			case '!=':
				return (lhs?.toString() ?? '') != (c.value?.toString() ?? '');
			case '>':
			case '>=':
			case '<':
			case '<=':
				final a = _asNum(lhs); final b = _asNum(c.value);
				if (a == null || b == null) return false;
				if (op == '>') return a > b;
				if (op == '>=') return a >= b;
				if (op == '<') return a < b;
				return a <= b;
			case 'in':
				final val = lhs?.toString();
				final list = (c.value is List) ? (c.value as List) : const [];
				return list.map((e) => e.toString()).contains(val);
			case 'has':
				if (lhs is List) {
					return lhs.map((e) => e.toString()).contains(c.value?.toString());
				}
				return false;
			case 'rep>=':
			case 'rep<=':
				final a2 = _asNum(lhs); final b2 = _asNum(c.value);
				if (a2 == null || b2 == null) return false;
				return op == 'rep>=' ? a2 >= b2 : a2 <= b2;
			default:
				return false;
		}
	}
}


