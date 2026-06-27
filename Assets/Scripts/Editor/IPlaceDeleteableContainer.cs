public interface IPlaceDeleteableContainer<T> where T : IPlaceDeleteable
{
    public void OnPlace(T objectToPlace);
    public void OnDelete(T objectToDelete);

}
