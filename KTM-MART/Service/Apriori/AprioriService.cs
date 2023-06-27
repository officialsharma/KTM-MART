using KTM_MART.Models;

namespace KTM_MART.Service.Apriori
{
	public class Apriori : IAprioriService//main class
	{

		private readonly TrainingSet m_set;//represents collection of samples
		private readonly int m_support;
		private readonly int m_confidence;

		//private readonly int m_thresholdSupport;
		public Apriori(TrainingSet set, int support, int confidence)
		{
			m_set = set;
			m_support = support > 100 ? 100 : support < 0 ? 0 : support;
			m_confidence = confidence > 100 ? 100 : confidence < 0 ? 0 : confidence;

			//m_thresholdSupport = m_set.Samples.Count * m_support / 100;
		}

		public string PrintFinalValues(Dictionary<string[], int> group)
		{
			string output = "RESULTS\n";
			output += "--------------------\n";
			int index = 1;

			foreach (KeyValuePair<string[], int> product in group)
			{
				string[] keys = product.Key;

				for (int i = 0; i < keys.Length; i++)
				{
					output += $"\nRESULT {index} : \n";

					string[] ins = new string[] { keys[i] };
					string[] outs = keys.Except(ins).ToArray();

					output += PrintThresholdRule(keys, ins, outs);

					index++;
				}
			}

			return output;
		}

		//public string PrintFinalValues(Dictionary<string[], int> group)
		//{
		//	string output = "RESULTS\n";//for result holding
		//	output += "--------------------\n";
		//	int index = 1;//for tracking rule number
		//	foreach (KeyValuePair<string[], int> product in group)
		//	{
		//		string[] keys = product.Key;//string of array ho

		//		for (int i = -1; i < keys.Length; i++)
		//		{
		//			output += $"\nRESULT {index} : \n";

		//			string[] ins;
		//			string[] outs;

		//			if (i == -1)
		//			{
		//				ins = new string[] { keys[0], keys[1] };
		//				outs = keys.Except(ins).ToArray();
		//				output += PrintThresholdRule(keys, ins, outs);
		//				index++;

		//				continue;
		//			}

		//			ins = new string[] { keys[i] };
		//			outs = keys.Except(ins).ToArray();

		//			output += PrintThresholdRule(keys, ins, outs);

		//			index++;

		//		}

		//	}
		//	return output;
		//}
		//method to calculate the confidence score//
		//ins for the support count value of antecedent part//
		//outs for consequent part of rule support count//
		private string PrintThresholdRule(string[] keys, string[] ins, string[] outs)
		{
			int XYZ = GetGroupCountInSamples(keys);//xyz is the support count of whole rule's itemset//
			int N = GetGroupCountInSamples(ins);//support count of antecedent//
			double result = (double)XYZ / (double)N * 100;//confidence calculate//
			string resultString = $"trust({string.Join(",", ins)} -> {string.Join(",", outs)})\n";//string.join is for concatenate
			resultString += $"The probability of being [{string.Join(",", outs)}] on the product set [{string.Join(",", ins)}]\t%{result}";
			return resultString;
		}


		//5
		//merge pair of products that have same sup count
		public Dictionary<string[], int> MergeGroupProducts(Dictionary<string[], int> grouped)
		{
			Dictionary<string[], int> temp = new Dictionary<string[], int>(new ArrayComparer()); //array compareer class is for compare arrays for itemsets
			List<string> datas = new List<string>();//for unique group products store initialize datas list
			foreach (KeyValuePair<string[], int> main in grouped)
			{
				string[] keys = main.Key;
				for (int i = 0; i < keys.Length; i++)
				{
					if (!datas.Contains(keys[i]))
					{
						datas.Add(keys[i]);
					}
				}
			}
			temp.Add(datas.ToArray(), GetGroupCountInSamples(datas.ToArray())); //for calculate sp count for combine items
			return temp;
		}

		//3
		//generate groups of product pairs
		public Dictionary<string[], int> GroupProducts(Dictionary<string, int> productCounts)
		{
			Dictionary<string[], int> temp = new Dictionary<string[], int>(new ArrayComparer());
			foreach (KeyValuePair<string, int> main in productCounts)
			{
				string mainKey = main.Key; //array compare to outer loop
				foreach (KeyValuePair<string, int> sub in productCounts)
				{
					string subKey = sub.Key; //array compare to inner loop
					if (!mainKey.Equals(subKey))
					{
						string[] head1 = new string[] { mainKey, subKey };
						string[] head2 = head1.Reverse().ToArray();
						if (!temp.ContainsKey(head1) && !temp.ContainsKey(head2))
						{
							temp.Add(head1, GetGroupCountInSamples(head1));
						}
					}
				}
			}
			return temp;
		}

		//4
		//for calculating number of sample contain a group of products
		public int GetGroupCountInSamples(string[] head)
		{
			int temp = 0;
			for (int i = 0; i < this.m_set.Samples.Count; i++)
			{
				if (head.Except(this.m_set.Samples[i].Products).Count() == 0)
				{
					temp++;
				}
			}
			return temp;
		}

		//6
		//calculate support threshold for a given group of products
		//support(A -> B) = count{X, Y, Z} / N
		//calculating support cout for group like A->B
		public double GetGroupSupportThreshold(string[] A, string[] B)
		{
			double temp = 0.0d;

			return temp;
		}

		//8
		//displaying counts of each products in itemset
		public string PrintCounts(Dictionary<string, int> productCounts)
		{
			string result = "";
			foreach (KeyValuePair<string, int> product in productCounts)
			{
				result += $"count({product.Key})\t{product.Value}\n";
			}
			return result;
		}

		//8
		//displaying merged products count
		public string PrintMergedGrouped(Dictionary<string[], int> productCounts)
		{
			string result1 = "";
			foreach (KeyValuePair<string[], int> product in productCounts)
			{
				result1 += ($"{string.Join(",", product.Key)}\t{product.Value}");
			}
			return result1;
		}


		//print individual groups of products and theri support counts
		public string PrintGrouped(Dictionary<string[], int> productCounts)
		{
			string result1 = "";
			foreach (KeyValuePair<string[], int> product in productCounts)
			{
				result1 += ($"{string.Join(",", product.Key)}\t{product.Value}\n");
			}
			return result1;
		}

		public string PrintGroups(Dictionary<string[], int> productCounts)
		{
			string result1 = "";
			foreach (KeyValuePair<string[], int> product in productCounts)
			{
				result1 += ($"{string.Join(",", product.Key)}\t{product.Value}\n");
			}
			return result1;
		}

		//2
		// remove products and group products that doesnot meet minimum threshold
		public Dictionary<string, int> RemoveThreshold(Dictionary<string, int> productCounts)
		{
			return productCounts.Where(pair => pair.Value >= 3).ToDictionary(pair => pair.Key, pair => pair.Value);
		}

		//7
		public Dictionary<string[], int> RemoveThresholds(Dictionary<string[], int> productCounts)
		{
			return productCounts.Where(pair => pair.Value >= 3).ToDictionary(pair => pair.Key, pair => pair.Value);
		}

		//1
		public Dictionary<string, int> CalulateProductCounts()
		{
			Dictionary<string, int> temp = new Dictionary<string, int>();//store products count
			for (int i = 0; i < m_set.Samples.Count; i++)//loop for all transaction
			{
				for (int j = 0; j < m_set.Samples[i].Products.Count; j++)//loop for all product in current sample
				{
					string product = m_set.Samples[i].Products[j];//get name of current products
					if (temp.ContainsKey(product))//checks if already contains entry for current product
					{
						temp[product]++;//if product is already there increment count
					}
					else
					{
						temp.Add(product, 1);//if not add product with 1
					}
				}
			}
			return temp;//return product count
		}
	}
}
