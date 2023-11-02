using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum Domain { air = 1, land = 10, naval = 30, submarine = 35 }

[Serializable]
public class Unit {
	[JsonProperty("options")]
	public Info info;
	[JsonProperty("subOrganizations")]
	public List<Unit> subordinates = new();

	public Unit() { }
	public Unit(Info options) : this() {
		info = options;
	}

	public override string ToString() {
		return $"[{info.unitTier,-4}] [{info.unitType}] {info.ID,3} {info.designation} {info.tierText} - {info.additionalInformation ?? ""}";
	}
}

[Serializable]
public class Info {
	#region Properties
	[JsonProperty("color")]
	public readonly string colour = "Custom Color";

	[JsonProperty("sidc")]
	public string sidc;

	[JsonProperty("uniqueDesignation")]
	public string FullDesignation {
		set { FullDesignation = value; }
		get { return $"{(UnitTier.Empty == unitTier ? ID : (Regex.IsMatch(tierText, ".*(base|instit).*") ? "" : AddOrdinalSuffix(ID)))} {designation} {(Regex.IsMatch(designation.ToLower(), ".*(school.*|academ.*|instit.*|command$|facility$|hospital$|centre$|depot$|storage$|administration$|station$|post$|base$|unit$|admiralty$|classis.*|ducenarii.*|corpo.*|copiis.*|branch.*|shop.*|team.*|group.*)") ? "" : tierText)}"; }
	}
	[JsonProperty("stack")]
	public int stack;
	[JsonProperty("specialheadquarter")]
	public string specialheadquarter = "";

	[JsonIgnore]
	public string ID;
	[JsonIgnore]
	public string designation;
	[JsonIgnore]
	public string tierText;
	[JsonIgnore]
	public UnitTier unitTier;
	[JsonIgnore]
	public UnitType unitType;
	[JsonIgnore]
	public Modifier1 m1;
	[JsonIgnore]
	public readonly Modifier2 m2;
	[JsonIgnore]
	public readonly Domain domain;
	[JsonIgnore]
	public string notes = "";

	[JsonProperty("additionalInformation")]
	public string additionalInformation;

	[JsonProperty("fillColor")]
	public string fillColor;

	private readonly bool isReserve = false;
	#endregion

	public Info() { }
	public Info(string ID, string designation, string tierText, string location = null, string notes = null, string eq = null) {
		domain = Domain.land; unitType = UnitType.Empty; m1 = Modifier1.Empty; m2 = Modifier2.Empty;

		if (Regex.IsMatch(tierText.ToLower(), UnitDictionary.dtps[UnitTier.Team])) unitTier = UnitTier.Team;
		else if (Regex.IsMatch(tierText.ToLower(), UnitDictionary.dtps[UnitTier.Squad])) unitTier = UnitTier.Squad;
		else if (Regex.IsMatch(tierText.ToLower(), UnitDictionary.dtps[UnitTier.Section])) unitTier = UnitTier.Section;
		else if (Regex.IsMatch(tierText.ToLower(), UnitDictionary.dtps[UnitTier.Platoon])) unitTier = UnitTier.Platoon;
		else if (Regex.IsMatch(tierText.ToLower(), UnitDictionary.dtps[UnitTier.I])) unitTier = UnitTier.I;
		else if (Regex.IsMatch(tierText.ToLower(), UnitDictionary.dtps[UnitTier.II])) unitTier = UnitTier.II;
		else if (Regex.IsMatch(tierText.ToLower(), UnitDictionary.dtps[UnitTier.III])) unitTier = UnitTier.III;
		else if (Regex.IsMatch(tierText.ToLower(), UnitDictionary.dtps[UnitTier.X])) unitTier = UnitTier.X;
		else if (Regex.IsMatch(tierText.ToLower(), UnitDictionary.dtps[UnitTier.XX])) unitTier = UnitTier.XX;
		else if (Regex.IsMatch(tierText.ToLower(), UnitDictionary.dtps[UnitTier.XXX])) unitTier = UnitTier.XXX;
		else if (Regex.IsMatch(tierText.ToLower(), UnitDictionary.dtps[UnitTier.XXXX])) unitTier = UnitTier.XXXX;
		else if (Regex.IsMatch(tierText.ToLower(), UnitDictionary.dtps[UnitTier.XXXXX])) unitTier = UnitTier.XXXXX;
		else if (Regex.IsMatch(tierText.ToLower(), UnitDictionary.dtps[UnitTier.XXXXXX])) unitTier = UnitTier.XXXXXX;

		this.ID = ID.Trim();
		this.designation = Regex.Replace(designation, "(?:^\"|\"$)", "").Trim();
		this.designation = Regex.Replace(this.designation, "\"\"", "\"");
		this.tierText = tierText.Trim();
		this.notes = notes.Trim();
		if (location != null && location != "") additionalInformation = location;
		if (notes != null && notes != "") { if (Regex.IsMatch(notes, "^[Rr]eserve.*")) isReserve = true; }

		if (unitTier == UnitTier.Empty) { //Ships
			if (Regex.IsMatch(tierText.ToUpper(), "^S(S|O)")) domain = Domain.submarine; else { domain = Domain.naval; }
			fillColor = "#0065bd";
			additionalInformation = eq.Trim();
			Enum.TryParse(tierText.ToUpper(), out unitType);
		} else {
			(unitType, fillColor) = ConvertIDToType(this);
			m1 = SetModifier1(this);
			m2 = SetModifier2(this);
		}

		if (domain == Domain.land && location == "" && eq != null && eq != "") additionalInformation = eq.Trim();

		sidc = CalculateSIDC();
	}


	public override string ToString() {
		return FullDesignation;
	}

	public string CalculateSIDC() {
		return "3003" + (int)domain + (isReserve ? 1 : 0) + "0" + $"{(int)unitTier:D2}" + $"{(int)unitType:D6}" + (domain == Domain.land ? $"{(int)m1:D2}" + $"{(int)m2:D2}" : "");
	}

	public static Modifier1 SetModifier1(Info unit) {
		string tierText = unit.tierText.ToLower();
		string designation = unit.designation.ToLower();
		if (Regex.IsMatch(designation, ".*(support command).*")) return Modifier1.Army;
		if (Regex.IsMatch(designation, "^(?=(.*?(?:hq|headquarters|hh|admiralty|command|praesidium|institute|directorate|department|medical support adm|c\\d).*))(?!.*secu)")) return Modifier1.HQ;
		if (designation.Contains("fighter")) return Modifier1.Force;
		if (designation.Contains("helicopter")) {
			if (Regex.IsMatch(designation, ".*(?<!c)sar.*")) return Modifier1.SAR;
			else if (Regex.IsMatch(designation, ".*(?:attack|assault).*")) return Modifier1.Attack;
			else if (Regex.IsMatch(designation, ".*sof.*")) return Modifier1.SOF;
			else if (Regex.IsMatch(designation, ".*csar.*")) return Modifier1.Empty;
			else return Modifier1.Utility;
		}
		if (Regex.IsMatch(designation, ".*(?:aviation).+")) return Modifier1.Aviation;
		if (Regex.IsMatch(designation, ".*ua[v|s].*") && unit.unitType != UnitType.UnmannedAerialVehicle) return Modifier1.UAV;
		if (Regex.IsMatch(designation, ".*(?:naval|anti-ship|coastal).*") && !Regex.IsMatch(tierText, ".*(?:task|fleet).*")) return Modifier1.Naval;
		if (designation.Contains("shoreline")) return Modifier1.AmphibiousShip;

		if (designation.Contains("rocket")) return Modifier1.MLRS;
		if (designation.Contains("strategic artillery")) return Modifier1.Missile;
		if (designation.Contains("fdc")) return Modifier1.FDC;
		if (designation.Contains("lhs")) return Modifier1.LHS;
		if (designation.Contains("pls")) return Modifier1.PLS;
		if (Regex.IsMatch(designation, ".*sof.*") && Regex.IsMatch(tierText, ".*(?:wing|squadron|flight).*")) return Modifier1.SOF;

		if (designation.Contains("weapons")) return Modifier1.Weapons;
		if (designation.Contains("airbase")) return Modifier1.Aifield;
		if (designation.Contains("bridg")) return Modifier1.Bridging;
		if (Regex.IsMatch(designation, ".*(?:drilling).*")) return Modifier1.Drilling;
		if (Regex.IsMatch(designation, "(?:.*satellite.*)") && !Regex.IsMatch(tierText, ".*(?:wing|squadron|flight).*")) return Modifier1.TacticalSatellite;
		if (designation.Contains("network")) return Modifier1.Network;
		if (Regex.IsMatch(designation, ".*construction(?!depot).*")) return Modifier1.Construction;

		if (Regex.IsMatch(designation, ".*medic.*(evac|trans).*")) return Modifier1.MEDEVAC;
		if (Regex.IsMatch(designation, ".*[^c]sar.*")) return Modifier1.SAR;
		if (Regex.IsMatch(unit.notes, ".*[rR]ole.1.*")) return Modifier1.MED1;
		if (Regex.IsMatch(unit.notes, ".*[rR]ole.2.*")) return Modifier1.MED2;
		if (Regex.IsMatch(unit.notes, ".*[rR]ole.3.*")) return Modifier1.MED3;
		if (Regex.IsMatch(unit.notes, ".*[rR]ole.4.*")) return Modifier1.MED4;
		if (Regex.IsMatch(designation, ".*combat.*eng.*")) return Modifier1.Combat;

		if (designation.Contains("brigade")) return Modifier1.Brigade;
		if (designation.Contains("division")) return Modifier1.Division;
		if (designation.Contains("corps")) return Modifier1.Corps;
		if (designation.Contains("army")) return Modifier1.Army;
		if (designation.Contains("theater")) return Modifier1.Theatre;
		if (designation.Contains("independent")) return Modifier1.IndependentCommand;

		if (designation.Contains("electro-optical")) return Modifier1.ElectroOptical;
		if (designation.Contains("node")) return Modifier1.JointNetworkNode;
		if (designation.Contains("c2")) return Modifier1.C2;
		if (designation.Contains("command post")) return Modifier1.CommandPostNode;
		if (designation.Contains("retransmission")) return Modifier1.RetransmissionSite;
		if (Regex.IsMatch(designation, ".*(?:sensor|space inf).*")) return Modifier1.Sensor;
		if (designation.Contains("virtual")) return Modifier1.Digital;
		if (Regex.IsMatch(designation, ".*(?:electronic installation).*")) return Modifier1.SIGINT;
		
		if (designation.Contains("dog")) return Modifier1.Dog;
		if (Regex.IsMatch(designation, ".*(?:radar|missile warn|space (?:comm|oper)).*")) return Modifier1.Radar;
		if (designation.Contains("radiofrequency")) return Modifier1.RFID;
		if (designation.Contains("route	safety")) return Modifier1.RouteReconnaissanceAndClearance;

		return Modifier1.Empty;
	}

	public static Modifier2 SetModifier2(Info unit) {
		string designation = unit.designation.ToLower();
		string tierText = unit.tierText.ToLower();
		if (Regex.IsMatch(designation, ".*(?:airborne|parachut).*")) return Modifier2.Airborne;
		if (Regex.IsMatch(designation, UnitDictionary.mms)) return Modifier2.Wheeled;
		if (designation.Contains("tracked")) return Modifier2.Tracked;
		if (Regex.IsMatch(designation, ".*(?:towed|horse).*")) return Modifier2.Towed;
		if (Regex.IsMatch(designation, ".*air.*assault.*")) return Modifier2.AirAssault;
		if (designation.Contains("amphibious")) return Modifier2.Amphibious;
		if (designation.Contains("arctic")) return Modifier2.Arctic;
		if (designation.Contains("bicycle")) return Modifier2.Bicycle;
		if (Regex.IsMatch(designation, ".*rail(?!.*transp).*")) return Modifier2.Railroad;
		if (designation.Contains("river")) return Modifier2.Riverine;

		if (designation.Contains("supply")) return Modifier2.Supply;
		if (designation.Contains("support") && !designation.Contains("hospital") && !designation.Contains("service")) return Modifier2.Support;

		if (designation.Contains("csar")) return Modifier2.CSAR;
		if (Regex.IsMatch(designation, ".*(helicopter|heavy.*defen[s|c]e).*")) return Modifier2.Heavy;
		if (Regex.IsMatch(designation, ".*transport.*") && Regex.IsMatch(tierText, ".*(?:wing|squadron|flight).*")) return Modifier2.Troop;
		if (designation.Contains("fighter-bomber")) return Modifier2.Attack;
		if (designation.Contains("cyber")) return Modifier2.Cyberspace;
		if (designation.Contains("decon")) return Modifier2.Decontamination;
		if (designation.Contains("dental")) return Modifier2.Dental;
		if (designation.Contains("psychological med")) return Modifier2.Psychological;
		if (designation.Contains("laboratory")) return Modifier2.Laboratory;
		if (designation.Contains("veterinary")) return Modifier2.Veterinary;
		if (designation.Contains("launcher")) return Modifier2.Launcher;
		if (designation.Contains("mountain")) return Modifier2.Mountain;
		if (designation.Contains("horse")) return Modifier2.PackAnimal;
		if (designation.Contains("pipeline management")) return Modifier2.RecoveryMaintenance;
		if (designation.Contains("launch and recovery")) return Modifier2.RecoveryUnmannedSystems;
		if (designation.Contains("tactical")) return Modifier2.Tactical;
		if (designation.Contains("ski")) return Modifier2.Ski;
		return Modifier2.Empty;
	}

	public static (UnitType, string) ConvertIDToType(Info unit) {
		string designation = unit.designation.ToLower();
		designation = Regex.Replace(designation, "\".*\"", "").Trim(); //Replace quoted parts
		UnitType type = UnitType.Empty;
		string colour = "#5baa5b";

		# region Echelon matching
		if (Regex.IsMatch(unit.tierText.ToLower(), UnitDictionary.dtyps[Domain.air])) {
			colour = "#80e0ff";
			if (designation.Contains("cavalr")) { }
			else if (designation.Contains("helicopter")) {
				if (Regex.IsMatch(designation, ".*(?:recon|combin|attack).*")) type = UnitType.AviationReconnaissance;
				else type = UnitType.Aviation;
			}
			else if (Regex.IsMatch(designation, ".*(?:fighter|school).*")) type = UnitType.AviationFixedWing;
			else if (designation.Contains("combined")) type = UnitType.AviationComposite;
			else if (designation.Contains("uav")) type = UnitType.UnmannedAerialVehicle;
			else if (designation.Contains("cyber")) return (UnitType.ElectronicWarfare, "#ffffff");
			else if (designation.Contains("satellite")) return (UnitType.Sattelite, "#0065bd");
			else if (Regex.IsMatch(designation, ".*(?:space).*")) {
				if (designation.Contains("infantry")) return (UnitType.Infantry, "#0065bd");
				else if (designation.Contains("surve")) return (UnitType.AviationFixedWingReconnaissance, "#0065bd");
				else if (designation.Contains("comm")) return (UnitType.SignalTacticalSatellite, "#ffffff");
				return (UnitType.AviationFixedWing, "#0065bd");
			}
			else type = UnitType.AviationFixedWing;

			if (designation.Contains("recon") && type == UnitType.AviationFixedWing) type = UnitType.AviationFixedWingReconnaissance;
			return (type, colour);
		} else if (Regex.IsMatch(unit.tierText.ToLower(), UnitDictionary.dtyps[Domain.land])) {
			if (Regex.IsMatch(designation, UnitDictionary.htau)) return (UnitType.AviationFixedWing, "#80e0ff");
			else if (Regex.IsMatch(designation, ".*(?:division)*.*(?:support command).*(?:division)*.*")) return (UnitType.Supply, "#d87600");
			else if (unit.notes.Contains("IgnoreTier")) { }
			else return (UnitType.Infantry, "#5baa5b");
		} else if (Regex.IsMatch(unit.tierText.ToLower(), UnitDictionary.dtyps[Domain.naval])) {
			if (Regex.IsMatch(designation, ".*(?:patriae|coast.*guard).*")) return (UnitType.CoastGuard, "#d87600");
			else if (Regex.IsMatch(designation, ".*(?:trahis|investig).*")) return (UnitType.Naval, "#ffffff");
			else return (UnitType.Naval, "#0065bd");
		} else if (unit.tierText.ToLower() == "base") {
			if (Regex.IsMatch(designation, ".*(?:school|academy).*")) return (UnitType.Naval, "#0065bd");
			else return (UnitType.SeaportOfDebarkation, "#0065bd");
		} else if (unit.tierText.ToLower() == "airbase") return (UnitType.AirportOfDebarkation, "#80e0ff");
		#endregion

		//Name matching
		#region Maneuvre
		if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.CombinedArms])) type = UnitType.CombinedArms;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Infantry])) {
			if (Regex.IsMatch(designation, ".*(?:mechani[z|s]ed|armou?red|vehicle).*")) type = UnitType.InfantryMechanized;
			else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.InfantryMotorized])) type = UnitType.InfantryMotorized;
			else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.InfantryAmphibious])) type = UnitType.InfantryAmphibious;
			else type = UnitType.Infantry;
		} else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.InfantryAmphibious])) {
			if (Regex.IsMatch(designation, ".*(?:mechani[z|s]ed|armou?red|medium|heavy).*")) type = UnitType.ArmouredAmphibious;
			else type = UnitType.InfantryAmphibious;
		} else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Sniper])) type = UnitType.Sniper;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.AirAssault])) type = UnitType.AirAssault;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Security])) {
			if (Regex.IsMatch(designation, ".*(?:air|aerial).*")) return (UnitType.SecurityPoliceAir, "#80e0ff");
			else if (Regex.IsMatch(designation, ".*motori[z|s]ed.*")) type = UnitType.SecurityMotorized;
			else if (Regex.IsMatch(designation, ".*(?:armou?red|mechani).*")) type = UnitType.SecurityArmoured;
			else type = UnitType.Security;
		} else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Space])) type = UnitType.Space;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Dog])) type = UnitType.Dog;
		else colour = "#ffd00b"; //Cavalry
								 //Colour matching
		if (Regex.IsMatch(designation, UnitDictionary.dtyps[Domain.submarine])) colour = "#0065bd";
		else if (Regex.IsMatch(designation, ".*(?:aerial).*")) colour = "#80e0ff";
		if (type != UnitType.Empty) return (type, colour);
		#endregion
		#region Cavalry
		if (Regex.IsMatch(designation, ".*(?:armou?red|mechani[z|s]ed)*.*(?:hh|hq|headquarter|medical support adm).*(?:armou?red|mechani[z|s]ed)*.*")) type = UnitType.Armoured;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Reconnaissance])) {
			if (designation.Contains("nbcr")) type = UnitType.Empty;
			else if (Regex.IsMatch(designation, ".*(?:armou?red|mechani[z|s]ed|cavalr).*")) type = UnitType.ReconnaissanceArmoured;
			else if (designation.Contains("amph")) type = UnitType.ReconnaissanceAmphibious;
			else if (designation.Contains("motor")) type = UnitType.ReconnaissanceMotorized;
			else type = UnitType.Reconnaissance;
		} else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Armoured])) type = UnitType.Armoured;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.NavalFireLiason])) type = UnitType.NavalFireLiason;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.ReconnaissanceSurveillance])) type = UnitType.ReconnaissanceSurveillance;
		else colour = "#ff3333"; //Arty
								 //Colour matching
		if (Regex.IsMatch(designation, ".*(?:naval).*")) colour = "#0065bd";
		else if (Regex.IsMatch(designation, ".*(?:aerial).*")) colour = "#80e0ff";
		if (type != UnitType.Empty) return (type, colour);
		#endregion
		#region Artillery
		if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.AntiTank])) {
			if (designation.Contains("motori[z|s]ed")) type = UnitType.AntiTankMotorized;
			else if (designation.Contains("mechani[z|s]ed")) type = UnitType.AntiTankMechanized;
			else type = UnitType.AntiTank;
		} else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.AirDefense])) type = UnitType.AirDefense;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.AirDefenseMissile])) type = UnitType.AirDefenseMissile;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.ArtillerySPH])) type = UnitType.ArtillerySPH;
		else if (Regex.IsMatch(designation, $"{UnitDictionary.dps[UnitType.Artillery]}(?!.*ammu).*")) type = UnitType.Artillery;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.ArtilleryTargetAcquisition])) type = UnitType.ArtilleryTargetAcquisition;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.ArtilleryForwardObserver])) type = UnitType.ArtilleryForwardObserver;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Missile])) type = UnitType.Missile;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Mortar])) {
			if (designation.Contains("motori[z|s]ed")) type = UnitType.MortarWheeled;
			else if (designation.Contains("mechani[z|s]ed")) type = UnitType.MortarArmoured;
			else if (Regex.IsMatch(designation, ".*(towed|light).*")) type = UnitType.MortarTowed;
			else type = UnitType.Mortar;
		} else colour = "#ffffff"; //CS
								   //Colour matching
		if (Regex.IsMatch(designation, ".*(?:naval).*")) colour = "#0065bd";
		else if (Regex.IsMatch(designation, ".*(?:aerial).*")) colour = "#80e0ff";
		if (type != UnitType.Empty) return (type, colour);

		#endregion
		#region CS
		if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.SpecialOperationForces])) {
			if (designation.Contains("surface")) type = UnitType.SpecialOperationForcesGround;
			else if (designation.Contains("naval")) type = UnitType.SpecialOperationForcesNaval;
			else if (designation.Contains("diving")) type = UnitType.SpecialOperationForcesSubmarine;
			else type = UnitType.SpecialOperationForces;
			return (type, "#ffffff");
		} else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.SpecialForces])) type = UnitType.SpecialForces;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.UnderwaterDemolitionTeam])) type = UnitType.UnderwaterDemolitionTeam;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Medical])) type = UnitType.Medical;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.MedicalTreatmentFacility])) type = UnitType.MedicalTreatmentFacility;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Mine])) {
			if (designation.Contains("clear")) type = UnitType.MineClearing;
			else if (designation.Contains("throw")) type = UnitType.MineLaunching;
			else if (designation.Contains("lay")) type = UnitType.MineLaying;
			else type = UnitType.Mine;
		} else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Engineer])) {
			if (designation.Contains("motori[z|s]ed")) type = UnitType.EngineerMotorized;
			else if (Regex.IsMatch(designation, ".*(?:mechani[z|s]ed|armou?red).*")) type = UnitType.EngineerMechanized;
			else if (Regex.IsMatch(designation, ".*(?:survey|recon).*")) type = UnitType.EngineerSurvey;
			else type = UnitType.Engineer;
		} else if (designation.Contains("drilling")) type = UnitType.Drilling;
		else if (designation.Contains("pipe")) type = UnitType.Pipeline;
		else if (Regex.IsMatch(designation, ".*(?:combat support).*")) type = UnitType.Supply;

		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.EOD])) type = UnitType.EOD;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.AirTrafficServices])) type = UnitType.AirTrafficServices;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.MilitaryPolice])) type = UnitType.MilitaryPolice;
		else if (designation.Contains("geospat")) type = UnitType.Geospatial;
		else if (designation.Contains("meteo")) type = UnitType.Meteorological;
		else if (designation.Contains("topog")) type = UnitType.Topographic;
		else if (designation.Contains("survey")) type = UnitType.Survey;

		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.NBCR])) {
			if (designation.Contains("motori[z|s]ed")) type = UnitType.NBCRMotorized;
			else if (Regex.IsMatch(designation, ".*(?:mechani[z|s]ed|armou?red).*")) type = UnitType.NBCRMechanized;
			else type = UnitType.NBCR;

			if (Regex.IsMatch(designation, ".*(?:recon|detection).*")) {
				if (type == UnitType.NBCRMotorized) type = UnitType.NBCRReconnaissanceMotorized;
				else if (type == UnitType.NBCRMechanized) type = UnitType.NBCRReconnaissanceMechanized;
				else type = UnitType.NBCRReconnaissance;
			}
		} else if (designation.Contains("analysis") && designation.Contains("electronic war")) type = UnitType.ElectronicWarfareAnalysis;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Analysis])) type = UnitType.Analysis;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.CounterIntelligence])) type = UnitType.CounterIntelligence;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.SignalTacticalSatellite])) type = UnitType.SignalTacticalSatellite;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.MilitaryIntelligence])) type = UnitType.MilitaryIntelligence;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.ElectronicWarfareDirectionFinding])) type = UnitType.ElectronicWarfareDirectionFinding;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.ElectronicRanging])) type = UnitType.ElectronicRanging;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.ElectronicWarfareIntercept])) type = UnitType.ElectronicWarfareIntercept;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.ElectronicWarfareJamming])) type = UnitType.ElectronicWarfareJamming;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.ElectronicWarfareSearch])) type = UnitType.ElectronicWarfareSearch;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.ElectronicWarfare])) type = UnitType.ElectronicWarfare;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Sensor])) type = UnitType.Sensor;
		
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Signal])) type = UnitType.Signal;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.RadioRelay])) type = UnitType.RadioRelay;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.RadioTeletypeCentre])) type = UnitType.RadioTeletypeCentre;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Radio])) type = UnitType.Radio;

		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.InformationOperations])) type = UnitType.InformationOperations;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.BroadcastTransmitterAntenna])) type = UnitType.BroadcastTransmitterAntenna;

		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.ArmyMusic])) type = UnitType.ArmyMusic;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Liaison])) type = UnitType.Liaison;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.CommandAndControl])) type = UnitType.CommandAndControl;
		else colour = "#d87600"; //CSS
								 //Colour matching
		if (Regex.IsMatch(designation, ".*(?:aerial).*")) colour = "#80e0ff";
		if (type != UnitType.Empty) return (type, colour);
		#endregion
		#region CSS
		if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Supply])) {
			if (Regex.IsMatch(designation, ".*food.*")) type = UnitType.Class1;
			else if (Regex.IsMatch(designation, ".*quartermaster.*")) type = UnitType.Class2;
			else if (Regex.IsMatch(designation, ".*(?:petrol|fuel).*")) type = UnitType.Class3;
			else if (Regex.IsMatch(designation, ".*(?:engineer|construct).*")) type = UnitType.Class4;
			else if (Regex.IsMatch(designation, ".*ammunition.*")) type = UnitType.Class5;
			else if (Regex.IsMatch(designation, ".*personnal.*")) type = UnitType.Class6;
			else if (Regex.IsMatch(designation, ".*heavy.*")) type = UnitType.Class7;
			else if (Regex.IsMatch(designation, ".*medical.*")) type = UnitType.Class8;
			else if (Regex.IsMatch(designation, ".*parts.*")) type = UnitType.Class9;
			else if (Regex.IsMatch(designation, ".*civilian.*")) type = UnitType.Class10;
			else type = UnitType.Supply;
		} else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Class1])) type = UnitType.Class1;


		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Railhead])) type = UnitType.Railhead;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Transportation])) type = UnitType.Transportation;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Maintenance])) type = UnitType.Maintenance;

		else if (designation.Contains("water purif")) type = UnitType.WaterPurification;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Water])) type = UnitType.Water;
		else if (designation.Contains("quartermaster")) type = UnitType.Quartermaster;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.PetroleumOilLubricants])) type = UnitType.PetroleumOilLubricants;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.Ammunition])) type = UnitType.Ammunition;
		else if (designation.Contains("personnel services")) type = UnitType.PersonnelServices;
		else if (designation.Contains("ordnance")) type = UnitType.Ordnance;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.CombatServiceSupport])) type = UnitType.CombatServiceSupport;

		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.PublicAffairs])) type = UnitType.PublicAffairs;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.CivilAffairs])) type = UnitType.CivilAffairs;
		else if (designation.Contains("postal")) type = UnitType.Postal;
		else if (designation.Contains("financ")) type = UnitType.Finance;
		else if (designation.Contains("labour")) type = UnitType.Labour;
		else if (designation.Contains("laundry")) type = UnitType.Laundry;
		else if (Regex.IsMatch(designation, "(^prison$|.*internment.*)")) type = UnitType.Prison;
		else if (designation.Contains("mortuary")) type = UnitType.Mortuary;
		else if (Regex.IsMatch(designation, UnitDictionary.dps[UnitType.JudgeAdvocateGeneral])) type = UnitType.JudgeAdvocateGeneral;
		else if (designation.Contains("fire fighting")) type = UnitType.Firefighting;
		else if (designation.Contains("welfare")) type = UnitType.MoraleWelfareRecreation;
		else if (designation.Contains("administrat")) type = UnitType.Administrative;
		else if (designation.Contains("inspection")) {unit.specialheadquarter = "INSP"; return (UnitType.Empty, "#ffffff"); }
		#endregion
		if (type != UnitType.Empty) return (type, colour);
		if (designation.Contains("admiralty")) return (UnitType.Naval, "#0065bd");
		return (type, colour);
	}

	public static string AddOrdinalSuffix(string num) {
		if (int.TryParse(num, out int number)) {
			if (number >= 11 && number <= 13) return number + "th";
			return (number % 10) switch {
				1 => number + "st",
				2 => number + "nd",
				3 => number + "rd",
				_ => number + "th",
			};
		} else return num;
	}
}