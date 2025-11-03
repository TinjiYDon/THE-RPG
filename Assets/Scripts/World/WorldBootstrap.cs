using System.Collections.Generic;

namespace TheRPG.World
{
	public static class WorldBootstrap
	{
		public static WorldState CreateDefault()
		{
			var w = new WorldState();
			var v = w.Vars;
			v["player.level"] = 1;
			v["player.hp"] = 100;
			v["player.energy"] = 50;
			v["player.stability"] = 60;
			v["player.implant_load"] = 0;

			v["credits"] = 100;

			v["faction.black_lotus"] = 0;
			v["faction.corp_hex"] = 0;

			v["inventory"] = new List<object>();
			v["traits"] = new List<object>();
			v["quests"] = new List<object>();

			return w;
		}
	}
}


