using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TeqTank.Applications.Mako;
using TeqTank.Services.Common.Configuration;
using TeqTank.Services.Common.Configuration.CompanyConfiguration;
using TeqTank.Services.Communications.SocketCommunication;
using TeqTank.Services.DataAccess.DataQueue;

namespace TeqTank.Services.MakoRunners.Runners
{
	/// <summary>
	/// 
	/// </summary>
	public interface IMakoRunner
	{
		#region Fields
		#endregion Fields

		#region Constructors
		#endregion Constructors

		#region Properties
		/// <summary>
		/// 
		/// </summary>
		CompanyConfig Config { get; set; }

		/// <summary>
		/// 
		/// </summary>
		int LastPlanId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		int LastRevisionId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		string ThresherToken { get; set; }

		/// <summary>
		/// 
		/// </summary>
		Assembly Assembly { get; set; }

		/// <summary>
		/// 
		/// </summary>
		string PlanName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		MakoAdmin Admin { get; set; }

		/// <summary>
		/// 
		/// </summary>
		DelegateLogProvider MakoLogger { get; set; }

		/// <summary>
		/// 
		/// </summary>
		SqlCacheRepo MakoCache { get; set; }

		/// <summary>
		/// 
		/// </summary>
		QueueManagement QueueManager { get; set; }

		/// <summary>
		/// 
		/// </summary>
		string ConnectString { get; set; }

		/// <summary>
		/// 
		/// </summary>
		ServicesWebSocket MakoSocket { get; set; }
		#endregion Properties

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		/// <param name="cache"></param>
		/// <param name="logger"></param>
		void UpdateMakoAdmin(RunQueueItem item, SqlCacheRepo cache, ILogProvider logger);
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}
