import 'package:flutter/material.dart';
import '../services/settings.dart';
import '../services/strings.dart';

class SettingsPage extends StatefulWidget {
	const SettingsPage({super.key});

	@override
	State<SettingsPage> createState() => _SettingsPageState();
}

class _SettingsPageState extends State<SettingsPage> {
    bool enableAi = true;
    int tokenBudget = 96;
    final _svc = SettingsService();

    @override
    void initState() {
        super.initState();
        _load();
    }

    Future<void> _load() async {
        await _svc.load();
        setState(() { enableAi = _svc.enableAi; tokenBudget = _svc.tokenBudget; });
    }

	@override
	Widget build(BuildContext context) {
        return Scaffold(
            appBar: AppBar(title: Text(Strings.get('settings.title', 'Settings'))),
			body: ListView(
				padding: const EdgeInsets.all(16),
				children: [
                    SwitchListTile(
						title: const Text('Enable AI'),
						value: enableAi,
                        onChanged: (v) async {
                            setState(() => enableAi = v);
                            _svc.enableAi = enableAi; await _svc.save();
                        },
					),
					Row(
						children: [
							const Text('Token Budget'),
							const SizedBox(width: 12),
                            Expanded(
								child: Slider(
									value: tokenBudget.toDouble(),
									min: 32,
									max: 256,
									divisions: 7,
									label: '$tokenBudget',
                                    onChanged: (v) async {
                                        setState(() => tokenBudget = v.toInt());
                                        _svc.tokenBudget = tokenBudget; await _svc.save();
                                    },
								),
							),
						],
					),
				],
			),
		);
	}
}


