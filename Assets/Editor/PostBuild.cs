using System;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

#if UNITY_EDITOR

public class PostBuild : IPostprocessBuildWithReport {
	public int callbackOrder => 0;

	public void OnPostprocessBuild(BuildReport report) {
		string sourcePath = Path.Combine(Application.dataPath, "Resources/UnitDictionary.json");

		if (File.Exists(sourcePath)) {
			string destinationPath = Path.GetDirectoryName(report.summary.outputPath) + "/OOB Converter_Data/Resources/UnitDictionary.json";
			if (report.summary.platform == UnityEditor.BuildTarget.StandaloneOSX) return; //destinationPath = report.summary.outputPath + "/Contents/Resources/UnitDictionary.json";

			try {
				File.Copy(sourcePath, destinationPath, true);
				Debug.Log("UnitDictionary.json copied to StreamingAssets folder.");
			} catch (Exception e) {
				Debug.LogError("Error copying UnitDictionary.json: " + e.Message);
			}
		} else {
			Debug.LogError("UnitDictionary.json not found in Resources folder.");
		}
	}
}


#endif