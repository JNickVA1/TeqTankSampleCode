using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using TeqTank.Applications.Mako;
using TeqTank.Services.Common.Authentication;
using TeqTank.Services.Common.Caching;
using TeqTank.Services.Common.Configuration;
using TeqTank.Services.Common.Configuration.CompanyConfiguration;
using TeqTank.Services.Common.Configuration.ProjectConfiguration;
using TeqTank.Services.Common.Cryptography;
using TeqTank.Services.DataAccess.DataQueue;
using TeqTank.Services.Logging;
using TeqTank.Services.Logging.LoggingHelpers;
using TeqTank.Services.MakoRunners.ThreadObjects;
using Topshelf;

namespace TeqTank.Services.MakoRunners.Runners
{
	/// <summary>
	/// 
	/// </summary>
	public class TopshelfWindowsService : BaseTopshelfWindowsService
	{
		#region Fields
		#endregion Fields

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		/// <param name="companyConfig"></param>
		/// <param name="serviceProgressInfo"></param>
		public TopshelfWindowsService(CompanyConfig companyConfig, ServiceProgressInfo serviceProgressInfo) : 
			base(companyConfig, serviceProgressInfo)
		{
		}
		#endregion Constructors

		#region Properties
		/// <summary>
		/// 
		/// </summary>
		private bool IsRunning { get; set; }
		#endregion Properties

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="hostControl"></param>
		/// <returns></returns>
		public override bool Start(HostControl hostControl)
		{
			//
			RunCommissions();

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hostControl"></param>
		/// <returns></returns>
		public override bool Stop(HostControl hostControl)
		{
			// Set the flag to signal to the queue to stop processing.
			IsRunning = false;

			hostControl.Stop();

			//
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Shutdown()
		{
			// Set the flag to signal to the queue to stop processing.
			IsRunning = false;

			// TODO: Add any necessary code for a Shutdown.
		}
		
		/// <summary>
		/// The method that does the work of calculating and posting the commissions.
		/// </summary>
		public override void RunCommissions()
		{
			try
			{
				// Create the object used for user and application authentication and authorization.
				var api = new AuthenticateAndAuthorize();

				// Create an authentication token for the user encoded in the configuration file for the current Company ID.
				// TODO: Look into better ways of doing this.
				ThresherToken = api.GetTokenForUser(Config.Username, Config.Pwd);

				// Retrieve the encrypted data that is returned from the user-specific URL.
				var encryptStr = AuthenticateAndAuthorize.GetApplication(ThresherToken);

				// Authenticate and decrypt the data returned from the user-specific URL.
				var decrypt = AesThenHmac.SimpleDecrypt(encryptStr, Convert.FromBase64String(Config.CryptKey), Convert.FromBase64String(Config.AuthKey));

				// Retrieve the List of application configurations.
				var list = JsonConvert.DeserializeObject<List<ApplicationContract>>(decrypt);

				// Select the First item in the List of application configuration objects.
				var appContract = list.FirstOrDefault(listItem => listItem.CompanyId == Config.CompanyId && listItem.ApplicationTy == 1);

				// Ensure that we obtained a List item (an ApplicationContract instance).
				ProcessQueue(appContract);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="appContract"></param>
		private void ProcessQueue(ApplicationContract appContract)
		{
			if (appContract != null)
			{
				// Retrieve the connection string for this Application Contract.
				var connString = appContract.GenerateConnectionStr();

				// Retrieve the company data for the specified ID.
				CompanySettingsForId = new CompanySettings(connString, Config.CompanyId);

				// Create a new SQL Cache.
				MakoCache = new SqlCacheRepo(appContract.GenerateConnectionStr(), MakoLogger);

				// Create a new Queue management object.
				var queueManager = new QueueManagement(Config.CompanyId, appContract.GenerateConnectionStr());

				// Clear the queue for this Company ID.
				queueManager.ClearQueue();

				//
				IsRunning = true;

				//
				while (IsRunning)
				{
					//
					ProcessQueueItem(appContract, MakoLogger, MakoCache, queueManager);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="appContract"></param>
		/// <param name="makoLogger"></param>
		/// <param name="makoCache"></param>
		/// <param name="queueManager"></param>
		private void ProcessQueueItem(ApplicationContract appContract, DelegateLogProvider makoLogger, SqlCacheRepo makoCache, QueueManagement queueManager)
		{
			// TODO: Find a smart way to check to see if there are changes to the connection strings and so forth

			//
			while (queueManager.FindNextQueueItem(out RunQueueItem item)) // TODO: Write authentication for app
			{
				//
				var startTime = DateTimeOffset.Now;
				_logRunTy = item.RunTy;
				ConnectString = appContract.GenerateConnectionStr();
				_logCompanyId = Config.CompanyId;
				_logQueueId = item.QueueId;
				_logPeriodType = item.PeriodTy;
				_logPeriodId = item.PeriodId;

				try
				{
					//
					UpdateMakoAdmin(item, makoCache, makoLogger);

					//
					_listLog = new List<string>();
					_listLog.Add("");
					_listLog.Add($"{DateTime.UtcNow.ToString("G")} ------------------------------------------");
					_listLog.Add(
						$"{DateTime.UtcNow.ToString("G")} Now Processing Real Time -  QueueId {item.QueueId}");
					_listLog.Add(
						$"{DateTime.UtcNow.ToString("G")} RunTy({item.RunTy}), PlanId({item.PlanId}), RevisionId({item.RevisionId})");
					_listLog.Add($"{DateTime.UtcNow.ToString("G")} ------------------------------------------");
					_listLog.Add("");

					// Assembly assm, string typesXML, string bonusXML, string glossaryXML, 
					// string overrideXML, string rankXML, string volumeXML
					var myschema = new MakoXMLSchemas(
						Assembly,
						PlanName + ".MakoTypes.xml",
						PlanName + ".MakoBonuses.xml",
						"",
						PlanName + ".MakoOverrides.xml",
						PlanName + ".MakoRanks.xml",
						PlanName + ".MakoVolumes.xml"
					);
					//
					MakoConfig comConfig = new MakoConfig()
					{
						PeriodID = item.PeriodId,
						PeriodTy = item.PeriodTy,
						QueueID = item.QueueId,
						RunDescr = item.RunDescr,
						RunID = item.RunId,
						RunTy = item.RunTy,
						XMLSchema = myschema,
						PlanID = item.PlanId,
						RevisonID = item.RevisionId
					};
					//
					Admin.ProcessCommissions(comConfig);

					//
					var endTime = DateTimeOffset.Now;

					//
					if (comConfig.RunID != 0 && comConfig.RunTy != 4)
					{
						LoggingHelpers.UpdateRunLog(appContract.GenerateConnectionStr(), Config.CompanyId, item.QueueId,
							comConfig.RunID);
						queueManager.UpdateRunStartAndEndDate(appContract.GenerateConnectionStr(), Config.CompanyId, item.RunId,
							startTime, endTime);
					}

					queueManager.DeleteQueueItem(item.QueueId);

					var log = new FileLogger(Config.CompanyKey, item.QueueId, item.RunTy);
					log.GenerateLog(_listLog);
					_listLog.Clear();

					//
					if (!IsRunning)
						break;
				}
				catch (Exception ex)
				{
					var log = new FileLogger(Config.CompanyKey, item.QueueId, item.RunTy);
					_listLog.Add($"{DateTime.UtcNow.ToString("G")} [Error] " + ex.Message);
					_listLog.Add($"{DateTime.UtcNow.ToString("G")} [Error] " + ex.StackTrace);
					log.GenerateLog(_listLog);
					_listLog.Clear();

					//
					queueManager.LogQueueError(appContract.GenerateConnectionStr(), Config.CompanyId, item.QueueId, ex);
				}
			}
			//
			Thread.Sleep(TimeSpan.FromSeconds(1));
		}
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}
