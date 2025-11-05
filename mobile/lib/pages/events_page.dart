import 'package:flutter/material.dart';
import '../services/content_loader.dart';
import '../services/models.dart';
import '../services/runtime.dart';
import 'dialogue_page.dart';
import '../services/strings.dart';

class EventsPage extends StatefulWidget {
	const EventsPage({super.key});

	@override
	State<EventsPage> createState() => _EventsPageState();
}

class _EventsPageState extends State<EventsPage> {
	List<NarrativeEvent> _events = const [];
	bool _loading = true;
	String? _error;

	@override
	void initState() {
		super.initState();
		_load();
	}

	Future<void> _load() async {
		setState(() { _loading = true; _error = null; });
		try {
			// Initialize runtime first
			await NarrativeRuntime.initialize();
			// Load events for display
			final list = await ContentLoader.loadFromAssets('assets/events');
			setState(() { _events = list; });
		} catch (e) {
			setState(() { _error = e.toString(); });
		} finally {
			setState(() { _loading = false; });
		}
	}

	@override
	Widget build(BuildContext context) {
        return Scaffold(
            appBar: AppBar(
                title: Text(Strings.get('events.title', 'Events')),
				actions: [
					IconButton(onPressed: () => Navigator.pushNamed(context, '/settings'), icon: const Icon(Icons.settings))
				],
			),
			body: _body(),
			floatingActionButton: FloatingActionButton(
				onPressed: _load,
				child: const Icon(Icons.refresh),
			),
		);
	}

	Widget _body() {
		if (_loading) return const Center(child: CircularProgressIndicator());
		if (_error != null) return Center(child: Text(_error!));
		if (_events.isEmpty) return const Center(child: Text('No events'));
		return ListView.builder(
			itemCount: _events.length,
			itemBuilder: (context, index) {
				final e = _events[index];
				return ListTile(
					title: Text(e.id),
					subtitle: Text('${e.category}  ·  weight=${e.weight}  ·  cd=${e.cooldown}'),
					onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => DialoguePage(event: e))),
				);
			},
		);
	}
}


