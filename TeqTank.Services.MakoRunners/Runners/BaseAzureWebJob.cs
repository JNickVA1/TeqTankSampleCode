using System;
using System.Reflection;
using TeqTank.Applications.Mako;
using TeqTank.Services.Common.Caching;
using TeqTank.Services.Common.Configuration.CompanyConfiguration;
using TeqTank.Services.Common.Configuration.ProjectConfiguration;
using TeqTank.Services.DataAccess.DataQueue;

namespace TeqTank.Services.MakoRunners.Runners
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class BaseAzureWebJob

	{
		#region Fields
		#endregion Fields

		#region Constructors
		#endregion Constructors

		#region Properties
		/// <summary>
		/// 
		/// </summary>
		public CompanyConfig Config { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int LastPlanId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string ThresherToken { get; set; }
		#endregion Properties

		#region Methods

		/// <summary>
		/// A placeholder for the method implemented in the derived classes.
		/// </summary>
		/// <param name="config"></param>
		/// <param name="item"></param>
		/// <param name="makoAdmin"></param>
		/// <param name="connStr"></param>
		/// <param name="queueManager"></param>
		/// <remarks>
		/// Derived classes must declare the RunCommissions as: 
		///		public new static void RunCommissionsStatic(...)
		/// </remarks>
		public static void RunCommissionsStatic(CompanyConfig config, RunQueueItem item, MakoAdmin makoAdmin, string connStr, QueueManagement queueManager)
		{
			throw new NotImplementedException(
				"Error in RunCommissionsStatic. The method must have a new implementation in derived classes.");
		}

		/// <summary>
		/// A placeholder for the method implemented in the derived classes.
		/// </summary>
		/// <param name="config"></param>
		/// <param name="item"></param>
		/// <param name="makoAdmin"></param>
		/// <param name="connStr"></param>
		/// <param name="queueManager"></param>
		/// <remarks>
		/// Derived classes must declare the RunCommissions as: 
		///		public override void RunCommissions(...)
		/// </remarks>
		public virtual void RunCommissions(CompanyConfig config, RunQueueItem item, MakoAdmin makoAdmin, string connStr, QueueManagement queueManager)
		{
			throw new NotImplementedException(
				"Error in RunCommissions. The method must have a new implementation in derived classes.");
		}
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}
