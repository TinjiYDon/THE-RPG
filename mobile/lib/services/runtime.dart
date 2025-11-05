import 'content_loader.dart';
import 'graph.dart';
import 'models.dart';
import 'world_state.dart';

class NarrativeRuntime {
	static NarrativeGraph? _graph;
	static WorldState? _world;

	static NarrativeGraph get graph {
		if (_graph == null) {
			_graph = NarrativeGraph();
		}
		return _graph!;
	}

	static WorldState get world {
		if (_world == null) {
			_world = WorldState();
		}
		return _world!;
	}

	static Future<void> initialize() async {
		if (_graph == null) _graph = NarrativeGraph();
		if (_world == null) _world = WorldState();
		final events = await ContentLoader.loadFromAssets('assets/events');
		_graph!.load(events);
	}

	static void reset() {
		_world = WorldState();
	}
}

