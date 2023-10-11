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


public class FileManager : MonoBehaviour {
	//File
	private string filePath;
	private string[] filePaths;
	private readonly List<Unit> unitDatabase = new();

	//UI
	public TMP_Dropdown dropdown;
	public UIPopup popup;

	//Assignable by user
	public string UnitIdentificator { get; set; }
	public string UnitName { get; set; }
	public string UnitTierText { get; set; }
	public UnitTier Tier;
	public void SetTier(int value) { Tier = (UnitTier)value + 10 + (value > 8 ? 2 : 0); } //UnitTier shift
	public int GetTier() { return (int)Tier - 10 - (Tier > UnitTier.X ? 2 : 0); } //UnitTier shift

	private void Start() {
		dropdown.ClearOptions();

		UnitTier[] enumValues = (UnitTier[])Enum.GetValues(typeof(UnitTier));
		string[] enumNames = new string[enumValues.Length];
		for (int i = 1; i < enumValues.Length; i++) {
			enumNames[i] = enumValues[i].ToString();
		}
		dropdown.AddOptions(new List<string>(enumNames));
		dropdown.value = GetTier();

		Screen.fullScreen = false;

		
#if UNITY_EDITOR_OSX
		filePath = "/Users/toonu/Downloads/Iconian Order of Battle - OOB.csv";
#elif UNITY_EDITOR
		filePath = "C:/Users/Toonu/Downloads/Iconian Order of Battle - OOB.csv";
#elif UNITY_STANDALONE_OSX
		filePath = "/Users/toonu/Downloads/Iconian Order of Battle - OOB.csv";
#else
		filePath = "C:/Users/Toonu/Downloads/Iconian Order of Battle - OOB.csv";
#endif
		LoadCSVFile();

		foreach (Unit unit in unitDatabase) {
			if (unit.info.unitTier > UnitTier.III && unit.info.unitTier < UnitTier.XX) {
				UnitIdentificator = unit.info.fullDesignation.ToLower();
				Tier = unit.info.unitTier;
				ParseJSONFile($"{EnumUtils.ParseTier(unit.info.unitTier),5} {unit.info.fullDesignation}");
			}
		}


#if UNITY_EDITOR
		EditorApplication.ExitPlaymode();
#else
		Application.Quit();
#endif

	}

	public void LoadCSVFile() {
		unitDatabase.Clear();
		popup.PopUp("Loading!", 0);
		popup.gameObject.SetActive(true);
		List<string> csvRows;
		try {
			if (filePath == null || filePath == "") { filePaths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "", false); filePath = filePaths[0]; } if (filePath == null || filePath == "") return;
			csvRows = File.ReadAllLines(filePath, Encoding.Default).ToList();
			unitDatabase.Clear();
		} catch (Exception e) {
			popup.PopUp("There was error loading the file! " + e.Message, 5);
			return;
		}

		csvRows.Skip(1); //Skip the header row
		foreach (var row in csvRows.Skip(1)) {
			string[] fields = row.Split(',');
			if (fields.Length > 3 && fields[0] == "XXENDXX") break; // End of chart useful space

			if (fields.Length > 1 && !string.IsNullOrEmpty(fields[1]) && !string.IsNullOrEmpty(fields[2])) {
				unitDatabase.Add(new(new Info(fields[1], fields[2], fields[3], fields[0], fields[4])));
			}
		}
		popup.gameObject.SetActive(false);
		Debug.Log("Import finished");
	}

	public void ParseJSONFile(string fileName = "Units") {
		if (unitDatabase.Count == 0) { popup.PopUp("First load your unit file!", 5); return; }

		//Finding unit and creating list of its subordinate units.
		List<Unit> units = new();
		Unit unit = null;
		//When no searched unit assigned
		if (string.IsNullOrEmpty(UnitIdentificator)) { UnitIdentificator = unitDatabase[0].info.designation; Tier = unitDatabase[0].info.unitTier; }

		foreach (var candidateUnit in unitDatabase) {
			if (unit == null && candidateUnit.info.unitTier == Tier && (candidateUnit.info.ID == UnitIdentificator || candidateUnit.info.fullDesignation.ToLower().Contains(UnitIdentificator.ToLower()))) {
				unit = candidateUnit;
				units.Add(candidateUnit);
			} else if (unit != null && candidateUnit.info.unitTier >= Tier) {
				break;
			} else if (unit != null) {
				units.Add(candidateUnit);
			}
		}
		if (unit == null) { popup.PopUp("Unit not found! Unit number is checked or its description!", 5); return; }

		popup.PopUp("Exporting!", 0);
		popup.gameObject.SetActive(true);

		//Create a hierarchy of units
		Dictionary<UnitTier, int> currentUnits = new() {{ Tier, 0 }};
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
			currentUnit.subordinates = currentUnit.subordinates
				.OrderByDescending(x => x.info.unitTier)
				.ThenBy(z => int.Parse(z.info.ID))
				.ToList();

			//Put HQ at the top no matter what
			Unit HQ = currentUnit.subordinates.Find(x => x.info.m1 == Modifier1.HQ);
			if (HQ != null) {
				currentUnit.subordinates.Remove(HQ);
				currentUnit.subordinates.Insert(0, HQ);
			}
		}		

		ExportJSON(unit, fileName);

		//Cleanup
		foreach (Unit u in units) {
			u.subordinates.Clear();
		}
		unit.subordinates.Clear();
		units.Clear();
		popup.gameObject.SetActive(false);
		Debug.Log("Export complete!");
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
		string newFilePath = $"{string.Join("/", filePath.Split('/').Take(filePath.Split('/').Length - 1))}/{fileName}.json";
		if (!File.Exists(newFilePath)) File.Create(newFilePath).Close();
		string jsonString = JsonConvert.SerializeObject(unit,
			new JsonSerializerSettings() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore });
		jsonString = jsonString.Replace("  ", "    ");
		File.WriteAllText(newFilePath, "");
		File.WriteAllText(newFilePath, jsonString);
	}
}


