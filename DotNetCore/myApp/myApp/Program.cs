using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myApp
{
    class Program
    {
        static void Main(string[] args)
        {
            WorkingWithStrings();
        }

        public static void WorkingWithStrings()
            {
                var names = new List<string> { "Chris", "Anna", "Felipe" };
                foreach (var name in names)
                {
                    Console.WriteLine($"Hello { name.ToUpper()}!");
                }

                Console.WriteLine();
                names.Add("Maria");
                names.Add("Bill");
                names.Add("Ana");
                foreach (var name in names)
                {
                Console.WriteLine($"Hello { name.ToUpper()}!");
                }

                Console.WriteLine($"My name is {names[0]}");
                Console.WriteLine($"I've added {names[2]} and {names[3]} to the list");

                Console.WriteLine($"The list has {names.Count} people in it");

                var index = names.IndexOf("Felipe");
                Console.WriteLine($"The name {names[index]} is at index {index}");

                var notFound = names.IndexOf("Not Found");
                Console.WriteLine($"When an item is not found, IndexOf returns {notFound}");

                names.Sort();
                foreach(var name in names)
                {
                    Console.WriteLine($"Hello {name.ToUpper()}!");
                }

                var fibonacciNumbers = new List<int> { 1, 1 };
                var previous = fibonacciNumbers[fibonacciNumbers.Count - 1];
                var previous2 = fibonacciNumbers[fibonacciNumbers.Count - 2];

                fibonacciNumbers.Add(previous + previous2);

                foreach (var item in fibonacciNumbers)
                Console.WriteLine(item);
                Console.ReadLine();

            }
        }
    }

