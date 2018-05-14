namespace TeqTank.Services.Common.EnumsAndConstants
{
	/// <summary>
	/// A simple enumeration that is used to specify the order 
	/// in which a list of company IDs is processed
	/// </summary>
	public enum ProcessingOrder	
	{
		/// <summary>
		/// This specifies that we will run multiple company IDs 
		/// through the runner at the same time. 
		/// NOTE: The ConsoleCommand can be modified to accept a
		///		  number that represents the concurrent limit for 
		///		  parallel runs.
		/// </summary>
		Parallel,
		/// <summary>
		/// This specifies that multiple company IDs will be processed sequentially.
		/// </summary>
		Sequential,
		/// <summary>
		/// This specifies that multiple company IDs will be processed by fetching a queue item
		/// for the first ID, processing it, and then fetching a queue item for the next ID in 
		/// the list of IDs.
		/// </summary>
		RoundRobin,
		/// <summary>
		/// This specifies that the method of processing can not be determined..
		/// </summary>
		Undefined
	}
}