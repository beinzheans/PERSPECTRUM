using Newtonsoft.Json;
using System;
using UnityEngine;

[Serializable]
public class VisualHitbox : GameplayObject, IEquatable<VisualHitbox>
{
    public VisualHitbox(Vector2 normalizedPosition, double renderTime, float normalizedSize, HitboxType hitboxType) : base(renderTime)
    {
        NormalizedPosition = normalizedPosition;
        NormalizedSize = normalizedSize;
        HitboxType = hitboxType;
    }

    [JsonProperty("pos")]
    public Vector2 NormalizedPosition { get; protected set; }
    [JsonProperty("size")]
    public float NormalizedSize { get; protected set; }

    [NonSerialized]
    public bool IsInteracted;
    [JsonProperty("type")]

    public HitboxType HitboxType;
    /// <summary>
    /// Whether or not this hitbox is in range of the player to interact
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public bool IsInPlayerRange(double time)
    {
        double maxInteractTime = time + GameplayManager.k_EARLYTIMEFRAME;
        double minInteractTime = HitboxType != HitboxType.BOMB ? time - GameplayManager.k_LENIENCYTIMEFRAME : time;

        return (RenderTime >= minInteractTime && RenderTime < maxInteractTime);
    }

    public bool IsPlayerMissed(double time)
    {
        double minInteractTime = time - GameplayManager.k_LENIENCYTIMEFRAME;
        return RenderTime < minInteractTime;
    }

    public bool IsMousePositionSuccessfullyInside()
    {
        float scaledSize = HitboxType != HitboxType.BOMB ? NormalizedSize + GameplayManager.k_HITBOXINTERACTSIZEADDDELTA : NormalizedSize;
        Vector2 max = NormalizedPosition + 0.5f * scaledSize * Vector2.one;
        Vector2 min = NormalizedPosition - 0.5f * scaledSize * Vector2.one;

        Vector2 point = GameplayManager.GameplayInstance.GameplayMousePosition;

        return point.x >= min.x && point.x <= max.x && point.y >= min.y && point.y <= max.y;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as VisualHitbox);
    }

    public bool Equals(VisualHitbox other)
    {
        return other is not null &&
               base.Equals(other) &&
               NormalizedPosition.Equals(other.NormalizedPosition) &&
               NormalizedSize == other.NormalizedSize &&
               HitboxType == other.HitboxType;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), NormalizedPosition, NormalizedSize, HitboxType);
    }
}
