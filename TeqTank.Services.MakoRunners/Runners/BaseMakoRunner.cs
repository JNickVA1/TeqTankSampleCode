using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using TeqTank.Applications.Mako;
using TeqTank.Services.Common.Caching;
using TeqTank.Services.Common.Commands;
using TeqTank.Services.Common.Configuration;
using TeqTank.Services.Common.Configuration.CompanyConfiguration;
using TeqTank.Services.Common.Configuration.ProjectConfiguration;
using TeqTank.Services.Communications.SocketCommunication;
using TeqTank.Services.DataAccess.DataQueue;
using TeqTank.Services.Logging.LoggingHelpers;

namespace TeqTank.Services.MakoRunners.Runners
{
	/// <summary>
	/// 
	/// </summary>
	public class BaseMakoRunner : IMakoRunner
	{
		#region Fields
		#endregion Fields

		#region Properties
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
		public string ThresherToken { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Assembly Assembly { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string PlanName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public MakoAdmin Admin { get; set; }

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
		public Guid MyRunnerId { get; }

		/// <summary>
		/// 
		/// </summary>
		public RunnerInfo Info { get; set; }

		/// <summary>
		/// A flag that indicates whether the Runner's action method can be used in multiple, parallel, Tasks.
		/// For example, we may want to process multiple Companies simultaneously.
		/// </summary>
		/// <remarks>
		/// Defaults to 'false' and must be explicitly set to 'true' in a derived class. If the 
		/// action to be performed is in a static method, then leave the default value intact. 
		/// Within a static method you can always perform actions in a child Task that run in 
		/// parallel. For example, we may want to process items from a concurrent queue in child
		/// tasks of the main Runner Task.
		/// </remarks>
		public bool CanRunInParallel { get; set; } = false;

		/// <summary>
		/// 
		/// </summary>
		public CompanyConfig Config { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public static bool IsDevelopment { get; set; }
		#endregion Properties

		#region Constructors
		/// <summary>
		/// The constructor of the base class, from which Mako Runners should be derived,
		/// will store various bits of data relating to the service being created. These
		/// bits of data will be similar for all Mako Runners.
		/// </summary>
		/// <param name="keyServer"></param>
		/// <param name="consoleCommand"></param>
		public BaseMakoRunner(Guid keyServer, ConsoleCommand consoleCommand)
		{
			//
			Info = new RunnerInfo(consoleCommand);

			//
			MyRunnerId = keyServer;

			// Create the log provider to log progress.
			MakoLogger = new DelegateLogProvider();

			//
			MakoLogger._onProgressUpdate += MakoLogger__onProgressUpdate;
		}
		#endregion Constructors

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
		/// Performs the logic needed to start the job.
		/// </summary>
		public virtual void Start()
		{
		}

		/// <summary>
		/// Uses the Task and CancellationToken obtained in the Start method to halt the thread.
		/// </summary>
		public virtual void Stop()
		{
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
			// TODO: Fix this!
			//Task.Factory.StartNew(() => LoggingHelpers.InsertIntoLog(ConnectString, Config.CompanyId, _logQueueId, (string)e.UserState));

			////MakoSocket.SendMessage()
			////_listLog.Add((string)e.UserState);\

			//MakoSocket.SendMessage(_logCompanyId, _logQueueId, _logPeriodType, _logPeriodId, (string)e.UserState, _logPeriodType != 4, e.ProgressPercentage);
			Console.WriteLine((string)e.UserState);
		}
		#endregion Event Handlers
	}
}
