namespace TeqTank.Services.Communications.SocketCommunication
{
	/// <summary>
	/// 
	/// </summary>
	public interface ISocket
	{
		#region Properties
		/// <summary>
		/// The URL 
		/// </summary>
		string Url { get; set; }

		/// <summary>
		/// 
		/// </summary>
		string[] SubProtocols { get; set; }
		#endregion Properties
	}
}