
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

        public override string ToString()
        {
            return $"{Id}-{Name}-{Age}";
        }

        static Person()
        {
            _data = new List<Person>() {
                new Person{Id=1, Name = "张三", Age = 10 },
                new Person{Id=2, Name = "李四", Age = 13 },
                new Person{Id=3, Name = "王五", Age = 12 },
                new Person{ Id=4,Name = "赵六", Age = 10 }
            };
        }

        static List<Person> _data;
        public static List<Person> Data
        {
            get
            {
                return _data;
            }
        }
    }
}
