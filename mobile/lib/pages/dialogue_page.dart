import 'package:flutter/material.dart';
import '../services/models.dart';
import '../services/runtime.dart';
import '../services/evaluator.dart';
import 'events_page.dart';

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
                                itemCount: _combinedChoices().length,
                                itemBuilder: (context, index) {
                                    final ch = _combinedChoices()[index];
                                    final ok = ConditionEvaluator().evaluate(ch.requirements, NarrativeRuntime.world);
                                    return Padding(
                                        padding: const EdgeInsets.only(bottom: 8),
                                        child: ElevatedButton(
                                            onPressed: ok ? () => _onChoice(context, ch) : null,
                                            child: Text(ch.label + (ok ? '' : ' (条件不足)')),
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

	void _onChoice(BuildContext context, Choice ch) {
		final graph = NarrativeRuntime.graph;
		final world = NarrativeRuntime.world;
		final res = graph.resolve(event.id, ch.id, world);
		graph.tickCooldowns();

		if (res.nextId != null && res.nextId!.isNotEmpty) {
			// Find and navigate to next event
			final next = _findEventById(res.nextId!);
			if (next != null) {
				Navigator.pushReplacement(
					context,
					MaterialPageRoute(builder: (_) => DialoguePage(event: next)),
				);
				return;
			}
		}

		// No next event, return to events list
		ScaffoldMessenger.of(context).showSnackBar(
			SnackBar(content: Text('Choice applied: ${ch.label} (no next)')),
		);
		Navigator.of(context).popUntil((route) => route.isFirst);
	}

	NarrativeEvent? _findEventById(String id) {
		return NarrativeRuntime.graph.findById(id);
	}
}

List<Choice> _combinedChoices() {
    final base = event.choices;
    final gen = event.aiGenerated;
    if (gen.isEmpty) return base;
    return [
        ...base,
        ...gen.map((t) => Choice(
            id: t.id,
            label: t.aiLabelHint ?? t.label,
            aiLabelHint: t.aiLabelHint,
            requirements: t.requirements,
            effects: t.effects,
            next: t.next,
        ))
    ];
}


