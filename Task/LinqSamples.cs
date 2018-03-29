﻿// Copyright © Microsoft Corporation.  All Rights Reserved.
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
                        suppliers = o
                    });


            foreach (var c in customersWithSuppliers)
            {
                ObjectDumper.Write("Customer: " + c.customer);
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
                    startDate = c.Orders.Any() ? c.Orders.Max(o => o.OrderDate) : new DateTime()
                });

            foreach (var c in customerWithDates)
            {
                ObjectDumper.Write("Customer: " + c.customer + " " + c.startDate);
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
                    startDate = c.Orders.Any() ? c.Orders.Max(o => o.OrderDate) : new DateTime()
                });
            var orderedCustomers = customerWithDates.OrderBy(c => c.startDate.Year);
            //orderedCustomers = customerWithDates.OrderBy(c => c.startDate.Month);
            //orderedCustomers = customerWithDates.OrderBy(c => c.customer);
            foreach (var c in orderedCustomers)
            {
                ObjectDumper.Write("Customer: " + c.customer + " " + c.startDate);
            }

        }
    }
}