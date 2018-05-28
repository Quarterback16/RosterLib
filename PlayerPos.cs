using System;

namespace RosterLib
{
	/// <summary>
	/// Summary description for PlayerPos.
	/// </summary>
	public abstract class PlayerPos
	{
		private string category;
      private string name;

		public string Category
		{
			get { return category; }
			set { category = value; }
		}

      public string Name
      {
         get { return name; }
         set { name = value; }
      }
	   
		public virtual string PerfCol1()
		{
			return "Perf1";
		}
		
		public virtual string PerfCol2()
		{
			return "Perf1";
		}
	   
	   public virtual void ProjectNextWeek( NFLPlayer p )
	   {
	      //  update projections for a player
	   }
	}
	
	public class QuarterbackCategory : PlayerPos
	{
	   public QuarterbackCategory()
	   {
         Name = "Quarterback";
         Category = RosterLib.Constants.K_QUARTERBACK_CAT;
	   }
	   
		public override string PerfCol1()
		{
			return "Com-Att-YDp (Tdp)";
		}
		
		public override string PerfCol2()
		{
			return "Att-YDr (Tdr)";
		}
      
	   public override void ProjectNextWeek(NFLPlayer p)
      {
         SimpleTDpPredictor sp = new SimpleTDpPredictor();
         p.ProjectedTDp = sp.PredictTDp(p, Utility.CurrentSeason(), Int32.Parse(Utility.CurrentWeek()) + 1);
      }
	   
	}
	
	public class RunningbackCategory : PlayerPos
	{
      public RunningbackCategory()
	   {
         Name = "Runningback";
         Category = RosterLib.Constants.K_RUNNINGBACK_CAT;
      }
	   	   
		public override string PerfCol1()
		{
			return "Att-YDr (Tdr)";
		}
		
		public override string PerfCol2()
		{
			return "Rec-YDc (TDc)";
		}
	   
      public override void ProjectNextWeek(NFLPlayer p)
      {
         SimpleTDrPredictor sp = new SimpleTDrPredictor();
         p.ProjectedTDr = sp.PredictTDr(p, Utility.CurrentSeason(), Int32.Parse(Utility.CurrentWeek()) + 1);
         SimpleYDrPredictor spy = new SimpleYDrPredictor();
         p.ProjectedYDr = spy.PredictYDr( p, Utility.CurrentSeason(), Int32.Parse(Utility.CurrentWeek()) + 1);
      }	   
	}	
	
	public class ReceiverCategory : PlayerPos
	{
      public ReceiverCategory()
	   {
         Name = "Receiver";
         Category = RosterLib.Constants.K_RECEIVER_CAT;        
	   }	   
	   
		public override string PerfCol2()
		{
			return "Att-YDr (Tdr)";
		}
		
		public override string PerfCol1()
		{
			return "Rec-YDc (TDc)";
		}
      public override void ProjectNextWeek(NFLPlayer p)
      {
         SimpleTDcPredictor spt = new SimpleTDcPredictor();
         p.ProjectedTDc = spt.PredictTDc(p, Utility.CurrentSeason(), Int32.Parse(Utility.CurrentWeek()) + 1);
         
         SimpleYDcPredictor spc = new SimpleYDcPredictor();
         p.ProjectedYDc = spc.PredictYDc(p, Utility.CurrentSeason(), Int32.Parse(Utility.CurrentWeek()) + 1);
      }		   
	}	
	
	public class KickerCategory : PlayerPos
	{
      public KickerCategory()
	   {
         Name = "Kicker";
         Category = RosterLib.Constants.K_KICKER_CAT;
      }	   
	   	   
		public override string PerfCol1()
		{
			return "Fg-PAT";
		}
		
		public override string PerfCol2()
		{
			return "";
		}
	   
	   public override void ProjectNextWeek( NFLPlayer p )
	   {
         SimpleFGPredictor sp = new SimpleFGPredictor();
         p.ProjectedFg = sp.PredictFGs( p, Utility.CurrentSeason(), Int32.Parse( Utility.CurrentWeek() ) + 1 );
	   }
	   
	}
	
	public class DefensiveLineCategory : PlayerPos
	{
      public DefensiveLineCategory()
	   {
         Name = "DefensiveLine";
	   }		   
		public override string PerfCol1()
		{
			return "Sak-Int";
		}
		
		public override string PerfCol2()
		{
			return "";
		}		
	}
	
	public class DefensiveBackCategory : PlayerPos
	{
      public DefensiveBackCategory()
	   {
         Name = "DefensiveBack";
	   }		   
		public override string PerfCol1()
		{
			return "Sak-Int";
		}
		
		public override string PerfCol2()
		{
			return "";
		}		
	}

	public class OffensiveLineCategory : PlayerPos
	{
      public OffensiveLineCategory()
	   {
         Name = "OffensiveLine";
	   }		   
		public override string PerfCol1()
		{
			return "";
		}
		
		public override string PerfCol2()
		{
			return "";
		}		
	}

   public class DefensiveTeamCategory : PlayerPos
   {
      public DefensiveTeamCategory()
      {
         Name = "DefensiveTeam";
         Category = RosterLib.Constants.K_DEFENSIVETEAM_CAT;
      }

      public override string PerfCol1()
      {
         return "DefTeam";
      }

      public override string PerfCol2()
      {
         return "DefTeam";
      }

      public void ProjectNextWeek(NflTeam t)
      {
         SimpleDefencePredictor sp = new SimpleDefencePredictor();
         t.ProjectedSacks = sp.PredictSacks( t, Utility.CurrentSeason(), Int32.Parse(Utility.CurrentWeek()) + 1);
         t.ProjectedSteals = sp.PredictSteals(t, Utility.CurrentSeason(), Int32.Parse(Utility.CurrentWeek()) + 1);
      }

   }
	
   
}
