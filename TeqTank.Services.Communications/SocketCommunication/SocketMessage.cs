using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeqTank.Services.Communications.SocketCommunication
{
	/// <summary>
	/// 
	/// </summary>
	public class SocketMessage : IMessage
	{
		/// <summary>
		/// 
		/// </summary>
		public int CompanyId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public int Percentage { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public int QueueId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public int PeriodType { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public int PeriodId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public DateTimeOffset Processtime { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string Information { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public bool IsRun { get; set; }
	}
}
