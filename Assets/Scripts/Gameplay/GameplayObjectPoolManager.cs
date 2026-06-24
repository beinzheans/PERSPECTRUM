using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A general class to handle object pool to render the gameplay objects.
/// </summary>
/// <typeparam name="TObjectData">The object data</typeparam>
/// <typeparam name="TBehavior">The object rendering class</typeparam>
public abstract class GameplayObjectPoolManager<TObjectData, TBehavior> : MonoBehaviour where TObjectData : GameplayObject where TBehavior : GameplayObjectRenderBehavior<TObjectData>
{
    [SerializeField] protected TBehavior gameplayObjectPrefab;
    [SerializeField] private Transform parentTransform;
    protected GameplayManager gameplayManager;

    [SerializeField] protected int k_POOLSIZE;

    protected Queue<TBehavior> objectPool = new();
    protected Dictionary<TObjectData, TBehavior> currentActiveObjectsMapping = new();


    private int minIndex;


    private void Awake()
    {
        InstaniateObjectPool();
    }

    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;
        gameplayManager.OnGameplayRestarted += GameplayManager_OnGameplayRestarted;
        OnStartEvent();
    }

    private void GameplayManager_OnGameplayRestarted()
    {
        foreach (var (data, _) in currentActiveObjectsMapping)
        {
            UnrenderObject_ReturnToPool(data);
        }

        minIndex = 0;
    }

    /// <summary>
    /// Implementation of events when the script starts. Use this for subscriber events to listeners.
    /// </summary>
    protected virtual void OnStartEvent()
    {
        gameplayManager.OnGameplayTimeUpdated += GameplayManager_OnGameplayTimeUpdated;
    }

    private void OnDestroy()
    {
        gameplayManager.OnGameplayRestarted -= GameplayManager_OnGameplayRestarted;
        OnDestroyEvent();
    }

    /// <summary>
    /// Implementation of events when the script is destroyed. Use this for desubscriber events to listeners.
    /// </summary>
    protected virtual void OnDestroyEvent()
    {
        gameplayManager.OnGameplayTimeUpdated -= GameplayManager_OnGameplayTimeUpdated;
    }

    private void GameplayManager_OnGameplayTimeUpdated(double time)
    {
        double maxTime = time + GameManager.GameInstance.GlobalSettings.GameSettings.GameLookaheadTime + GameplayManager.k_POOLLOOKAHEADTIME;

        for (int i = minIndex; i < gameplayManager.CurrentGameplayChart.GameplayObjects.Length; i++)
        {
            GameplayObject gameplayObject = gameplayManager.CurrentGameplayChart.GameplayObjects[i];

            if (gameplayObject.RenderTime > maxTime)
            {
                break;
            }

            if (gameplayObject is not TObjectData obj)
            {
                continue;
            }

            bool isInRenderRange = obj.IsInRenderRange(time);


            if (isInRenderRange && !obj.IsRendered)
            {
                obj.IsRendered = true;
                RenderObject_GetFromPool(obj);
            }
            else if (isInRenderRange && obj.IsRendered)
            {
                UpdateRenderedObject(obj);
            }
            else if (obj.IsInUnrenderRange(time) && obj.IsRendered)
            {
                minIndex = i; // set this so we don't unnecessarily search for old objects
                obj.IsRendered = false;
                UnrenderObject_ReturnToPool(obj);
            }
        }
    }

    protected void UpdateRenderedObject(TObjectData objectData)
    {
        bool result = currentActiveObjectsMapping.TryGetValue(objectData, out TBehavior behaviorToUpdate);
        if (!result)
        {
            return;
        }

        behaviorToUpdate.OnUpdate();
    }

    /// <summary>
    /// Tries to get a new behavior from the current pool and render the object
    /// </summary>
    /// <param name="objectData"></param>
    protected void RenderObject_GetFromPool(TObjectData objectData)
    {
        bool result = objectPool.TryDequeue(out TBehavior newObjectFromPool);

        if (!result)
        {
            Debug.LogWarning($"Failed to get object from pool! The limit is {k_POOLSIZE}.");
            return;
        }

        if (currentActiveObjectsMapping.ContainsKey(objectData))
        {
            Debug.Log($"Hash collision: \n" +
                      $"{objectData.RenderTime}");
        }
        currentActiveObjectsMapping.Add(objectData, newObjectFromPool);
        newObjectFromPool.OnRender(objectData);
    }

    /// <summary>
    /// Returns the behavior from the current pool and unrender the object
    /// </summary>
    /// <param name="unrenderObject"></param>
    protected void UnrenderObject_ReturnToPool(TObjectData unrenderObject)
    {
        bool result = currentActiveObjectsMapping.TryGetValue(unrenderObject, out TBehavior behaviorToReturn);
        currentActiveObjectsMapping.Remove(unrenderObject);

        if (!result)
        {
            return;
        }

        behaviorToReturn.OnUnrender();
        objectPool.Enqueue(behaviorToReturn);
    }

    private void InstaniateObjectPool()
    {
        for (int i = 0; i < k_POOLSIZE; i++)
        {
            TBehavior newObject = Instantiate(gameplayObjectPrefab, parentTransform, false);
            newObject.gameObject.SetActive(false);
            objectPool.Enqueue(newObject);
        }
    }
}
