using System;
using System.Collections.Generic;

namespace TeqTank.Services.DataAccess.DataQueue
{
	/// <summary>
	/// 
	/// </summary>
    public class RunQueueItem
    {
		#region Fields
		#endregion Fields

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		public RunQueueItem()
	    {
		    Links = new List<DataModels.LinkModel>();
	    }
		#endregion Constructors

		#region Properties
		/// <summary>
		/// 
		/// </summary>
		public List<DataModels.LinkModel> Links { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    public int QueueId { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    public int RunId { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    public int PeriodTy { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    public int PeriodId { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    public string RunDescr { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    public string Descr { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    public string RunQueueStatus { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    public int TreeSnapshotId { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    public int VolumeSnapshotId { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    public DateTimeOffset StartDate { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    public int PlanId { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    public int RevisionId { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    public string User { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    public int RunPriorityTy { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    public int RunTy { get; set; }
		#endregion Properties

		#region Methods
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}
