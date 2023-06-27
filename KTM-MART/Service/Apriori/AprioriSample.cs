using KTM_MART.DataAccess;
using KTM_MART.Models;
using Microsoft.EntityFrameworkCore;

namespace KTM_MART.Service.Apriori
{
	public class AprioriSample
	{

		private readonly ApplicationDbContext _context;
		private TrainingSet _trainingSet;

		public AprioriSample(ApplicationDbContext applicationDbContext)
		{
			//_context = applicationDbContext;

			_context = applicationDbContext;
			_trainingSet = new TrainingSet("Customers", "Products");
		}


		public TrainingSet GetTrainingSet()
		{
			var result = _context.samplesGet.FromSqlRaw("EXEC testData").ToList();
			var count = result.Count;
			for (int i = 1; i <= count; i++)
			{
				var item = result[i - 1];
				var productsString = item.Products.ToString(); // Convert to string if necessary
				var productsArray = productsString.Split(','); // Split the comma-separated values into an array
				var products = new List<string>(productsArray); // Create a new List<string> with the separated values
				_trainingSet.AddSample(new TrainingSample(i, products));
			}
			_trainingSet.Lock();

			return _trainingSet;
		}
	}
}
