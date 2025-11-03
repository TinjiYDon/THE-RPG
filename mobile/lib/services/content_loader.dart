import 'dart:convert';
import 'package:flutter/services.dart' show rootBundle;
import 'models.dart';

class ContentLoader {
	static Future<List<NarrativeEvent>> loadFromAssets(String dir) async {
		// Very simple: expects an index list file or known files; here we hardcode a small set for demo
		final files = [
			'$dir/evt_town_contract_001.json',
			'$dir/evt_town_rumor_001.json',
			'$dir/evt_wild_implant_001.json',
			'$dir/evt_dun_affix_001.json',
			'$dir/evt_story_choice_001.json',
		];
		final out = <NarrativeEvent>[];
		for (final f in files) {
			try {
				final txt = await rootBundle.loadString(f);
				final j = json.decode(txt) as Map<String, dynamic>;
				out.add(NarrativeEvent.fromJson(j));
			} catch (_) {}
		}
		return out;
	}
}


