using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[Serializable]
public class UnitDictionary {
	public static Dictionary<UnitType, string> dps { get; } = new Dictionary<UnitType, string>();
	public static Dictionary<UnitTier, string> dtps { get; } = new Dictionary<UnitTier, string>();
	public static Dictionary<Domain, string> dtyps { get; } = new Dictionary<Domain, string>();
	public static string mms { get; private set; } = "";
	public Dictionary<UnitType, string> dictionaryPatterns;
	public Dictionary<UnitTier, string> dictionaryTierPatterns;
	public Dictionary<Domain, string> dictionaryTypePatterns;
	public string mobilityMotorisation;

	public void SetDictionaryPatternsStatic() {
		foreach (KeyValuePair<UnitType,string> item in dictionaryPatterns) {
			string newPattern;
			//Checking for complex regex
			if (Regex.IsMatch(item.Value, "^(?:\\^|\\.|\\().*")) newPattern = item.Value;
			else {
				List<string> correctedValues = new();
				string[] values = item.Value.Split("|");
				foreach (string value in values) {
					if (string.IsNullOrEmpty(value)) continue;
					else correctedValues.Add(value.ToLower());
				}
				newPattern = $".*(?:{string.Join("|", correctedValues)}).*";
			}
			dps.Add(item.Key, newPattern);
		}
		foreach (KeyValuePair<UnitTier, string> item in dictionaryTierPatterns) {
			string newPattern;
			List<string> correctedValues = new();
			string[] values = item.Value.Split("|");
			foreach (string value in values) {
				if (string.IsNullOrEmpty(value)) continue;
				else correctedValues.Add(value.ToLower());
			}
			newPattern = $".*(?:{string.Join("|", correctedValues)}).*";
			dtps.Add(item.Key, newPattern);
		}
		foreach (KeyValuePair<Domain, string> item in dictionaryTypePatterns) {
			string newPattern;
			List<string> correctedValues = new();
			string[] values = item.Value.Split("|");
			foreach (string value in values) {
				if (string.IsNullOrEmpty(value)) continue;
				else correctedValues.Add(value.ToLower());
			}
			newPattern = $".*(?:{string.Join("|", correctedValues)}).*";
			dtyps.Add(item.Key, newPattern);
		}
		mms = $".*(?:{string.Join("|", mobilityMotorisation.Split("|"))}).*";
	}
}