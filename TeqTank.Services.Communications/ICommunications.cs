namespace TeqTank.Services.Communications
{
	/// <summary>
	/// 
	/// </summary>
	public interface ICommunications
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		void SendMessage(IMessage message);
	}
}