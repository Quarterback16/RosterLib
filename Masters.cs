namespace RosterLib
{
	public static class Masters
	{
		private static EpMaster _epm;
		private static TEPMaster _tepm;
		private static GameMaster _gm;
		private static PlayerMaster _pm;
		private static TeamMaster _tm;
		private static SeasonMaster _sm;

		public static EpMaster Epm
		{
			get
			{
				if (Equals(_epm, null)) _epm = new EpMaster(name: "EP");
				return _epm;
			}
			set { _epm = value; }
		}

		public static SeasonMaster Sm
		{
			get
			{
				if (Equals(_sm, null)) _sm = new SeasonMaster("Season", "season.xml");
				return _sm;
			}
			set { _sm = value; }
		}

		public static PlayerMaster Pm
		{
			get
			{
				if (Equals(_pm, null)) _pm = new PlayerMaster("Player", "player.xml");
				return _pm;
			}
			set { _pm = value; }
		}

		public static TeamMaster Tm
		{
			get
			{
				if (Equals(_tm, null)) _tm = new TeamMaster("Team", "team.xml");
				return _tm;
			}
			set { _tm = value; }
		}

		public static TEPMaster Tepm
		{
			get
			{
				if (Equals(_tepm, null)) _tepm = new TEPMaster();
				return _tepm;
			}
			set { _tepm = value; }
		}

		public static GameMaster Gm
		{
			get
			{
				if (Equals(_gm, null)) _gm = new GameMaster("Game");
				return _gm;
			}
			set { _gm = value; }
		}
	}
}