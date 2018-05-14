using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Hangfire;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using TeqTank.Applications.Mako;
using TeqTank.Services.Common.Authentication;
using TeqTank.Services.Common.Commands;
using TeqTank.Services.Common.Configuration;
using TeqTank.Services.Common.Configuration.CompanyConfiguration;
using TeqTank.Services.Common.Cryptography;
using TeqTank.Services.Common.EnumsAndConstants;
using TeqTank.Services.DataAccess.DataQueue;
using TeqTank.Services.MakoRunners.Properties;
using Unity;

namespace TeqTank.Services.MakoRunners.Runners
{
	/// <summary>
	/// The AzureMakoRunner uses an Azure WebJob to perform the tasks, such as RunCommissions.
	/// </summary>
    public class AzureMakoRunner : BaseMakoRunner
    {
		#region Fields
		#endregion Fields

		#region Constructors
		/// <summary>
		/// The default constructor for the Azure Mako Runner.
		/// </summary>
		/// <param name="keyAzure">A unique identifier for the Runner.</param>
		/// <param name="consoleCommand">The command that will be used in creating and starting the Runner.</param>
		public AzureMakoRunner(Guid keyAzure, ConsoleCommand consoleCommand) : base(keyAzure, consoleCommand)
	    {
		    // Read the boolean setting to determine whether or not we are allowed to run
		    // multiple Azure WebJobs in parallel. 
		    //
		    // Use with discretion and only if you know what you're doing. Just because 
		    // you CAN run parallel tasks doesn't always mean you should.
		    CanRunInParallel = Settings.Default.RunAzureJobaInParallel;

		    // Set the Mako Runner type.
			Info.ServiceType = RunnerType.AzureWebJob;
	    }
		#endregion Constructors

		#region Properties
		/// <summary>
	    /// 
	    /// </summary>
	    public CompanySettings CompanySettingsForId { get; set; }

		/// <summary>
		/// 
		/// </summary>
	    public string Token { get; set; }
		#endregion Properties

		#region Methods

		/// <inheritdoc />
		/// <summary>
		/// The Start method will create a JobHost, retrieve the necessary data for each Company ID
		/// in the RunnerInfo object, create a QueueManager for each Company ID, and perform other 
		/// WebJob startup tasks specific to the Azure Mako Runner.
		/// </summary>
		public override void Start()
	    {
			// Make sure that the RunnerInfo object is non-null.
			if (Info == null)
				throw new ApplicationException($"RunnerInfo has not been allocated for Runner with ID: {MyRunnerId}");

			// We will create and start Tasks based on the values in the RunnerInfo object.
			// NOTE: For the initial version of this code, we will process one or more Company IDs 
			//		 as Parallel method calls in the Azure JobHost.
			if (Info.SequentialOrParallel == ProcessingOrder.Sequential)
			{
				// TODO: Determine if we actually need this code.

				// All company IDs will be processed sequentially.
				foreach (var infoCompanyId in Info.CompanyIds)
				{
					try
					{
						// Retrieve the encoded Company Configuration data from the Settings.
						var encodedCompanyConfig = Settings.Default["CompanyId" + infoCompanyId].ToString();

						// Convert the encoded company configuration value to a byte array.
						var data = Convert.FromBase64String(encodedCompanyConfig);

						// Decode the byte array to a string.
						var decodedString = Encoding.UTF8.GetString(data);

						// Deserialize the decoded string to a CompanyConfig object.
						var config = JsonConvert.DeserializeObject<CompanyConfig>(decodedString);

						//// Create a ProgressInfo object.
						//var runnerProgress = new ServiceProgressInfo();

						//// Create a new CancellationTokenSource.
						//var tokenSource = new CancellationTokenSource();

						//// Create a Cancellation Token.
						//var cancelRunner = tokenSource.Token;

						//// Create and start a Task using the Windows service library currently being used.
						//Task<ServiceTaskResult> newTask = new Task<ServiceTaskResult>(() =>
						//	RunWindowsService(cancelRunner, config, runnerProgress), cancelRunner, TaskCreationOptions.LongRunning);

						//// Store the Task in the RunerInfo for this Mako Runner.
						//if (Info.RunnerTaskWithResult != null)
						//{
						//	// Add the Task to the List of this type of Task.
						//	Info.RunnerTaskWithResult.Add(newTask);

						//	// Save the Task and associated CancellationTokenSource.
						//	Info.TaskTokenSources.Add(newTask, tokenSource);

						//	// Start the Task.
						//	newTask.Start();
						//}
						//else
						//	throw new ApplicationException($"The RunnerInfo object's RunnerTaskWithResult property has not been initialized.");
					}
					catch (Exception e)
					{
						// TODO: Instead of throwing an exception and breaking the loop, log the error and continue.
						throw new ApplicationException("Error starting WebJob for company ID" + infoCompanyId, e);
					}
				}
			}
			else if (Info.SequentialOrParallel == ProcessingOrder.Parallel && CanRunInParallel)
			{
				// All company IDs will be processed in parallel, up to a predefined maximum number of parallel tasks.
				// TODO: Define maximum number of parallel tasks and add to read that value.

				try
				{
					//
					var jobHostConfig = new JobHostConfiguration();

					// Set a flag that we will use for code execution decisions.
					// TODO: 1. Make sure we can access the JobHostConfiguration before creating the JobHost.
					// TODO: 2. Find a better way of doing this, like putting all config decisions in one place.
					if (jobHostConfig.IsDevelopment)
					{
						jobHostConfig.UseDevelopmentSettings();
						IsDevelopment = true;
					}
					else
						IsDevelopment = false;

					// Create an Azure JobHost.
					var host = new JobHost(jobHostConfig);

					// Start the JobHost.
					host.Start();

					// All company IDs will be processed sequentially.
					// NOTE: For the time being, and until this can be tested and verified, we will only
					//		 allow one Company ID to be passed to the Azure Runner.
					// TODO: Test to determine if we can indeed have multiple calls to the JobHost Call method.
					foreach (var infoCompanyId in Info.CompanyIds)
					{
						// Get the runtime values for the current Company ID
						var runtimeValues = GetCompanyRunnerInfo(infoCompanyId);

						// Loop endlessly (for the time being - we need to re-think this).
						// TODO: After researching the mechanism for making the JobHost Call, try to rewrite this.
						while (true)
						{
							//
							if (runtimeValues["queueManager"] is QueueManagement qMgr && qMgr.FindNextQueueItem(out RunQueueItem item))
							{
								// check to make sure the Mako Admin is up to date
								if (item.PlanId != LastPlanId || item.RevisionId != LastRevisionId)
								{
									UpdateMakoAdmin(item, MakoCache, MakoLogger);
								}

								//
								runtimeValues["item"] = item;

								//
								runtimeValues["makoAdmin"] = Admin;

							}

							// Call the WebJob method with the runtime values.
							host.Call(typeof(AzureWebJob).GetMethod("RunCommissions"), runtimeValues);
						}
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}
			}
			else
			{
				throw new ApplicationException("Error in ServerMakoRunner.Start(). Invalid processing order.");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="infoCompanyId"></param>
		/// <returns></returns>
		private Dictionary<string, object> GetCompanyRunnerInfo(string infoCompanyId)
		{
			//
			Dictionary<string, object> dict;

			try
			{
				// Retrieve the encoded Company Configuration data from the Settings.
				var encodedCompanyConfig = Settings.Default["CompanyId" + infoCompanyId].ToString();

				// Convert the encoded company configuration value to a byte array.
				var data = Convert.FromBase64String(encodedCompanyConfig);

				// Decode the byte array to a string.
				var decodedString = Encoding.UTF8.GetString(data);

				// Deserialize the decoded string to a CompanyConfig object.
				var config = JsonConvert.DeserializeObject<CompanyConfig>(decodedString);

				// Create the object used for user and application authentication and authorization.
				var api = new AuthenticateAndAuthorize();

				// Create an authentication token for the user encoded in the configuration file for the current Company ID.
				// TODO: Look into better ways of doing this. Try to use GetTokenForUser by adding to it the additional code found in this method call.
				Token = api.GetTokenForAzureUser(config.Username, config.Pwd);

				// Retrieve the encrypted data that is returned from the user-specific URL.
				var encryptStr = AuthenticateAndAuthorize.GetApplication(Token);

				// Authenticate and decrypt the data returned from the user-specific URL.
				var decrypt = AesThenHmac.SimpleDecrypt(encryptStr, Convert.FromBase64String(config.CryptKey), Convert.FromBase64String(config.AuthKey));

				// Retrieve the List of application configurations.
				var list = JsonConvert.DeserializeObject<List<ApplicationContract>>(decrypt);

				// Select the First item in the List of application configuration objects.
				var appContract = list.FirstOrDefault(listItem => listItem.CompanyId == config.CompanyId && listItem.ApplicationTy == 1);

				if (appContract != null)
				{
					// Retrieve the connection string for this Application Contract.
					ConnectString = appContract.GenerateConnectionStr();

					// Retrieve the company data for the specified ID.
					CompanySettingsForId = new CompanySettings(ConnectString, config.CompanyId);

					// Create a new SQL Cache.
					MakoCache = new SqlCacheRepo(appContract.GenerateConnectionStr(), MakoLogger);

					// Create a new Queue management object.
					QueueManager = new QueueManagement(config.CompanyId, appContract.GenerateConnectionStr());

					// Clear the queue for this Company ID.
					QueueManager.ClearQueue();

					// Create a Dictionary of values that will be used on the call to RunCommissions.
					// NOTE: 'item' and 'makoAdmin' values will be populated on each read from the queue.
					dict = new Dictionary<string, object>
					{
						{"config", config},
						{"item", null},
						{"makoAdmin", null},
						{"connStr", ConnectString},
						{"queueManager", QueueManager}
					};
				}
				else
				{
					throw new ApplicationException("Invalid ApplicationContract in GetCompanyRunnerInfo.");
				}

				//
				return dict;
			}
			catch (Exception e)
			{
				// TODO: Instead of throwing an exception and breaking the loop, log the error and continue.
				throw new ApplicationException("Error starting Windows Service for company ID" + infoCompanyId, e);
			}
		}

		/// <summary>
		/// The Stop method sends a Cancellation Token to the thread started in the Start method.
		/// </summary>
		public override void Stop()
	    { }
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}
