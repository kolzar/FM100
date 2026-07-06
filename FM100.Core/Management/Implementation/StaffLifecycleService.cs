using FM100.Core.GameState;

namespace FM100.Core.Management.Implementation;

public sealed class StaffLifecycleService : IStaffLifecycleService
{
    public StaffLifecycleResult ApplySeasonReview(GameState.GameState gameState)
    {
        var club = gameState.GetPlayerClub();
        if (club == null)
        {
            return new StaffLifecycleResult(false, false, 0, 0, "No player club is available.");
        }

        var staff = gameState.Staff;
        var coachBefore = staff.CoachQuality;
        var physioBefore = staff.PhysioQuality;
        var scoutBefore = staff.ScoutQuality;
        var annualCost = CalculateAnnualCost(staff);
        var renewalDue = staff.ContractExpiresSeason <= gameState.CurrentSeason;
        var renewalFee = renewalDue ? CalculateRenewalFee(staff) : 0;
        var totalCost = annualCost + renewalFee;
        var retained = club.BudgetInMillions >= totalCost;
        var qualityLost = 0;
        string outcome;
        string message;

        if (retained)
        {
            club.BudgetInMillions -= totalCost;
            if (renewalDue)
            {
                staff.ContractExpiresSeason = gameState.CurrentSeason + 3;
            }

            outcome = renewalDue ? "Renewed" : "Retained";
            message = renewalDue
                ? $"Staff contracts renewed until season {staff.ContractExpiresSeason} for EUR {totalCost}M."
                : $"Annual staff cost paid: EUR {totalCost}M.";
        }
        else
        {
            var paid = Math.Min(club.BudgetInMillions, annualCost);
            club.BudgetInMillions -= paid;
            staff.CoachQuality = Math.Max(1, staff.CoachQuality - 1);
            staff.PhysioQuality = Math.Max(1, staff.PhysioQuality - 1);
            staff.ScoutQuality = Math.Max(1, staff.ScoutQuality - 1);
            qualityLost = (coachBefore - staff.CoachQuality) +
                          (physioBefore - staff.PhysioQuality) +
                          (scoutBefore - staff.ScoutQuality);
            totalCost = paid;
            outcome = "Downsized";
            message = $"Staff budget shortfall caused {qualityLost} total quality point(s) of departures.";
        }

        gameState.StaffHistory.Add(new StaffHistoryRecord
        {
            Season = gameState.CurrentSeason,
            Outcome = outcome,
            CostInMillions = totalCost,
            CoachQualityBefore = coachBefore,
            CoachQualityAfter = staff.CoachQuality,
            PhysioQualityBefore = physioBefore,
            PhysioQualityAfter = staff.PhysioQuality,
            ScoutQualityBefore = scoutBefore,
            ScoutQualityAfter = staff.ScoutQuality,
            ContractExpiresSeason = staff.ContractExpiresSeason,
            Summary = message
        });
        gameState.Finances.Add(new FinanceRecord
        {
            Season = gameState.CurrentSeason,
            Day = gameState.DaysElapsed,
            Type = "StaffCost",
            AmountInMillions = -totalCost,
            ClubId = club.Id,
            ClubName = club.Name,
            Description = message
        });
        staff.UpdatedAt = DateTime.UtcNow;
        club.UpdatedAt = DateTime.UtcNow;
        return new StaffLifecycleResult(retained, retained && renewalDue, totalCost, qualityLost, message);
    }

    public static int CalculateAnnualCost(StaffSetup staff)
    {
        return Math.Max(3, (int)Math.Ceiling((staff.CoachQuality + staff.PhysioQuality + staff.ScoutQuality) / 4m));
    }

    private static int CalculateRenewalFee(StaffSetup staff)
    {
        return Math.Max(2, (int)Math.Ceiling((staff.CoachQuality + staff.PhysioQuality + staff.ScoutQuality) / 6m));
    }
}
