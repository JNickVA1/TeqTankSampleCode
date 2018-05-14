using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using TeqTank.Services.Common.Commands;
using TeqTank.Services.MakoRunnerConsolidated.Properties;
using TeqTank.Services.ServicesController.EventArguments;

namespace TeqTank.Services.MakoRunnerConsolidated
{
	/// <summary>
	/// The class containing the console application entry point.
	/// </summary>
	/// <remarks>
	/// The MakoRunnerConsolidated console application can be used for either a server execution 
	/// or a cloud execution depending on the parameters used for starting the application, which
	/// are supplied to the application in the RuntimeArguments.txt file. 
	/// 
	/// NOTE: This application no longer will support sequential OR parallel WebJobs or parallel Windows services.
	/// 
	///		  In the case of WebJobs, this is due to the fact that a single console application, when deployed to Azure,
	///		  is limited to exactly one WebJob, meaning there is a on-to-one WebJob to console application relationship.
	///		  The runtime instructions have been changed to reflect this. The code has been left intact but will not be 
	///		  executed because it is prevented by runtime parameter checks when the program starts. In the future we may 
	///		  want to develop a controller application or shell script that can deploy multiple WebJobs. The basic logic
	///		  for running sequential and/or parallel WebJobs could be implemented in that controller application.
	/// 
	///		  In the case of Windows services, there is no reason other than the constraints of time for testing and 
	///		  debugging for limiting the execution of Windows services to sequential operation. Since each service is run 
	///		  within a separate Task, we could easily implement parallelization, if time was available for testing and debugging.
	///		  Sequential Windows services is still supported and implemented in this application (although, due to the 
	///		  lack of test data, sequential operation has not been tested.)
	/// 
	///	NOTE 2: A new, and totally different, method of processing queues has been introduced in this application that will
	///			allow the user to specify multiple company IDs and have the queues processed in a "round robin" method.
	/// 
	///			For the Azure WebJob, the new approach means that we only need one WebJob running to process multiple company IDs.
	/// 
	///			For Windows services, the new approach means that we can process multiple company IDs using a single Task, much
	///			like the existing code does for Sequential processing.
	/// 
	///			I have introduced a new ProcessingOrder enumeration value, RoundRobin, that will be the default for both Windows services,
	///			specified in the RuntimeArguments text file with the "-server" command, and for Azure WebJobs, specified in the 
	///			RuntimeArguments text file with the "-azure" command. 
	/// 
	///			Essentially, we will no longer need either the Sequential OR the Parallel ProcessingOrder values, or the corresponding
	///			code paths, but I will leave the code in place for now until Jeff and Elisa and whoever else may be interested, tells me 
	///			that one method is preferable over any of the others.
	/// 
	/// NOTE 3: I have added a brief description to the RuntimeArguments.txt file of the runtime arguments that should and could be used
	///			along with some examples. I have also added all of the necessary information to the application runtime help, which can 
	///			be viewed by running the application from the command line with the only valid command line argument, -help, like so: 
	/// 
	///				TeqTank.Services.MakoRunnerConsolidated -help
	/// 
	///			The "-help" command line argument is the ONLY valid argument. Anything else will result in a runtime error.
	/// </remarks>
	public class Program
    {
	    #region Fields
	    // The console window default prompt.
	    private const string ReadPrompt = "console> ";

		// A flag that indicates whether or not there are still tasks running.
	    private const bool ContinueRunning = true;
		#endregion Fields

		/// <summary>
		/// The console application entry point.
		/// </summary>
		/// <param name="args">The command line arguments.</param>
		/// <remarks>
		/// We will enforce the use of specific command arguments, 
		/// restricting the execution to specific circumstances.
		/// </remarks>
		private static void Main(string[] args)
        {
	        // A simple nicety to add a title to our console window.
			Console.Title = typeof(Program).Name;

	        // Based on the number of command arguments, perform configurations, runtime setup, 
	        // and other actions.
	        try
			{
				// Make sure that there are no command line arguments.
				if (args?.Length > 0)
				{
					// For the time being we are only allowing one argument to be sent to the 
					// application on the command line, the help parameter, "-help"
					if (args.Length == 1 && string.Equals(args[0], "-help"))
					{
						// Display the runtime help message and exit.
						DisplayCommandLineInstructions();
						//return;
					}
					else
						// Since we cannot have command line arguments, this is an error.
						throw new ApplicationException(
							"All runtime arguments must be supplied to the application in the RuntimeArguments.txt file. " + 
							"\r\nFor information on specific commands available, run the application as \"MakoRunnerConsolidated -help\"");
				}
				// Inform the user that we are attempting to read the runtime arguments from the file.
				Console.WriteLine("Attempting to read from RuntimeArguments.txt file.");

				// Due to an issue when using TopShelf to run Windows Services and the fact that TopShelf attempts
				// to parse the console application's command line arguments, I am resorting to an alternative method
				// for supplying arguments to the application - reading the arguments from a text file.
				var appArgStrings = GetRuntimeArguments();

				// Verify that we found the runtime arguments.
				if (appArgStrings == null || appArgStrings.Length == 0)
				{
					// Inform the user that a runtime parameter must be supplied.
					throw new ApplicationException(
						"\r\nNo runtime arguments found! \r\nFor information on specific commands available, run the application as \"MakoRunnerConsolidated -help\"");
				}
				// Parse command arguments. One of the following is valid:
				//  1. The Server argument, with the company ID(s), "-service:<ID>[,<ID>...]"
				//  2. The Azure argument, with the company ID(s), "-azure:<ID>[,<ID>...]"
				// NOTE: We will only use the first valid command in the list, no matter how many are present.
				var commands = ParseCommandLine(appArgStrings);

		        // Call the static Run method to start processing.
		        Run(commands);
	        }
	        catch (Exception e)
	        {
		        Console.WriteLine("An error was encountered.");
		        Console.WriteLine(e.Message);
	        }
	        finally
	        {
		        // Exit the application.
		        Console.WriteLine("\r\nPress any key to exit.");
		        Console.ReadLine();
	        }
		}

		/// <summary>
		/// The method will attempt to open and read a text file containing runtime arguments.
		/// </summary>
		/// <returns>An array of command strings.</returns>
		/// <remarks>
		/// Will throw an exception on error, which will be caught in the caller.
		/// </remarks>
	    private static string[] GetRuntimeArguments()
		{
		    // Read the lines from the file and split them into an array.
			var lines = File.ReadAllLines("RuntimeArguments.txt");

			// Remove all lines that are comments (lines beginning with a '#') or blank.
			lines = (lines.Where(line => !line.StartsWith("#") && !string.IsNullOrEmpty(line))).ToArray();

		    return lines;
		}

		/// <summary>
		/// Simply create a new ConsoleCommand object using the single argument string.
		/// </summary>
		/// <param name="argList">The list of command line arguments.</param>
		/// <returns>A ConsoleCommand object.</returns>
		/// <remarks>
		/// Will throw an exception on error, which will be caught in the caller.
		/// </remarks>
		private static ConsoleCommand ParseCommandLine(IReadOnlyList<string> argList)
        {
			// The object returned to the method caller.
	        ConsoleCommand consoleCommand = null;

	        // Create a new ConsoleCommand using the current command line argument.
			if (argList != null)
	        {
				// create and populate a ConsoleCommand object.
		        consoleCommand = new ConsoleCommand(argList[0]);
	        }
	        return consoleCommand;
        }

	    ///  <summary>
	    ///  The Run method will start a thread using an object appropriate to the command line argument. 
	    ///  The console application will continue to run until the user cancels execution, at which time
	    ///  the thread will be canceled, the logger stopped, and the user informed of the application's status.
	    ///  </summary>
	    /// <param name="command"></param>
	    /// <remarks>
	    ///  IMPORTANT--> You no longer can use the command line arguments. Instead, put the arguments in the RuntimeArguments.txt file.
	    ///  
	    ///  NOTE: I will be adding the capability to specify a specific Windows Service machine
	    /// 		  and.or a specific Azure environment in which to run a service. I have in mind
	    /// 		  to use either a named instance, like serverA or azureD, OR add an identifier
	    /// 		  to the list of values after the ':' and preceding the company ID(s) associated 
	    /// 		  with a service command, as in server:A,1,2,3, OR insert a colon after the list 
	    /// 		  of company ID(s) and then specify the machine identifier, as in server:1:A.
	    /// 		  All of this is to be determined later and NONE of this will be implemented in
	    /// 		  the initial code-base.
	    ///  
	    ///  NOTE: For changes in the syntax of a console command, and to handle such changes, 
	    /// 		  the ConsoleCommand class should be modified. In addition, anywhere that a 
	    /// 		  ConsoleCommand object is used, such as in the ServiceController class, should
	    /// 		  be modified to take advantage of changed command syntax.
	    ///  </remarks>
	    private static void Run(ConsoleCommand command)
        {
			// Validate the List of ConsoleCommands.
			if (command == null)
				throw new ApplicationException("Command empty in Run method.");

			//      try
			//      {
			//       // Check for any commands that are meant only for the console.
			//       // NOTE: If the command list contains ANY commands meant for the console alone,
			//       //		 then only those will be processed. All other commands will be ignored.
			//       if (ConsoleOnlyCommands(commandList))
			//       {
			//        // Process any console-only commands and then return.

			//		// NOTE: I have made the decision to store the console-only commands in the Settings.
			//		//		 This can be changed at any time, of course.

			//		// Retrieve any and all settings that contain the substring "Command".
			//        GetCommandsFromSettings(out var query);

			//        // Loop through all console-only commands contained in the command list and
			//		// perform the required action for each command. Exit the loop and return when
			//		// all console-only commands have been processed OR when the 'exit' command is 
			//		// processed.
			//        if (query != null)
			//	        foreach (var settingsProperty in query)
			//			{
			//				//case "help":
			//				//		// Display the help information.
			//				//		// TODO: Add code to invoke delegate to output messages to the console.
			//				//		//DisplayCommandLineInstructions();
			//				//		//DisplayOperatingInstructions();
			//				//		// TODO: Add code to display help.
			//				//		break;
			//	        }
			//	}
			//	else
			//       {
			//		// No console-only commands exist in the command list, so attempt 
			//		// to process the list in the Services Controller.

			//        // Create the object that will, in turn, create and manage the various services.
			//        var controller = new ServicesController.ServicesController(commandList);

			//        // Add an event handler for receiving info messages from the controller.
			//        controller.ServicesControllerInfoEvent += ServicesControllerInfoEvent;

			//		// Add an event handler for receiving error messages from the controller.
			//		controller.ServicesControllerErrorEvent += ServicesControllerErrorEvent;

			//		// Check the status of the Services Controller to make sure we are ready to start.
			//		// TODO: Put in a status check and, if not OK, throw an exception.

			//		// Display the instructions for stopping the running tasks.
			//		// NOTE: These instructions are also displayed if the user uses the 
			//		//		 "-help" command line argument.
			//		DisplayOperatingInstructions();

			//		// Start the Services Controller which will cause Tasks to be created and the service(s) run.


			//        // Wait until the user ends the process or until there are no longer any running tasks.
			//        while (ContinueRunning)
			//        {
			//			//
			//	        var consoleInput = ReadFromConsole();

			//			//
			//	        switch (consoleInput)
			//	        {
			//			        case "q":
			//					case "quit":
			//					case "exit":
			//						// Exit the application.
			//						return;
			//			}
			//        }
			//	}
			//}
			//catch (Exception e)
			//      {
			//       Console.WriteLine(e);
			//       throw;
			//      }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="query"></param>
	    private static void GetCommandsFromSettings(out IEnumerable<SettingsProperty> query)
	    {
			//
		    var numSettingsProperties = Settings.Default.Properties.Count;

		    //
		    Array appSettingsPropertyArray = new SettingsProperty[numSettingsProperties];
		    Settings.Default.Properties.CopyTo(appSettingsPropertyArray, 0);

		    //
		    var appSettingsProperties = new List<SettingsProperty>();

		    //
		    for (var i = 0; i < numSettingsProperties; i++)
		    {
			    appSettingsProperties.Add(appSettingsPropertyArray.GetValue(i) as SettingsProperty);
		    }

		    //
		    query = appSettingsProperties.Where(propertyKey => propertyKey.Name.Contains("Command"));
	    }

	    private static void ServicesControllerErrorEvent(object sender, ServicesControllerErrorArgs e)
		{
			Console.WriteLine(e.ErrorMessage);
		}

		private static void ServicesControllerInfoEvent(object sender, ServicesControllerInfoArgs servicesControllerInfoArgs)
		{
			Console.WriteLine(servicesControllerInfoArgs.InfoMessage);
		}

		private static bool ConsoleOnlyCommands(IReadOnlyCollection<ConsoleCommand> commandList)
	    {
			// NOTE: For the time being I am only checking for the "help" command, but we can
			//		 add as many commands as we like that are meant to be used by the console 
			//		 application.
			//
			// Get a List of all of the Command strings in the List of ConsoleCommands
			// and check to see if the console-specific command(s) are in the list.
			// TODO: Decide on using either Settings or a custom section in the app.config file
			// TODO: and store the list of console-only commands there instead of hard-coding here.
			var commandSelect = (commandList.Select(x => x.Command)).Contains("help");

		    return commandSelect;
	    }

		/// <summary>
		/// Displays the basic instructions for running the application from the command line.
		/// </summary>
		/// <remarks>
		/// The instructions will be displayed when the command line argument "-help" is used.
		/// </remarks>
		private static void DisplayCommandLineInstructions()
	    {
			//
		    var helpMessage = "###############################################################################" +
		                      "\r\n#\r\n#\tThe Runtime Arguments.txt file contains the arguments passed to the" +
		                      "\r\n#\tTeqTank.Services.MakoRunnerConsolidated console application." +
		                      "\r\n#\r\n#\tThe valid options and their meaning(s) follows, but is subject to change at" +
		                      "\r\n#\tany time, so please re-read this file from time to time to ensure that you" +
		                      "\r\n#\tare running the application properly." +
							  "\r\n#\r\n#\tMain arguments:" +
							  "\r\n#\t\t-server\t\tdenotes that the user wants to run a Windows service." +
							  "\r\n#\t\t-azure\t\tdenotes that the user wants to run an Azure WebJob." +
							  "\r\n#\r\n#\tNOTE:\tOnly one command, with argument(s), is valid for any single execution" +
							  "\r\n#\t\tof the console application. The first valid command will be used and" +
							  "\r\n#\t\tall others will be ignored." +
		                      "\r\n#\r\n#\tRequired command values" +
							  "\r\n#\t\t<company ID>{,<company ID>}*" +
							  "\r\n#\r\n#\tAt least one company ID is required for both the -server and the -azure" +
							  "\r\n#\tcommands." +
							  "\r\n#\t*Additional company IDs may be supplied in the format shown," +
		                      "\r\n#\twithout blanks, in the comma-separated string of company IDs." +
							  "\r\n#\r\n#\tOptional command values" +
							  "\r\n#\t\tThere are currently no optional command values." +
							  "\r\n#\r\n#\tField separator character \":\"" +
							  "\r\n#\r\n#\tExamples:" +
							  "\r\n#\t\t-server:11,13,77" +
							  "\r\n#\t\t-server:11" +
							  "\r\n#\t\t-azure:11" +
							  "\r\n#\t\t-azure:11,13" +
							  "\r\n#\t\t-server\t\t\t\tERROR - At least ONE company ID must be supplied" +
							  "\r\n#\t\t-azure:\t\t\t\tERROR - At least ONE company ID must be supplied" +
		                      "\r\n#\r\n###############################################################################";

			Console.WriteLine(helpMessage);
	    }

		/// <summary>
		/// Displays the instructions for operating the application once it has started.
		/// </summary>
		private static void DisplayOperatingInstructions()
	    {
		    //
	    }

	    /// <summary>
		/// Simply reads input from the user and returns the string that was entered.
		/// </summary>
		/// <param name="promptMessage">An optional prompt message. Defaults to an empty string.</param>
		/// <returns>The line of characters entered by the user.</returns>
	    public static string ReadFromConsole(string promptMessage = "")
	    {
		    // Show a prompt, and get input.
		    Console.Write(ReadPrompt + promptMessage);
		    return Console.ReadLine();
	    }
	}
}
