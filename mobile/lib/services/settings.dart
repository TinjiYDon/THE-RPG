import 'package:shared_preferences/shared_preferences.dart';

class SettingsService {
	static const _kEnableAi = 'enable_ai';
	static const _kTokenBudget = 'ai_token_budget';

	bool enableAi = true;
	int tokenBudget = 96;

	Future<void> load() async {
		final sp = await SharedPreferences.getInstance();
		enableAi = sp.getBool(_kEnableAi) ?? true;
		tokenBudget = sp.getInt(_kTokenBudget) ?? 96;
	}

	Future<void> save() async {
		final sp = await SharedPreferences.getInstance();
		await sp.setBool(_kEnableAi, enableAi);
		await sp.setInt(_kTokenBudget, tokenBudget);
	}
}


