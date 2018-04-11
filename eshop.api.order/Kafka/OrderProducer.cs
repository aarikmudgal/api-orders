using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;

namespace eshop.api.order.Kafka
{
    public class OrderProducer : IOrderProducer
    {
        private const string articlesTopic = "articles-topic";
        public void Produce(string message)
        {
            var config = new Dictionary<string, object>
            {
                {"bootstrap.servers", "10.160.0.8:9092" },
                {"debug", "cgrp" }
            };
            try
            {
                using (var producer = new Producer<Null, string>(config, null, new StringSerializer(Encoding.UTF8)))
                {
                    producer.OnLog += Loggers.ConsoleLogger;
                    producer.OnError += Producer_OnError;

                    var result = producer.ProduceAsync(articlesTopic, null, message).Result;
                    Console.WriteLine($"----- Delivered '{result.Value}' to: {result.TopicPartitionOffset}");
                };
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message + ". Inner Exception - " + ex.InnerException.Message);
            }

        }

        private void Producer_OnError(object sender, Error e)
        {
            Console.WriteLine($"ERROR ****** Error code: {e.Code}, Reason: {e.Reason} ***** ");
        }

    }
}
