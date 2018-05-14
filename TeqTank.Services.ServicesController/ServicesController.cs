using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using TeqTank.Services.Common.Commands;
using TeqTank.Services.MakoRunners.Runners;
using TeqTank.Services.ServicesController.EventArguments;
using TeqTank.Services.ServicesController.ThreadTaskManagement;

namespace TeqTank.Services.ServicesController
{
	/// <summary>
	/// The ServicesController is the primary point of creation, access, and discovery of the TeqTank Services.
	/// 
	/// The public constructors, methods, and properties of the ServicesController offer the means to start, pause, and stop/cancel
	/// TeqTank Services. In addition, the ServicesController allows the user to specify various options for running Services, specify 
	/// logging options, specify data access options, etc.
	/// </summary>
	/// <remarks>
	/// </remarks>
	public class ServicesController
	{
		#region Fields
		/// <summary>
		/// The event used to send string messages to listeners.
		/// </summary>
		/// <remarks>
		/// The intent of this event is to use it to respond to user input, status requests, 
		/// and informational messages from the Services Controller to listeners.
		/// </remarks>
		public event EventHandler<ServicesControllerInfoArgs> ServicesControllerInfoEvent;

		/// <summary>
		/// The event used to report errors to listeners.
		/// </summary>
		public event EventHandler<ServicesControllerErrorArgs> ServicesControllerErrorEvent;

		// A Dictionary of all current Mako Runners. Uses a GUID as the key and an object 
		// derived from the BaseMakoRunner as the value.
		private static Dictionary<Guid, BaseMakoRunner> _activeRunners = new Dictionary<Guid, BaseMakoRunner>();
		#endregion Fields

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		/// <param name="commandList"></param>
		public ServicesController(IReadOnlyCollection<ConsoleCommand> commandList)
		{
			try
			{
				// In order to prepare for the eventuality that multiple commands are specified for the console application,
				// I am adding a loop to process each command. Currently, we are only going to be concerned with the 2 basic
				// commands: -server, and -azure.
				foreach (var consoleCommand in commandList)
				{
					//
					Guid key;

					// This code is written to be specific to the different commands available, and, therefore,
					// the code for each command is not easily re-factored.
					// NOTE: As new commands are added, new code will have to be added below to handle each.
					// TODO: Add a new "case" statement for each new type of service.
					switch (consoleCommand.Command)
					{
						case "server":
							// Create and invoke the objects that perform the server tasks.

							// Create a GUID to use as the Dictionary key.
							key = Guid.NewGuid();

							// Create the server Mako Runner object.
							var serverObj = new ServerMakoRunner(key, consoleCommand);

							// Add the Runner object to the List of Runners.
							_activeRunners.Add(key, serverObj);
							break;
						case "azure":
							// Create and invoke the objects that perform the Azure tasks.

							// Create a GUID to use as the Dictionary key.
							key = Guid.NewGuid();

							// Create the Azure Mako Runner object.
							var azureObj = new AzureMakoRunner(key, consoleCommand);

							// Add the Runner object to the List of Runners.
							_activeRunners.Add(key, azureObj);
							break;
						default:
							// Ignore this ConsoleCommand for now.
							// NOTE: We may want to inform the user?
							continue;
					}
				}
				// Create the ThreadTaskManager as long as we have at least one Runner.
				if (_activeRunners != null && _activeRunners.Count > 0) TaskManager = new ThreadTaskManager(_activeRunners);

				//
				TaskManager.StartAll();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}
		#endregion Constructors

		#region Properties
		/// <summary>
		/// 
		/// </summary>
		private ThreadTaskManager TaskManager { get; }
		#endregion Properties

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		public void StartAllServices()
		{
			//
			TaskManager.StartAll();
		}

		/// <summary>
		/// 
		/// </summary>
		public void StopAllServices()
		{
			//
			TaskManager.StopAll();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="serviceName"></param>
		public bool StartServiceByName(string serviceName)
		{
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="serviceName"></param>
		/// <returns></returns>
		public bool StopServiceByName(string serviceName)
		{
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="serviceId"></param>
		/// <returns></returns>
		public bool StartServiceById(Guid serviceId)
		{
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="serviceId"></param>
		/// <returns></returns>
		public bool StopServiceById(Guid serviceId)
		{
			TaskManager.Stop(serviceId);

			return true;
		}
		#endregion Methods

		#region EventHandlers
		/// <summary>
		/// The ServicesControllerInfoEvent event invocation.
		/// </summary>
		/// <param name="info">The message string being supplied to the event listeners.</param>
		/// <remarks>
		/// Always a good idea to wrap event invocations inside a protected virtual method
		/// to allow derived classes to override the event invocation behavior.
		/// </remarks>
		protected virtual void OnServicesControllerInfoEvent(ServicesControllerInfoArgs info)
		{
			ServicesControllerInfoEvent?.Invoke(this, info);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnServicesControllerErrorEvent(ServicesControllerErrorArgs e)
		{
			ServicesControllerErrorEvent?.Invoke(this, e);
		}
		#endregion EventHandlers
	}
}
