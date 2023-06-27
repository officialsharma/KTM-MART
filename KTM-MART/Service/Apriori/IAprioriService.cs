namespace KTM_MART.Service.Apriori
{
    public interface IAprioriService
    {
        public Dictionary<string, int> CalulateProductCounts();
        public Dictionary<string, int> RemoveThreshold(Dictionary<string, int> productCounts);

        public string PrintCounts(Dictionary<string, int> productCounts);

        public Dictionary<string[], int> GroupProducts(Dictionary<string, int> productCounts);

        public string PrintGroups(Dictionary<string[], int> productCounts);
        public Dictionary<string[], int> RemoveThresholds(Dictionary<string[], int> productCounts);

        public string PrintGrouped(Dictionary<string[], int> productCounts);
        public string PrintMergedGrouped(Dictionary<string[], int> grouped);
        public Dictionary<string[], int> MergeGroupProducts(Dictionary<string[], int> productCounts);

        public string PrintFinalValues(Dictionary<string[], int> group);
    }
}
