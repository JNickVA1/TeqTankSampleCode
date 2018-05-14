using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TeqTank.Services.Common.Authentication;
using TeqTank.Services.Common.Caching;
using TeqTank.Services.Common.Cryptography;

namespace TeqTank.Services.Common.Configuration
{
	/// <summary>
	/// 
	/// </summary>
    public class ApplicationContract
    {
		#region Fields
		#endregion Fields

		#region Constructors
		#endregion Constructors

		#region Properties
		/// <summary>
	    /// 
	    /// </summary>
	    public int CompanyId { get; set; }
	    /// <summary>
	    /// 
	    /// </summary>
	    public int ApplicationTy { get; set; }
	    /// <summary>
	    /// 
	    /// </summary>
	    public string AppVersion { get; set; }
	    /// <summary>
	    /// 
	    /// </summary>
	    public int DatabaseTy { get; set; }
	    /// <summary>
	    /// 
	    /// </summary>
	    public string DatabaseVersion { get; set; }
	    /// <summary>
	    /// 
	    /// </summary>
	    public string CryptUid { get; set; }
	    /// <summary>
	    /// 
	    /// </summary>
	    public string CryptPwd { get; set; }
	    /// <summary>
	    /// 
	    /// </summary>
	    public string CryptDb { get; set; }
	    /// <summary>
	    /// 
	    /// </summary>
	    public string CryptServer { get; set; }
	    /// <summary>
	    /// 
	    /// </summary>
	    public string Port { get; set; }
		#endregion Properties

		#region Methods
		/// <summary>
	    /// 
	    /// </summary>
	    /// <returns></returns>
	    public string GenerateConnectionStr()
	    {
		    return
			    $"Server={CryptServer}{(string.IsNullOrWhiteSpace(Port) ? "" : $",{Port}")};database={CryptDb};uid={CryptUid};pwd={CryptPwd};";
	    }

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="token"></param>
	    /// <param name="companyId"></param>
	    /// <param name="cryptKey"></param>
	    /// <param name="authKey"></param>
	    /// <returns></returns>
	    public string GetConnectionString(string token, int companyId, string cryptKey, string authKey)
	    {
		    var connStr = "";
		    var key = $"ConnStr_CompanyId{companyId}";

		    if (CacheHelper.IsIncache(key))
		    {
			    connStr = CacheHelper.GetFromCache<string>(key);
		    }
		    else
		    {
			    string encryptStr = AuthenticateAndAuthorize.GetApplication(token);
			    var dencrypt = AesThenHmac.SimpleDecrypt(encryptStr, Convert.FromBase64String(cryptKey), Convert.FromBase64String(authKey));
			    var list = JsonConvert.DeserializeObject<List<ApplicationContract>>(dencrypt);

			    //_makoToken = api.AuthenticateUserForApp(_companyId, 1, _thresherToken);
			    var connList = list.FirstOrDefault(l => l.CompanyId == companyId && l.ApplicationTy == 1);
			    if (connList != null) connStr = connList.GenerateConnectionStr();

			    if (connStr != null) CacheHelper.SaveTocache(key, connStr, DateTime.UtcNow.AddMinutes(5));
		    }

		    return connStr;
	    }
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}
