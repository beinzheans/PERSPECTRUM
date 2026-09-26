using UnityEngine;

public class GameplayModsModule : BaseDataModule
{
    private const int k_GAMEPLAYSPEEDMOD = 0;
    private const int k_GAMEPLAYSTARTTIMEMOD = 1;
    protected override void OnModuleAwake()
    {
        return;
    }

    protected override void OnModuleInitialized()
    {
        moduleDataGroups[k_GAMEPLAYSPEEDMOD].SetGroupAction_InputField(x =>
        {
            bool parseResult = double.TryParse(x, out double result);

            if (!parseResult)
            {
                return;
            }

            ChartChooseManager.ChartChooseInstance.CurrentChartChooseModifcations.OnEdit(() => ChartChooseManager.ChartChooseInstance.CurrentChartChooseModifcations.GameplaySpeed, result);
        }, ChartChooseManager.ChartChooseInstance.CurrentChartChooseModifcations.GameplaySpeed.ToString("F2"));

        moduleDataGroups[k_GAMEPLAYSTARTTIMEMOD].SetGroupAction_InputField(x =>
        {
            bool parseResult = double.TryParse(x, out double result);

            if (!parseResult)
            {
                return;
            }

            ChartChooseManager.ChartChooseInstance.CurrentChartChooseModifcations.OnEdit(() => ChartChooseManager.ChartChooseInstance.CurrentChartChooseModifcations.GameplayStartTime, result);
        }, ChartChooseManager.ChartChooseInstance.CurrentChartChooseModifcations.GameplayStartTime.ToString("F2"));
    }
}
