using System;
using System.Runtime.Caching;

namespace TeqTank.Services.Common.Caching
{
	/// <summary>
	/// The CacheHelper class contains methods to: store to, read from, delete from a MemoryCache. 
	/// </summary>
    public static class CacheHelper
    {
		/// <summary>
		/// 
		/// </summary>
        public enum CacheObjectType
        {
			/// <summary>
			/// 
			/// </summary>
            Assembly,
			/// <summary>
			/// 
			/// </summary>
            ProjectName
        }

		#region Fields
		#endregion Fields

		#region Constructors
		#endregion Constructors

		#region Properties
		#endregion Properties

		#region Methods

	    /// <summary>
	    /// Saves an Object To Cache
	    /// </summary>
	    /// <param name="companyId"></param>
	    /// <param name="projectId"></param>
	    /// <param name="revisionId"></param>
	    /// <param name="saveItem"></param>
	    /// <param name="type"></param>
	    public static void SaveToCache(int companyId, int projectId, int revisionId, object saveItem, CacheObjectType type)
	    {
		    SaveTocache(GenerateCacheKey(companyId, projectId, revisionId, type), saveItem, DateTime.UtcNow.Add(TimeSpan.FromHours(4)));
	    }

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="companyId"></param>
		/// <param name="projectId"></param>
		/// <param name="revisionId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
	    public static T GetFromCache<T>(int companyId, int projectId, int revisionId, CacheObjectType type) where T : class
	    {
		    return MemoryCache.Default[GenerateCacheKey(companyId, projectId, revisionId, type)] as T;
	    }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="projectId"></param>
		/// <param name="revisionId"></param>
		/// <param name="type"></param>
	    public static void RemoveFromCache(int companyId, int projectId, int revisionId, CacheObjectType type)
	    {
		    RemoveFromCache(GenerateCacheKey(companyId, projectId, revisionId, type));
	    }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="projectId"></param>
		/// <param name="revisionId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
	    public static bool IsIncache(int companyId, int projectId, int revisionId, CacheObjectType type)
	    {
		    return IsIncache(GenerateCacheKey(companyId, projectId, revisionId, type));
	    }
	    /// <summary>
	    /// Returns 
	    /// </summary>
	    /// <param name="companyId"></param>
	    /// <param name="projectId"></param>
	    /// <param name="revisionId"></param>
	    /// <param name="type"></param>
	    /// <returns></returns>
	    internal static string GenerateCacheKey(int companyId, int projectId, int revisionId, CacheObjectType type)
	    {
		    return $"cId:{companyId}_pId:{projectId}_rId:{revisionId}_t:{type.ToString()}";
	    }
	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="cacheKey"></param>
	    /// <param name="savedItem"></param>
	    /// <param name="absoluteExpiration"></param>
	    public static void SaveTocache(string cacheKey, object savedItem, DateTime absoluteExpiration)
	    {
		    MemoryCache.Default.Add(cacheKey, savedItem, absoluteExpiration);
	    }

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <typeparam name="T"></typeparam>
	    /// <param name="cacheKey"></param>
	    /// <returns></returns>
	    public static T GetFromCache<T>(string cacheKey) where T : class
	    {
		    return MemoryCache.Default[cacheKey] as T;
	    }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cacheKey"></param>
	    private static void RemoveFromCache(string cacheKey)
	    {
		    MemoryCache.Default.Remove(cacheKey);
	    }

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="cacheKey"></param>
	    /// <returns></returns>
	    public static bool IsIncache(string cacheKey)
	    {
		    return MemoryCache.Default[cacheKey] != null;
	    }
	    #endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}

