namespace TeqTank.Services.MakoRunners.ThreadObjects
{
	/// <inheritdoc />
	/// <summary>
	/// The object used to return results from a service Task.
	/// </summary>
	public class ServiceTaskResult : IServiceTaskResult
	{
		/// <summary>
		/// The integer representation of the service return code.
		/// </summary>
		public int ServiceReturnCode { get; set; }
	}
}
