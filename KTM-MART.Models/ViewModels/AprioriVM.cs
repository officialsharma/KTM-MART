using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTM_MART.Models.ViewModels
{
    public class AprioriVM
    {
        public string ProductCounts { get; set; } = string.Empty;
        public string GroupedProducts { get; set; } = string.Empty;
        public string ThresholdedGroupedProducts { get; set; } = string.Empty;
        public string MergedGroupedProducts { get; set; } = string.Empty;
        public string MergedGrouped { get; set; } = string.Empty;
        public string FinalResult { get; set; } = string.Empty;
    }
}
