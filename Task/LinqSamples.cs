// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
    [Title("LINQ Module")]
    [Prefix("Linq")]
    public class LinqSamples : SampleHarness
    {

        private DataSource dataSource = new DataSource();

        [Category("Linq Operators")]
        [Title("Task 1")]
        [Description("Выдайте список всех клиентов, чей суммарный оборот (сумма всех заказов) превосходит некоторую величину X. " +
                     "Продемонстрируйте выполнение запроса с различными X (подумайте, можно ли обойтись без копирования запроса несколько раз)")]

        public void Linq1()
        {
            int value = 10000;
            IEnumerable<Customer> customers = new List<Customer>();
            do
            {

                customers = dataSource.Customers.Where(c => c.Orders.Sum(order => order.Total) > value);

                ObjectDumper.Write("more than " + value);

                foreach (var c in customers)
                {
                    ObjectDumper.Write(c.Orders.Sum(order => order.Total));
                }
                value *= 2;
                ObjectDumper.Write("++++++++++++++++++++++++++++++++++++++++++");
            } while (customers.Count() != 0);
        }


        [Category("Linq Operators")]
        [Title("Task 2")]
        [Description("Для каждого клиента составьте список поставщиков, находящихся в той же стране и том же городе. Сделайте задания с использованием группировки и без.")]

        public void Linq2()
        {
            var customersWithSuppliers = dataSource.Customers.
                GroupJoin(
                    dataSource.Suppliers,
                    e => new { e.City, e.Country },
                    o => new { o.City, o.Country },
                    (e, o) => new
                    {
                        customer = e.CompanyName,
                        city = e.City,
                        suppliers = o
                    });


            foreach (var c in customersWithSuppliers)
            {
                ObjectDumper.Write("Customer: " + c.customer + "(City: " + c.city + ")");
                foreach (var e in c.suppliers)
                {
                    ObjectDumper.Write("Supplier: " + e.SupplierName);
                }
                ObjectDumper.Write("*****");
            }

        }

        [Category("Linq Operators")]
        [Title("Task 3")]
        [Description("Найдите всех клиентов, у которых были заказы, превосходящие по сумме величину X")]

        public void Linq3()
        {
            int value = 2000;
            var customers = dataSource.Customers.Where(c => c.Orders.Any(o => o.Total > value));
            foreach (var c in customers)
            {
                ObjectDumper.Write("Customer: " + c.CustomerID);
            }
        }


        [Category("Linq Operators")]
        [Title("Task 4")]
        [Description("Выдайте список клиентов с указанием, начиная с какого месяца какого года они стали клиентами (принять за таковые месяц и год самого первого заказа)")]

        public void Linq4()
        {
            var customerWithDates = dataSource.Customers.
                Select(c => new
                {
                    customer = c.CustomerID,
                    startDate = c.Orders.Any() ? c.Orders.Min(o => o.OrderDate) : new DateTime()
                });

            foreach (var c in customerWithDates)
            {
                ObjectDumper.Write("Customer: " + c.customer + " is a client since " + c.startDate.Month + "th month " + c.startDate.Year + " year");
            }

        }

        [Category("Linq Operators")]
        [Title("Task 5")]
        [Description("Сделайте предыдущее задание, но выдайте список отсортированным по году, месяцу," +
                     " оборотам клиента (от максимального к минимальному) и имени клиента")]

        public void Linq5()
        {
            var customerWithDates = dataSource.Customers.
                Select(c => new
                {
                    customer = c.CustomerID,
                    startDate = c.Orders.Any() ? c.Orders.Min(o => o.OrderDate) : new DateTime(),
                    sumOfOrders = c.Orders.Sum(t => t.Total)
                });
            var orderedCustomers = customerWithDates.OrderBy(c => c.startDate.Year)
                .ThenBy(c => c.startDate.Month)
                .ThenByDescending(c => c.sumOfOrders)
                .ThenBy(c => c.customer);

            foreach (var c in orderedCustomers)
            {
                ObjectDumper.Write("Customer: " + c.customer + " " + c.startDate + " " + c.sumOfOrders);
            }

        }

        [Category("Linq Operators")]
        [Title("Task 6")]
        [Description("Укажите всех клиентов, у которых указан нецифровой почтовый код или не заполнен регион " +
                     "или в телефоне не указан код оператора(для простоты считаем, что это равнозначно - нет круглых скобочек в начале")]

        public void Linq6()
        {
            var customerWithDates = dataSource.Customers
                .Where((c, e) =>
                int.TryParse(c.PostalCode, out e) == false
                || string.IsNullOrEmpty(c.Region)
                || !c.Phone.StartsWith("("));

            foreach (var c in customerWithDates)
            {
                ObjectDumper.Write("Customer: " + c.CustomerID + " *** " + c.PostalCode + " *** " + c.Region + " *** " + c.Phone);
            }

        }

        [Category("Linq Operators")]
        [Title("Task 7")]
        [Description("Сгруппируйте все продукты по категориям, внутри – по наличию на складе, внутри последней группы отсортируйте по стоимости")]

        public void Linq7()
        {
            var products = dataSource.Products.GroupBy(x => x.Category, (key, g1) => new
            {
                Category = key,
                StockGroups = g1.GroupBy(x => x.UnitsInStock, (key2, g2) => new
                {
                    UnitsInStock = key2,
                    Products = g2.OrderBy(z => z.UnitPrice)
                })

            });
            foreach (var p in products)
            {
                ObjectDumper.Write("Category : " + p.Category);
                foreach (var s in p.StockGroups)
                {
                    ObjectDumper.Write("StockGroups : " + s.UnitsInStock);
                    foreach (var product in s.Products)
                    {
                        ObjectDumper.Write("Product : " + product.ProductName);
                    }
                }
            }
        }

        [Category("Linq Operators")]
        [Title("Task 8")]
        [Description("Сгруппируйте товары по группам «дешевые», «средняя цена», «дорогие». Границы каждой группы задайте сами")]

        public void Linq8()
        {
            Func<Product, string> getGroup =
                product => product.UnitPrice < 10.0000M ? "cheap" : product.UnitPrice < 20.0000M ? "middle" : "expensive";
            var groupedProducts = dataSource.Products.GroupBy(p => getGroup(p));
            foreach (var group in groupedProducts)
            {
                ObjectDumper.Write("Group : " + group.Key);
                foreach (var product in group)
                {
                    ObjectDumper.Write("Product : " + product.ProductName + " with price : " + product.UnitPrice);
                }
            }

        }

        [Category("Linq Operators")]
        [Title("Task 9")]
        [Description("Рассчитайте среднюю прибыльность каждого города (среднюю сумму заказа по всем клиентам из данного города) " +
                     "и среднюю интенсивность (среднее количество заказов, приходящееся на клиента из каждого города)")]

        public void Linq9()
        {
            var res = dataSource.Customers.GroupBy(c => c.City, (key, g1) => new
            {
                city = key,
                averagePrice = g1.Average(p => p.Orders.Sum(s => s.Total)),
                averageNumerOfOrders = g1.Average(w => w.Orders.Count())
            });

            foreach (var r in res)
            {
                ObjectDumper.Write("City : " + r.city);
                ObjectDumper.Write("Average price : " + r.averagePrice);
                ObjectDumper.Write("Average number of orders : " + r.averageNumerOfOrders);
            }
        }

        [Category("Linq Operators")]
        [Title("Task 10")]
        [Description("Сделайте среднегодовую статистику активности клиентов по месяцам (без учета года), статистику по годам," +
                     " по годам и месяцам (т.е. когда один месяц в разные годы имеет своё значение")]

        public void Linq10()
        {
            //по месяцам (без учета года)
            ObjectDumper.Write("по месяцам (без учета года)");
            var monthsStatistic = dataSource.Customers
                .SelectMany(c => c.Orders
                .Select(o => new
                {
                    order = o,
                    month = o.OrderDate.Month
                }))
                .GroupBy(e => e.month, (key, orders) => new
                {
                    month = key, ordersNumber = orders.Count()
                });

            foreach (var e in monthsStatistic)
            {
                ObjectDumper.Write("month : " + e.month);
                ObjectDumper.Write("count : " + e.ordersNumber);
            }
            ObjectDumper.Write("******************************");

            //по годам 
            ObjectDumper.Write("по годам");
            var yearsStatistic = dataSource.Customers
                .SelectMany(c => c.Orders
                .Select(o => new
                {
                    order = o,
                    year = o.OrderDate.Year
                }))
                .GroupBy(e => e.year, (key, orders) => new
                {
                    year = key,
                    ordersNumber = orders.Count()
                });

            foreach (var e in yearsStatistic)
            {
                ObjectDumper.Write("month : " + e.year);
                ObjectDumper.Write("count : " + e.ordersNumber);
            }

            ObjectDumper.Write("******************************");

            //по годам и месяцам (т.е.когда один месяц в разные годы имеет своё значение
            ObjectDumper.Write("по годам и месяцам (т.е.когда один месяц в разные годы имеет своё значение)");
            var monthsAndYearsStatistic = dataSource.Customers
               .SelectMany(c => c.Orders
               .Select(o => new
               {
                   order = o,
                   monthAndYear = new
                   {
                       month = o.OrderDate.Month,
                       year = o.OrderDate.Year
                   }
                   
               }))
               .GroupBy(e => e.monthAndYear, (key, orders) => new
               {
                   monthAndYear = key,
                   ordersNumber = orders.Count()
               });

            foreach (var e in monthsAndYearsStatistic)
            {
                ObjectDumper.Write("month : " + e.monthAndYear.month + " year : " + e.monthAndYear.year);
                ObjectDumper.Write("count : " + e.ordersNumber);
            }
            ObjectDumper.Write("******************************");


        }

    }
}
