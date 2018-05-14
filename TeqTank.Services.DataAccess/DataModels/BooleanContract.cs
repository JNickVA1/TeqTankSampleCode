using System;

namespace TeqTank.Services.DataAccess.DataModels
{
	/// <summary>
	/// 
	/// </summary>
    public class BooleanContract
    {       
		/// <summary>
		/// 
		/// </summary>
        public bool Data { get; set; }
		/// <summary>
		/// 
		/// </summary>
        public string DataHeader { get; set; }
		/// <summary>
		/// 
		/// </summary>
        public bool Success { get; set; }
		/// <summary>
		/// 
		/// </summary>
        public string ErrorMessage { get; set; }
		/// <summary>
		/// 
		/// </summary>
        public string ErrorCd { get; set; }
		/// <summary>
		/// 
		/// </summary>
        public Guid Transaction { get; set; }
    }
}
