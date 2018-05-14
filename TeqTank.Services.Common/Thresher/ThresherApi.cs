using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeqTank.Services.Common.Thresher.Model;

namespace TeqTank.Services.Common.Thresher
{
	/// <summary>
	/// 
	/// </summary>
    public class ThresherApi
    {
        private string _baseUrl;

        public ThresherApi()
        {
            _baseUrl = @"http://thresherapi.azurewebsites.net/api/";
        }

        #region Applications

        public string GetApplication(string token)
        {
            var strResult = HttpGetResponse(_baseUrl + $"Application", "", token);

            return JObject.Parse(strResult)["data"].ToString();

            // return JsonConvert.DeserializeObject<IEnumerable<VSProject>>(strResult)
        }

        #endregion

        #region VsProject API Calls

        /// <summary>
        /// Returns a list of VsProjects that the user has access to
        /// </summary>
        /// <param name="applicationTy">The TeqTank Application Type ID</param>
        /// <param name="token">The Thresher JWT token</param>
        /// <returns>List of VsProjects that the authenticated user has access to</returns>
        public IEnumerable<VSProject> GetVsProjectsByApplicationType(int applicationTy, string token)
        {
            var strResult = HttpGetResponse(_baseUrl + $"VSProjects//Application//{applicationTy}", "", token);
            return JsonConvert.DeserializeObject<IEnumerable<VSProject>>(strResult);
        }

        /// <summary>
        /// Returns a list of files and file path for a particular VsProject
        /// </summary>
        /// <param name="projectId">Unique Identifier for project</param>
        /// <param name="token">The Thresher JWT token</param>
        /// <returns></returns>
        public IEnumerable<VsProjectFile> GetVsProjectFileList(int projectId, string token)
        {
            var strResult = HttpGetResponse(_baseUrl + $"VSProjects//GetLatestFiles//{projectId}", "", token);
            return JsonConvert.DeserializeObject<IEnumerable<VsProjectFile>>(strResult);
        }

        /// <summary>
        /// Checks out a project and returns the list of latest files
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public IEnumerable<VsProjectFile> CheckOutVsProject(int projectId, string token)
        {
            var requestJson = new JObject(
                new JProperty("ProcessMachine", Environment.MachineName), 
                new JProperty("ForkId", 0)).ToString();
            var strResult = HttpPostResponse(_baseUrl + $"VSProjects//CheckOut//{projectId}", requestJson, token);
            return JsonConvert.DeserializeObject<IEnumerable<VsProjectFile>>(strResult);
        }

        /// <summary>
        /// Creates a new Project and returns the files for the project
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="projectTy"></param>
        /// <param name="planName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public IEnumerable<VsProjectFile> CreateNewVsProject(int companyId, int projectTy, string planName, string token)
        {
            var requestJson = new JObject(
                new JProperty("CompanyId", companyId),
                new JProperty("Name", planName),
                new JProperty("ProcessMachine", Environment.MachineName),
                new JProperty("ProjectTy", projectTy)
                ).ToString();

            var strResult = HttpPostResponse(_baseUrl + @"VSProjects/CreateProject", requestJson, token);
            return JsonConvert.DeserializeObject<IEnumerable<VsProjectFile>>(strResult);
        }

        /// <summary>
        /// This needs to be reworked
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="token"></param>
        public void UndoCheckoutVsProject(int projectId, string token)
        {
            var resultStr = HttpGetResponse(_baseUrl + @"VSProjects/UndoCheckOut/" + projectId, token);
        }

        public VsProjectDll GetPlanDll(int projectId, int revisionId, string token)
        {
            var strResult = HttpGetResponse(_baseUrl + $"VSProjects/{projectId}/{revisionId}", "", token);
            return JsonConvert.DeserializeObject<VsProjectDll>(strResult);
        }

        #endregion

        #region Tokens API Calls

        /// <summary>
        /// Generates a TeqTank Token that allows for multiple companies to be logged into
        /// </summary>
        /// <param name="usernmae">Username of user</param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string AuthenticateUser(string usernmae, string password)
        {
            var payload = new JObject(new JProperty("Email", usernmae), new JProperty("Password", password)).ToString();

            var response = HttpPostResponse(_baseUrl + @"users/Authenticating",  payload);

            return response;
        }

        /// <summary>
        /// Authenticates for a specific app
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="applicationTy"></param>
        /// <param name="token"></param>
        public string AuthenticateUserForApp(int companyId, int applicationTy, string token)
        {
            var payload = new JObject(new JProperty("CompanyID", companyId), new JProperty("ApplicationTy", applicationTy)).ToString();

            var response = HttpPostResponse(_baseUrl + @"users/AuthenticateCompanyApp", payload, token);

            return response;
        }

        #endregion

        #region Support Methods

        private string HttpGetResponse(string url, string payload = "", string token = "")
        {
            return HttpResponse(url, "GET", payload, token);
        }

        private string HttpPostResponse(string url, string payload = "", string token = "")
        {
            return HttpResponse(url, "POST", payload, token);
        }

        private string HttpResponse(string url, string method, string payload, string token)
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

        #endregion
    }
}
