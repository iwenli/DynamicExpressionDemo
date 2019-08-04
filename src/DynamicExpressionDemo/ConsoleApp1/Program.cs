using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ConsoleApp1
{
    //参考
    //https://blog.csdn.net/sdnjiejie65/article/details/18798541
    //https://blog.csdn.net/Shiyaru1314/article/details/49508181

    class Program
    {
        static void Main(string[] args)
        {
            ////(a,b)=>(a+b)
            ////参数的构建  (定义参数的名字和参数的类型)
            //ParameterExpression exp1 = Expression.Parameter(typeof(int), "a");
            //ParameterExpression exp2 = Expression.Parameter(typeof(int), "b");
            ////表达式主体的构建 
            //BinaryExpression exp = Expression.Add(exp1, exp2);
            ////表达式树的构建（如下定义，表达式的类型为Lambda 
            ////lambda表达式的类型为Func<int, int, int>）
            //var lambda = Expression.Lambda<Func<int, int, int>>(exp, exp1, exp2);

            //p=>p.Name 可以动态构建OrderBy
            var orderByExperssion = ExpressionExtension.CreateOrderByExpression<Person>("age");
            var orderByList = Person.Data.OrderBy(orderByExperssion.Compile()).ToList();
            Print(orderByList, "age 升序");

            var orderByExperssion1 = ExpressionExtension.GenerateOrderExpression<Person>("id");
            var OrderByList1 = orderByList.OrderByDescending(orderByExperssion1.Compile()).ToList();
            Print(OrderByList1, "id 降序");

            FilterCollection filters = new FilterCollection();
            filters.Add(new List<Filter> { new Filter("age", 10) ,new Filter("id", 4)});
            var whereExperssion = ExpressionExtension.GenerateQueryExpression<Person>(filters);
            var data = Person.Data.Where(whereExperssion.Compile());
            Print(data, "age==10 || id==4");

            FilterCollection filters1 = new FilterCollection();
            filters1.Add(new List<Filter> { new Filter("age", 10) });
            filters1.Add(new List<Filter> { new Filter("id", 4) });
            var whereExperssion1 = ExpressionExtension.GenerateQueryExpression<Person>(filters1);
            var data1 = Person.Data.Where(whereExperssion1.Compile());
            Print(data1, "age==10 && id==4");

            

            FilterCollection filters2 = new FilterCollection();
            filters2.Add(new List<Filter> { new Filter("name","赵",Op.Contains) });
            var whereExperssion2 = ExpressionExtension.GenerateQueryExpression<Person>(filters2);
            var data2 = Person.Data.Where(whereExperssion2.Compile());
            Print(data2, "名字包含赵");

            //Person.Data.Where()
            ////p=>p.Name == "daisy" 
            //var compareExp = simpleCompare<Person>("Name", "daisy");
            //var daisys = Person.Data.Where(compareExp).ToList();
            //foreach (var item in daisys)
            //{
            //    Console.WriteLine("Name:  " + item.Name + "    Age:  " + item.Age);
            //}
            //Console.ReadKey();
        }
        public static Func<TSource, bool> simpleCompare<TSource>(string property, object value)
        {
            var type = typeof(TSource);
            var pe = Expression.Parameter(type, "p");
            var propertyReference = Expression.Property(pe, property);
            var constantReference = Expression.Constant(value);

            //compile 是表达式的一个接口，生成该lambda表达式树对的委托
            return Expression.Lambda<Func<TSource, bool>>(Expression.Equal(propertyReference, constantReference), pe).Compile();


        }

        static void Print<T>(IEnumerable<T> list, string actionName)
        {
            Console.WriteLine($"[{actionName}]后的集合如下：");
            foreach (var item in list)
            {
                Console.WriteLine(item.ToString());
            }

        }
    }
}
