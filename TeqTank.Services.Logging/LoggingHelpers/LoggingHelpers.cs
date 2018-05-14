using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeqTank.Services.Common.Configuration.CompanyConfiguration;
using TeqTank.Services.DataAccess.DataQueue;

namespace TeqTank.Services.Logging.LoggingHelpers
{
	/// <summary>
	/// 
	/// </summary>
	public class LoggingHelpers
	{
		#region Fields
		#endregion Fields

		#region Constructors
		#endregion Constructors

		#region Properties
		#endregion Properties

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="logRunType"></param>
		/// <param name="companySettingsForId"></param>
		/// <param name="connStr"></param>
		/// <param name="companyId"></param>
		/// <param name="queueId"></param>
		/// <param name="detail"></param>
		public static void InsertIntoLog(int logRunType, CompanySettings companySettingsForId, string connStr, int companyId, int queueId, string detail)
		{
			try
			{
				if (logRunType == 4)
				{
					companySettingsForId.Settings.TryGetValue(10, out var rtLogSetting);
					if (rtLogSetting == null)
						return;
					if (!bool.TryParse(rtLogSetting.Value, out bool b))
						b = false; // default to false

					if (!b)
						return;
				}
				else
				{
					companySettingsForId.Settings.TryGetValue(11, out var rLogSetting);
					if (rLogSetting == null)
						return;
					if (!bool.TryParse(rLogSetting.Value, out bool b))
						b = false; // default to false

					if (!b)
						return;
				}

				using (var conn = new SqlConnection(connStr))
				using (var cmd = conn.CreateCommand())
				{
					conn.Open();
					cmd.CommandText = $@"
                        BEGIN TRANSACTION
                            Declare @RecordNumber int;

                            Select @RecordNumber = ISNULL(MAX([RecordNumber]),0)+1
                            FROM
	                            {(logRunType == 4 ? "[dbo].[LogRealTime]" : "[dbo].[LogRun]")}
                            Where
	                            CompanyId = @CompanyId and
	                            QueueId = @QueueId

                            INSERT INTO {(logRunType == 4 ? "[dbo].[LogRealTime]" : "[dbo].[LogRun]")}
                            ([CompanyId], [QueueId], [RecordNumber], [Detail])
                            VALUES (@CompanyId, @QueueId, @RecordNumber, @Detail)
                        COMMIT TRANSACTION
                    ";
					cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = companyId;
					cmd.Parameters.Add("@QueueId", SqlDbType.Int).Value = queueId;
					cmd.Parameters.Add("@Detail", SqlDbType.NVarChar, 1024).Value = detail;

					cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="connStr"></param>
		/// <param name="companyId"></param>
		/// <param name="queueId"></param>
		/// <param name="runId"></param>
		public static void UpdateRunLog(string connStr, int companyId, int queueId, int runId)
		{
			try
			{
				using (var conn = new SqlConnection(connStr))
				using (var cmd = conn.CreateCommand())
				{
					conn.Open();
					cmd.CommandText = @"
                        Update [dbo].[LogRun]
                        set
	                        RunId = @RunId
                        where
	                        CompanyId = @CompanyId and
	                        QueueId = @QueueId
                    ";

					cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = companyId;
					cmd.Parameters.Add("@QueueId", SqlDbType.Int).Value = queueId;
					cmd.Parameters.Add("@runId", SqlDbType.Int).Value = runId;

					cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="connectionString"></param>
		/// <param name="companyId"></param>
		/// <param name="itemQueueId"></param>
		/// <param name="logDetail"></param>
		public static void InsertIntoLog(string connectionString, int companyId, int itemQueueId, string logDetail)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="connStr"></param>
		/// <param name="companyId"></param>
		/// <param name="queueId"></param>
		/// <param name="exc"></param>
		public static void LogQueueError(string connStr, int companyId, int queueId, Exception exc)
		{
			try
			{
				using (var conn = new SqlConnection(connStr))
				using (var cmd = conn.CreateCommand())
				{
					conn.Open();

					cmd.CommandText = @"
                        Insert into [dbo].[LogCommRunner] (
	                     [CompanyID]			
	                    ,[QueueID]			
	                    ,[RunTy]				
	                    ,[RunPriorityTy]		
	                    ,[RunQueueStatusTy]	
	                    ,[RunID]				
	                    ,[TreeSnapshotID]   
	                    ,[VolumeSnapshotID] 
	                    ,[RunDescr]			
	                    ,[PeriodTy]			
	                    ,[PeriodID]			
	                    ,[PlanID]           
	                    ,[RevisionID]       
	                    ,[StartTime]			
	                    ,[ErrorTime]			
	                    ,[ErrorMessage]		
	                    ,[StackTrace]		
	                    ,[PercentComplete]  
	                    ,[ProcessLog]		
	                    ,[CreatedDate]      
	                    ,[CreatedBy]        
	                    ,[ModifiedDate]     
	                    ,[ModifiedBy]       
	                    ,[ProgName]         
                    )
                    Select
	                    [CompanyID]			
	                    ,[QueueID]			
	                    ,[RunTy]				
	                    ,[RunPriorityTy]		
	                    ,[RunQueueStatusTy]	
	                    ,[RunID]				
	                    ,[TreeSnapshotID]   
	                    ,[VolumeSnapshotID] 
	                    ,[RunDescr]			
	                    ,[PeriodTy]			
	                    ,[PeriodID]			
	                    ,[PlanID]           
	                    ,[RevisionID]       
	                    ,[StartTime]			
	                    ,SYSDATETIMEOFFSET()			
	                    ,@ErrorMessage
	                    ,@StackTrace		
	                    ,[PercentComplete]  
	                    ,[ProcessLog]		
	                    ,[CreatedDate]      
	                    ,[CreatedBy]        
	                    ,[ModifiedDate]     
	                    ,[ModifiedBy]       
	                    ,[ProgName]     
                    From [dbo].[RunQueue]
                    Where
	                    [CompanyID] = @CompanyID and
	                    [QueueID] = @QueueID

                    Delete From [dbo].[RunQueue] Where [CompanyID] = @CompanyID and [QueueID] = @QueueID
                    ";
					cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = companyId;
					cmd.Parameters.Add("@QueueID", SqlDbType.Int).Value = queueId;
					cmd.Parameters.Add("@ErrorMessage", SqlDbType.NVarChar).Value = exc.Message;
					cmd.Parameters.Add("@StackTrace", SqlDbType.NVarChar, 512).Value = exc.StackTrace;

					cmd.ExecuteNonQuery();

				}
			}
			catch (Exception ex)
			{
				// log it somehow later
				Console.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="connStr"></param>
		/// <param name="companyId"></param>
		/// <param name="queueItem"></param>
		/// <param name="exc"></param>
		public static void LogQueueError(string connStr, int companyId, RunQueueItem queueItem, Exception exc)
		{
			try
			{
				using (var conn = new SqlConnection(connStr))
				using (var cmd = conn.CreateCommand())
				{
					conn.Open();

					cmd.CommandText = @"
                        Insert into [dbo].[LogCommRunner] (
	                     [CompanyID]			
	                    ,[QueueID]			
	                    ,[RunTy]				
	                    ,[RunPriorityTy]		
	                    ,[RunQueueStatusTy]	
	                    ,[RunID]				
	                    ,[TreeSnapshotID]   
	                    ,[VolumeSnapshotID] 
	                    ,[RunDescr]			
	                    ,[PeriodTy]			
	                    ,[PeriodID]			
	                    ,[PlanID]           
	                    ,[RevisionID]       
	                    ,[StartTime]			
	                    ,[ErrorTime]			
	                    ,[ErrorMessage]		
	                    ,[StackTrace]		
	                    ,[PercentComplete]  
	                    ,[ProcessLog]		
	                    ,[CreatedDate]      
	                    ,[CreatedBy]        
	                    ,[ModifiedDate]     
	                    ,[ModifiedBy]       
	                    ,[ProgName]         
                    )
                    Select
	                    [CompanyID]			
	                    ,[QueueID]			
	                    ,[RunTy]				
	                    ,[RunPriorityTy]		
	                    ,[RunQueueStatusTy]	
	                    ,[RunID]				
	                    ,[TreeSnapshotID]   
	                    ,[VolumeSnapshotID] 
	                    ,[RunDescr]			
	                    ,[PeriodTy]			
	                    ,[PeriodID]			
	                    ,@PlanId          
	                    ,@RevisionId     
	                    ,[StartTime]			
	                    ,SYSDATETIMEOFFSET()			
	                    ,@ErrorMessage
	                    ,@StackTrace		
	                    ,[PercentComplete]  
	                    ,[ProcessLog]		
	                    ,[CreatedDate]      
	                    ,[CreatedBy]        
	                    ,[ModifiedDate]     
	                    ,[ModifiedBy]       
	                    ,[ProgName]     
                    From [dbo].[RunQueue]
                    Where
	                    [CompanyID] = @CompanyID and
	                    [QueueID] = @QueueID

                    Delete From [dbo].[RunQueue] Where [CompanyID] = @CompanyID and [QueueID] = @QueueID
                    ";
					cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = companyId;
					cmd.Parameters.Add("@QueueID", SqlDbType.Int).Value = queueItem.QueueId;
					cmd.Parameters.Add("@PlanId", SqlDbType.Int).Value = queueItem.PlanId;
					cmd.Parameters.Add("@RevisionId", SqlDbType.Int).Value = queueItem.RevisionId;
					cmd.Parameters.Add("@ErrorMessage", SqlDbType.NVarChar).Value = exc.Message;
					cmd.Parameters.Add("@StackTrace", SqlDbType.NVarChar, 512).Value = exc.StackTrace;

					cmd.ExecuteNonQuery();

				}
			}
			catch (Exception ex)
			{
				// log it somehow later
				Console.WriteLine(ex.Message);
			}
		}
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}
