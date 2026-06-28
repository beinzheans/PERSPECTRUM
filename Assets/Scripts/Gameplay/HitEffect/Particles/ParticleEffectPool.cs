using UnityEngine;

/// <summary>
/// A class to allow for object pooling for <see cref="ParticleSystem"/>s.
/// </summary>
public abstract class ParticleEffectPool : MonoBehaviour
{

    [SerializeField] private int k_POOLSIZE;
    [SerializeField] private ParticleSystem particleSystemPrefab;

    private ParticleSystem[] currentParticleSystemPrefabPool;

    protected GameplayManager gameplayManager;

    private int poolIndex = 0;
    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;

        currentParticleSystemPrefabPool = new ParticleSystem[k_POOLSIZE];
        for (int i = 0; i < k_POOLSIZE; i++)
        {
            ParticleSystem matchParticleSystem = Instantiate(particleSystemPrefab, gameplayManager.GameplayCamera.transform, false);

            currentParticleSystemPrefabPool[i] = matchParticleSystem;
        }

        OnStartEvent();
    }

    protected void PlayParticles()
    {
        ParticleSystem particleSystem = currentParticleSystemPrefabPool[poolIndex];
        OnBeforeParticlePlayEvent(ref particleSystem);
        particleSystem.Play();
        poolIndex = (poolIndex + 1) % k_POOLSIZE;
    }

    private void OnDestroy()
    {
        OnDestroyEvent();
    }

    /// <summary>
    /// Custom implementations of events when the particle system pool starts.
    /// </summary>
    protected abstract void OnStartEvent();

    /// <summary>
    /// Custom implementations of events when the particle system pool is destroyed.
    /// </summary>
    protected abstract void OnDestroyEvent();

    /// <summary>
    /// Custom implementation of events when the particle system is about to be played.
    /// </summary>
    protected abstract void OnBeforeParticlePlayEvent(ref ParticleSystem particleSystem);
}
