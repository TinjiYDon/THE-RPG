using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TheRPG.Content;
using TheRPG.World;

namespace TheRPG.Narrative
{
	public interface IConditionEvaluator
	{
		bool Evaluate(IReadOnlyList<Condition> conditions, WorldState world);
	}

	public sealed class ConditionEvaluator : IConditionEvaluator
	{
		static object GetByPath(Dictionary<string, object> root, string path)
		{
			if (root == null || string.IsNullOrEmpty(path)) return null;
			var parts = path.Split('.');
			object cur = root;
			foreach (var p in parts)
			{
				if (cur is Dictionary<string, object> dict)
				{
					if (!dict.TryGetValue(p, out cur)) return null;
				}
				else
				{
					return null;
				}
			}
			return cur;
		}

		static bool TryToDouble(object v, out double d)
		{
			switch (v)
			{
				case null: d = 0; return false;
				case double dd: d = dd; return true;
				case float f: d = f; return true;
				case int i: d = i; return true;
				case long l: d = l; return true;
				case string s: return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out d);
				default: d = 0; return false;
			}
		}

		static string AsString(object v) => v?.ToString() ?? "";

		static bool IsTruthy(object v)
		{
			return v switch
			{
				bool b => b,
				string s => !string.IsNullOrEmpty(s),
				ICollection c => c.Count > 0,
				null => false,
				_ => true
			};
		}

		public bool Evaluate(IReadOnlyList<Condition> conditions, WorldState world)
		{
			if (conditions == null || conditions.Count == 0) return true;
			var vars = world?.Vars;
			foreach (var c in conditions)
			{
				if (c == null) return false;
				var lhs = GetByPath(vars, c.Key);
				var op = c.Op?.ToLowerInvariant() ?? "==";
				var ok = false;

				switch (op)
				{
					case "exists":
						ok = lhs != null && IsTruthy(lhs);
						break;
					case "!exists":
						ok = lhs == null || !IsTruthy(lhs);
						break;
					case "==":
						ok = Equals(AsString(lhs), AsString(c.Value));
						break;
					case "!=":
						ok = !Equals(AsString(lhs), AsString(c.Value));
						break;
					case ">":
					case ">=":
					case "<":
					case "<=":
					{
						if (TryToDouble(lhs, out var a) && TryToDouble(c.Value, out var b))
						{
							ok = op switch
							{
								">"  => a >  b,
								">=" => a >= b,
								"<"  => a <  b,
								"<=" => a <= b,
								_ => false
							};
						}
						break;
					}
					case "between":
					case "between_incl":
					case "between_excl":
					{
						if (lhs != null && c.Value is IList list && list.Count >= 2 &&
							TryToDouble(lhs, out var val) &&
							TryToDouble(list[0], out var lo) && TryToDouble(list[1], out var hi))
						{
							ok = op switch
							{
								"between"       => val >  lo && val <  hi,
								"between_incl"  => val >= lo && val <= hi,
								"between_excl"  => val >  lo && val <  hi,
								_ => false
							};
						}
						break;
					}
					case "contains":
						ok = AsString(lhs).Contains(AsString(c.Value), StringComparison.Ordinal);
						break;
					case "starts_with":
						ok = AsString(lhs).StartsWith(AsString(c.Value), StringComparison.Ordinal);
						break;
					case "ends_with":
						ok = AsString(lhs).EndsWith(AsString(c.Value), StringComparison.Ordinal);
						break;
					case "in":
					{
						if (c.Value is IList set)
						{
							var lv = AsString(lhs);
							var hit = false;
							for (int i = 0; i < set.Count; i++)
							{
								if (AsString(set[i]) == lv) { hit = true; break; }
							}
							ok = hit;
						}
						break;
					}
					case "has":
					{
						if (lhs is IList col)
						{
							var target = AsString(c.Value);
							var hit = false;
							for (int i = 0; i < col.Count; i++)
							{
								if (AsString(col[i]) == target) { hit = true; break; }
							}
							ok = hit;
						}
						break;
					}
					case "rep>=":
					case "rep<=":
					{
						if (TryToDouble(lhs, out var a) && TryToDouble(c.Value, out var b))
						{
							ok = op == "rep>=" ? a >= b : a <= b;
						}
						break;
					}
					default:
						ok = false; // 未知操作符
						break;
				}

				if (!ok) return false;
			}
			return true;
		}
	}
}


