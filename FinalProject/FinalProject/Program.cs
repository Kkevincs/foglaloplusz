using System;
using MySql.Data.MySqlClient;

namespace FinalProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = new MySqlConnectionStringBuilder();
            builder.Server = "81.182.153.15";
            builder.UserID = "dori";
            builder.Password = "Kiskutya3";
            builder.Database = "Salonic";

            using (var connection = new MySqlConnection(builder.ConnectionString))
            {
                try
                {
                    Console.WriteLine("Pls connect");
                    connection.Open();
                    Console.WriteLine("Connection is successfull.");

                    string createTablesSql = @"
                        CREATE TABLE IF NOT EXISTS Users (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            email VARCHAR(255) NOT NULL UNIQUE,
                            userName VARCHAR(255) NOT NULL
                        );

                        CREATE TABLE IF NOT EXISTS Workers (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            email VARCHAR(255) NOT NULL UNIQUE,
                            workerName VARCHAR(255) NOT NULL
                        );

                        CREATE TABLE IF NOT EXISTS Services (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            serviceName VARCHAR(255) NOT NULL,
                            available BOOL NOT NULL DEFAULT 0
                        );

                        CREATE TABLE IF NOT EXISTS Worker_Services (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            workerId INT NOT NULL,
                            serviceId INT NOT NULL,
                            price INT NOT NULL,
                            timeToComplete INT NOT NULL COMMENT 'time in minutes',
                            FOREIGN KEY (workerId) REFERENCES Workers(id) ON DELETE CASCADE,
                            FOREIGN KEY (serviceId) REFERENCES Services(id) ON DELETE CASCADE
                        );

                        CREATE TABLE IF NOT EXISTS Appointments (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            userId INT NOT NULL,
                            serviceId INT NOT NULL,
                            appointmentDate DATETIME NOT NULL,
                            FOREIGN KEY (userId) REFERENCES Users(id) ON DELETE CASCADE,
                            FOREIGN KEY (serviceId) REFERENCES Services(id) ON DELETE CASCADE
                        );";

                    using (var command = new MySqlCommand(createTablesSql, connection))
                    {
                        Console.WriteLine("DO IT...");
                        command.ExecuteNonQuery();
                        Console.WriteLine("Tables created successfully:)");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"This is the error: {ex.Message}");
                }
            }
        }
    }
}

