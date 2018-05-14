using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeqTank.Services.MakoRunnerConsolidated.ThreadObjects
{
	/// <summary>
	/// The interface that defines general properties and methods to be used in Progress Info classes.
	/// </summary>
	public interface IServiceProgressInfo
	{
		/// <summary>
		/// The name of the service.
		/// </summary>
		/// <remarks>
		/// This is a specific, unique, name, such as WindowsService27, NOT a generic name, such as "Windows" or "Azure".
		/// </remarks>
		string ServiceName { get; set; }
	}
}
