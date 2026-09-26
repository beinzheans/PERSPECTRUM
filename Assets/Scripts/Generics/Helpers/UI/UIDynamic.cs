using UnityEngine;

/// <summary>
/// A class to create UI elements that can be dynamically moved with a displacement vector. <br></br>
/// This will assume that the position on the UI canvas is the fixed position. The UI element can not move freely and can only have a displacement about the fixed position.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIDynamic : MonoBehaviour
{
    public RectTransform RectTransform { get; private set; }
    private Vector2 initialStartingPosition;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }
    private void Start()
    {
        initialStartingPosition = RectTransform.anchoredPosition;
    }

    /// <summary>
    /// Displaces an UI element around it's fixed position. The displacement vector should be in screen space.
    /// </summary>
    /// <param name="displacement"></param>
    public void Displace(Vector2 displacement)
    {
        RectTransform.anchoredPosition = initialStartingPosition + displacement;
    }
}
