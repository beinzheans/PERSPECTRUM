/// <summary>
/// Declares an object to be convertable to another type.
/// </summary>
/// <typeparam name="T">Type to be converted to</typeparam>
public interface IConvertable<T>
{
    /// <summary>
    /// Converts the current object to another type without modifying the current object. <br></br>
    /// Returns false if conversion fails.
    /// </summary>
    /// <param name="converted"></param>
    /// <returns></returns>
    public bool Convert(out T converted);
}
