using Newtonsoft.Json;
using TeqTank.Services.Common.Properties;
using TeqTank.Services.Communications.WebCommunication;

namespace TeqTank.Services.Common.Configuration.ProjectConfiguration
{
	/// <summary>
	/// 
	/// </summary>
    public class VsProjectDll : BaseCommon
	{
		#region Fields
		#endregion Fields

		#region Constructors
		#endregion Constructors

		#region Properties
		/// <summary>
		/// 
		/// </summary>
		public byte[] Dll { get; set; }
		/// <summary>
		/// 
		/// </summary>
	    public string PlanName { get; set; }
		#endregion Properties

		#region Methods
	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="projectId"></param>
	    /// <param name="revisionId"></param>
	    /// <param name="token"></param>
	    /// <returns></returns>
	    public VsProjectDll GetPlanDll(int projectId, int revisionId, string token)
	    {
		    var strResult = HttpHelpers.HttpGetResponse(ApiBaseUrl + $"VSProjects/{projectId}/{revisionId}", "", token);
		    return JsonConvert.DeserializeObject<VsProjectDll>(strResult);
	    }
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}
