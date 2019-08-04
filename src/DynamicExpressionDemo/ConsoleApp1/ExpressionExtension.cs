using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ConsoleApp1
{
    /// <summary>
    /// 
    /// </summary>
    public class ExpressionExtension
    {
        private static MethodInfo containsMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
        private static MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
        private static MethodInfo endsWithMethod = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });
        private static Expression GetExpression(ParameterExpression param, Filter filter)
        {
            MemberExpression member = Expression.Property(param, filter.PropertyName);
            // Expression handledMember = member;
            ConstantExpression constant = Expression.Constant(filter.Value);

            // if (member.Member.MemberType == MemberTypes.Property)
            // {
            //     Type propertyType = ((PropertyInfo)member.Member).PropertyType;
            //     if (propertyType == typeof(string))
            //     {
            //         handledMember = Expression.Call(member, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
            //     }
            //     if (propertyType == typeof(DateTime?))
            //     {
            //         handledMember = Expression.Property(member, typeof(DateTime?).GetProperty("Value"));
            //     }
            // }

            switch (filter.Operation)
            {
                case Op.Equals:
                    return Expression.Equal(member, constant);
                case Op.GreaterThan:
                    return Expression.GreaterThan(member, constant);
                case Op.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(member, constant);
                case Op.LessThan:
                    return Expression.LessThan(member, constant);
                case Op.LessThanOrEqual:
                    return Expression.LessThanOrEqual(member, constant);
                case Op.Contains:
                    return Expression.Call(member, containsMethod, constant);
                case Op.StartsWith:
                    return Expression.Call(member, startsWithMethod, constant);
                case Op.EndsWith:
                    return Expression.Call(member, endsWithMethod, constant);
            }

            return null;
        }
        private static BinaryExpression GetORExpression(ParameterExpression param, Filter filter1, Filter filter2)
        {
            Expression bin1 = GetExpression(param, filter1);
            Expression bin2 = GetExpression(param, filter2);

            return Expression.Or(bin1, bin2);
        }

        private static Expression GetExpression(ParameterExpression param, IList<Filter> orFilters)
        {
            if (orFilters.Count == 0)
                return null;

            Expression exp = null;

            if (orFilters.Count == 1)
            {
                exp = GetExpression(param, orFilters[0]);
            }
            else if (orFilters.Count == 2)
            {
                exp = GetORExpression(param, orFilters[0], orFilters[1]);
            }
            else
            {
                while (orFilters.Count > 0)
                {
                    var f1 = orFilters[0];
                    var f2 = orFilters[1];

                    if (exp == null)
                    {
                        exp = GetORExpression(param, orFilters[0], orFilters[1]);
                    }
                    else
                    {
                        exp = Expression.Or(exp, GetORExpression(param, orFilters[0], orFilters[1]));
                    }
                    orFilters.Remove(f1);
                    orFilters.Remove(f2);

                    if (orFilters.Count == 1)
                    {
                        exp = Expression.Or(exp, GetExpression(param, orFilters[0]));
                        orFilters.RemoveAt(0);
                    }
                }
            }

            return exp;
        }

        /// <summary>
        /// 条件表达式树生成
        /// list<filter>内or  多个list<filter>and
        /// </summary>
        /// <param name="filters"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> GenerateQueryExpression<T>(FilterCollection filters)
        {
            if (filters == null || filters.Count == 0)
                return null;
            //var property = typeof(T).GetProperty(propertyName,
            //    BindingFlags.Public
            //    | BindingFlags.Instance
            //    | BindingFlags.IgnoreCase);

            ParameterExpression param = Expression.Parameter(typeof(T), "m");
            Expression exp = null;

            if (filters.Count == 1)
            {
                exp = GetExpression(param, filters[0]);
            }
            else if (filters.Count == 2)
            {
                exp = Expression.AndAlso(GetExpression(param, filters[0]), GetExpression(param, filters[1]));
            }
            else
            {
                while (filters.Count > 0)
                {
                    var f1 = filters[0];
                    var f2 = filters[1];
                    var f1Andf2 = Expression.AndAlso(GetExpression(param, filters[0]), GetExpression(param, filters[1]));
                    if (exp == null)
                    {
                        exp = f1Andf2;
                    }
                    else
                    {
                        exp = Expression.AndAlso(exp, f1Andf2);
                    }

                    filters.Remove(f1);
                    filters.Remove(f2);

                    if (filters.Count == 1)
                    {
                        exp = Expression.AndAlso(exp, GetExpression(param, filters[0]));
                        filters.RemoveAt(0);
                    }
                }
            }

            return Expression.Lambda<Func<T, bool>>(exp, param);
        }

        /// <summary>
        /// 生成排序Expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Expression<Func<T, object>> GenerateOrderExpression<T>(string propertyName)
        {
            //var property = typeof(T).GetProperty(propertyName,
            //    BindingFlags.Public
            //    | BindingFlags.Instance
            //    | BindingFlags.IgnoreCase);

            //if (property == null)
            //{
            //    throw new Exception($"类型中不存在名称为{propertyName}的属性");
            //}
            var parameter = Expression.Parameter(typeof(T), "m");
            var propertyExpression = Expression.Property(parameter, propertyName);
            var converted = Expression.Convert(propertyExpression, typeof(object));
            return Expression.Lambda<Func<T, object>>(converted, parameter);
        }

        /// <summary>
        /// 生成映射Expression
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Expression<Func<T, TResult>> GenerateSelectExpression<T, TResult>(IEnumerable<string> properties)
        {
            var source = Expression.Parameter(typeof(T), "source");
            var result = Expression.Variable(typeof(TResult), "result");
            var propertyList = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var binaryExpressions = new List<BinaryExpression>();
            foreach (var property in propertyList)
            {
                if (properties.Any(m => property.Name.Equals(m, StringComparison.CurrentCultureIgnoreCase)))
                {
                    var resultProp = typeof(TResult).GetProperty(property.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (resultProp != null && resultProp.CanWrite)
                    {
                        binaryExpressions.Add(Expression.Assign(Expression.Property(result, resultProp), Expression.Property(source, property)));
                    }
                }
            }
            var body = new List<Expression>();
            body.Add(Expression.Assign(result, Expression.New(typeof(TResult))));
            body.AddRange(binaryExpressions);
            body.Add(result);
            var expr = Expression.Lambda<Func<T, TResult>>(Expression.Block(new[] { result }, body.ToArray()), source);
            return expr;
        }
    }

    public class FilterCollection : Collection<IList<Filter>>
    {
        public FilterCollection()
            : base()
        { }
    }

    public class Filter
    {
        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <param name="op"></param>
        public Filter(string propertyName, object value, Op op = Op.Equals)
        {
            PropertyName = propertyName;
            Value = value;
            Operation = op;
        }
        public string PropertyName { get; set; }
        public Op Operation { get; set; }
        public object Value { get; set; }
    }

    /// <summary>
    /// 操作类型
    /// </summary>
    public enum Op
    {
        /// <summary>
        /// ==
        /// </summary>
        Equals,
        /// <summary>
        /// >
        /// </summary>
        GreaterThan,
        /// <summary>
        /// <
        /// </summary>
        LessThan,
        /// <summary>
        /// >=
        /// </summary>
        GreaterThanOrEqual,
        /// <summary>
        /// <=
        /// </summary>
        LessThanOrEqual,
        /// <summary>
        /// 包含
        /// </summary>
        Contains,
        /// <summary>
        /// 指定值开始
        /// </summary>
        StartsWith,
        /// <summary>
        ///指定值结尾
        /// </summary>
        EndsWith
    }
}
