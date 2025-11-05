import 'dart:convert';
import 'package:flutter/services.dart' show rootBundle;

class Strings {
	static Map<String, String>? _map;

	static Future<void> load([String locale = 'zh_CN']) async {
		final txt = await rootBundle.loadString('assets/i18n/$locale.json');
		final j = json.decode(txt) as Map<String, dynamic>;
		_map = j.map((k, v) => MapEntry(k, v.toString()));
	}

	static String get(String key, [String? fallback]) {
		if (_map == null) return fallback ?? key;
		return _map![key] ?? (fallback ?? key);
	}
}


