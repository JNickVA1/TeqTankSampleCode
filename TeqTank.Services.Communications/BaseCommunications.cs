using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeqTank.Services.Communications
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class BaseCommunications : ICommunications
	{
		#region Fields
		#endregion Fields

		#region Constructors
		#endregion Constructors

		#region Properties
		#endregion Properties

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public abstract void SendMessage(IMessage message);
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}
