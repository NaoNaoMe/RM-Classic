using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rmApplication
{
    class PeriodAveraging
    {
        private long previousTime;
        private Queue<long> periods;

        public PeriodAveraging()
        {
            Clear();
        }

        public void Clear()
        {
            previousTime = 0;
            periods = new Queue<long>();
        }

        public double Calculate(long currentTime)
        {
            if(previousTime == 0)
            {
                previousTime = currentTime;
                return 0;
            }

            periods.Enqueue(currentTime - previousTime);
            previousTime = currentTime;

            while (periods.Count > 100)
                periods.Dequeue();

            var count = periods.Count;
            var localPeriods = new Queue<long>(periods);

            double sum = 0;
            while(localPeriods.Count != 0)
                sum += localPeriods.Dequeue();

            return (sum / count);

        }

    }
}
