using FM100.Core.GameState;
using FM100.Core.Management;
using FM100.Core.Management.Implementation;
using FM100.Domain.Club;
using FM100.Domain.Competition;

namespace FM100.UnitTest.Core.Management;

public class HistoryServiceTests
{
    [Fact]
    public void GetTitleHistory_OrdersTitlesDescendingThenClubName()
    {
        var clubA = CreateClub("Aurora FC", Division.SerieA);
        var clubB = CreateClub("Boreale", Division.SerieB);
        var clubC = CreateClub("Centrale", Division.SerieC);
        var gameState = new GameState
        {
            Clubs = new Dictionary<Guid, Club>
            {
                [clubA.Id] = clubA,
                [clubB.Id] = clubB,
                [clubC.Id] = clubC
            },
            HallOfFame = new HallOfFame
            {
                TitlesByClub = new Dictionary<Guid, int>
                {
                    [clubC.Id] = 1,
                    [clubB.Id] = 3,
                    [clubA.Id] = 3
                }
            }
        };

        var service = new HistoryService();

        var history = service.GetTitleHistory(gameState);

        Assert.Collection(
            history,
            entry =>
            {
                Assert.Equal("Aurora FC", entry.ClubName);
                Assert.Equal(3, entry.Titles);
            },
            entry =>
            {
                Assert.Equal("Boreale", entry.ClubName);
                Assert.Equal(3, entry.Titles);
            },
            entry =>
            {
                Assert.Equal("Centrale", entry.ClubName);
                Assert.Equal(1, entry.Titles);
            });
    }

    [Fact]
    public void GetManagerAndUnbeatenHistory_ReturnsRankedHallOfFameRecords()
    {
        var club = CreateClub("Aurora FC", Division.SerieA);
        var gameState = new GameState
        {
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club },
            HallOfFame = new HallOfFame
            {
                TopManagers =
                [
                    new ManagerRecord
                    {
                        ClubId = club.Id,
                        ManagerName = "Ada Coach",
                        Seasons = 4,
                        Titles = 2,
                        MatchesPlayed = 120,
                        MatchesWon = 72,
                        WinPercentage = 60
                    }
                ],
                UnbeatableStreaks =
                [
                    new UnbeatableStreak
                    {
                        ClubId = club.Id,
                        MatchCount = 18,
                        StartDate = new DateTime(2028, 8, 1),
                        EndDate = new DateTime(2028, 12, 1)
                    }
                ]
            }
        };
        var service = new HistoryService();

        var manager = Assert.Single(service.GetManagerHistory(gameState));
        var streak = Assert.Single(service.GetUnbeatenHistory(gameState));

        Assert.Equal("Ada Coach", manager.ManagerName);
        Assert.Equal("Aurora FC", manager.ClubName);
        Assert.Equal(72, manager.MatchesWon);
        Assert.Equal(18, streak.MatchCount);
        Assert.Equal("Aurora FC", streak.ClubName);
    }

    [Fact]
    public void GetBestSeasonHistory_OrdersByGoalsAndResolvesClub()
    {
        var club = CreateClub("Aurora FC", Division.SerieA);
        var playerId = Guid.NewGuid();
        var gameState = new GameState
        {
            Clubs = new Dictionary<Guid, Club> { [club.Id] = club },
            HallOfFame = new HallOfFame
            {
                BestSeasons = new Dictionary<Guid, SeasonRecord>
                {
                    [playerId] = new()
                    {
                        PlayerId = playerId,
                        ClubId = club.Id,
                        PlayerName = "Alex Record",
                        Season = 8,
                        Appearances = 32,
                        GoalsScored = 24,
                        Assists = 11,
                        AverageRating = 8
                    }
                }
            }
        };

        var entry = Assert.Single(new HistoryService().GetBestSeasonHistory(gameState));

        Assert.Equal("Alex Record", entry.PlayerName);
        Assert.Equal("Aurora FC", entry.ClubName);
        Assert.Equal(24, entry.Goals);
        Assert.Equal(11, entry.Assists);
        Assert.Equal(8, entry.AverageRating);
    }

    [Fact]
    public void GetLeagueTableHistory_ReturnsAllRowsInFinalPositionOrder()
    {
        var gameState = new GameState();
        gameState.LeagueTableArchive.Add(new LeagueTableArchiveRecord
        {
            Season = 9,
            Division = Division.SerieB,
            Rows =
            [
                new LeagueTableArchiveRow { Position = 2, ClubName = "Boreale", Points = 60 },
                new LeagueTableArchiveRow { Position = 1, ClubName = "Aurora", Points = 66 }
            ]
        });

        var entry = Assert.Single(new HistoryService().GetLeagueTableHistory(gameState));

        Assert.Equal(9, entry.Season);
        Assert.Equal(Division.SerieB, entry.Division);
        Assert.Equal(["Aurora", "Boreale"], entry.Rows.Select(row => row.ClubName));
        Assert.Equal([66, 60], entry.Rows.Select(row => row.Points));
    }

    [Fact]
    public void GetRollOfHonour_ReturnsSeasonChampionsAndFallsBackToAwards()
    {
        var gameState = new GameState();
        gameState.LeagueTableArchive.Add(new LeagueTableArchiveRecord
        {
            Season = 2,
            Division = Division.SerieA,
            Rows = [new LeagueTableArchiveRow { Position = 1, ClubName = "Aurora" }]
        });
        gameState.SeasonAwards.Add(new SeasonAwardRecord
        {
            Season = 2,
            AwardKey = "2:SerieB:champion",
            Title = "League Champion",
            WinnerName = "Boreale"
        });

        var entry = Assert.Single(new HistoryService().GetRollOfHonour(gameState));

        Assert.Equal(2, entry.Season);
        Assert.Equal("Aurora", entry.SerieAChampion);
        Assert.Equal("Boreale", entry.SerieBChampion);
        Assert.Equal("-", entry.SerieCChampion);
    }

    [Fact]
    public void GetCupRollOfHonour_ReturnsHistoricalAndCurrentCupWinners()
    {
        var gameState = new GameState
        {
            HistoricalEndYear = 2025
        };
        gameState.HistoricalCupArchive.AddRange(
        [
            new HistoricalCupRecord { Season = 2025, Type = CupType.SerieACup, ChampionClubName = "Aurora" },
            new HistoricalCupRecord { Season = 2025, Type = CupType.SerieBCup, ChampionClubName = "Boreale" },
            new HistoricalCupRecord { Season = 2025, Type = CupType.SerieCCup, ChampionClubName = "Centrale" },
            new HistoricalCupRecord { Season = 2025, Type = CupType.MasterCup, ChampionClubName = "Aurora" }
        ]);

        var currentChampionId = Guid.NewGuid();
        gameState.Clubs[currentChampionId] = CreateClub("Delta", Division.SerieA);
        gameState.CupCompetitions[Guid.NewGuid()] = new CupCompetition
        {
            Name = "Master Cup",
            Type = CupType.MasterCup,
            Season = 1,
            ChampionClubId = currentChampionId,
            IsComplete = true
        };

        var history = new HistoryService().GetCupRollOfHonour(gameState);

        Assert.Equal(2, history.Count);
        Assert.Equal(2026, history[0].Season);
        Assert.Equal("-", history[0].SerieACupWinner);
        Assert.Equal("Delta", history[0].MasterCupWinner);
        Assert.Equal(2025, history[1].Season);
        Assert.Equal("Aurora", history[1].SerieACupWinner);
        Assert.Equal("Boreale", history[1].SerieBCupWinner);
        Assert.Equal("Centrale", history[1].SerieCCupWinner);
        Assert.Equal("Aurora", history[1].MasterCupWinner);
    }

    [Fact]
    public void GetClubSeasonHistory_TracksDivisionMovesAndCareerTotals()
    {
        var clubId = Guid.NewGuid();
        var gameState = new GameState();
        AddArchivedSeason(gameState, clubId, 1, Division.SerieA, position: 18, points: 28, wins: 6, goals: 30);
        AddArchivedSeason(gameState, clubId, 2, Division.SerieB, position: 1, points: 74, wins: 22, goals: 66);
        AddArchivedSeason(gameState, clubId, 3, Division.SerieA, position: 3, points: 65, wins: 19, goals: 58);
        gameState.ClubFinanceHistory.AddRange(
        [
            new ClubFinanceHistoryRecord { ClubId = clubId, Season = 1, NetAmountInMillions = -5, ClosingBudgetInMillions = 20 },
            new ClubFinanceHistoryRecord { ClubId = clubId, Season = 2, NetAmountInMillions = 12, ClosingBudgetInMillions = 32 },
            new ClubFinanceHistoryRecord { ClubId = clubId, Season = 3, NetAmountInMillions = 8, ClosingBudgetInMillions = 40 }
        ]);
        gameState.ClubSeasonStars.Add(new ClubSeasonStarRecord
        {
            ClubId = clubId,
            Season = 2,
            PlayerName = "Alex Hero",
            Goals = 21,
            Assists = 9,
            AverageRating = 8
        });
        var service = new HistoryService();

        var seasons = service.GetClubSeasonHistory(gameState, clubId);
        var summary = service.GetClubCareerSummary(gameState, clubId);
        var reports = service.GetClubSeasonSummaries(gameState, clubId);

        Assert.Equal([3, 2, 1], seasons.Select(entry => entry.Season));
        Assert.Equal("Stayed", seasons[0].Outcome);
        Assert.Equal("Champion + Promoted", seasons[1].Outcome);
        Assert.Equal("Relegated", seasons[2].Outcome);
        Assert.Equal(3, summary.Seasons);
        Assert.Equal(1, summary.Titles);
        Assert.Equal(1, summary.Promotions);
        Assert.Equal(1, summary.Relegations);
        Assert.Equal(1, summary.BestPosition);
        Assert.Equal(2, summary.BestSeason);
        Assert.Equal(167, summary.TotalPoints);
        Assert.Equal(47, summary.TotalWins);
        Assert.Equal(154, summary.TotalGoals);
        Assert.Equal(15, summary.NetFinanceInMillions);
        Assert.Equal("A+", reports[1].Grade);
        Assert.Equal("Up to SerieA", reports[0].Trend);
        Assert.Equal("Down to SerieB", reports[1].Trend);
        Assert.Equal("Alex Hero", reports[1].StarPlayerName);
        Assert.Equal(21, reports[1].StarGoals);
    }

    [Fact]
    public void GetInjuryHistory_PrioritizesLatestUnavailablePlayers()
    {
        var gameState = new GameState();
        gameState.InjuryHistory.Add(new InjuryHistoryRecord
        {
            Season = 3,
            Day = 20,
            PlayerName = "Recovered Player",
            ClubName = "Aurora",
            InjuryType = "Match knock",
            Severity = "Minor",
            InitialDays = 5,
            RecoveredAtDay = 25
        });
        gameState.InjuryHistory.Add(new InjuryHistoryRecord
        {
            Season = 4,
            Day = 8,
            PlayerName = "Unavailable Player",
            ClubName = "Boreale",
            InjuryType = "Muscle strain",
            Severity = "Moderate",
            InitialDays = 14
        });

        var history = new HistoryService().GetInjuryHistory(gameState);

        Assert.Equal("Unavailable Player", history[0].PlayerName);
        Assert.Null(history[0].RecoveredAtDay);
        Assert.Equal("Moderate", history[0].Severity);
    }

    [Fact]
    public void GetMediaHistory_ReturnsMostRecentStoriesWithResolvedStatus()
    {
        var gameState = new GameState();
        gameState.MediaEvents.Add(new MediaEventRecord
        {
            Headline = "Old pressure",
            Question = "Can you recover?",
            Season = 1,
            Day = 1,
            CreatedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        });
        gameState.MediaEvents.Add(new MediaEventRecord
        {
            Headline = "Derby reaction",
            Response = "Challenge",
            Outcome = "The squad looked sharper.",
            Season = 1,
            Day = 2,
            IsResolved = true,
            CreatedAt = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc)
        });

        var service = new HistoryService();

        var history = service.GetMediaHistory(gameState, take: 1);

        var entry = Assert.Single(history);
        Assert.Equal("Derby reaction", entry.Headline);
        Assert.Equal("Challenge", entry.Status);
        Assert.Equal("The squad looked sharper.", entry.Outcome);
        Assert.Equal("general", entry.StorylineKey);
        Assert.Equal(1, entry.StorylineStage);
        Assert.Equal(1, entry.PressureLevel);
        Assert.Equal(0, entry.Effectiveness);
    }

    [Fact]
    public void GetAwardHistory_ReturnsRecentAwards()
    {
        var gameState = new GameState();
        gameState.SeasonAwards.Add(new SeasonAwardRecord
        {
            Season = 1,
            Title = "League Champion",
            WinnerName = "Aurora FC",
            Description = "Aurora FC won Serie A.",
            CreatedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        });
        gameState.SeasonAwards.Add(new SeasonAwardRecord
        {
            Season = 2,
            Title = "Best Attack",
            WinnerName = "Boreale",
            Description = "Boreale scored 70 goals.",
            CreatedAt = new DateTime(2027, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        });

        var service = new HistoryService();

        var history = service.GetAwardHistory(gameState, take: 1);

        var entry = Assert.Single(history);
        Assert.Equal("Best Attack", entry.Title);
        Assert.Equal("Boreale", entry.WinnerName);
        Assert.Equal(2, entry.Season);
        Assert.Equal("CLUB", entry.Category);
        Assert.Equal(3, entry.Priority);
    }

    [Fact]
    public void GetAwardHistory_OrdersSameSeasonByImportanceAndClassifiesWinners()
    {
        var gameState = new GameState();
        gameState.SeasonAwards.Add(new SeasonAwardRecord
        {
            Season = 4,
            AwardKey = "4:SerieA:best-defense",
            Title = "Best Defense",
            WinnerName = "Aurora FC"
        });
        gameState.SeasonAwards.Add(new SeasonAwardRecord
        {
            Season = 4,
            AwardKey = "4:SerieA:player-of-season",
            Title = "Player of the Season",
            WinnerName = "Alex Growth"
        });
        gameState.SeasonAwards.Add(new SeasonAwardRecord
        {
            Season = 4,
            AwardKey = "4:SerieA:champion",
            Title = "League Champion",
            WinnerName = "Boreale"
        });

        var service = new HistoryService();

        var history = service.GetAwardHistory(gameState);

        Assert.Collection(
            history,
            entry =>
            {
                Assert.Equal("League Champion", entry.Title);
                Assert.Equal("TITLE", entry.Category);
                Assert.Equal(1, entry.Priority);
            },
            entry =>
            {
                Assert.Equal("Player of the Season", entry.Title);
                Assert.Equal("PLAYER", entry.Category);
                Assert.Equal(2, entry.Priority);
            },
            entry =>
            {
                Assert.Equal("Best Defense", entry.Title);
                Assert.Equal("CLUB", entry.Category);
                Assert.Equal(4, entry.Priority);
            });
    }

    [Fact]
    public void GetPlayerDevelopmentHistory_ReturnsMostRelevantRecentChanges()
    {
        var gameState = new GameState();
        gameState.PlayerDevelopmentHistory.Add(new PlayerDevelopmentRecord
        {
            PlayerName = "Alex Growth",
            Season = 2,
            ReputationBefore = 10,
            ReputationAfter = 12,
            PotentialBefore = 15,
            PotentialAfter = 16,
            MarketValueBefore = 12,
            MarketValueAfter = 30,
            Summary = "Rep +2, Pot +1, Value +18M after 2800 minutes."
        });

        var service = new HistoryService();

        var history = service.GetPlayerDevelopmentHistory(gameState);

        var entry = Assert.Single(history);
        Assert.Equal("Alex Growth", entry.PlayerName);
        Assert.Equal(2, entry.Season);
        Assert.Equal(2, entry.ReputationChange);
        Assert.Equal(1, entry.PotentialChange);
        Assert.Equal(18, entry.MarketValueChange);
    }

    [Fact]
    public void GetPlayerCareerEvents_ReturnsRecentRetirementsAndAcademyPromotions()
    {
        var gameState = new GameState();
        gameState.PlayerCareerEvents.Add(new PlayerCareerEventRecord
        {
            Season = 4,
            EventType = "Retirement",
            PlayerName = "Old Captain",
            ClubName = "Aurora FC",
            Age = 39,
            Summary = "Retired from Aurora FC."
        });
        gameState.PlayerCareerEvents.Add(new PlayerCareerEventRecord
        {
            Season = 5,
            EventType = "AcademyPromotion",
            PlayerName = "Young Prospect",
            ClubName = "Boreale",
            Age = 18,
            Summary = "Promoted from the Boreale academy."
        });

        var history = new HistoryService().GetPlayerCareerEvents(gameState, take: 1);

        var entry = Assert.Single(history);
        Assert.Equal(5, entry.Season);
        Assert.Equal("AcademyPromotion", entry.EventType);
        Assert.Equal("Young Prospect", entry.PlayerName);
        Assert.Equal(18, entry.Age);
    }

    [Fact]
    public void GetTransferHistory_OrdersRecentSeasonByHighestFee()
    {
        var gameState = new GameState();
        gameState.TransferHistory.Add(new TransferHistoryRecord
        {
            Season = 3,
            PlayerName = "Older Move",
            FromClubName = "Aurora",
            ToClubName = "Boreale",
            FeeInMillions = 20
        });
        gameState.TransferHistory.Add(new TransferHistoryRecord
        {
            Season = 4,
            PlayerName = "Major Move",
            FromClubName = "Centrale",
            ToClubName = "Aurora",
            FeeInMillions = 15
        });
        gameState.TransferHistory.Add(new TransferHistoryRecord
        {
            Season = 4,
            PlayerName = "Minor Move",
            FromClubName = "Boreale",
            ToClubName = "Centrale",
            FeeInMillions = 5
        });

        var history = new HistoryService().GetTransferHistory(gameState);

        Assert.Equal(["Major Move", "Minor Move", "Older Move"], history.Select(entry => entry.PlayerName));
        Assert.Equal("Centrale", history[0].FromClubName);
        Assert.Equal("Aurora", history[0].ToClubName);
    }

    [Fact]
    public void GetContractHistory_PrioritizesReleasedPlayersWithinSeason()
    {
        var gameState = new GameState();
        gameState.ContractHistory.Add(new ContractHistoryRecord
        {
            Season = 6,
            Outcome = "Renewed",
            PlayerName = "Renewed Player",
            ClubName = "Aurora",
            ContractExpiresSeason = 9,
            Summary = "Renewed until season 9."
        });
        gameState.ContractHistory.Add(new ContractHistoryRecord
        {
            Season = 6,
            Outcome = "Released",
            PlayerName = "Free Player",
            ClubName = "Boreale",
            ContractExpiresSeason = 6,
            Summary = "Left as a free agent."
        });

        var history = new HistoryService().GetContractHistory(gameState);

        Assert.Equal(["Free Player", "Renewed Player"], history.Select(entry => entry.PlayerName));
        Assert.Equal("Released", history[0].Outcome);
        Assert.Equal(6, history[0].ContractExpiresSeason);
    }

    [Fact]
    public void GetClubFinanceHistory_OrdersLatestSeasonByNetResult()
    {
        var gameState = new GameState();
        gameState.ClubFinanceHistory.Add(new ClubFinanceHistoryRecord
        {
            Season = 7,
            ClubName = "Aurora",
            FinalPosition = 1,
            SponsorshipInMillions = 50,
            PrizeMoneyInMillions = 30,
            WageCostInMillions = 40,
            NetAmountInMillions = 40,
            ClosingBudgetInMillions = 120
        });
        gameState.ClubFinanceHistory.Add(new ClubFinanceHistoryRecord
        {
            Season = 7,
            ClubName = "Boreale",
            FinalPosition = 2,
            SponsorshipInMillions = 40,
            PrizeMoneyInMillions = 20,
            WageCostInMillions = 45,
            NetAmountInMillions = 15,
            ClosingBudgetInMillions = 90
        });

        var history = new HistoryService().GetClubFinanceHistory(gameState);

        Assert.Equal(["Aurora", "Boreale"], history.Select(entry => entry.ClubName));
        Assert.Equal(40, history[0].NetAmountInMillions);
        Assert.Equal(120, history[0].ClosingBudgetInMillions);
    }

    [Fact]
    public void GetFinanceHistory_ReturnsMostRecentFinanceRecords()
    {
        var gameState = new GameState();
        gameState.Finances.Add(new FinanceRecord
        {
            Season = 1,
            Day = 3,
            Type = "MatchdayRevenue",
            AmountInMillions = 2,
            Description = "Home match revenue.",
            CreatedAt = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        });
        gameState.Finances.Add(new FinanceRecord
        {
            Season = 2,
            Day = 1,
            Type = "MatchdayRevenue",
            AmountInMillions = 4,
            Description = "Opening day revenue.",
            CreatedAt = new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc)
        });

        var service = new HistoryService();

        var history = service.GetFinanceHistory(gameState, take: 1);

        var entry = Assert.Single(history);
        Assert.Equal(2, entry.Season);
        Assert.Equal(1, entry.Day);
        Assert.Equal("MatchdayRevenue", entry.Type);
        Assert.Equal(4, entry.AmountInMillions);
        Assert.Equal("Opening day revenue.", entry.Description);
    }

    [Fact]
    public void GetStaffHistory_ReturnsLatestReviewWithQualityAndCost()
    {
        var gameState = new GameState();
        gameState.StaffHistory.Add(new StaffHistoryRecord
        {
            Season = 2,
            Outcome = "Retained",
            CostInMillions = 7,
            CoachQualityBefore = 11,
            CoachQualityAfter = 11,
            PhysioQualityBefore = 10,
            PhysioQualityAfter = 10,
            ScoutQualityBefore = 9,
            ScoutQualityAfter = 9,
            ContractExpiresSeason = 3,
            Summary = "Staff retained."
        });
        gameState.StaffHistory.Add(new StaffHistoryRecord
        {
            Season = 3,
            Outcome = "Renewed",
            CostInMillions = 12,
            CoachQualityBefore = 11,
            CoachQualityAfter = 11,
            PhysioQualityBefore = 10,
            PhysioQualityAfter = 10,
            ScoutQualityBefore = 9,
            ScoutQualityAfter = 9,
            ContractExpiresSeason = 6,
            Summary = "Staff renewed."
        });

        var entry = Assert.Single(new HistoryService().GetStaffHistory(gameState, take: 1));

        Assert.Equal(3, entry.Season);
        Assert.Equal("Renewed", entry.Outcome);
        Assert.Equal(12, entry.CostInMillions);
        Assert.Equal(6, entry.ContractExpiresSeason);
        Assert.Equal(11, entry.CoachQualityAfter);
    }

    [Fact]
    public void GetTeamTalkHistory_ReturnsLatestTalkWithMeasuredImpact()
    {
        var gameState = new GameState();
        gameState.TeamTalkHistory.Add(new TeamTalkHistoryRecord
        {
            Season = 2,
            Day = 5,
            Style = TeamTalkStyle.Calm,
            Effectiveness = 115,
            AffectedPlayers = 23,
            MoraleBefore = 10,
            MoraleAfter = 11,
            MotivationBefore = 9,
            MotivationAfter = 10,
            TrustBefore = 12,
            TrustAfter = 13,
            Summary = "Calm response."
        });

        var entry = Assert.Single(new HistoryService().GetTeamTalkHistory(gameState));

        Assert.Equal(TeamTalkStyle.Calm, entry.Style);
        Assert.Equal(115, entry.Effectiveness);
        Assert.Equal(23, entry.AffectedPlayers);
        Assert.Equal(13, entry.TrustAfter);
    }

    [Fact]
    public void GetSeasonReviews_AggregatesAwardsDevelopmentMediaAndFinanceBySeason()
    {
        var gameState = new GameState();
        gameState.SeasonAwards.Add(new SeasonAwardRecord
        {
            Season = 3,
            Title = "League Champion",
            WinnerName = "Aurora FC",
            Description = "Aurora FC won the league."
        });
        gameState.PlayerDevelopmentHistory.Add(new PlayerDevelopmentRecord
        {
            Season = 3,
            PlayerName = "Alex Growth",
            ReputationBefore = 10,
            ReputationAfter = 12,
            MarketValueBefore = 12,
            MarketValueAfter = 30,
            Summary = "Rep +2."
        });
        gameState.MediaEvents.Add(new MediaEventRecord
        {
            Season = 3,
            Headline = "Pressure week"
        });
        gameState.Finances.Add(new FinanceRecord
        {
            Season = 3,
            Type = "MatchdayRevenue",
            AmountInMillions = 6,
            Description = "Home match revenue."
        });

        var service = new HistoryService();

        var reviews = service.GetSeasonReviews(gameState);

        var review = Assert.Single(reviews);
        Assert.Equal(3, review.Season);
        Assert.Contains("Aurora FC", review.Headline);
        Assert.Contains("Alex Growth", review.Summary);
        Assert.Contains("+EUR 6M", review.Summary);
        Assert.Equal(1, review.AwardsCount);
        Assert.Equal(1, review.DevelopmentCount);
        Assert.Equal(1, review.MediaCount);
        Assert.Equal(1, review.FinanceCount);
        Assert.Equal(6, review.FinanceAmountInMillions);
    }

    [Fact]
    public void GetSeasonReviews_DefaultTimelineIncludesOneHundredSeasons()
    {
        var gameState = new GameState();
        for (var season = 1; season <= 100; season++)
        {
            gameState.SeasonAwards.Add(new SeasonAwardRecord
            {
                Season = season,
                AwardKey = $"{season}:SerieA:champion",
                Title = "League Champion",
                WinnerName = $"Champion {season}"
            });
        }

        var reviews = new HistoryService().GetSeasonReviews(gameState);

        Assert.Equal(100, reviews.Count);
        Assert.Equal(100, reviews[0].Season);
        Assert.Equal(1, reviews[^1].Season);
    }

    [Fact]
    public void GetSeasonReviews_BuildsCompletePlayerClubDossier()
    {
        var clubId = Guid.NewGuid();
        var gameState = new GameState { PlayerClubId = clubId };
        AddArchivedSeason(gameState, clubId, 7, Division.SerieA, position: 2, points: 78, wins: 23, goals: 70);
        gameState.ClubSeasonStars.Add(new ClubSeasonStarRecord
        {
            ClubId = clubId,
            Season = 7,
            PlayerName = "Alex Star",
            Goals = 20,
            Assists = 12,
            AverageRating = 8
        });
        gameState.SeasonAwards.AddRange(
        [
            new SeasonAwardRecord { Season = 7, AwardKey = "7:SerieA:champion", Title = "League Champion", WinnerName = "Alpha" },
            new SeasonAwardRecord { Season = 7, AwardKey = "7:SerieB:champion", Title = "League Champion", WinnerName = "Beta" },
            new SeasonAwardRecord { Season = 7, AwardKey = "7:SerieC:champion", Title = "League Champion", WinnerName = "Gamma" }
        ]);
        gameState.TransferHistory.Add(new TransferHistoryRecord { Season = 7, PlayerName = "Big Signing", FeeInMillions = 35 });
        gameState.InjuryHistory.Add(new InjuryHistoryRecord { Season = 7, Severity = "Severe" });
        gameState.Achievements.Add(new AchievementRecord { Season = 7, Title = "Title Challenger" });
        gameState.Finances.Add(new FinanceRecord { Season = 7, ClubId = clubId, AmountInMillions = 18 });

        var review = Assert.Single(new HistoryService().GetSeasonReviews(gameState));

        Assert.Equal("A", review.Grade);
        Assert.Contains("Serie A #2", review.ClubResult);
        Assert.Contains("Serie A: Alpha", review.WorldChampions);
        Assert.Contains("Serie B: Beta", review.WorldChampions);
        Assert.Contains("Serie C: Gamma", review.WorldChampions);
        Assert.Contains("Alex Star", review.StarPlayer);
        Assert.Contains("Big Signing", review.MarketHeadline);
        Assert.Contains("1 severe", review.MedicalHeadline);
        Assert.Contains("Title Challenger", review.AchievementHeadline);
        Assert.Equal(1, review.TransferCount);
        Assert.Equal(1, review.InjuryCount);
        Assert.Equal(1, review.AchievementCount);
    }

    private static Club CreateClub(string name, Division division)
    {
        return new Club
        {
            Id = Guid.NewGuid(),
            Name = name,
            Abbreviation = name[..Math.Min(3, name.Length)].ToUpperInvariant(),
            City = name,
            Stadium = new Stadium { Name = $"{name} Stadium", Capacity = 30000 },
            Division = division
        };
    }

    private static void AddArchivedSeason(
        GameState gameState,
        Guid clubId,
        int season,
        Division division,
        int position,
        int points,
        int wins,
        int goals)
    {
        gameState.LeagueTableArchive.Add(new LeagueTableArchiveRecord
        {
            Season = season,
            Division = division,
            Rows =
            [
                new LeagueTableArchiveRow
                {
                    ClubId = clubId,
                    ClubName = "Career FC",
                    Position = position,
                    Played = 38,
                    Wins = wins,
                    Draws = 8,
                    Losses = 38 - wins - 8,
                    GoalsFor = goals,
                    GoalsAgainst = 40,
                    GoalDifference = goals - 40,
                    Points = points
                }
            ]
        });
    }
}
