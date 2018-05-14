using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TeqTank.Services.Common.Caching;
using TeqTank.Services.Common.Properties;
using TeqTank.Services.Communications.WebCommunication;

namespace TeqTank.Services.Common.Authentication
{
	/// <summary>
	/// 
	/// </summary>
	public class AuthenticateAndAuthorize : BaseCommon
	{
		#region Fields
		#endregion Fields

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		public AuthenticateAndAuthorize()
		{ }
		#endregion Constructors

		#region Properties
		/// <summary>
		/// 
		/// </summary>
		public string CurrentBaseUrl { get; set; }
		#endregion Properties

		#region Methods
		/// <summary>
		/// Generates a TeqTank Token that allows for multiple companies to be logged into
		/// </summary>
		/// <param name="username">User name of user</param>
		/// <param name="password"></param>
		/// <returns></returns>
		public string GetTokenForUser(string username, string password)
		{
			var payload = new JObject(new JProperty("Email", username), new JProperty("Password", password)).ToString();

			var response = HttpHelpers.HttpPostResponse(ApiBaseUrl + @"users/Authenticating", payload);

			return response;
		}

		/// <summary>
		/// Retrieves the application data for the URL and token value provided.
		/// </summary>
		/// <param name="token"></param>
		/// <returns>The JSON string returned from the GET call.</returns>
		public static string GetApplication(string token)
		{
			var strResult = HttpHelpers.HttpGetResponse(ApiBaseUrl + $"Application", "", token);

			return JObject.Parse(strResult)["data"].ToString();

			// return JsonConvert.DeserializeObject<IEnumerable<VSProject>>(strResult)
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="configUsername"></param>
		/// <param name="configPwd"></param>
		/// <returns></returns>
		public string GetTokenForAzureUser(string configUsername, string configPwd)
		{
			var thresherToken = "";
			var key = "ThresherToken";

			if (CacheHelper.IsIncache(key))
			{
				thresherToken = CacheHelper.GetFromCache<string>(key);
			}
			else
			{
				thresherToken = GetTokenForUser(configUsername, configPwd);
				CacheHelper.SaveTocache(key, thresherToken, DateTime.UtcNow.AddMinutes(90));

			}
			return thresherToken;
		}
		#endregion Methods
	}
}
