using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TeqTank.Services.Common.Commands;
using TeqTank.Services.Common.Configuration;
using TeqTank.Services.Common.Configuration.CompanyConfiguration;
using TeqTank.Services.Common.EnumsAndConstants;
using TeqTank.Services.MakoRunners.Properties;
using TeqTank.Services.MakoRunners.ThreadObjects;
using Topshelf;

namespace TeqTank.Services.MakoRunners.Runners
{
	/// <summary>
	/// The ServerMakoRunner uses a Windows service to perform the tasks, such as RunCommissions.
	/// </summary>
	/// <remarks>
	/// We are currently using the TopShelf library to start and configure the Windows service,
	/// however, we should be able to run the Windows service directly. In support of this, I have
	/// placed all TopShelf related calls in methods that can be replaced by methods that don't
	/// use TopShelf.
	/// </remarks>
    public class ServerMakoRunner : BaseMakoRunner
    {
		#region Fields
		#endregion Fields

		#region Constructors
	    /// <summary>
	    /// The default constructor for the Server Mako Runner.
	    /// </summary>
	    /// <param name="keyServer">A unique identifier for the Runner.</param>
	    /// <param name="consoleCommand">The command that will be used in creating and starting the Runner.</param>
	    public ServerMakoRunner(Guid keyServer, ConsoleCommand consoleCommand) : base(keyServer, consoleCommand)
	    {
			// Read the boolean setting to determine whether or not we are allowed to run
			// multiple Windows services in parallel. 
			//
			// Use with discretion and only if you know what you're doing. Just because 
		    // you CAN run parallel tasks doesn't always mean you should.
		    CanRunInParallel = Settings.Default.RunServerJobsInParallel;

			// Set the Mako Runner type.
		    Info.ServiceType = RunnerType.WindowsService;
		}
		#endregion Constructors

		#region Properties
		#endregion Properties

		#region Methods
		/// <inheritdoc />
		/// <summary>
		/// The Start method will create and start the task(s).
		/// </summary>
		public override void Start()
	    {
			// Make sure that the RunnerInfo object is non-null.
			if (Info == null)
				throw new ApplicationException($"RunnerInfo has not been allocated for Runner with ID: {MyRunnerId}");

			// We will create and start Tasks based on the values in the RunnerInfo object.
			if ((Info.SequentialOrParallel == ProcessingOrder.Sequential || 
			    Info.SequentialOrParallel == ProcessingOrder.Undefined) &&
			    !CanRunInParallel)
		    {
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
						
						// Create a ProgressInfo object.
						var runnerProgress = new ServiceProgressInfo();

						// Create a new CancellationTokenSource.
						var tokenSource = new CancellationTokenSource();

						// Create a Cancellation Token.
						var cancelRunner = tokenSource.Token;

					    // Create and start a Task using the Windows service library currently being used.
					    Task<ServiceTaskResult> newTask = new Task<ServiceTaskResult>(() => 
						    RunWindowsService(cancelRunner, config, runnerProgress), cancelRunner, TaskCreationOptions.LongRunning);

						// Store the Task in the RunerInfo for this Mako Runner.
						if (Info.RunnerTaskWithResult != null)
						{
							// Add the Task to the List of this type of Task.
							Info.RunnerTaskWithResult.Add(newTask);

							// Save the Task and associated CancellationTokenSource.
							Info.TaskTokenSources.Add(newTask, tokenSource);

							// Start the Task.
							newTask.Start();
						}
						else
							throw new ApplicationException($"The RunnerInfo object's RunnerTaskWithResult property has not been initialized.");
					}
					catch (Exception e)
				    {
						// TODO: Instead of throwing an exception and breaking the loop, log the error and continue.
					    throw new ApplicationException("Error starting Windows Service for company ID" + infoCompanyId, e);
				    }
			    }
		    }
			else if (Info.SequentialOrParallel == ProcessingOrder.Parallel && CanRunInParallel)
		    {
				// All company IDs will be processed in parallel, up to a predefined maximum number of parallel tasks.
				// TODO: Define maximum number of parallel tasks and add to read that value.
		    }
			else
			{
				throw new ApplicationException("Error in ServerMakoRunner.Start(). Invalid processing order.");
			}
	    }

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="cancelRunner"></param>
	    /// <param name="companyConfig"></param>
	    /// <param name="runnerProgress"></param>
	    /// <returns></returns>
	    private ServiceTaskResult RunWindowsService(CancellationToken cancelRunner, CompanyConfig companyConfig,
		    ServiceProgressInfo runnerProgress)
	    {
		    // NOTE: In order to get the initial code delivered, I am reusing the code, or parts of it at least,
		    //		 from the original MakoRunner project that uses the Topshelf library. This is not ideal since
		    //		 we lose a great deal of control over our services. However, we know the code works for one 
		    //		 company ID at a time, which is why I'm reusing here, in the Sequential portion of the IF clause.

		    //
		    TopshelfExitCode retCode = HostFactory.Run(x =>
		    {
			    x.Service<TopshelfWindowsService>(sc =>
			    {
					sc.ConstructUsing(name => new TopshelfWindowsService(companyConfig, runnerProgress));
					sc.WhenStarted((s, hostControl) => s.Start(hostControl));
				    sc.WhenShutdown(s => s.Shutdown());
				    sc.WhenStopped((s, hostControl) => s.Stop(hostControl));
			    });
				//
			    x.SetServiceName($"CommRunner {companyConfig.CompanyName + companyConfig.CompanyId}");
			    x.SetDescription($"Mako Commission Runner for CompanyID ({companyConfig.CompanyId})");
			    x.SetDisplayName($"Mako Commission Runner {companyConfig.CompanyId}");
				//
			    //x.StartAutomaticallyDelayed();
			});
			//
		    int exitCode = (int)Convert.ChangeType(retCode, retCode.GetTypeCode());

			// Create and return a ServiceTaskResult.
			// TODO: Add properties to the ServiceTaskResult and populate based on service exit code, etc.
			return new ServiceTaskResult() {ServiceReturnCode = exitCode};
	    }

	    /// <summary>
	    /// The Stop method sends a Cancellation Token to the thread started in the Start method.
	    /// </summary>
	    /// <remarks>
	    /// Since we may have multiple Tasks running in parallel, we want to cancel all of the Tasks in the 
	    /// RunnerInfo RUnnerTaskList for this Runner.
	    /// </remarks>
	    public override void Stop()
	    { }
	    #endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}
