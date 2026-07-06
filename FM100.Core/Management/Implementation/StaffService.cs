using FM100.Core.GameState;

namespace FM100.Core.Management.Implementation;

public sealed class StaffService : IStaffService
{
    public StaffReport BuildReport(GameState.GameState gameState)
    {
        var staff = gameState.Staff;
        var departments = new[]
        {
            (Department: StaffDepartment.Coaching, Quality: staff.CoachQuality, Label: "Coaching"),
            (Department: StaffDepartment.Physio, Quality: staff.PhysioQuality, Label: "Physio"),
            (Department: StaffDepartment.Scouting, Quality: staff.ScoutQuality, Label: "Scouting")
        };
        var average = (int)Math.Round(departments.Average(item => item.Quality));
        var strongest = departments
            .OrderByDescending(item => item.Quality)
            .ThenBy(item => item.Label)
            .First();
        var weakest = departments
            .OrderBy(item => item.Quality)
            .ThenBy(item => item.Label)
            .First();
        var grade = average switch
        {
            >= 17 => "Elite",
            >= 14 => "Strong",
            >= 11 => "Solid",
            >= 8 => "Developing",
            _ => "Fragile"
        };

        return new StaffReport(
            average,
            grade,
            weakest.Department,
            $"{strongest.Label} {strongest.Quality}/20",
            $"{weakest.Label} {weakest.Quality}/20",
            StaffLifecycleService.CalculateAnnualCost(staff),
            staff.ContractExpiresSeason,
            $"{grade} staff | Avg {average}/20 | EUR {StaffLifecycleService.CalculateAnnualCost(staff)}M/y | Contract S{staff.ContractExpiresSeason} | Upgrade {weakest.Label}");
    }

    public StaffUpgradeResult UpgradeDepartment(GameState.GameState gameState, StaffDepartment department)
    {
        var playerClub = gameState.GetPlayerClub();
        if (playerClub == null)
        {
            return Failed(department, "No player club is available.");
        }

        var currentQuality = GetQuality(gameState.Staff, department);
        if (currentQuality >= 20)
        {
            return Failed(department, $"{department} staff is already elite.");
        }

        var cost = CalculateUpgradeCost(currentQuality);
        if (playerClub.BudgetInMillions < cost)
        {
            return Failed(department, $"Budget is not enough. Upgrade costs EUR {cost}M.");
        }

        playerClub.BudgetInMillions -= cost;
        SetQuality(gameState.Staff, department, currentQuality + 1);
        gameState.Staff.UpdatedAt = DateTime.UtcNow;
        gameState.LastSavedAt = DateTime.UtcNow;

        return new StaffUpgradeResult
        {
            Success = true,
            Department = department,
            CostInMillions = cost,
            QualityAfter = currentQuality + 1,
            Message = $"{department} staff upgraded to {currentQuality + 1}/20 for EUR {cost}M."
        };
    }

    private static int CalculateUpgradeCost(int currentQuality)
    {
        return Math.Max(1, currentQuality / 4);
    }

    private static int GetQuality(StaffSetup staff, StaffDepartment department)
    {
        return department switch
        {
            StaffDepartment.Physio => staff.PhysioQuality,
            StaffDepartment.Scouting => staff.ScoutQuality,
            _ => staff.CoachQuality
        };
    }

    private static void SetQuality(StaffSetup staff, StaffDepartment department, int quality)
    {
        switch (department)
        {
            case StaffDepartment.Physio:
                staff.PhysioQuality = Math.Clamp(quality, 1, 20);
                break;
            case StaffDepartment.Scouting:
                staff.ScoutQuality = Math.Clamp(quality, 1, 20);
                break;
            default:
                staff.CoachQuality = Math.Clamp(quality, 1, 20);
                break;
        }
    }

    private static StaffUpgradeResult Failed(StaffDepartment department, string message)
    {
        return new StaffUpgradeResult
        {
            Success = false,
            Department = department,
            Message = message
        };
    }
}
