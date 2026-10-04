using System;
using System.Linq.Expressions;
using System.Reflection;

public interface IEditable
{
    /// <summary>
    /// A callback when <see cref="MathHelper.OnEdit{T, TValue}(T, Expression{Func{T, TValue}}, TValue)"/> is called.
    /// </summary>
    public void OnEditCallback();
}
