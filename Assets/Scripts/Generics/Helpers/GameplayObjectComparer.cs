using System.Collections;
using UnityEngine;

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
