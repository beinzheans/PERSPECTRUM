using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A general class to handle object pool to render the gameplay objects.
/// </summary>
/// <typeparam name="TObjectData">The object data</typeparam>
/// <typeparam name="TBehavior">The object rendering class</typeparam>
public abstract class GameplayObjectPoolManager<TObjectData, TBehavior> : MonoBehaviour where TObjectData : GameplayObject where TBehavior : GameplayObjectRenderBehavior<TObjectData>
{
    [SerializeField] private TBehavior gameplayObjectPrefab;

    private GameplayManager gameplayManager;

    [SerializeField] private int k_POOLSIZE;

    private Queue<TBehavior> objectPool = new();
    private Dictionary<TObjectData, TBehavior> currentActiveObjectsMapping = new();

    private int minIndex;


    private void Awake()
    {
        InstaniateObjectPool();
    }

    private void Start()
    {
        gameplayManager = GameplayManager.GameplayInstance;

        gameplayManager.OnGameplayTimeUpdated += GameplayManager_OnGameplayTimeUpdated;
    }

    private void GameplayManager_OnGameplayTimeUpdated(double time)
    {
        double maxTime = time + GameManager.GameInstance.GlobalSettings.GameSettings.GameLookaheadTime + GameplayManager.k_POOLLOOKAHEADTIME;

        for (int i = minIndex; i < gameplayManager.CurrentGameplayChart.GameplayObjects.Length; i++)
        {
            GameplayObject gameplayObject = gameplayManager.CurrentGameplayChart.GameplayObjects[i];

            if (gameplayObject.RenderTime > maxTime)
            {
                break; // early exit
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

    private void UpdateRenderedObject(TObjectData objectData)
    {
        TBehavior behaviorToUpdate = currentActiveObjectsMapping[objectData];
        behaviorToUpdate.OnUpdate();
    }

    private void RenderObject_GetFromPool(TObjectData objectData)
    {
        bool result = objectPool.TryDequeue(out TBehavior newObjectFromPool);

        if (!result)
        {
            Debug.LogWarning($"Failed to get object from pool! The limit is {k_POOLSIZE}.");
            return;
        }

        currentActiveObjectsMapping.Add(objectData, newObjectFromPool);
        newObjectFromPool.OnRender(objectData);
    }

    private void UnrenderObject_ReturnToPool(TObjectData unrenderObject)
    {
        TBehavior behaviorToReturn = currentActiveObjectsMapping[unrenderObject];
        currentActiveObjectsMapping.Remove(unrenderObject);
        behaviorToReturn.OnUnrender();
        objectPool.Enqueue(behaviorToReturn);
    }
    private void InstaniateObjectPool()
    {
        for (int i = 0; i < k_POOLSIZE; i++)
        {
            TBehavior newObject = Instantiate(gameplayObjectPrefab, transform, false);
            newObject.gameObject.SetActive(false);
            objectPool.Enqueue(newObject);
        }
    }
}
