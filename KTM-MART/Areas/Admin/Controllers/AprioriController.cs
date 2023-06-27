using KTM_MART.Models.ViewModels;
using KTM_MART.Service.Apriori;
using KTM_MART.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KTM_MART.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class AprioriController : Controller
    {
        private readonly IAprioriService _service;

        public AprioriController(IAprioriService aprioriService)
        {
            _service = aprioriService;

        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Apriori()
        {

            var productCounts = _service.CalulateProductCounts();
            //PrintCounts Of Products
            var printProductCounts = _service.PrintCounts(productCounts);
            var thresholdedProductCounts = _service.RemoveThreshold(productCounts);
            //PrintCounts of products after removing the threshold
            var printthresholdedProductCounts = _service.PrintCounts(thresholdedProductCounts);
            Dictionary<string[], int> groupProducts = _service.GroupProducts(thresholdedProductCounts);
            //PrintGrouped products
            var result = _service.PrintGroups(groupProducts);
            var thresholdedProductCount2 = _service.RemoveThresholds(groupProducts);
            //print Grouped Values
            string GroupedResult = _service.PrintGrouped(thresholdedProductCount2);
            Dictionary<string[], int> mergedProducts = _service.MergeGroupProducts(thresholdedProductCount2);
            //Print Final Merged Products
            var mergedGrouped = _service.PrintMergedGrouped(mergedProducts);

            //var mergedResults = _service.PrintMergedGrouped(mergedProducts);
            var finalResult = _service.PrintFinalValues(mergedProducts);

            //ViewBag.Result = finalResult; 
            var aprioriVM = new AprioriVM
            {
                ProductCounts = printProductCounts,
                ThresholdedGroupedProducts = printthresholdedProductCounts,
                GroupedProducts = result,
                MergedGroupedProducts = GroupedResult,
                MergedGrouped = mergedGrouped,
                FinalResult = finalResult

            };
            return View(aprioriVM);
        }
    }
}
