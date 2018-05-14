namespace TeqTank.Services.Common.Configuration.CompanyConfiguration
{
	/// <summary>
	/// The <see cref="CompanyConfiguration"/> class defines properties that correspond to 
	/// the Company section in the application configuration file.
	/// </summary>
	public class CompanyConfiguration
	{
		#region Fields
		#endregion Fields

		#region Constructors
		#endregion Constructors

		#region Properties
		/// <summary>
		/// The identifier used to universally refer to one specific company.
		/// </summary>
		public int CompanyId { get; set; }
		/// <summary>
		/// The identifier used to refer to a company subsidiary. 
		/// </summary>
		public int CompanySubId { get; set; }
		/// <summary>
		/// The identifier used to refer to a company's commission plan. 
		/// </summary>
		public int PlanId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string CompanyName { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string CompanyKey { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string Username { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string Pwd { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string CryptKey { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string AuthKey { get; set; }
		#endregion Properties

		#region Methods
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}
