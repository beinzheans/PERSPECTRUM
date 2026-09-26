using System;
using System.Linq.Expressions;

public interface IEditable
{
    /// <summary>
    /// Edits a property of this object to a new value using an expression tree.
    /// </summary>
    /// <typeparam name="TClass">The class of this object</typeparam>
    /// <typeparam name="TValue">The type of the property to change</typeparam>
    /// <param name="editAction">The expression used to edit the current object. Write the property you want to target inside <typeparamref name="TClass"/>.</param>
    /// <param name="newValue">The new value assigned to the property targetted by <paramref name="editAction"/>.</param>
    public void OnEdit<TClass, TValue>(Expression<Func<TClass, TValue>> editAction, TValue newValue);
}
