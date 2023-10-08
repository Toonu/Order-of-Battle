using System.IO;
using UnityEngine;

public class Logging : MonoBehaviour {
	public string logFileName = "output.log";
	private readonly System.Collections.Generic.List<string> logMessages = new();
	private StreamWriter logFileWriter;

	void Start() {
		// Subscribe to the log message received event
		Application.logMessageReceived += HandleLog;

		// Open the log file for writing
		logFileWriter = File.CreateText(Path.Combine(Application.dataPath, logFileName));
	}

	void HandleLog(string logString, string stackTrace, LogType type) {
		// Add the log message to the list
		logMessages.Add(logString);

		// Write the log message to the log file
		logFileWriter.WriteLine("[{0}] {1}: {2}", type, System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), logString);

		// Flush the log messages to the file
		FlushLogMessages();
	}

	void FlushLogMessages() {
		logFileWriter.Flush();
	}

	void OnEnable() {
		Application.logMessageReceived += HandleException;
	}

	void OnDisable() {
		Application.logMessageReceived -= HandleException;
	}

	void HandleException(string logString, string stackTrace, LogType type) {
		Application.Quit();
	}
}