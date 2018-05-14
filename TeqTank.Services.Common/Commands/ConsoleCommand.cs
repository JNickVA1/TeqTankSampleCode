using System;
using System.Collections.Generic;
using System.Linq;
using TeqTank.Services.Common.EnumsAndConstants;
using TeqTank.Services.Common.Properties;

namespace TeqTank.Services.Common.Commands
{
	/// <summary>
	/// The ConsoleCommand class represents a single command line argument with an optional value.
	/// </summary>
	/// <remarks>
	/// To allow for future enhancements, the ConsoleCommand class is designed to read and parse
	/// command line arguments with the following syntax:
	/// 
	/// -commandName:[REQUIRED]commaSeperatedListOfCompanyIDs:[OPTIONAL]RunnerNames:[OPTIONAL]PreferredServiceIdentifiers
	/// 
	/// Some examples would be: 
	/// 
	/// -server:1,22,11:ServiceMain:WindowsServer1 OR
	/// -server:1 OR
	/// -server:11::WindowsServer1
	/// 
	/// Hopefully, the code as written can handle the optional items. Also, I have allowed for 
	/// comma-separated values for the company IDs but NOT for the optional values..
	/// </remarks>
	public class ConsoleCommand
	{
		/// <summary>
		/// The default ctor that parses the input string to extract the command and the optional value.
		/// </summary>
		/// <param name="parseableCommandString"></param>
		/// <exception cref="ApplicationException"></exception>
		/// <remarks>
		/// The ConsoleCommand has been written specifically for the Mako Runner application and makes use
		/// of the specialized syntax defined for the console commands. It is not worth the time or effort 
		/// to make this reusable for other applications at this time.
		/// </remarks>
		public ConsoleCommand(string parseableCommandString)
		{
			try
			{
				// Make a List with the CSV list of command strings.
				ValidCommandList = ValidCommands.Split(',').ToList();

				// Cleanup the input.
				var cleanString = parseableCommandString.Trim();

				// Ensure that the first character of the command string is a "-".
				if (!cleanString.StartsWith("-")) throw new ApplicationException($"Invalid command line argument. All commands must start with \"-\".");

				// Determine if the command value is present in the command string.
				var valueIndex = cleanString.IndexOf(":", StringComparison.Ordinal);
				if (valueIndex == -1) throw new ApplicationException($"Command value not found in command string: {cleanString}");

				// Extract the command portion of the command line argument.
				Command = cleanString.Substring(1, valueIndex - 1);

				// Make sure the command is one that has been specified in the Settings.
				if (!ValidCommandList.Contains(Command)) throw new ApplicationException($"Invalid command: {Command}");

				// Extract the command value from the command string.
				if (valueIndex > 1 && cleanString.Length > valueIndex + 1)
					Value = cleanString.Substring(valueIndex + 1);
				else
					throw new ApplicationException("Invalid command value. Company ID not found.");
			}
			catch (Exception e)
			{
				// Throw an exception with a detailed message.
				throw new ApplicationException($"Error parsing command line argument, {parseableCommandString}. Original error: {e.Message}");
			}
		}

		#region Properties
		/// <summary>
		/// The Console Command gleaned from a command line argument.
		/// </summary>
		public string Command { get; set; }

		/// <summary>
		/// The value part(s) for the Console Command.
		/// </summary>
		/// <remarks>
		/// The Value property may consist of multiple command values in a delimited string.
		/// For example, we may add the capability for a service to run on multiple companies,
		/// either sequentially or in parallel. In that case, the command Value string could
		/// contain a delimited list of company IDs, such as: 11,73,1.
		/// </remarks>
		public string Value { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public static string ValidCommands { get; } = Settings.Default.ValidCommands;

		/// <summary>
		/// 
		/// </summary>
		public List<string> ValidCommandList { get; }
		#endregion Properties
	}
}
