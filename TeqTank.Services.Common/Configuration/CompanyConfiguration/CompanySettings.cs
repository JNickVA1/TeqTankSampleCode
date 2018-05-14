using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace TeqTank.Services.Common.Configuration.CompanyConfiguration
{
	/// <summary>
	/// 
	/// </summary>
    public class CompanySettings
    {
		#region Fields
		private string _connStr;
	    private DateTime _nextCheckTime;
	    private DateTime _lastModified;
	    private Dictionary<int, CompanySetting> _settings;
	    private int _companyId;
		#endregion Fields

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		/// <param name="connStr"></param>
		/// <param name="companyId"></param>
		public CompanySettings(string connStr, int companyId)
	    {
		    _connStr = connStr;
		    _companyId = companyId;
		    _settings = new Dictionary<int, CompanySetting>();
		    UpdateCompanySettings();
		    _lastModified = TableLastModified(connStr, "CompanySetting");
		    _nextCheckTime = DateTime.UtcNow.AddSeconds(5);
	    }
		#endregion Constructors

		#region Properties
		/// <summary>
		/// 
		/// </summary>
	    public Dictionary<int, CompanySetting> Settings
	    {
		    get
		    {
			    if (_nextCheckTime > DateTime.UtcNow)
				    return _settings;
			    else
			    {
				    var tableModifiedDate = TableLastModified(_connStr, "CompanySetting");
				    if (tableModifiedDate > _lastModified)
				    {
					    UpdateCompanySettings();
					    _lastModified = tableModifiedDate;
				    }

				    _nextCheckTime = DateTime.UtcNow.AddSeconds(5);
				    return _settings;
			    }
		    }
	    }
		#endregion Properties

		#region Methods
		/// <summary>
		/// 
		/// </summary>
	    private void UpdateCompanySettings()
	    {
		    using (var conn = new SqlConnection(_connStr))
		    using (var cmd = conn.CreateCommand())
		    {
			    conn.Open();
			    cmd.CommandText = @"
                        Select cs.CompanySettingTy, ISNULL(ty.Descr,''), ISNULL(ty.ShortDescr,''), cs.Value, ty.DataTy
                        from CompanySetting cs
                        Left Join TypeCompanySetting ty
                        on  ty.CompanyId = cs.CompanyId
                        and ty.CompanySettingTy = cs.CompanySettingTy
                        Where
	                        cs.CompanyId = @CompanyId
                    ";
			    cmd.Parameters.Add("@CompanyID", SqlDbType.Int).Value = _companyId;

			    _settings.Clear();

			    using (var rd = cmd.ExecuteReader())
			    {
				    while (rd.Read())
				    {
					    CompanySetting setting = new CompanySetting();
					    setting.CompanySettingTypeId = rd.GetInt32(0);
					    setting.Description = rd.GetString(1);
					    setting.ShortDescription = rd.GetString(2);
					    setting.Value = rd.GetString(3);
					    setting.DataTypeId = rd.GetInt32(4);

					    _settings.Add(setting.CompanySettingTypeId, setting);
				    }
			    }
		    }
	    }

	    /// <summary>
	    /// http://dba.stackexchange.com/questions/12749/finding-the-last-time-a-table-was-updated
	    /// </summary>
	    /// <param name="connStr"></param>
	    /// <param name="tables"></param>
	    /// <returns></returns>
	    private DateTime TableLastModified(string connStr, params string[] tables)
	    {
		    StringBuilder sb = new StringBuilder(@"
                Select  GetDate() as ServerTime,
                        max(last_user_update) as LastUpdated,
                        count(*) as RecordCount from sys.dm_db_index_usage_stats
                Where   database_id     = db_id()
                And     object_id in (");

		    for (int i = 0; i < tables.Length; i++)
		    {
			    if (i > 0)
				    sb.Append(",");
			    sb.Append(string.Format("object_id(N'{0}')", tables[i]));
		    }
		    sb.Append(")");

		    using (var conn = new SqlConnection(connStr))
		    {
			    conn.Open();
			    using (var cmd = new SqlCommand(sb.ToString(), conn))
			    using (var rd = cmd.ExecuteReader())
			    {
				    rd.Read();

				    if (rd.IsDBNull(1) && rd.GetInt32(2) > 0)
					    return DateTime.MinValue.AddDays(1);
				    else if (rd.IsDBNull(1))
					    return rd.GetDateTime(0);
				    else
					    return rd.GetDateTime(1);
			    }
		    }
	    }
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}
