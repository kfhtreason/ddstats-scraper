using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

const float level2Threshold = 30, level3Threshold = 100, level4Threshold = 200, leviDownThreshold = 350, orbDownThreshold = 375;
Console.WindowWidth = 120;
for (; ; )
{
	Console.WriteLine("Enter your player ID and press enter:");
	uint playerId = 0;
	while (playerId == 0)
		_ = uint.TryParse(Console.ReadLine(), out playerId);

	Console.WriteLine($"Fetching games for player with ID {playerId}...{Environment.NewLine}");
	const int pageSize = int.MaxValue;
	using HttpClient client = new HttpClient();
	string responseString = await client.GetStringAsync($"https://ddstats.com/api/v2/game/recent?player_id={playerId}&page_size={pageSize}&page_num=1");
	dynamic response = JsonConvert.DeserializeObject(responseString);
	Console.WriteLine($"Found {response.total_game_count} games for player '{response.player_name}'.{Environment.NewLine}");

	IEnumerable<dynamic> v3Games = response.games.ToObject<IEnumerable<dynamic>>();
	v3Games = v3Games.Where(g => g.spawnset == "v3");

	List<(string Name, dynamic Game, Func<dynamic, dynamic> Stat)> results = new List<(string Name, dynamic Game, Func<dynamic, dynamic> Stat)>
	{
		("Fastest level 2", v3Games.Where(g => g.level_two_time > level2Threshold).OrderBy(g => g.level_two_time).FirstOrDefault(), (game) => game?.level_two_time ?? "N/A"),
		("Fastest level 3", v3Games.Where(g => g.level_three_time > level3Threshold).OrderBy(g => g.level_three_time).FirstOrDefault(), (game) => game?.level_three_time ?? "N/A"),
		("Fastest level 4", v3Games.Where(g => g.level_four_time > level4Threshold).OrderBy(g => g.level_four_time).FirstOrDefault(), (game) => game?.level_four_time ?? "N/A"),
		("Fastest levi down", v3Games.Where(g => g.levi_down_time > leviDownThreshold).OrderBy(g => g.levi_down_time).FirstOrDefault(), (game) => game?.levi_down_time ?? "N/A"),
		("Fastest orb down", v3Games.Where(g => g.orb_down_time > orbDownThreshold).OrderBy(g => g.orb_down_time).FirstOrDefault(), (game) => game?.orb_down_time ?? "N/A"),
		("Longest run", v3Games.OrderByDescending(g => g.game_time).FirstOrDefault(), (game) => game?.game_time ?? "N/A"),
		("Longest pacifist run", v3Games.Where(g => g.enemies_killed == 0).OrderByDescending(g => g.game_time).FirstOrDefault(), (game) => game?.game_time ?? "N/A"),
		("Highest homing peak run", v3Games.OrderByDescending(g => g.homing_daggers_max).FirstOrDefault(), (game) => game?.homing_daggers_max ?? "N/A")
	};

	Console.WriteLine(string.Join(Environment.NewLine, results.Select(r => $"{r.Name,-24} {(r.Game == null ? "N/A" : $"https://ddstats.com/games/{r.Game.id}"),-32} {r.Stat(r.Game),20}")));
	Console.WriteLine();
}