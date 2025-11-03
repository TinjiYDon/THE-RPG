import 'package:flutter/material.dart';

void main() {
	runApp(const TheRpgApp());
}

class TheRpgApp extends StatelessWidget {
	const TheRpgApp({super.key});

	@override
	Widget build(BuildContext context) {
		return MaterialApp(
			title: 'THE-RPG',
			theme: ThemeData(colorSchemeSeed: const Color(0xFF00BFA5), useMaterial3: true),
			home: const HomePage(),
		);
	}
}

class HomePage extends StatelessWidget {
	const HomePage({super.key});

	@override
	Widget build(BuildContext context) {
		return Scaffold(
			appBar: AppBar(title: const Text('THE-RPG Mobile')),
			body: const Center(child: Text('Hello, Cyber Wuxia.')),
		);
	}
}


