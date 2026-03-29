using System.Collections.Generic;

public interface IPlaceDeleteable<T> : IRenderable
{
    public void OnPlace(ref List<T> listToEdit);
    public void OnDelete(ref List<T> listToEdit);
}
