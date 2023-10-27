using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[Serializable]
public class UnitDictionary {
	public static Dictionary<UnitType, string> dps { get; } = new Dictionary<UnitType, string>();
	public Dictionary<UnitType, string> dictionaryPatterns;

	public void SetDictionaryPatternsStatic() {
		foreach (KeyValuePair<UnitType,string> item in dictionaryPatterns) {
			string newPattern;
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
	}
}