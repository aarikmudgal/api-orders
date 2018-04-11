using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eshop.api.order.Kafka
{
    interface IOrderProducer
    {
        void Produce(string message);
    }
}
