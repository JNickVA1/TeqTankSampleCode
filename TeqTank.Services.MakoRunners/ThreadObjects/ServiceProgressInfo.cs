namespace TeqTank.Services.MakoRunners.ThreadObjects
{
	/// <inheritdoc />
	/// <summary>
	/// The ServiceProgressInfo class is used as the value sent on a progress
	/// event by the Progress class which is invoked within the Task that runs
	/// the service method.
	/// </summary>
	public class ServiceProgressInfo : IServiceProgressInfo
	{
		/// <inheritdoc />
		/// <summary>
		/// </summary>
		public string ServiceName { get; set; }
	}
}
