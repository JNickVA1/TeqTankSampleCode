using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeqTank.Services.Common.Properties;

namespace TeqTank.Services.Common
{
	/// <summary>
	/// 
	/// </summary>
	public class BaseCommon
	{
		#region Fields
		#endregion Fields

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		public BaseCommon()
		{
		}
		#endregion Constructors

		#region Properties
		/// <summary>
		/// 
		/// </summary>
		public string AppBaseUrl { get; } = Settings.Default.AppBaseUrl;

		/// <summary>
		/// 
		/// </summary>
		public static string ApiBaseUrl { get; } = Settings.Default.ApiBaseUrl;
		#endregion Properties

		#region Methods
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}
