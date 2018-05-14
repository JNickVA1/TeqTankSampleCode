using System;
using TeqTank.Services.Common.EnumsAndConstants;

namespace TeqTank.Services.ServicesController.EventArguments
{
	/// <summary>
	/// 
	/// </summary>
	public class ServicesControllerErrorArgs : ServiceControllerEventArgs
	{
		/// <summary>
		/// 
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ErrorSeverityCode ErrorSeverity { get; set; }
	}
}
