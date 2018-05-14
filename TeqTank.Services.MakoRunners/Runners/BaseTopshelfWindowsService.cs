using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TeqTank.Applications.Mako;
using TeqTank.Services.Common.Caching;
using TeqTank.Services.Common.Configuration;
using TeqTank.Services.Common.Configuration.CompanyConfiguration;
using TeqTank.Services.Common.Configuration.ProjectConfiguration;
using TeqTank.Services.Communications.SocketCommunication;
using TeqTank.Services.DataAccess.DataQueue;
using TeqTank.Services.Logging.LoggingHelpers;
using TeqTank.Services.MakoRunners.ThreadObjects;
using Topshelf;

namespace TeqTank.Services.MakoRunners.Runners
{
	/// <summary>
	/// 
	/// </summary>
	public class BaseTopshelfWindowsService : ServiceControl, IMakoRunner
	{
		#region Fields
		/// <summary>
		/// 
		/// </summary>
		protected int _logRunTy;
		/// <summary>
		/// 
		/// </summary>
		protected List<string> _listLog;
		/// <summary>
		/// 
		/// </summary>
		protected int _logCompanyId;
		/// <summary>
		/// 
		/// </summary>
		protected int _logQueueId;
		/// <summary>
		/// 
		/// </summary>
		protected int _logPeriodType;
		/// <summary>
		/// 
		/// </summary>
		protected int _logPeriodId;

		#endregion Fields

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		public BaseTopshelfWindowsService()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="companyConfig"></param>
		/// <param name="serviceProgressInfo"></param>
		protected BaseTopshelfWindowsService(CompanyConfig companyConfig, ServiceProgressInfo serviceProgressInfo)
		{
			//
			MakoSocket = new ServicesWebSocket();

			// Create the log provider to log progress.
			if (MakoLogger == null)
				MakoLogger = new DelegateLogProvider();

			// Set the event handler for change events.
			MakoLogger._onProgressUpdate += MakoLogger__onProgressUpdate;

			//
			ServiceProgressInfo = serviceProgressInfo;

			//
			Config = companyConfig;
		}
		#endregion Constructors

		#region Properties
		/// <summary>
		/// 
		/// </summary>
		public ServiceProgressInfo ServiceProgressInfo { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public CompanyConfig Config { get; set;  }

		/// <summary>
		/// 
		/// </summary>
		public int LastPlanId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int LastRevisionId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DelegateLogProvider MakoLogger { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public SqlCacheRepo MakoCache { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public QueueManagement QueueManager { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string ConnectString { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public ServicesWebSocket MakoSocket { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public CompanySettings CompanySettingsForId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public MakoAdmin Admin { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string ThresherToken { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Assembly Assembly { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string PlanName { get; set; }
		#endregion Properties

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		/// <param name="cache"></param>
		/// <param name="logger"></param>
		public void UpdateMakoAdmin(RunQueueItem item, SqlCacheRepo cache, ILogProvider logger)
		{
			if (item.PlanId == LastPlanId && item.RevisionId == LastRevisionId)
				return;

			if (CacheHelper.IsIncache(Config.CompanyId, item.PlanId, item.RevisionId, CacheHelper.CacheObjectType.Assembly))
			{
				Assembly = CacheHelper.GetFromCache<Assembly>(Config.CompanyId, item.PlanId, item.RevisionId, CacheHelper.CacheObjectType.Assembly);
				PlanName = CacheHelper.GetFromCache<string>(Config.CompanyId, item.PlanId, item.RevisionId, CacheHelper.CacheObjectType.ProjectName);
			}
			else
			{
				var response = new VsProjectDll().GetPlanDll(item.PlanId, item.RevisionId, ThresherToken); // TODO: Set up token
				Assembly = Assembly.Load(response.Dll);
				PlanName = response.PlanName;

				CacheHelper.SaveToCache(Config.CompanyId, item.PlanId, item.RevisionId, Assembly, CacheHelper.CacheObjectType.Assembly);
				CacheHelper.SaveToCache(Config.CompanyId, item.PlanId, item.RevisionId, PlanName, CacheHelper.CacheObjectType.ProjectName);
			}
			//
			// TODO: Figure out why we are doing a 'new' here.
			Admin = new MakoAdmin(Config.CompanyId, Assembly, cache, logger, item.PlanId, item.RevisionId);

			LastPlanId = item.PlanId;
			LastRevisionId = item.RevisionId;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hostControl"></param>
		/// <returns></returns>
		public virtual bool Start(HostControl hostControl)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hostControl"></param>
		/// <returns></returns>
		public virtual bool Stop(HostControl hostControl)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual void Shutdown()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// A placeholder for the method implemented in the derived classes.
		/// </summary>
		/// <remarks>
		/// Derived classes must declare the RunCommissions as: 
		///		public override void RunCommissions()
		/// </remarks>
		public virtual void RunCommissions()
		{
			throw new NotImplementedException(
				"Error in RunCommissions. The method must have a new implementation in derived classes.");
		}
		#endregion Methods

		#region Event Handlers
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void MakoLogger__onProgressUpdate(object sender, ProgressChangedEventArgs e)
		{
			//
			_listLog.Add((string)e.UserState);

			//
			LoggingHelpers.InsertIntoLog(_logRunTy, CompanySettingsForId, ConnectString, _logCompanyId, _logQueueId, (string)e.UserState);

			//
			var message = new SocketMessage
			{
				CompanyId = _logCompanyId,
				QueueId = _logQueueId,
				PeriodType = _logPeriodType,
				PeriodId = _logPeriodId,
				Information = (string)e.UserState,
				IsRun = _logPeriodType != 4,
				Percentage = e.ProgressPercentage
			};

			// If the socket is not null, send a message.
			MakoSocket?.SendMessage(message);

			//
			Console.WriteLine((string)e.UserState);
		}
		#endregion Event Handlers
	}
}
