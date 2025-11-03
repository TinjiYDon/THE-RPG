class WorldState {
	final Map<String, dynamic> vars = <String, dynamic>{};

	WorldState() {
		vars['player.level'] = 1;
		vars['player.hp'] = 100;
		vars['player.energy'] = 50;
		vars['player.stability'] = 60;
		vars['player.implant_load'] = 0;
		vars['credits'] = 100;
		vars['faction.black_lotus'] = 0;
		vars['faction.corp_hex'] = 0;
		vars['inventory'] = <String>[];
		vars['traits'] = <String>[];
		vars['quests'] = <String>[];
	}
}


