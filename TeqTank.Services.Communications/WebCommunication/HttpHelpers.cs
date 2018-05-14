using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TeqTank.Services.Communications.WebCommunication
{
	/// <summary>
	/// 
	/// </summary>
	public class HttpHelpers
	{
		#region Fields
		#endregion Fields

		#region Constructors
		#endregion Constructors

		#region Properties
		#endregion Properties

		#region Methods
		/// <summary>
		/// Makes a GET call to the server at the URL provided.
		/// </summary>
		/// <param name="url"></param>
		/// <param name="payload"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public static string HttpGetResponse(string url, string payload = "", string token = "")
		{
			return HttpResponse(url, "GET", payload, token);
		}

		/// <summary>
		/// Makes a POST call to the server at the URL provided.
		/// </summary>
		/// <param name="url"></param>
		/// <param name="payload"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public static string HttpPostResponse(string url, string payload = "", string token = "")
		{
			return HttpResponse(url, "POST", payload, token);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <param name="method"></param>
		/// <param name="payload"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private static string HttpResponse(string url, string method, string payload, string token)
		{
			// attempt to use the highest tls protocal first
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
			var catchWebEx = false; // leave this right now

			var postData = new StringBuilder();

			var req = (HttpWebRequest)WebRequest.Create(url);
			req.Timeout = 270000;
			req.Method = method;
			req.ContentType = "application/json";
			req.Accept = "application/json";
			req.ServicePoint.ConnectionLimit = 100;
			if (!string.IsNullOrWhiteSpace(token))
			{
				req.Headers.Add("Authorization", token);
			}


			Stream requestStream = null;
			if (payload.Length > 0)
			{
				byte[] bts = Encoding.UTF8.GetBytes(payload);
				req.ContentLength = bts.Length;// xml.Length;
				requestStream = req.GetRequestStream();
				requestStream.Write(bts, 0, bts.Length);
				requestStream.Flush();  //added in to see about eliminating the tls stream error
										//requestStream.Close();
			}
			else
			{
				req.ContentLength = 0;
			}

			string result = "";
			HttpWebResponse res = null;

			try
			{
				res = (HttpWebResponse)req.GetResponse();
				using (var responseStream = new StreamReader(res.GetResponseStream()))
				{
					result = responseStream.ReadToEnd();
					responseStream.Close();
				}
			}
			catch (WebException e)
			{
				if (!catchWebEx)
					throw;
				using (WebResponse response = e.Response)
				{
					HttpWebResponse httpResponse = (HttpWebResponse)response;
					using (Stream data = response.GetResponseStream())
					using (var reader = new StreamReader(data))
					{
						result = reader.ReadToEnd();
					}
				}
			}
			return result;
		}
		#endregion Methods
	}
}
