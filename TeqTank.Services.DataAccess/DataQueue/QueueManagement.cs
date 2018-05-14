using System;
using System.Data;
using System.Data.SqlClient;

namespace TeqTank.Services.DataAccess.DataQueue
{
	/// <summary>
	/// 
	/// </summary>
	public class QueueManagement
    {
		#region Fields
		#endregion Fields

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="connStr"></param>
		public QueueManagement(int companyId, string connStr)
	    {
		    CompanyId = companyId;
		    ConnString = connStr;
		    DelayTimeSpan = new TimeSpan(0, 0, 300);
	    }
		#endregion Constructors

		#region Properties
		/// <summary>
		/// 
		/// </summary>
		internal int CompanyId { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    internal string ConnString { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    internal TimeSpan DelayTimeSpan { get; set; }
		#endregion Properties

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="queueItem"></param>
		/// <returns></returns>
		public bool FindNextQueueItem(out RunQueueItem queueItem)
		{
			queueItem = null;
			CleanUpQueue();
			InsertRealtimePush();

			try
			{
				using (var conn = new SqlConnection(ConnString))
				using (var cmd = conn.CreateCommand())
				{
					conn.Open();
					cmd.CommandText = @"    Declare @RunTy int = 0
                                            Declare @QueueID bigint

		                                    Select top 1    @QueueId    = QueueID, 
                                                            @RunTy      = RunTy     from RunQueue  
		                                    Where   CompanyID           = @CompanyID  
		                                    And     RunQueueStatusTy    = 1
		                                    And     StartTime           < @CurrentTime
		                                    AND     ProcessMachine      is NULL
		                                    Order by StartTime asc

                                            Update RunQueue 
                                                set     RunQueueStatusTy    = 2, 
                                                        ProcessMachine      = @ProcessMachine
		                                    Where QueueID = @QueueId 
		                                    And CompanyID = @companyID 

                                            Select  a.QueueID,                
			                                        a.RunID,                  
			                                        a.PeriodTy,               
			                                        a.PeriodID,               
			                                        a.RunDescr,               
			                                        b.Descr,                    
			                                        a.TreeSnapshotID,          
			                                        a.VolumeSnapshotID,        
			                                        a.StartTime,              
			                                        case when a.PlanID != 0     then a.PlanId
                                                                                else Cast(c.Value as int) end as PlanId,    
                                                    case when a.RevisionID != 0 then a.RevisionID
                                                                                else Cast(d.Value as int) end as RevisionID,             
			                                        a.CreatedBy,                
			                                        a.RunTy from RunQueue  a 
		                                    inner join TypeRunQueueStatus b     on      a.CompanyID         = b.CompanyID
											                                    and     a.RunQueueStatusTy  = b.RunQueueStatusTy
                                            inner join CompanySetting c         on      a.CompanyID         = c.CompanyId
                                                                                and     c.CompanySettingTy  = 2
                                            inner join CompanySetting d         on      a.CompanyID         = d.CompanyID
                                                                                and     d.CompanySettingTy  = 3
		                                    Where   a.CompanyID     = @companyID 
                                            and     a.QueueID       = @QueueId  
                    ";

					cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = CompanyId;
					cmd.Parameters.Add("@ProcessMachine", SqlDbType.VarChar).Value = Environment.MachineName;
					cmd.Parameters.Add("@StartTime", SqlDbType.DateTimeOffset).Value = DateTimeOffset.Now.Add(DelayTimeSpan);
					cmd.Parameters.Add("@CurrentTime", SqlDbType.DateTimeOffset).Value = DateTimeOffset.Now;

					using (var rd = cmd.ExecuteReader())
					{
						if (!rd.Read())
						{
							Console.WriteLine("No items found in the RunQueue");
							return false;
						}

						queueItem = new RunQueueItem
						{
							QueueId = rd.GetInt32(0),
							RunId = rd.GetInt32(1),
							PeriodTy = rd.GetInt32(2),
							PeriodId = rd.GetInt32(3),
							RunDescr = rd.GetString(4),
							Descr = rd.GetString(5),
							TreeSnapshotId = rd.GetInt32(6),
							VolumeSnapshotId = rd.GetInt32(7),
							StartDate = rd.GetDateTimeOffset(8),
							PlanId = rd.GetInt32(9),
							RevisionId = rd.GetInt32(10),
							RunTy = rd.GetInt32(12)
						};

						Console.WriteLine($"Found QueueId({queueItem.QueueId}) and processing it.");

						InsertRealtimePush();
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		internal void InsertRealtimePush()
		{
			using (var conn = new SqlConnection(ConnString))
			{
				using (var cmd = conn.CreateCommand())
				{
					conn.Open();
					cmd.CommandText = @"   
                                            if not exists (     Select  QueueID from RunQueue
                                                                Where   CompanyId   = @CompanyID
                                                                And     RunTy       = 4
                                                                And     RunQueueStatusTy = 1    )
                                            
                                            Begin
                                                Declare @QueueId bigint = 0
		                                        exec sMako_GetNextIdentity @CompanyId = @CompanyID, @Code = 'RunQueueID', @Value =  @QueueId out

                                                INSERT INTO RunQueue
                                                (   CompanyID, QueueID, RunPriorityTy, RunID, RunTy, RunDescr, RunQueueStatusTy, PeriodTy, PeriodID,
                                                    PlanID, RevisionID, StartTime, CreatedDate, CreatedBy, ModifiedDate, ModifiedBy, ProgName, ProcessMachine )
                                                VALUES
						                        (   @CompanyID,
                                                    @QueueID,
                                                    0,
                                                    0,
                                                    4,
                                                    'RealTime Volume Push',
                                                    1,
                                                    0,
                                                    0,
                                                    0,
                                                    0,
                                                    @StartTime,
                                                    SysDatetimeOffset(),
                                                    'MakoRunner',
                                                    SysDatetimeOffset(),
                                                    'MakoRunner',
                                                    'MakoRunner',
                                                    null
                                                )
                                            End
                    ";

					cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = CompanyId;
					cmd.Parameters.Add("@StartTime", SqlDbType.DateTimeOffset).Value = DateTimeOffset.Now.Add(DelayTimeSpan);
					cmd.ExecuteNonQuery();
				}
			}
		}

		/// <summary>
		/// Delete all records for the Company ID used in the instance construction and on the local machine.
		/// </summary>
		public void ClearQueue()
		{
			try
			{
				using (var conn = new SqlConnection(ConnString))
				using (var cmd = conn.CreateCommand())
				{
					conn.Open();
					cmd.CommandText = @"  DELETE FROM [dbo].[RunQueue] 
                                            WHERE CompanyID = @CompanyID 
                                            AND ProcessMachine = @ProcessMachine
                    ";

					cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = CompanyId;
					cmd.Parameters.Add("@ProcessMachine", SqlDbType.VarChar).Value = Environment.MachineName;
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
		internal void CleanUpQueue()
		{
			using (var conn = new SqlConnection(ConnString))
			{
				using (var cmd = conn.CreateCommand())
				{
					conn.Open();
					cmd.CommandText = @"   --*******************************************************************************************
                                            --Idea here is that if we are looking for a run to process, we should not have one processing
                                            --*******************************************************************************************
                                            DELETE RunQueue 
                                            WHERE   RunQueueStatusTy = 2 
                                            and     ( ProcessMachine = @ProcessMachine or ProcessMachine is null )

                                            --*******************************************************************************************
                                            --We should never have more than 1 Realtime push pending.
                                            --*******************************************************************************************
                                            Declare @QueueID		int
                                            Set @QueueID = (	Select top 1 QueueID from RunQueue
					                                            where CompanyID		= @CompanyID
					                                            And		RunTy		= 4
					                                            Order by StartTime Asc				)

                                            Delete from RunQueue
                                            Where	CompanyID	= @CompanyId
                                            And		RunTy		= 4
                                            And		QueueID		!= @QueueID

                                            Select  Cast(Value as int) from CompanySetting
                                            Where   CompanyId           = @CompanyID
                                            And     CompanySettingTy    = 9
                    ";

					cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = CompanyId;
					cmd.Parameters.Add("@ProcessMachine", SqlDbType.VarChar).Value = Environment.MachineName;
					var scalarAmt = cmd.ExecuteScalar();
					DelayTimeSpan = new TimeSpan(0, 0, (scalarAmt == null) ? 300 : (int)scalarAmt);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="queueId"></param>
		public void DeleteQueueItem(int queueId)
		{
			try
			{
				using (var conn = new SqlConnection(ConnString))
				using (var cmd = conn.CreateCommand())
				{
					conn.Open();
					cmd.CommandText = @"    DELETE FROM [dbo].[RunQueue] 
                                            WHERE   CompanyID   = @CompanyID 
                                            AND     QueueID     = @QueueID
                    ";

					cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = CompanyId;
					cmd.Parameters.Add("@QueueID", SqlDbType.Int).Value = queueId;
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
		/// <param name="exc"></param>
		public void LogQueueError(string connStr, int companyId, int queueId, Exception exc)
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
	    /// <param name="runId"></param>
	    /// <param name="startTime"></param>
	    /// <param name="endTime"></param>
	    public void UpdateRunStartAndEndDate(string connStr, int companyId, int runId, DateTimeOffset startTime, DateTimeOffset endTime)
	    {
		    try
		    {
			    using (var conn = new SqlConnection(connStr))
			    using (var cmd = conn.CreateCommand())
			    {
				    conn.Open();
				    cmd.CommandText = $@"   Update a
                                                Set     StartDate       = @StartTime,
                                                        CompletedDate   = @EndTime from Run a
                                            Where   CompanyID   = @CompanyID
                                            And     RunId       = @RunID ";

				    cmd.Parameters.Add("@CompanyId", SqlDbType.Int).Value = companyId;
				    cmd.Parameters.Add("@RunId", SqlDbType.Int).Value = runId;
				    cmd.Parameters.Add("@StartTime", SqlDbType.DateTimeOffset).Value = startTime;
				    cmd.Parameters.Add("@EndTime", SqlDbType.DateTimeOffset).Value = endTime;

				    cmd.ExecuteNonQuery();
			    }
		    }
		    catch (Exception ex)
		    {
			    Console.WriteLine("Error updating start and end time");
			    Console.Write(ex.Message);
		    }
	    }
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers


	}
}
