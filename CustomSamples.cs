using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;

namespace SampleQueries
{
    [Title("LINQ Query Samples")]
    [Prefix("Linq")]
    public class CustomSamples : SampleHarness
    {
        LinqSamples list = new LinqSamples();

        [Category("Category1")]
        [Title("Title1")]
        [Description("Получить список всех клиентов, сумма всех заказов которых превосходит некоторую заданную величину.")]
        public void LinqQuery1()
        {
            var t = list.GetCustomerList();
            decimal x = 100000;
            var customersList = t
                .Where(c => c.Orders.Sum(o => o.Total) > x)
                .Select(c => new
                {
                    CustomerId = c.CustomerID,
                    TotalSum = c.Orders.Sum(o => o.Total)
                });

            Console.WriteLine($"Greater than {x}");

            foreach (var customer in customersList)
            {
                Console.WriteLine($"CustomerId = {customer.CustomerId} TotalSum = {customer.TotalSum}");
            }
        }

        [Category("Category2")]
        [Title("Title2")]
        [Description("Для каждого клиента получить список поставщиков, находящихся в той же стране и том же городе. Задание выполнить, как используя операцию группировки, так и без нее.")]
        public void LinqQuery2()
        {
            var customers = list.GetCustomerList();
            var suppliers = list.GetSupplierList();

            var customersWithSuppliers = customers
                .Select(c => new
                {
                    Customer = c,
                    Suppliers = suppliers.Where(s => s.City == c.City && s.Country == c.Country)
                });

            Console.WriteLine("Select\n");
            foreach (var customer in customersWithSuppliers)
            {
                Console.WriteLine($"{customer.Customer.CustomerID} " + $"===>Suppliers: {string.Join(", ", customer.Suppliers.Select(s => s.SupplierName))}");
            }

            var result = customers.GroupJoin(suppliers,
                c => new { c.City, c.Country },
                s => new { s.City, s.Country },
                (c, s) => new { Customer = c, Suppliers = s });

            Console.WriteLine("GroupGoin\n");
            foreach (var c in result)
            {
                Console.WriteLine($"CustomerId: {c.Customer.CustomerID} " + $"===>Suppliers: {string.Join(", ", c.Suppliers.Select(s => s.SupplierName))}");
            }
        }

        [Category("Category3")]
        [Title("Title3")]
        [Description("Получить список тех клиентов, заказы которых превосходят по сумме заданную величину.")]
        public void LinqQuery3()
        {
            var customers = list.GetCustomerList();

            decimal x = 10000;

            var query = customers.Where(c => c.Orders.Any(s => s.Total > x));

            foreach (var c in query)
            {
                Console.WriteLine($"{c.CustomerID}");
            }
        }

        [Category("Category4")]
        [Title("Title4")]
        [Description("Получить список всех клиентов в отсортированном виде по году, месяцу певого заказа клиента, оборотам клиента (от максимального к минимальному) и имени клиента")]
        public void LinqQuery4()
        {
            var customers = list.GetCustomerList();
            var query = customers.Where(c => c.Orders.Any())
                 .Select(c => new
                 {
                     CustomerId = c.CustomerID,
                     StartDate = c.Orders.OrderBy(o => o.OrderDate).Select(o => o.OrderDate).First(),
                     TotalSum = c.Orders.Sum(o => o.Total)
                 }).OrderByDescending(c => c.StartDate.Year)
                 .ThenByDescending(c => c.StartDate.Month)
                 .ThenByDescending(c => c.TotalSum)
                 .ThenByDescending(c => c.CustomerId);

            foreach (var c in query)
            {
                Console.WriteLine($"CustomerId = {c.CustomerId} TotalSum: {c.TotalSum} " +
                    $"Month = {c.StartDate.Month} Year = {c.StartDate.Year}");
            }
        }

        [Category("Category5")]
        [Title("Title5")]
        [Description("Получить список тех клиентов, у которых указан нецифровой почтовый код или не заполнен регион или в телефоне не указан код оператора (что равнозначно «нет круглых скобок в начале»).")]
        public void LinqQuery5()
        {
            var customers = list.GetCustomerList();
            var query = customers.Where(
                c => c.PostalCode != null && c.PostalCode.Any(sym => sym < '0' || sym > '9')
                    || string.IsNullOrWhiteSpace(c.Region)
                    || c.Phone.FirstOrDefault() != '(');

            foreach (var c in customers)
            {
                Console.WriteLine(c.CustomerID);
            }
        }

        [Category("Category6")]
        [Title("Title6")]
        [Description("Сгруппировать все продукты по категориям, внутри – по наличию на складе, внутри последней группы - по стоимости..")]
        public void LinqQuery6()
        {
            var products = list.GetProductList();
            var query = products
                .GroupBy(p => p.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    ProductsByStock = g.GroupBy(p => p.UnitsInStock > 0)
                        .Select(a => new
                        {
                            HasInStock = a.Key,
                            Products = a.OrderBy(prod => prod.UnitPrice)
                        })
                });

            foreach (var productsByCategory in query)
            {
                Console.WriteLine(($"Category: {productsByCategory.Category}\n"));
                foreach (var productsByStock in productsByCategory.ProductsByStock)
                {
                    Console.WriteLine(($"\tHas in stock: {productsByStock.HasInStock}"));
                    foreach (var product in productsByStock.Products)
                    {
                        Console.WriteLine(($"\t\tProduct: {product.ProductName} Price: {product.UnitPrice}"));
                    }
                }
            }
        }

        [Category("Category7")]
        [Title("Title7")]
        [Description("Сгруппировать все товары по группам «дешевые», «средняя цена», «дорогие», определив границы каждой группы произвольным образом.")]
        public void LinqQuery7()
        {
            var products = list.GetProductList();
            decimal lowAverageBoundary = 50;
            decimal averageExpensiveBoundary = 80;

            var productGroups = products
                .GroupBy(p => p.UnitPrice < lowAverageBoundary ? "Cheap"
                    : p.UnitPrice < averageExpensiveBoundary ? "Average price" : "Expensive");

            foreach (var group in productGroups)
            {
                Console.WriteLine(($"{group.Key}:"));
                foreach (var product in group)
                {
                    Console.WriteLine(($"\tProduct: {product.ProductName} Price: {product.UnitPrice}\n"));
                }
            }
        }

        [Category("Category8")]
        [Title("Title8")]
        [Description("Рассчитать среднюю сумму заказа по всем клиентам из данного города и среднее количество заказов, приходящееся на клиента из каждого города.")]
        public void LinqQuery8()
        {
            var customers = list.GetCustomerList();
            var query = customers
              .GroupBy(c => c.City)
              .Select(c => new
              {
                  City = c.Key,
                  Intensity = c.Average(p => p.Orders.Length),
                  AverageIncome = c.Average(p => p.Orders.Sum(o => o.Total))
              });

            foreach (var group in query)
            {
                Console.WriteLine($"City: {group.City}");
                Console.WriteLine($"\tIntensity: {group.Intensity}");
                Console.WriteLine($"\tAverage Income: {group.AverageIncome}");
            }
        }

        [Category("Category9")]
        [Title("Title9")]
        [Description("")]
        public void LinqQuery9()
        {
            var customers = list.GetCustomerList();
            var query = customers
               .Where(c => c.Orders.Any(x => x.Total > 500));

            foreach (var group in query)
            {
                Console.WriteLine($"Id: {group.CustomerID} + Country: {group.Country}");
            }
        }

        [Category("Category10")]
        [Title("Title10")]
        [Description("")]
        public void LinqQuery10()
        {
            var customers = list.GetProductList();
            var query = customers
               .Where(a => a.ProductID> 12)
               .Select(c => new
               {
                   c.ProductID, c.ProductName
               });              

            foreach (var group in query)
            {
                Console.WriteLine($"Id: {group.ProductID} + ProductName: {group.ProductName}");
            }
        }

        [Category("Category11")]
        [Title("Title11")]
        [Description("")]
        public void LinqQuery11()
        {
            var customers = list.GetSupplierList();
            var query = customers
               .GroupBy(a => a.City)
               .Select(c => new
               {
                   c.Key
               });

            foreach (var group in query)
            {
                Console.WriteLine($"Id: {group.Key}");
            }
        }

        [Category("Category12")]
        [Title("Title12")]
        [Description("")]
        public void LinqQuery12()
        {
            var products = list.GetProductList();

            var query =
                from prod in products
                where prod.UnitPrice > 50
                orderby prod.UnitPrice
                select prod;

            foreach (var group in query)
            {
                Console.WriteLine($"{group.ProductName} {group.UnitPrice}");
            }
        }

        [Category("Category13")]
        [Title("Title13")]
        [Description("")]
        public void LinqQuery13()
        {
            var customers = list.GetCustomerList();

            var query =
                from cust in customers
                from order in cust.Orders
                where order.Total > 1200
                select new { cust.CompanyName, order.OrderID, order.Total };

            foreach (var group in query)
            {
                Console.WriteLine($"{group.CompanyName} {group.OrderID} {group.Total}");
            }
        }
    }
}
