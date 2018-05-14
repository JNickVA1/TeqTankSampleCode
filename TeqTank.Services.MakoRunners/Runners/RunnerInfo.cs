using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TeqTank.Services.Common.Commands;
using TeqTank.Services.Common.EnumsAndConstants;
using TeqTank.Services.Common.TaskResultObjects;
using TeqTank.Services.MakoRunners.ThreadObjects;

namespace TeqTank.Services.MakoRunners.Runners
{
	/// <summary>
	/// Contains information used to identify and manipulate a Mako Runner.
	/// </summary>
	public class RunnerInfo
	{
		#region Fields
		// Private fields used to store the intermediate values from the ConsoleCommand Value property.
		private string _companyIdValues = string.Empty;
		private string _runnerNameValues = string.Empty;
		private string _processingOrder = string.Empty;
		private string _preferredService = string.Empty;
		#endregion Fields

		#region Constructors
		/// <summary>
		/// The RunnerInfo class is responsible for parsing the ConsoleCommand and 
		/// storing various items used in running and maintaining a Mako Runner.
		/// </summary>
		/// <param name="consoleCommand"></param>
		public RunnerInfo(ConsoleCommand consoleCommand)
		{
			// Extract the runner's info from the ConsoleCommand.
			if (consoleCommand != null) ExtractRunnerInfoFromCommand(consoleCommand);
			else throw new ApplicationException("ConsoleCommand null in RunnerInfo constructor.");
		}
		#endregion Constructors

		#region Properties
		/// <summary>
		/// A List of Company IDs as string values.
		/// </summary>
		public List<string> CompanyIds { get; set; } = new List<string>();

		/// <summary>
		/// The Tasks started by one of the Runner objects. To be used to communicate with the threads.
		/// </summary>
		/// <remarks>
		/// To be used for Windows Services or any other type of Runner EXCEPT an Azure WebJob.
		/// </remarks>
		public ConcurrentBag<Task> RunnerTasks { get; } = new ConcurrentBag<Task>();

		/// <summary>
		/// The Task, with TResult, started by one of the Runner objects. To be used to communicate with the thread.
		/// </summary>
		/// <remarks>
		/// To be used for Windows Services or any other type of Runner EXCEPT an Azure WebJob.
		/// </remarks>
		public ConcurrentBag<Task<ServiceTaskResult>> RunnerTaskWithResult { get; } = new ConcurrentBag<Task<ServiceTaskResult>>();

		/// <summary>
		/// The name(s) of the Runners, if supplied.
		/// </summary>
		/// <remarks>
		/// I am allowing for a future when we might allow for unique Runner names for each Company ID being processed.
		/// </remarks>
		public List<string> RunnerName { get; set; } = new List<string>();

		/// <summary>
		/// The type of service that the Runner represents.
		/// </summary>
		/// <remarks>
		/// Common values could be WindowsService or AzureWebJob.
		/// </remarks>
		public RunnerType ServiceType { get; set; } = RunnerType.UndefinedOrUnknown;

		/// <summary>
		/// The order in which a service should process a list of multiple company IDs.
		/// 
		/// Defaults to ProcessingOrder.Sequential which indicates that multiple company IDs
		/// will be processed sequentially instead of in parallel.
		/// </summary>
		/// <remarks>
		/// This value can be changed in the future by modifying the command line argument
		/// syntax to include a value for this item. 
		/// I am thinking that we could append a ":S" or ":P" to the command line argument.
		/// </remarks>
		public ProcessingOrder SequentialOrParallel { get; set; } = ProcessingOrder.Sequential;

		/// <summary>
		/// The identifying string for the preferred location, or machine, or service instance, 
		/// where the service identified in the command should be run.
		/// </summary>
		/// <remarks>
		/// I am allowing for a future when we might allow for unique Service Locations for each Company ID being processed.
		/// If the Preferred Service Location is empty, we will use a predefined Default.
		/// </remarks>
		public List<string> PreferredServiceLocation { get; set; } = new List<string>();

		/// <summary>
		/// 
		/// </summary>
		public Dictionary<Task, CancellationTokenSource> TaskTokenSources { get; set; } = new Dictionary<Task, CancellationTokenSource>();
		#endregion Properties

		#region Methods
		/// <summary>
		/// Parse the Value portion of the ConsoleCommand in order to extract any and all user-supplied values.
		/// </summary>
		/// <param name="consoleCommand"></param>
		private void ExtractRunnerInfoFromCommand(ConsoleCommand consoleCommand)
		{
			try
			{
				// Set the service type.
				ExtractRunnerServiceType(consoleCommand);

				// Extract the value substrings, if they exist.
				ExtractValueSubstrings(consoleCommand.Value);

				// Extract all info from the ConsoleCommand Value.
				ExtractCompanyIds();
				ExtractRunnerNames();
				ExtractProcessingOrder();
				ExtractPreferredService();
			}
			catch (Exception e)
			{
				throw new ApplicationException($"Error extracting runtime arguments from the console command Value: {consoleCommand.Value}", e);
			}
		}

		/// <summary>
		/// Set the Service Type from the value in the ConsoleCommand Command property.
		/// </summary>
		/// <param name="consoleCommand"></param>
		private void ExtractRunnerServiceType(ConsoleCommand consoleCommand)
		{
			// Set the Runner type.
			switch (consoleCommand.Command)
			{
				case "server":
					ServiceType = RunnerType.WindowsService;
					break;
				case "azure":
					ServiceType = RunnerType.AzureWebJob;
					break;
				default:
					// We should never get to this case since we are checking
					// the command in the ServicesController, but it is best to 
					// prepare for the eventuality that this may happen.
					ServiceType = RunnerType.UndefinedOrUnknown;
					break;
			}
		}

		/// <summary>
		/// Extract the Value sub-values, if they exist.
		/// </summary>
		/// <param name="consoleCommandValue"></param>
		private void ExtractValueSubstrings(string consoleCommandValue)
		{
			// Based on the number of value item delimiters, split the Value string.
			if (string.IsNullOrEmpty(consoleCommandValue))
			{
				throw new ApplicationException("ConsoleCommand value is null or empty in RunnerInfo.ExtractValueSubstrings()");
			}

			// Split the Value on the delimiter, which is preset to a ':'.
			// TODO: Change to get the delimiter from settings.
			var delimitedStrings = consoleCommandValue.Split(':');

			// Since we are not currently enforcing the rule that all delimiters must exist
			// even if there is no value, we will assign only those values that were found
			// when splitting the Value string at the delimiter.

			// Get the number of elements in the array.
			var numSubstrings = delimitedStrings.Length;

			// The Company IDs are required so if the length of the delimited strings is 0
			// or the string at delimitedStrings[0] is empty, then throw an exception.
			if (numSubstrings < 1 || string.IsNullOrEmpty(delimitedStrings[0]))
				throw new ApplicationException("Company ID must be specified.");
			// Set the field for later use.
			_companyIdValues = delimitedStrings[0];

			// Check for existence of the Runner Name and save it to the appropriate field if present.
			if (numSubstrings > 1)
				_runnerNameValues = delimitedStrings[1];

			// Check for existence of the Processing Order value and save it to the appropriate field if present.
			if (numSubstrings > 2)
				_processingOrder = delimitedStrings[2];

			// Check for existence of the Preferred Service Identifier value and save it to the appropriate field if present.
			if (numSubstrings > 3)
				_preferredService = delimitedStrings[3];
		}

		/// <summary>
		/// Retrieve all of the Preferred Service Location values from the command Value.
		/// </summary>
		private void ExtractPreferredService()
		{
			// Return if the field is null or empty.
			if (string.IsNullOrEmpty(_preferredService)) return;

			// Split the comma-separated list into individual strings in a List<string>.
			PreferredServiceLocation = _preferredService.Split(',').ToList();
		}

		/// <summary>
		/// Retrieve the processing order specifier from the command Value.
		/// </summary>
		private void ExtractProcessingOrder()
		{
			// Return if the field is null or empty.
			if (string.IsNullOrEmpty(_processingOrder)) return;

			// Set the property based on the input string value.
			if (_processingOrder.Equals("S"))
				SequentialOrParallel = ProcessingOrder.Sequential;
			else if (_processingOrder.Equals("P"))
				SequentialOrParallel = ProcessingOrder.Parallel;
			else SequentialOrParallel = ProcessingOrder.Undefined;
		}

		/// <summary>
		/// Retrieve all of the Runner Names from the command Value.
		/// </summary>
		private void ExtractRunnerNames()
		{
			// Return if the field is null or empty.
			if (string.IsNullOrEmpty(_runnerNameValues)) return;

			// Split the comma-separated list into individual strings in a List<string>.
			RunnerName = _runnerNameValues.Split(',').ToList();
		}

		/// <summary>
		/// Retrieve all of the company IDs from the command Value.
		/// </summary>
		/// <remarks>
		/// For the initial version, we will only attempt to extract Company IDs when the
		/// ConsoleCommand Value is in the form id1, id2, id3...
		/// 
		/// The initial version syntax for the command line is:
		///		"-service-type:ID1,ID2,...IDN
		/// </remarks>
		private void ExtractCompanyIds()
		{
			// Return if the field is null or empty.
			if (string.IsNullOrEmpty(_companyIdValues)) return;

			// Split the comma-separated list into individual strings in a List<string>.
			CompanyIds = _companyIdValues.Split(',').ToList();
		}
		#endregion Methods
	}
}