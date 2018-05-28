namespace RosterLib
{
    public static class Filters
    {
        /// <summary>
        /// 
        ///  For new report:
        ///  
        ///    0)  Which teams do you want
        ///    1)  do you want game logs? (set ShowGameLog)
        ///    2)  Which Units (there are 7) do you want
        ///    3)  Which player types within the units? (use player type filters)
        ///    4)  Which roles S, B or *
        ///
        /// 
        /// </summary>
        /// <returns>String of team codes</returns>
        public static string TeamFilter()
        {
#if DEBUG
            return "AC,AF,BR,BB,CH,CP,CI,CL,DC,DB,DL,GB,HT,IC,JJ,KC,MD,MV,PS,NE,NG,NJ,NO,OR,PE,LC,SF,LR,SS,TB,TT,WR";
            //return "DC";
#else      	
			return "AC,AF,BR,BB,CH,CP,CI,CL,DC,DB,DL,GB,HT,IC,JJ,KC,MD,MV,PS,NE,NG,NJ,NO,OR,PE,LC,SF,LR,SS,TB,TT,WR";
#endif
        }

		#region Unit filters

		public static bool DoPassingUnit()
		{
			return true;
		}

		public static bool DoRunningUnit()
		{
         return true;
		}

		public static bool DoProtectionUnit()
		{
         return true;
		}

		public static bool DoKickingUnit()
		{
         return true;
		}

		public static bool DoPassDefenceUnit()
		{
         return true;
		}

		public static bool DoPassRushUnit()
		{
         return true;
		}

		public static bool DoRunDefenceUnit()
		{
         return true;
		}

      public static bool CalculateUnitExperience()
      {
         return true;
      }

		#endregion

		#region Player type filters

		public static bool ShowQBs()
		{
			return true;
		}

		public static bool ShowRBs()
		{
            return true;
		}

		public static bool ShowHBs()
		{
           return true;
		}

		public static bool ShowFBs()
		{
            return true;
		}

		public static bool ShowWRs()
		{
           return true;
		}

		public static bool ShowTEs()
		{
           return true;
		}

		/// <summary>
		///   These positions will be dropped from the old roster
		/// </summary>
		/// <returns></returns>
		public static string DropPositions()
		{
			//return "FB,P,TE";
			return "P";
		}

		#endregion

		#region Role filters

		public static string QbRoleFilter()
		{
			return "*";
		}

		public static string RbRoleFilter()
		{
			return "*";
		}

		public static string WrRoleFilter()
		{
			return "*";
		}

		public static string TeRoleFilter()
		{
			return "*";
		}

		public static string DefRoleFilter()
		{
			return "*";
		}

		public static string OlRoleFilter()
		{
			return "*";
		}

		#endregion

}

}