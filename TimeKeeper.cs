using NLog;
using RosterLib.Helpers;
using RosterLib.Interfaces;
using System;
using System.Configuration;
using System.Globalization;

namespace RosterLib
{
	/// <summary>
	///   It is useful to know what time it is in reference to the NFL season
	///   Approach 1 - base it on the month of the season
	///   Approach 2 - base it on the schedule - more accurate
	///   Approach 3 - a bit of both (if no schedule use month, typically schedule will be set in May)
	/// </summary>
	public class TimeKeeper : IKeepTheTime
	{
		public Logger Logger { get; set; }

		public IClock SystemClock { get; set; }

		public ISeasonScheduler SeasonScheduler { get; set; }

		public string Season { get; set; }

		public bool ScheduleAvailable { get; set; }

		public DateTime SeasonStarts { get; set; }
		public DateTime RegularSeasonEnds { get; set; }

		public TimeKeeper( IClock clock )
		{
			Logger = LogManager.GetCurrentClassLogger();
            if (clock == null)
            {
                Logger.Debug("Setting the Clock");

                SystemClock = new SystemClock();
            }
            else
                SystemClock = clock;
			SetSchedule();
		}

		private void SetSchedule()
		{
            Logger.Debug("Setting the Schedule");
            Season = CurrentSeason();
            Logger.Debug($"Season={Season}");
            SeasonScheduler = new SeasonScheduler();
			ScheduleAvailable = SeasonScheduler.ScheduleAvailable( Season );
			if ( !ScheduleAvailable ) return;
			SeasonStarts = SeasonScheduler.SeasonStarts( Season );
			RegularSeasonEnds = SeasonScheduler.RegularSeasonEnds( Season );
		}

		public bool IsItPreseason()
		{
			if ( ScheduleAvailable )
			{
				var now = SystemClock.Now;
				return now < SeasonStarts.AddDays( 1 );
			}
			var month = SystemClock.GetMonth();
			return month >= 3 && month < 9;
		}

		public bool IsItRegularSeason()
		{
			if ( ScheduleAvailable )
			{
				var now = SystemClock.Now;
				//  add a day for Aus time
				return now >= SeasonStarts.AddDays( 1 ) && now <= RegularSeasonEnds;
			}
			var month = SystemClock.GetMonth();
			return month >= 9 && month <= 12;
		}

		public bool IsItPostSeason()
		{
			return ( !IsItRegularSeason() 
                && !IsItPreseason() 
                && CurrentWeek(SystemClock.Now) > 0 );
		}

		public bool IsItQuietTime()
		{
			return SystemClock.Now.Hour < 6;
		}

		public bool IsItPeakTime()
		{
			return IsItPeakTime( SystemClock.Now );
		}

		public bool IsItPeakTime( DateTime theDateTime )
		{
			var startHour = 5;
			var endHour = 23;
			var configStartHour = ConfigurationManager.AppSettings.Get( "PeakStartHour" );
			var configEndHour = ConfigurationManager.AppSettings.Get( "PeakFinishHour" );

			if ( !string.IsNullOrEmpty( configStartHour ) ) startHour = Int32.Parse( configStartHour );
			if ( !string.IsNullOrEmpty( configEndHour ) ) endHour = Int32.Parse( configEndHour );

			Logger.Debug( string.Format( "Peak time is between {0} and {1}", startHour, endHour ) );

			if ( endHour == startHour ) return false;  //  forget about Peak scenario
			if ( endHour > startHour )
				return theDateTime.Hour > startHour && theDateTime.Hour < endHour;
			else
				return theDateTime.Hour > startHour && theDateTime.Hour < endHour + 24;
		}

		public DateTime GetDate()
		{
			return SystemClock.Now;
		}

		public bool IsDateDaysOld( int daysOld, DateTime theDate )
		{
			var daysSince = SystemClock.Now.Subtract( theDate );
			return daysSince.Days <= daysOld;
		}

		public bool IsItWednesdayOrThursday( DateTime focusDate )
		{
			return focusDate.DayOfWeek == DayOfWeek.Wednesday || focusDate.DayOfWeek == DayOfWeek.Thursday;
		}

		public string CurrentSeason( DateTime focusDate )
		{
			return CurrentSeason();  // not Utility CurrentSeason();
		}

		public string CurrentSeason()
		{
			var season = SystemClock.Now.Year;
			if ( IsItPostSeason() )
				season--;
			return season.ToString( CultureInfo.InvariantCulture );
		}

		public int CurrentWeek( DateTime focusDate )
		{
			return Int32.Parse( Utility.CurrentWeek() );
		}

		public int CurrentWeek()
		{
			var weekKey = SeasonScheduler.WeekKey( GetDate() );
			var weekStr = weekKey.Substring( 5, 2 );
			return Int32.Parse( weekStr );
		}

		public string PreviousWeek()
		{
			var currentWeek = CurrentWeek();
			var previousWeek = currentWeek - 1;
			return string.Format( "{0:0#}", previousWeek );
		}

		public string Week
		{
			get
			{
				return string.Format( "{0:0#}", CurrentWeek() );
			}
			set { }
		}

		public bool IsItFridaySaturdayOrSunday( DateTime focusDate )
		{
			return focusDate.DayOfWeek == DayOfWeek.Friday || focusDate.DayOfWeek == DayOfWeek.Saturday || focusDate.DayOfWeek == DayOfWeek.Sunday;
		}

		public bool IsItMondayMorning()
		{
			var isIt = false;
			var currTime = CurrentDateTime();
			if ( currTime.DayOfWeek == DayOfWeek.Monday )
			{
				if ( currTime.Hour < 12 )
				{
					isIt = true;
				}
			}
			return isIt;
		}

		public DateTime CurrentDateTime()
		{
			return SystemClock.Now;
		}

		public string PreviousSeason( DateTime focusDate )
		{
			Season = CurrentSeason( focusDate );
			return PreviousSeason();
		}

		public string PreviousSeason()
		{
			var ps = Int32.Parse( Season ) - 1;
			return ps.ToString( CultureInfo.InvariantCulture );
		}

		public int DumpSeasonSundays()
		{
			int numberOfSundays = 0;
			DateTime sunday = new DateTime( 1, 1, 1 );
			for ( int i = 1; i < Constants.K_WEEKS_IN_A_SEASON + 1; i++ )
			{
				numberOfSundays++;
				if ( numberOfSundays == 1 )
					sunday = Utility.TflWs.GetSeasonStartDate( CurrentSeason() );
				else
					sunday = sunday.Date.AddDays( 7 );
				Console.WriteLine( $"{numberOfSundays} : {sunday:d}" );
			}
			return numberOfSundays;
		}

		public DateTime GetSundayFor( DateTime when )
		{
			var theSeason = Utility.SeasonFor( when );
			var theSunday = Utility.TflWs.GetSeasonStartDate( theSeason );
			if ( when <= theSunday )
				return theSunday;
			for ( var i = 1; i < 16; i++ )
			{
				var sunday = theSunday.AddDays( i * 7 );
				if ( when > sunday ) continue;
				theSunday = sunday;
				break;
			}
			return theSunday;
		}

		public bool IsItWednesday( DateTime focusDate )
		{
			return focusDate.DayOfWeek == DayOfWeek.Wednesday;
		}

		public bool IsItTuesday()
		{
			var focusDate = CurrentDateTime();
			return focusDate.DayOfWeek == DayOfWeek.Tuesday;
		}
	}
}