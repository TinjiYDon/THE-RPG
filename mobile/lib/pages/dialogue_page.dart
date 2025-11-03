import 'package:flutter/material.dart';
import '../services/models.dart';
import '../services/world_state.dart';
import '../services/graph.dart';

class DialoguePage extends StatelessWidget {
	final NarrativeEvent event;
	const DialoguePage({super.key, required this.event});

	@override
	Widget build(BuildContext context) {
		return Scaffold(
			appBar: AppBar(title: Text(event.id)),
			body: Padding(
				padding: const EdgeInsets.all(16),
				child: Column(
					crossAxisAlignment: CrossAxisAlignment.start,
					children: [
						Text(event.text.base, style: Theme.of(context).textTheme.titleMedium),
						const SizedBox(height: 16),
						Expanded(
							child: ListView.builder(
								itemCount: event.choices.length,
								itemBuilder: (context, index) {
									final ch = event.choices[index];
									return Padding(
										padding: const EdgeInsets.only(bottom: 8),
										child: ElevatedButton(
											onPressed: () {
												final graph = NarrativeGraph();
												graph.load([event]);
												final world = WorldState();
												final res = graph.resolve(event.id, ch.id, world);
												final msg = 'Choice ${ch.id} applied, next: ${res.nextId ?? '-'}';
												ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(msg)));
											},
											child: Text(ch.label),
										),
									);
								},
							),
						),
					],
				),
			),
		);
	}
}


