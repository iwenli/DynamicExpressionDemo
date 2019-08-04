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

            var orderByExperssion = ExpressionExtension.GenerateOrderExpression<Person>("age");
            var orderByList = Person.Data.OrderBy(orderByExperssion.Compile()).ToList();
            Print(orderByList, "age 升序");

            var orderByExperssion1 = ExpressionExtension.GenerateOrderExpression<Person>("id");
            var OrderByList1 = orderByList.OrderByDescending(orderByExperssion1.Compile()).ToList();
            Print(OrderByList1, "id 降序");

            FilterCollection filters = new FilterCollection();
            filters.Add(new List<Filter> { new Filter("age", 10), new Filter("id", 4) });
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
            filters2.Add(new List<Filter> { new Filter("name", "赵", Op.Contains) });
            var whereExperssion2 = ExpressionExtension.GenerateQueryExpression<Person>(filters2);
            var data2 = Person.Data.Where(whereExperssion2.Compile());
            Print(data2, "名字包含赵");


            // ========================= select 

            // 生成表达式的区别
            //.Lambda #Lambda1<System.Func`2[ConsoleApp1.Person,ConsoleApp1.Program+Person1]>(ConsoleApp1.Person $source) {
            //.Block(ConsoleApp1.Program + Person1 $result) {
            //$result = .New ConsoleApp1.Program + Person1();
            //$result.Age = $source.Age;
            //$result.Name = $source.Name;
            //$result
            //}
            //}

            //.Lambda #Lambda1<System.Func`2[ConsoleApp1.Person,ConsoleApp1.Program+Person1]>(ConsoleApp1.Person $m) {
            //.New ConsoleApp1.Program + Person1(){
            //Age = $m.Age,
            //Name = $m.Name
            //}
            //}

            //Expression<Func<Person, Person1>> select = m => new Person1
            //{
            //    Age = m.Age,
            //    Name = m.Name
            //};

            var selectExpression = ExpressionExtension.GenerateSelectExpression<Person, Person1>
                    (new List<string> { "age", "name" });
            var selectData = Person.Data.Select(selectExpression.Compile());
            Print(selectData, "select age,name");


            Console.ReadKey();
        }
        static void Print<T>(IEnumerable<T> list, string actionName)
        {
            Console.WriteLine($"[{actionName}]后的集合如下：");
            foreach (var item in list)
            {
                Console.WriteLine(item.ToString());
            }
        }

        class Person1 : Person
        {
            public string Gid { get; set; } = Guid.NewGuid().ToString("N");

            public override string ToString()
            {
                return $"{Gid}-{Id}-{Name}-{Age}";
            }
        }
    }
}
