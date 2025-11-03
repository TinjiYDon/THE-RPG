import 'package:flutter/material.dart';

class SettingsPage extends StatefulWidget {
	const SettingsPage({super.key});

	@override
	State<SettingsPage> createState() => _SettingsPageState();
}

class _SettingsPageState extends State<SettingsPage> {
	bool enableAi = true;
	int tokenBudget = 96;

	@override
	Widget build(BuildContext context) {
		return Scaffold(
			appBar: AppBar(title: const Text('Settings')),
			body: ListView(
				padding: const EdgeInsets.all(16),
				children: [
					SwitchListTile(
						title: const Text('Enable AI'),
						value: enableAi,
						onChanged: (v) => setState(() => enableAi = v),
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
									onChanged: (v) => setState(() => tokenBudget = v.toInt()),
								),
							),
						],
					),
				],
			),
		);
	}
}


