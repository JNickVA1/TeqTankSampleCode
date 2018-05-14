using System;
using System.Collections.Generic;
using TeqTank.Services.MakoRunners.Runners;

namespace TeqTank.Services.ServicesController.ThreadTaskManagement
{
	/// <summary>
	/// The <see cref="ThreadTaskManager"/> is used to start, stop, 
	/// pause, and cancel thread tasks that are running the services. 
	/// </summary>
	public class ThreadTaskManager
	{
		#region Fields
		#endregion Fields

		#region Constructors
		/// <summary>
		/// The ThreadTaskManager will ... 
		/// </summary>
		/// <param name="activeRunners"></param>
		public ThreadTaskManager(Dictionary<Guid, BaseMakoRunner> activeRunners)
		{
			ActiveRunners = activeRunners;
		}
		#endregion Constructors

		#region Properties
		/// <summary>
		/// A collection of active Runners with the Runner ID (GUID) as the key.
		/// </summary>
		public Dictionary<Guid, BaseMakoRunner> ActiveRunners { get; private set; }
		#endregion Properties

		#region Methods
		/// <summary>
		/// The method used to start all Runners listed in the ConsoleCommand.
		/// </summary>
		public void StartAll()
		{
			// Start each of the Runners in the list.
			try
			{
				foreach (var runner in ActiveRunners)
				{
					// Call the method on the Runner to start the task.
					runner.Value.Start();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		/// <summary>
		/// The method used to start a specific Runner based on the Runner ID.
		/// </summary>
		/// <param name="serviceId"></param>
		public void Start(Guid serviceId)
		{
			//
		}

		/// <summary>
		/// The method used to stop all currently running Runners.
		/// </summary>
		public void StopAll()
		{
			//
		}

		/// <summary>
		/// The method used to stop a specific Runner based on the Runner ID.
		/// </summary>
		/// <param name="serviceId"></param>
		public void Stop(Guid serviceId)
		{
			//
		}

		/// <summary>
		/// The method used to pause all currently running Runners.
		/// </summary>
		public void PauseAll()
		{

		}

		/// <summary>
		/// The method used to pause a specific Runner based on the Runner ID.
		/// </summary>
		/// <param name="serviceId"></param>
		public void Pause(Guid serviceId)
		{

		}
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers

	}
}
