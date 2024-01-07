namespace BisleriumCafe.Pages
{
	public partial class Counter
	{
        public const string Route = "/counter";
        private int currentCount = 0;

        private void IncrementCount()
        {
            currentCount++;
        }
	}
}

