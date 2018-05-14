using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeqTank.Services.MakoRunnerConsolidated.ThreadObjects
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
