using System;

namespace RosterLib
{
	public class FakeNibbleRetriever : IRetrieveNibbleRatings
	{
		public NibbleTeamRating GetNibbleRatingFor( NflTeam team, DateTime when )
		{
			return new NibbleTeamRating(20, 10);
		}

			//homeOff = _tflWs.GetTeamStat(game.HomeTeam, "OFFENSE", rankSeason.ToString());
			//homeDef = _tflWs.GetTeamStat(game.HomeTeam, "DEFENSE", rankSeason.ToString());
			//awayOff = _tflWs.GetTeamStat(game.AwayTeam, "OFFENSE", rankSeason.ToString());
			//awayDef = _tflWs.GetTeamStat(game.AwayTeam, "DEFENSE", rankSeason.ToString());
	}
}
