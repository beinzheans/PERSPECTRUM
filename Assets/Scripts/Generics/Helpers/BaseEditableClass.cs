using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public class BaseEditableClass
{    /// <summary>
     /// Edits the current settings using an expression and invokes <see cref="GameManager.OnGameSettingsChanged"/>
     /// </summary>
     /// <typeparam name="TValue">The type of the setting to edit</typeparam>
     /// <param name="editAction">The expression tree used to edit. Write the property you want to target here.</param>
     /// <param name="newValue">The new value you want to assign to your target property.</param>

    public virtual void OnEdit<TValue>(Expression<Func<TValue>> editAction, TValue newValue)
    {
        if (editAction.Body is not MemberExpression expression)
        {
            return;
        }

        if (expression.Member is not PropertyInfo property)
        {
            return;
        }

        Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(Expression.Convert(expression.Expression, typeof(object)));
        object targetInstance = lambda.Compile()();

        property.SetValue(targetInstance, newValue);
    }

}
