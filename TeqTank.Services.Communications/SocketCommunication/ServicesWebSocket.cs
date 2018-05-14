using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using WebSocketSharp;

namespace TeqTank.Services.Communications.SocketCommunication
{
	/// <summary>
	/// 
	/// </summary>
	public class ServicesWebSocket : BaseCommunications, ISocket
	{
		#region Fields
		#endregion Fields

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		public ServicesWebSocket() : base()
		{
			// Create the socket using the default endpoint.
			// TODO: Move this to configuration or settings.
			ServicesSocket = new WebSocket("ws://app1981.azurewebsites.net/socket.io/?EIO=2&transport=websocket");
		}
		#endregion Constructors

		#region Properties
		/// <summary>
		/// 
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string[] SubProtocols { get; set; }

		/// <summary>
		/// 
		/// </summary>
		private WebSocket ServicesSocket { get; }
		#endregion Properties

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		public override void SendMessage(IMessage message)
		{
			//
			var json = new JavaScriptSerializer().Serialize(message);
			var modifiedStr = "42[\"message\"," + json + "]";

			if (!ServicesSocket.IsAlive)
				ServicesSocket.Connect();

			if (ServicesSocket.IsAlive)
				ServicesSocket.Send(modifiedStr);
		}
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}
