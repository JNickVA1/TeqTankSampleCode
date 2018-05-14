namespace TeqTank.Services.Common.EnumsAndConstants
{
	/// <summary>
	/// An enumeration of the different types of Mako Runners currently supported.
	/// </summary>
	public enum RunnerType
	{
		/// <summary>
		/// A Windows Service. 
		/// </summary>
		WindowsService,
		/// <summary>
		/// An Azure WebJob.
		/// </summary>
		AzureWebJob,
		/// <summary>
		/// Undefined or unknown.
		/// </summary>
		UndefinedOrUnknown
	}
}