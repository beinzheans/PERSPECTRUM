using System;
using System.Linq.Expressions;

public interface IEditable : ISelectable
{
    public void OnEdit<TClass, TValue>(Expression<Func<TClass, TValue>> editAction, TValue newValue);
}
