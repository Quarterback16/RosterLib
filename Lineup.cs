using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace RosterLib
{
    public class Lineup
    {
        public string TeamCode { get; set; }
        public int MissingKeys { get; set; }

        public int Starters { get; set; }
        public int Backups { get; set; }
        public int OffStarters { get; set; }
        public int DefStarters { get; set; }

        public List<NFLPlayer> PlayerList;

        public Lineup( DataSet ds )
        {
            LoadPlayerList( ds );
            MissingKeys = 0;
        }

        public Lineup( string teamCode, string seasonIn, string week )
        {
            TeamCode = teamCode;
            var ds = Utility.TflWs.GetLineup( teamCode, seasonIn, Int32.Parse( week ) );
            LoadPlayerList( ds );
            MissingKeys = 0;
        }

        public void DumpLineup()
        {
            DumpOffence();
            DumpDefence();
        }

        public string DumpAsHtml( string header )
        {
            var sb = new StringBuilder();
            sb.Append( HtmlLib.H3( header ) );
            sb.Append( HtmlLib.TableOpen( "border='0'" ) );
            sb.Append( HtmlLib.TableRowOpen() );
            sb.Append( HtmlLib.TableData( DumpOffenceHtml() ) );
            sb.Append( HtmlLib.TableData( DumpDefenceHtml() ) );
            sb.Append( HtmlLib.TableRowClose() );
            sb.Append( HtmlLib.TableClose() );
            return sb.ToString();
        }

        private string DumpDefenceHtml()
        {
            var sb = new StringBuilder();
            sb.Append( HtmlLib.ListOpen() );
            foreach ( var p in PlayerList )
            {
                if ( p.LineupPos.Trim().Length > 0 )
                    if ( p.IsDefence() )
                        sb.Append( HtmlLib.ListItem( $"{p.LineupPos} {p.PlayerName}" ) );
            }
            sb.Append( HtmlLib.ListOpen() );
            return sb.ToString();
        }

        private string DumpOffenceHtml()
        {
            var sb = new StringBuilder();
            sb.Append( HtmlLib.ListOpen() );
            foreach ( var p in PlayerList )
            {
                if ( p.LineupPos.Trim().Length > 0 )
                    if ( p.IsOffence() )
                        sb.Append( HtmlLib.ListItem( $"{p.LineupPos} {p.PlayerName}" ) );
            }
            sb.Append( HtmlLib.ListOpen() );
            return sb.ToString();
        }

        public void DumpOffence()
        {
            Utility.Announce( $"--{TeamCode}--Offence-------------------" );
            foreach ( var p in PlayerList )
            {
                if ( p.LineupPos.Trim().Length > 0 )
                    if ( p.IsOffence() )
                        AnnouncePlayer( p );
            }
        }

        private void AnnouncePlayer( NFLPlayer p )
        {
            Utility.Announce( $"  {p.LineupPos,-5} {p.PlayerNameShort,-15}" );
        }

        public void DumpDefence()
        {
            Utility.Announce( $"--{TeamCode}--Defence-------------------" );
            foreach ( var p in PlayerList )
            {
                if ( p.LineupPos.Trim().Length > 0 )
                    if ( p.IsDefence() )
                        AnnouncePlayer( p );
            }
        }

        public void DumpKeyPlayers()
        {
            Utility.Announce( $"{"QB",3} {KeyPlayer( "QB" )}" );
            Utility.Announce( $"{"RB",3} {KeyPlayer( "RB" )}" );
            Utility.Announce( $"{"C",3} {KeyPlayer( "C" )}" );
            Utility.Announce( $"{"DE",3} {KeyPlayer( "DE" )}" );
            Utility.Announce( $"{"MLB",3} {KeyPlayer( "MLB" )}" );
            Utility.Announce( $"{"FS",3} {KeyPlayer( "FS" )}" );
        }

        public string KeyPlayer( string pos )
        {
            var star = "";
            var player = GetPlayerAt( pos );
            if ( player != null )
                star = player.PlayerNameShort;
            else
                MissingKeys++;
            return star;
        }

        public NFLPlayer GetPlayerAt( string lineupPos )
        {
            return PlayerList.FirstOrDefault( p => IsPos( lineupPos, p.LineupPos ) );
        }

        private static bool IsPos( string posType, string actPos )
        {
            if ( actPos.Trim().Length == 0 ) return false;

            string allPositions;
            switch ( posType )
            {
                case "RB":
                    allPositions = "RB,HB,TB,";
                    break;

                case "MLB":
                    allPositions = "MIKE,MLB,ILB,";
                    break;

                case "DE":
                    allPositions = "RDT,DRT,RE,RDE,DRE,RUSH,";
                    break;

                case "QB":
                    allPositions = "QB,";
                    break;

                case "C":
                    allPositions = "C,C/G,";
                    break;

                case "FS":
                    allPositions = "FS,";
                    break;

                default:
                    allPositions = "";
                    break;
            }
            var isPos = !( allPositions.IndexOf( actPos + "," ) < 0 );
            return isPos;
        }

        public List<NFLPlayer> LoadPlayerList( DataSet ds )
        {
            Starters = 0;
            DefStarters = 0;
            OffStarters = 0;
            PlayerList = new List<NFLPlayer>();
            var dt = ds.Tables[ "lineup" ];
            foreach ( DataRow dr in dt.Rows )
            {
                var p = Masters.Pm.GetPlayer( dr[ "PLAYERID" ].ToString() );
                var oldJersey = p.JerseyNo;
                var newJersey = dr[ "SHIRT" ].ToString().Trim();
                if ( oldJersey.Equals( newJersey ) )
                    newJersey = "0"; // so it is ignored by update
                p.JerseyNo = newJersey;
                p.LineupPos = dr[ "POS" ].ToString().Trim();
                if ( ( bool ) dr[ "START" ] )
                {
                    Starters++;
                    if ( p.IsOffence() )
                        OffStarters++;
                    else
                    {
                        DefStarters++;
                        p.PlayerRole = Constants.K_ROLE_STARTER;
                        p.PlayerCat = Constants.K_LINEMAN_CAT;
                    }
                }
                else
                {
                    Backups++;
                    if ( p.IsDefence() )
                    {
                        p.PlayerRole = Constants.K_ROLE_BACKUP;
                        p.PlayerCat = Constants.K_LINEMAN_CAT;
                    }
                }
                PlayerList.Add( p );
            }
            return PlayerList;
        }

        public int PlayerCount()
        {
            return PlayerList.Count;
        }

        public void UpdateJerseysAndDefensiveRoles(string teamCode)
        {
            // blank out all defensive roles
            Utility.TflWs.ClearDefensiveRoles(teamCode);

            foreach ( var p in PlayerList )
            {
                if ( !string.IsNullOrEmpty( p.JerseyNo ) && p.JerseyNo != "0" )
                {
                    Utility.TflWs.StoreJersey( p.JerseyNo, p.PlayerCode );
                    Console.WriteLine($"{p.PlayerName} jersey set to {p.JerseyNo}");
                }
                if ( p.IsDefence() )
                {
                    var role = string.Empty;
                    if ( p.IsStarter() )
                        role = Constants.K_ROLE_STARTER;
                    else
                        role = Constants.K_ROLE_BACKUP;
                    Utility.TflWs.SetRole(p.PlayerCode, role);
                    Console.WriteLine( $"{p.PlayerName} role set to {role}" );
                }
            }
        }

        public void UpdateDefendersRoles()
        {
            foreach ( var p in PlayerList )
            {
                if ( p.IsDefence() )
                {
                    Utility.TflWs.StoreJersey( p.JerseyNo, p.PlayerCode );
                    Console.WriteLine( $"{p.PlayerName} jersey set to {p.JerseyNo}" );
                }
            }
        }

    }
}