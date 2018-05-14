using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

// https://stackoverflow.com/questions/12881254/avoid-crashing-when-file-is-locked
// ^^^^ File Locking ^^^

namespace TeqTank.Services.Logging
{
	/// <summary>
	/// 
	/// </summary>
    public class FileLogger 
    {
		#region Fields
	    private string m_exePath = string.Empty;
	    private string _companyKey;
	    private int _queueId;
	    private int _runTy;
		#endregion Fields

		#region Constructors
		/// <summary>
		/// 
		/// </summary>
		/// <param name="companyKey"></param>
		/// <param name="queueId"></param>
		/// <param name="runTy"></param>
		public FileLogger(string companyKey, int queueId, int runTy)
	    {
		    if (string.IsNullOrWhiteSpace(companyKey))
			    throw new ArgumentException("A CompanyKey is Required", "CompanyKey");

		    if (queueId == 0)
			    throw new ArgumentException("A queueId cannot be null", "queueId");

		    _companyKey = companyKey;
		    _queueId = queueId;
		    _runTy = runTy;
	    }
		#endregion Constructors

		#region Properties
		#endregion Properties

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="log"></param>
	    public void GenerateLog(List<string> log)
	    {
		    try
		    {
			    var path = GetFilePath();
			    if (_runTy == 4)
				    path += "\\RealTime.txt";
			    else
				    path += $"\\{_queueId}.txt";

			    WaitSharingVio(
				    action: () =>
				    {
					    using (var fStream = File.Open(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
					    using (StreamWriter sw = new StreamWriter(fStream))
					    {
						    foreach (var s in log)
							    sw.WriteLine(s);
					    }
				    },
				    onSharingVio: () =>
				    {
					    Console.WriteLine("Sharing violation. Trying again soon...");
				    }, maximum: TimeSpan.FromMinutes(1)
			    );
		    }
		    catch (Exception ex)
		    {
			    Console.WriteLine(ex.Message);
		    }
	    }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
	    private string GetFilePath()
	    {
		    var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		    string path = Path.Combine(exePath ?? throw new InvalidOperationException(),
			    $"{_companyKey}\\{DateTime.UtcNow.ToString("yyyy-MM-dd")}");

		    PathVerification(path);

		    return path;
	    }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
	    private void PathVerification(string path)
	    {
		    path = path.TrimEnd('\\');

		    if (!Directory.Exists(path))
		    {
			    var split = path.Split('\\');

			    var startBuild = split[0] + "\\";

			    for (int i = 1; i < split.Length; i++)
			    {
				    startBuild += "\\" + split[i];

				    if (!Directory.Exists(startBuild))
					    Directory.CreateDirectory(startBuild);
			    }
		    }
	    }
		
	    #region File Locking Methods
		/// <summary>
		/// Executes the specified action. If the action results in a file sharing violation exception, the action will be
		/// repeatedly retried after a short delay (which increases after every failed attempt).
		/// </summary>
		/// <param name="action">The action to be attempted and possibly retried.</param>
		/// <param name="maximum">Maximum amount of time to keep retrying for. When expired, any sharing violation
		/// exception will propagate to the caller of this method. Use null to retry indefinitely.</param>
		/// <param name="onSharingVio">Action to execute when a sharing violation does occur (is called before the waiting).</param>
		public static void WaitSharingVio(Action action, TimeSpan? maximum = null, Action onSharingVio = null)
		{
			WaitSharingVio(() => { action(); return true; }, maximum, onSharingVio);
		}

		/// <summary>
		/// Executes the specified function. If the function results in a file sharing violation exception, the function will be
		/// repeatedly retried after a short delay (which increases after every failed attempt).
		/// </summary>
		/// <param name="func">The function to be attempted and possibly retried.</param>
		/// <param name="maximum">Maximum amount of time to keep retrying for. When expired, any sharing violation
		/// exception will propagate to the caller of this method. Use null to retry indefinitely.</param>
		/// <param name="onSharingVio">Action to execute when a sharing violation does occur (is called before the waiting).</param>
		public static T WaitSharingVio<T>(Func<T> func, TimeSpan? maximum = null, Action onSharingVio = null)
		{
			var started = DateTime.UtcNow;
			int sleep = 279;
			while (true)
			{
				try
				{
					return func();
				}
				catch (IOException ex)
				{
					var hResult = 0;
					try { hResult = (int)ex.GetType().GetProperty("HResult", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ex, null); }
					catch { }
					if (hResult != -2147024864) // 0x80070020 ERROR_SHARING_VIOLATION
						throw;
					if (onSharingVio != null)
						onSharingVio();
				}

				if (maximum != null)
				{
					int leftMs = (int)(maximum.Value - (DateTime.UtcNow - started)).TotalMilliseconds;
					if (sleep > leftMs)
					{
						Thread.Sleep(leftMs);
						return func(); // or throw the sharing vio exception
					}
				}

				Thread.Sleep(sleep);
				sleep = Math.Min((sleep * 3) >> 1, 10000);
			}
		}
		#endregion File Locking Methods
		#endregion Methods

		#region Event Handlers
		#endregion Event Handlers
	}
}
