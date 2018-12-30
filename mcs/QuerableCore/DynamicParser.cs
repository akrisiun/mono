
using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;

namespace Mono.Linq
{
    // https://stackoverflow.com/questions/821365/how-to-convert-a-string-to-its-equivalent-linq-expression-tree
    // https://github.com/StefH/System.Linq.Dynamic.Core
    public static class DynamicParser {

        public static LambdaExpression Lambda(string cstext, params object[] parm) 
        {
            // public static LambdaExpression Lambda(Expression body, params );
            var parameters = new ParameterExpression[] { };

            LambdaExpression exp = DynamicExpressionParser.ParseLambda(parameters, null, cstext);
            return exp;
        }

        public static object Run(this LambdaExpression exp, params ParameterExpression[] parm)
        {
            object result = null;
            LambdaExpression efn = null;
            var plan = exp as Expression<Func<object>>;
            if (plan != null) {
                efn = Expression.Lambda<Func<object>>(
                      Expression.Convert(plan, typeof(object)));
            } else {
                efn = exp;
            }
            
            var run = efn?.Compile();
            if (run is Delegate) {
                result = run.DynamicInvoke(parm);
            }

            return result;
        }

        public static Expression<object> Lambda<T>(string cstext, T p1, params object[] parm) 
        {
            var exp_p1 = Expression.Parameter(typeof(T), "p1");
            var parameters = new ParameterExpression[] { exp_p1 };

            var exp = DynamicExpressionParser.ParseLambda(parameters, null, cstext);
            return exp as Expression<object>;
        }

        public static object RunFunc<T>(this Expression<T> plan, params object[] parm) 
        {
            Expression<Func<object>> efn;
            efn = Expression.Lambda<Func<object>>(
                  Expression.Convert(plan, typeof(object)));

            Func<object> fn = efn.Compile();

            return fn();
        }
    }
}