using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTM_MART.Models
{
    public class TrainingSet
    {

        public List<TrainingSample> Samples { get; private set; }

        public string Param1 { get; private set; }
        public string Param2 { get; private set; }

        private bool m_canAddSample = true;

        public TrainingSet(string param1, string param2)
        {
            this.Param1 = param1;
            this.Param2 = param2;
            this.Samples = new List<TrainingSample>();
        }

        public int GetDataCount()
        {
            return this.Samples.Count;
        }

        public void AddSample(TrainingSample sample)
        {
            if (this.m_canAddSample)
            {
                if (!this.Samples.Contains(sample))
                {
                    this.Samples.Add(sample);
                }
                else
                {
                    throw new Exception("Already contains value");
                }
            }
            else
            {
                throw new Exception("Training set was locked");
            }
        }

        public void Lock()
        {
            this.m_canAddSample = false;
        }
    }
}
