using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using static System.Console;

namespace EffectivMobile
{
    public class Program
    {
        static void Main(string[] args)
        {
            string dataFilePath = "orders.txt"; // файл заказов
            string logFilePath = "delivery_log.txt"; // файл логов
            string resultFilePath = "filtered_orders.txt"; // файл вывода
            try
            {
                Write("Введите название района: ");
                string cityDistrict = ReadLine();
                
                DateTime startDeliveryDateTime;
                while (true)
                {
                    Write("Введите с какого времени выбрать заказы? (yyyy-MM-dd HH:mm:ss): ");
                    string input = ReadLine();
                    if (DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDeliveryDateTime))
                    {
                        break;
                    }
                    WriteLine("Неккоректный формат, попробуйте  еще раз.");
                }
                DateTime endDeliveryDateTime = startDeliveryDateTime.AddMinutes(30);
                
                LogOperation(logFilePath, "Старт логирования программы.");
                List<Order> orders = ReadOrders(dataFilePath);
                if (orders.Count == 0)
                {
                    LogOperation(logFilePath, "не найдено заказов.");
                    return;
                }
                LogOperation(logFilePath, $"Количество всех заказов: {orders.Count}");
                LogOperation(logFilePath, $"Заказы для района '{cityDistrict}' отфильтрованы с '{startDeliveryDateTime:yyyy-MM-dd HH:mm:ss}' до '{endDeliveryDateTime:yyyy-MM-dd HH:mm:ss}'");

                var filteredOrders = FilterOrders(orders, cityDistrict, startDeliveryDateTime, endDeliveryDateTime);
                int filteredCount = filteredOrders.Count;
                
                LogOperation(logFilePath, $"Количество найденных заказов за промежуток времени для'{cityDistrict}': {filteredCount}");
                if (filteredCount == 0)
                {
                    LogOperation(logFilePath, "В данном промежутке не найдено заказов.");
                }

                WriteFilteredOrders(resultFilePath, filteredOrders); //запись в файл

                LogOperation(logFilePath, "ЗАвершение логирования программы!");
                WriteLine($"Процесс фильтрации завершен, отфильтрованные данные храняться в {dataFilePath}");
            }
            catch (Exception ex)
            {
                LogOperation(logFilePath, "Произошла ошибка: " + ex.Message);
            }
        }
            

        public static List<Order> ReadOrders(string filePath)
        {
            List<Order> orders = new List<Order>();
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 4 && double.TryParse(parts[1].Replace('.', ','), out double weight) && DateTime.TryParseExact(parts[3], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime deliveryTime))
                    {
                        orders.Add(new Order
                        {
                            OrderID = parts[0],
                            Weight = weight,
                            District = parts[2],
                            DeliveryTime = deliveryTime
                        });
                    }
                    else
                    {
                        LogOperation("delivery_log.txt", "Неверный формат заказов: " + line);
                    }
                }
            }
            catch (Exception ex)
            {
                LogOperation("delivery_log.txt", "Ошибка чтения: " + ex.Message);
            }
            return orders;
        }

        public static List<Order> FilterOrders(List<Order> orders, string district, DateTime startDeliveryTime, DateTime endDeliveryTime)
        {
            return orders.Where(order => order.District.Equals(district, StringComparison.OrdinalIgnoreCase) && order.DeliveryTime >= startDeliveryTime && order.DeliveryTime <= endDeliveryTime).ToList();
        }

        public static void WriteFilteredOrders(string filePath, List<Order> orders)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    if (orders.Count == 0)
                    {
                        writer.WriteLine("Не найдено заказов за указанное время или в таком районе.");
                    }
                    else
                    {
                        foreach (var order in orders)
                        {
                            writer.WriteLine($"{order.OrderID},{order.Weight},{order.District},{order.DeliveryTime:yyyy-MM-dd HH:mm:ss}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogOperation("delivery_log.txt", "ошибка записи файла: " + ex.Message);
            }
        }

        public static void LogOperation(string filePath, string message)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                }
            }
            catch (Exception ex)
            {
                WriteLine("Ошибка записи логов: " + ex.Message);
            }
        }
    }

    public class Order
    {
        public string OrderID { get; set; } // Идентификатор заказа
        public double Weight { get; set; } // вес заказа
        public string District { get; set; } // район заказа
        public DateTime DeliveryTime { get; set; } // время заказа
    }
}   
