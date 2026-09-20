using System.Collections;
using UnityEngine;

/// <summary>
/// A class to define the comparisions between <see cref="GameplayObject"/>. <br></br>
/// In general, we do comparision based on render time, but we have the following cases: <br></br>
/// If there exist at least two <see cref="GameplayObject"/> that shares the same time, return 0 iff. they share the same type.
/// Otherwise, with a defined target type <typeparamref name="T"/>, prioritize <see cref="GameplayObject"/> with the same time as <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public class GameplayObjectComparer : IComparer
{
    public int Compare(object x, object y)
    {
        if (x is not GameplayObject x_obj || y is not GameplayObject y_obj)
        {
            return 0;
        }

        if (x_obj.RenderTime > y_obj.RenderTime) return 1;
        else if (x_obj.RenderTime < y_obj.RenderTime) return -1;
        else return 0;
    }
}
