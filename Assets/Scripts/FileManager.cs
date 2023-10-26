using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using SFB;
using UnityEngine.UI;
using Newtonsoft.Json.Converters;

public class FileManager : MonoBehaviour {
	//File
	private string filePath;
	private readonly List<Unit> unitDatabase = new();

	//UI
	public bool Automate = false;
	public UIPopup popup;
	private TMP_InputField foundUnit;
	private TMP_Dropdown sortBy;

	//Assignable by user
	public string SearchedIdentificator { get; set; }
	public string UnitName { get; set; }
	public string UnitTierText { get; set; }
	public UnitTier SearchedTier;
	public UnitTier MinTier;
	private Unit SearchedUnit;
	public void SetTier(int value) { SearchedTier = (UnitTier)value + 10 + (value > 8 ? 2 : 0); } //UnitTier shift
	public int GetTier() { return (int)SearchedTier - 10 - (SearchedTier > UnitTier.X ? 2 : 0); } //UnitTier shift

	public void SetMinTier(int value) { MinTier = (UnitTier)value + 10 + (value > 8 ? 2 : 0); } //UnitTier shift
	public int GetMinTier() { return (int)MinTier - 10 - (MinTier > UnitTier.X ? 2 : 0); } //UnitTier shift

	private void Start() {
		sortBy = gameObject.transform.parent.Find("SortBy").GetComponent<TMP_Dropdown>();
		foundUnit = gameObject.transform.parent.Find("Unit").GetChild(0).GetComponent<TMP_InputField>();
		sortBy.value = 0;
		SearchedIdentificator = "";

		Screen.SetResolution(Screen.currentResolution.width/2, Screen.currentResolution.height/2, FullScreenMode.Windowed);

		LoadDictionary();
		if (Automate) AutomateImport();
	}


	private void AutomateImport() {
		
#if UNITY_EDITOR_OSX
		filePath = "/Users/toonu/Downloads/Iconian Order of Battle - OOB.csv";
#elif UNITY_EDITOR
		filePath = "C:/Users/Toonu/Downloads/Order of Battle - OOB.csv";
#elif UNITY_STANDALONE_OSX
		filePath = "/Users/toonu/Downloads/Iconian Order of Battle - OOB.csv";
#else
		filePath = "C:/Users/Toonu/Downloads/Iconian Order of Battle - OOB.csv";
#endif
		LoadCSVFile();
		
		//Automatic printout of units of Brigade level
		foreach (Unit unit in unitDatabase) {
			if (unit.info.unitTier > UnitTier.III && unit.info.unitTier < UnitTier.XXX) {
				MinTier = UnitTier.I;
				SearchedUnit = unit;
				ParseJSONFile();
			} else if (unit.info.unitTier > UnitTier.X) {
				MinTier = UnitTier.III;
				SearchedUnit = unit;
				ParseJSONFile();
			}
		}

#if UNITY_EDITOR
		EditorApplication.ExitPlaymode();
#else
		Application.Quit();
#endif
		
	}

	public void FindUnit() {
		if (unitDatabase.Count == 0) popup.PopUp("Load your file first!", 1.2f);
		foreach (Unit unit in unitDatabase) {
			if (unit.info.unitTier == SearchedTier && unit.info.FullDesignation.ToLower().Contains(SearchedIdentificator.ToLower())) {
				SearchedUnit = unit;
				foundUnit.text = unit.info.FullDesignation;
			}
		}
	}

	public void LoadDictionary() {
		//Load dictionary from JSON file
		string json = File.ReadAllText(Application.dataPath + "/Resources/UnitDictionary.json");
		UnitDictionary data = JsonConvert.DeserializeObject<UnitDictionary>(json, new JsonSerializerSettings {
			Converters = { new StringEnumConverter() }
		});
		data.SetDictionaryPatternsStatic();
	}

	public void LoadCSVFile() {
		unitDatabase.Clear();
		List<string> csvRows;
		try {
			if (!Automate) filePath = StandaloneFileBrowser.OpenFilePanel("Open File", "", new ExtensionFilter[] { new ExtensionFilter("Chart", "csv") }, false)[0];
			csvRows = File.ReadAllLines(filePath, Encoding.Default).ToList();
			unitDatabase.Clear();
		} catch (Exception e) {
			popup.PopUp("There was error loading the file! " + e.Message, 5);
			return;
		}

		//Setting separator to either , or ; and assigning correct column to each field
		char separator;
		if (csvRows[0].Contains(',')) separator = ','; else separator = ';';
		string[] fields = csvRows[0].Split(separator);
		int rowLocation = 0;
		int rowID = 1;
		int rowDesignation = 2;
		int rowTier = 3;
		int rowNotes = 4;
		int rowEquipment = 5;
		for (int i = 0; i < fields.Length; i++) {
			if (fields[i].ToLower().Contains("base")) rowLocation = i;
			else if (fields[i].ToLower().Contains("no.")) rowID = i;
			else if (fields[i].ToLower().Contains("class /")) rowDesignation = i;
			else if (fields[i].ToLower().Contains("type")) rowTier = i;
			else if (fields[i].ToLower().Contains("notes")) rowNotes = i;
			else if (fields[i].ToLower().Contains("equipment")) rowEquipment = i;
		}

		csvRows.Skip(1); //Skip the header row
		foreach (var row in csvRows.Skip(1)) {
			fields = row.Split(separator);
			if (fields.Length > 3 && fields[rowLocation] == "XXENDXX") break; // End of chart useful space

			if (fields.Length > 1 && !string.IsNullOrEmpty(fields[rowID]) && !string.IsNullOrEmpty(fields[2])) {
				unitDatabase.Add(new(new Info(fields[rowID], fields[rowDesignation], fields[rowTier], fields[rowLocation], fields[rowNotes], fields[rowEquipment])));
			}
		}
		Debug.Log("Import finished");
		popup.PopUp($"File with {unitDatabase.Count} units loaded!", 0.9f);
	}

	public void ParseJSONFile() {
		if (unitDatabase.Count == 0) { popup.PopUp("First load your unit file!", 2); return; }

		//Finding unit and creating list of its subordinate units.
		List<Unit> units = new();
		Unit unit = null;

		//When no searched unit assigned
		if (string.IsNullOrEmpty(SearchedIdentificator) && SearchedUnit == null) { SearchedUnit = unitDatabase[0]; }

		foreach (var candidateUnit in unitDatabase) {
			if (unit == null && candidateUnit.info.ID == SearchedUnit.info.ID && candidateUnit.info.FullDesignation == SearchedUnit.info.FullDesignation) {
				unit = candidateUnit;
				units.Add(candidateUnit);
			} else if (unit != null && candidateUnit.info.unitTier >= SearchedUnit.info.unitTier) {
				break;
			} else if (unit != null && (candidateUnit.info.unitTier >= MinTier || (candidateUnit.info.domain != Domain.land && MinTier == UnitTier.I))) {
				units.Add(candidateUnit);
			}
		}
		if (unit == null) { popup.PopUp("Unit not found! Unit number is checked or its description!", 5); return; }

		//Create a hierarchy of units
		Dictionary<UnitTier, int> currentUnits = new() {{ SearchedUnit.info.unitTier, 0 }};
		for (int position = 1; position < units.Count; position++) {
			currentUnits[units[position].info.unitTier] = position;
			units[FindHigherEchelon(units[position], currentUnits, position)].subordinates.Add(units[position]);
		}

		//Assign HQ, Weapons and Ship colours
		foreach (Unit currentUnit in units) {
			SwapColour(currentUnit);
		}

		//Shuffle the lower echelons to back
		foreach (Unit currentUnit in units) {
			if (sortBy.value == 0) {
				currentUnit.subordinates = currentUnit.subordinates
				.OrderByDescending(x => x.info.unitTier)
				.ThenBy(z => {
					if (int.TryParse(z.info.ID, out int parsedValue)) return parsedValue;
					else return int.MaxValue;
				})
				.ToList();
			} else {
				currentUnit.subordinates = currentUnit.subordinates
				.OrderByDescending(z => {
					if (int.TryParse(z.info.ID, out int parsedValue)) return parsedValue;
					else return int.MaxValue;
				})
				.ThenBy(x => x.info.unitTier)
				.ToList();
			}


			//Put HQ at the top no matter what
			Unit HQ = currentUnit.subordinates.Find(x => x.info.m1 == Modifier1.HQ);
			if (HQ != null) {
				currentUnit.subordinates.Remove(HQ);
				currentUnit.subordinates.Insert(0, HQ);
			}
		}


		//Command check
		UnitTier backup = UnitTier.Empty;
		if (units.First().info.unitTier > UnitTier.III && units.First().info.FullDesignation.ToLower().Contains("command")) {
			backup = units.First().info.unitTier;
			units.First().info.unitTier = UnitTier.Command;
			units.First().info.sidc = units.First().info.CalculateSIDC();
		}

		ExportJSON(unit, $"{EnumUtils.ParseTier(unit.info.unitTier),-5} {unit.info.FullDesignation}");

		//Cleanup
		//Command switch back
		if (units.First().info.unitTier > UnitTier.III && units.First().info.FullDesignation.ToLower().Contains("command")) {
			units.First().info.unitTier = backup;
			units.First().info.sidc = units.First().info.CalculateSIDC();
		}
		foreach (Unit u in units) {
			u.subordinates.Clear();
		}
		unit.subordinates.Clear();
		units.Clear();
		Debug.Log("Export complete!");
		popup.PopUp("Export complete!", 1.2f);
	}

	private void SwapColour(Unit unit) {
		foreach (Unit subordinate in unit.subordinates) {
			if (subordinate.info.unitType == UnitType.Empty || subordinate.info.m1 == Modifier1.HQ || subordinate.info.unitType >= UnitType.CV || subordinate.info.m1 == Modifier1.Weapons) {
				subordinate.info.fillColor = unit.info.fillColor;
			}
			SwapColour(subordinate);
		}
	}

	private int FindHigherEchelon(Unit currentUnit, Dictionary<UnitTier, int> currentUnits, int position) {
		//Create a new temporary dictionary with units higher than the current unit echelon and get closest distance to unit.
		return currentUnits
			.Where(pair => pair.Key > currentUnit.info.unitTier)
			.ToDictionary(pair => pair.Key, pair => pair.Value).Values
			.OrderBy(value => Math.Abs(value - position))
			.First();
	}

	private void ExportJSON(Unit unit, string fileName) {
		//Transform old path into new file path, check for existing file and so on.
		filePath ??= Application.dataPath; //Assign default if empty
		string newFilePath = Path.Combine(Path.GetDirectoryName(filePath), $"{fileName}.json");
		if (!File.Exists(newFilePath)) File.Create(newFilePath).Close();
		string jsonString = JsonConvert.SerializeObject(unit,
			new JsonSerializerSettings() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore });
		jsonString = jsonString.Replace("  ", "    ");
		File.WriteAllText(newFilePath, "");
		File.WriteAllText(newFilePath, jsonString);
	}
}


