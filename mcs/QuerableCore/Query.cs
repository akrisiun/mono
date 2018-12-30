
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using IQToolkit;

namespace Mono.Entity
{
    public class Query<T> : IQueryable<T>, IQueryable, IQuery, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable
    {

        public IQueryProvider Provider { [DebuggerStepThrough] get; protected set; }
        public Expression Expression { [DebuggerStepThrough] get; protected set; }
        public object[] ParamValues { get; set; }

        public Query(IQueryProvider provider)
        {
            Provider = provider ?? throw new ArgumentNullException("provider");
            Expression = Expression.Constant(this);
        }
 
        public Query(IQueryProvider provider, Expression expression) 
        {
            Provider = provider ?? throw new ArgumentNullException("provider");
            Expression = expression ?? throw new ArgumentNullException("expression");
            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
                throw new ArgumentOutOfRangeException("expression");
        }

        Expression IQueryable.Expression { get => Expression; }
        IQueryProvider IQueryable.Provider { get => Provider; }
        public Type ElementType {  get => typeof(T); }

        public IEnumerator<T> GetEnumerator() {
            return ((IEnumerable<T>)Provider.Execute(Expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)Provider.Execute(Expression)).GetEnumerator();
        }

        public override string ToString() => (Provider as QueryProvider)?.GetQueryText(this.Expression) ?? this.Expression.ToString();
    }

    internal interface ILastError
    {
        Exception LastError { get; set; }

    }
    
}