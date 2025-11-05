import 'package:flutter/material.dart';
import 'pages/events_page.dart';
import 'pages/settings_page.dart';
import 'services/strings.dart';

class TheRpgApp extends StatelessWidget {
	const TheRpgApp({super.key});

	@override
	Widget build(BuildContext context) {
		return MaterialApp(
			title: Strings.get('app.title', 'THE-RPG'),
			theme: ThemeData(colorSchemeSeed: const Color(0xFF00BFA5), useMaterial3: true),
			routes: {
				'/': (_) => const EventsPage(),
				'/settings': (_) => const SettingsPage(),
			},
		);
	}
}


