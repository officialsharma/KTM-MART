using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTM_MART.Models
{
    public class TrainingSample
    {
        public int Observation { get; private set; }

        public List<String> Products { get; private set; }

        public TrainingSample(int observation, List<String> products)
        {
            this.Observation = observation;
            this.Products = products;
        }

    }
}
