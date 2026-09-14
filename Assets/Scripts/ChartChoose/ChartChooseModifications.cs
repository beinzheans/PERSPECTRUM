using UnityEngine;

/// <summary>
/// A script to handle the <see cref="GameplayModifications"/> for players to choose in the Chart Select screen.
/// </summary>
public class ChartChooseModifications : MonoBehaviour
{
    public static readonly GameplayModifications k_DEFAULTGAMEPLAYMODIFICATIONS = new GameplayModifications(1d, 0d);
    private GameplayModifications currentGameplayModifications;

    private void Start()
    {
        currentGameplayModifications = k_DEFAULTGAMEPLAYMODIFICATIONS;
        ChartChooseManager.ChartChooseInstance.OnRequestGameplayModifications += ChartChooseInstance_OnRequestGameplayModifications;
    }

    private void OnDestroy()
    {
        ChartChooseManager.ChartChooseInstance.OnRequestGameplayModifications -= ChartChooseInstance_OnRequestGameplayModifications;
    }
    private GameplayModifications ChartChooseInstance_OnRequestGameplayModifications()
    {
        return new GameplayModifications(0.5d, 100d);
    }
}
