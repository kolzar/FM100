using FM100.Core.GameState;

namespace FM100.Core.Management;

public interface IStaffService
{
    StaffReport BuildReport(GameState.GameState gameState);

    StaffUpgradeResult UpgradeDepartment(GameState.GameState gameState, StaffDepartment department);
}
