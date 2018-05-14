using System;
using Microsoft.Azure.WebJobs;
using TeqTank.Applications.Mako;
using TeqTank.Services.Common.Configuration.CompanyConfiguration;
using TeqTank.Services.DataAccess.DataQueue;
using TeqTank.Services.Logging.LoggingHelpers;

namespace TeqTank.Services.MakoRunners.Runners
{
	/// <inheritdoc />
	/// <summary>
	/// </summary>
	public class AzureWebJob : BaseAzureWebJob
	{
		#region Fields
		#endregion Fields

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		public AzureWebJob()
		{
		}
		#endregion Constructors

		#region Properties
		#endregion Properties

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="config"></param>
		/// <param name="item"></param>
		/// <param name="makoAdmin"></param>
		/// <param name="connStr"></param>
		/// <param name="queueManager"></param>
		[NoAutomaticTrigger]
		public new static void RunCommissionsStatic(CompanyConfig config, RunQueueItem item, MakoAdmin makoAdmin, string connStr, QueueManagement queueManager)
		{ }

		/// <inheritdoc />
		/// <summary>
		/// </summary>
		/// <param name="config"></param>
		/// <param name="item"></param>
		/// <param name="makoAdmin"></param>
		/// <param name="connStr"></param>
		/// <param name="queueManager"></param>
		[NoAutomaticTrigger]
		public override void RunCommissions(CompanyConfig config, RunQueueItem item, MakoAdmin makoAdmin, string connStr, QueueManagement queueManager)
		{
			try
			{
				//
				if (item.RunTy != 4)
					LoggingHelpers.InsertIntoLog(connStr, config.CompanyId, item.QueueId, $"{DateTime.UtcNow:G} [Info] Now Processing Real Time -  QueueId {item.QueueId}");
				else
					LoggingHelpers.InsertIntoLog(connStr, config.CompanyId, item.QueueId, $"{DateTime.UtcNow:G} [Info] Now Processing Run -  QueueId {item.QueueId}");

				//
				LoggingHelpers.InsertIntoLog(connStr, config.CompanyId, item.QueueId, $"{DateTime.UtcNow:G} RunTy({item.RunTy}), PlanId({item.QueueId}), RevisionId({item.RevisionId})");


				// Assembly assm, string typesXML, string bonusXML, string glossaryXML, 
				// string overrideXML, string rankXML, string volumeXML
				var myschema = new MakoXMLSchemas(
							makoAdmin.ProjectAssembly,
							makoAdmin.ProjectAssembly.GetName().Name + ".MakoTypes.xml",
							makoAdmin.ProjectAssembly.GetName().Name + ".MakoBonuses.xml",
							"",
							makoAdmin.ProjectAssembly.GetName().Name + ".MakoOverrides.xml",
							makoAdmin.ProjectAssembly.GetName().Name + ".MakoRanks.xml",
							makoAdmin.ProjectAssembly.GetName().Name + ".MakoVolumes.xml"
							);

				MakoConfig comConfig = new MakoConfig()
				{
					CompanyID = config.CompanyId,
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

				LoggingHelpers.InsertIntoLog(connStr, config.CompanyId, item.QueueId, $"{DateTime.UtcNow.ToString("G")} [Info] Commission complete");

				//
				makoAdmin.ProcessCommissions(comConfig);

				//
				if (comConfig.RunID != 0 && comConfig.RunTy != 4)
				{
					LoggingHelpers.UpdateRunLog(connStr, config.CompanyId, item.QueueId, comConfig.RunID);
				}
				//
				queueManager.DeleteQueueItem(item.QueueId);
			}
			catch (AggregateException e)
			{
				//
				foreach (var ex in e.InnerExceptions)
				{
					LoggingHelpers.InsertIntoLog(connStr, config.CompanyId, item.QueueId, $"{DateTime.UtcNow:G} [Error] " + ex.Message);
					LoggingHelpers.InsertIntoLog(connStr, config.CompanyId, item.QueueId, $"{DateTime.UtcNow:G} [Error] " + ex.StackTrace);
				}
				//
				LoggingHelpers.LogQueueError(connStr, config.CompanyId, item, e);

#if DEVELOPMENT

#else
				Common.API.SendGrid.SendGridHelper.SendAlertAsync("Mako Runner Exception", $"Company {config.CompanyName}, RunTy {item.RunTy}, Description {item.RunDescr} {Environment.NewLine} {e}",
						$"Company {config.CompanyName}, RunTy {item.RunTy}, Description {item.RunDescr} {Environment.NewLine} {e}");
#endif

				throw;
			}
			catch (Exception ex)
			{
				//
				LoggingHelpers.InsertIntoLog(connStr, config.CompanyId, item.QueueId, $"{DateTime.UtcNow:G} [Error] " + ex.Message);
				LoggingHelpers.InsertIntoLog(connStr, config.CompanyId, item.QueueId, $"{DateTime.UtcNow:G} [Error] " + ex.StackTrace);

				//
				LoggingHelpers.LogQueueError(connStr, config.CompanyId, item, ex);

#if DEVELOPMENT

#else
				if (!_isDevelopment)
					Common.API.SendGrid.SendGridHelper.SendAlertAsync("Mako Runner Exception", $"Company {config.CompanyName}, RunTy {item.RunTy}, Description {item.RunDescr} {Environment.NewLine} {ex}",
						$"Company {config.CompanyName}, RunTy {item.RunTy}, Description {item.RunDescr} {Environment.NewLine} {ex}");
#endif
				throw;
			}
		}
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}