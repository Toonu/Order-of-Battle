using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum Domain { land = 10, naval = 30, submarine = 35 }

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
		return $"[{info.sidc}] [{info.fillColor}] {info.ID,3} {info.designation} {info.tierText} - {info.additionalInformation ?? ""}";
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
	public string fullDesignation {
		set { fullDesignation = value; }
		get { return $"{(UnitTier.Empty == unitTier ? ID : (Regex.IsMatch(tierText, ".*(base|instit).*") ? "" : AddOrdinalSuffix(ID)))} {designation} {(Regex.IsMatch(designation.ToLower(), ".*(school.*|academ.*|instit.*|command$|facility$|hospital$|centre$|depot$|storage$|administration$|station$|post$|unit$|admiralty$|classis.*|ducenarii.*|corpo.*|copiis.*)") ? "" : tierText)}"; }
	}
	[JsonProperty("stack")]
	public int stack;

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
	Domain domain;

	[JsonProperty("additionalInformation")]
	public string additionalInformation;

	[JsonProperty("fillColor")]
	public string fillColor;

	private readonly bool isReserve = false;
	#endregion

	public Info() { }
	public Info(string ID, string designation, string tierText, string additionalInformation = null, string notes = null) {
		domain = Domain.land; unitType = UnitType.Empty; m1 = Modifier1.Empty; m2 = Modifier2.Empty;
		unitTier = tierText.ToLower() switch {
			"team" => UnitTier.Team,
			"squad" => UnitTier.Squad,
			"section" => UnitTier.Section,
			"platoon" => UnitTier.Platoon,
			"company" or "battery" or "flight" => UnitTier.I,
			"battalion" or "squadron" => UnitTier.II,
			"regiment" or "wing" or "ducenarii" => UnitTier.III,
			"brigade" or "classis" or "airbase" => UnitTier.X,
			"division" or "base" => UnitTier.XX,
			"corps" or "copiis" => UnitTier.XXX,
			"army" or "navy" or "air force" => UnitTier.XXXX,
			"armygroup" => UnitTier.XXXXX,
			"theater" => UnitTier.XXXXXX,
			"command" => UnitTier.Command,
			_ => UnitTier.Empty,
		};

		this.ID = ID;
		this.designation = designation;
		this.tierText = tierText;
		if (additionalInformation != null && additionalInformation != "") this.additionalInformation = additionalInformation;
		if (notes != null && notes != "") { if (Regex.IsMatch(notes, "^[Rr]eserve.*")) isReserve = true; } else { notes = ""; }

		if (unitTier == UnitTier.Empty) { //Ships
			if (Regex.IsMatch(tierText.ToUpper(), "^S(S|O)")) domain = Domain.submarine; else { domain = Domain.naval; }
			fillColor = "#0065bd";
			Enum.TryParse(tierText.ToUpper(), out unitType);
		} else {
			(unitType, fillColor) = ConvertIDToType(designation, tierText);
			m1 = SetModifier1(designation, notes);
			m2 = SetModifier2(designation);
		}

		sidc = CalculateSIDC();
	}

	public string CalculateSIDC() {
		return "3003" + (int)domain + (isReserve ? 1 : 0) + "0" + $"{(int)unitTier:D2}" + $"{(int)unitType:D6}" + (domain == Domain.land ? $"{(int)m1:D2}" + $"{(int)m2:D2}" : "");
	}

	public Modifier1 SetModifier1(string designation, string notes) {
		designation = designation.ToLower();
		if (Regex.IsMatch(designation, ".*(support command).*")) return Modifier1.Army;
		if (Regex.IsMatch(designation, "^(?=(.*?(hq|headquarters|hh|admiralty|command|praesidium|institute|department|medical support adm|c\\d).*))(?!.*secu)")) return Modifier1.HQ;
		if (designation.Contains("fighter")) return Modifier1.Force;
		if (designation.Contains("helicopter")) return Modifier1.Utility;
		if (designation.Contains("aviation")) return Modifier1.Aviation;
		if (Regex.IsMatch(designation, ".*(naval|anti-ship|coastal).*")) return Modifier1.Naval;

		if (designation.Contains("rocket")) return Modifier1.MLRS;
		if (designation.Contains("fdc")) return Modifier1.FDC;
		if (designation.Contains("lhs")) return Modifier1.LHS;
		if (designation.Contains("pls")) return Modifier1.PLS;

		if (designation.Contains("weapons")) return Modifier1.Weapons;
		if (designation.Contains("airbase")) return Modifier1.Aifield;
		if (designation.Contains("bridg")) return Modifier1.Bridging;
		if (Regex.IsMatch(designation, ".*(drilling).*")) return Modifier1.Drilling;
		if (designation.Contains("network")) return Modifier1.Network;
		if (designation.Contains("construction")) return Modifier1.Construction;

		if (Regex.IsMatch(designation, ".*med.*(evac|trans).*")) return Modifier1.MEDEVAC;
		if (Regex.IsMatch(designation, ".*[^c]sar.*")) return Modifier1.SAR;
		if (Regex.IsMatch(notes, ".*[rR]ole.1.*")) return Modifier1.MED1;
		if (Regex.IsMatch(notes, ".*[rR]ole.2.*")) return Modifier1.MED2;
		if (Regex.IsMatch(notes, ".*[rR]ole.3.*")) return Modifier1.MED3;
		if (Regex.IsMatch(notes, ".*[rR]ole.4.*")) return Modifier1.MED4;
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
		if (designation.Contains("direct")) return Modifier1.DirectCommunication;
		if (designation.Contains("retransmission")) return Modifier1.RetransmissionSite;
		if (designation.Contains("sensor")) return Modifier1.Sensor;
		if (designation.Contains("virtual")) return Modifier1.Digital;
		if (Regex.IsMatch(designation, ".*(electronic installation).*")) return Modifier1.SIGINT;
		if (designation.Contains("survey")) return Modifier1.Survey;
		if (Regex.IsMatch(designation, "^space$")) return Modifier1.TacticalSattelite;

		if (designation.Contains("dog")) return Modifier1.Dog;
		if (Regex.IsMatch(designation, ".*(radar|missile warn|space (comm|oper)).*")) return Modifier1.Radar;
		if (designation.Contains("radiofrequency")) return Modifier1.RFID;
		if (designation.Contains("route	safety")) return Modifier1.RouteReconnaissanceAndClearance;

		return Modifier1.Empty;
	}

	public Modifier2 SetModifier2(string designation) {
		designation = designation.ToLower();
		if (Regex.IsMatch(designation, ".*(airborne|parachut).*")) return Modifier2.Airborne;
		if (Regex.IsMatch(designation, ".*(wheeled|motorized|mechanized.*hh|medium marine|sph|self-propelled art).*")) return Modifier2.Wheeled;
		if (designation.Contains("towed")) return Modifier2.Towed;
		if (designation.Contains("tracked")) return Modifier2.Tracked;
		if (designation.Contains("assault")) return Modifier2.AirAssault;
		if (designation.Contains("amphibious")) return Modifier2.Amphibious;
		if (designation.Contains("arctic")) return Modifier2.Arctic;
		if (designation.Contains("bicycle")) return Modifier2.Bicycle;
		if (designation.Contains("rail")) return Modifier2.Railroad;
		if (designation.Contains("river")) return Modifier2.Riverine;

		if (designation.Contains("supply")) return Modifier2.Supply;
		if (Regex.IsMatch(designation, ".*[^service].*support.*^(?!.*\bhospital\b).*")) return Modifier2.Support;

		if (Regex.IsMatch(designation, ".*(helicopter|heavy.*defence).*")) return Modifier2.Heavy;
		if (designation.Contains("csar")) return Modifier2.CSAR;
		if (designation.Contains("cyber")) return Modifier2.Cyberspace;
		if (designation.Contains("decon")) return Modifier2.Decontamination;
		if (Regex.IsMatch(designation, "\\sdental\\s]")) return Modifier2.Dental;
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

	public static (UnitType, string) ConvertIDToType(string designation, string tierText) {
		designation = designation.ToLower();
		UnitType type = UnitType.Empty;
		string colour = "#5baa5b";

		//Echelon matching
		switch (tierText.ToLower()) {
			case "brigade":
				if (designation.Contains("school")) return (UnitType.AviationFixedWing, "#80e0ff");
				break;
			case "navy":
			case "copiis":
			case "naval":
			case "corps":
			case "ducenarii":
			case "classis":
				if (designation.Contains("patriae")) return (UnitType.CoastGuard, "#d87600");
				else if (Regex.IsMatch(designation, ".*(trahis|investig).*")) return (UnitType.Naval, "#ffffff");
				else return (UnitType.Naval, "#0065bd");
			case "base":
				if (Regex.IsMatch(designation, ".*(school|academy).*")) return (UnitType.Naval, "#0065bd");
				else return (UnitType.SeaportOfDebarkation, "#0065bd");
			case "airbase":
				return (UnitType.AirportOfDebarkation, "#80e0ff");
			case "army":
			case "division":
				if (Regex.IsMatch(designation, ".*(aerospace).*")) return (UnitType.AviationFixedWing, "#80e0ff");
				else if (Regex.IsMatch(designation, ".*(support command).*")) return (UnitType.Supply, "#d87600");
				else return (UnitType.Infantry, "#5baa5b");
			case "wing":
			case "squadron":
			case "flight":
			case "air force":
				colour = "#80e0ff";
				if (designation.Contains("helicopter")) type = UnitType.Aviation;
				else if (Regex.IsMatch(designation, ".*(fighter|school).*")) type = UnitType.AviationFixedWing;
				else if (designation.Contains("combined")) type = UnitType.AviationComposite;
				else if (designation.Contains("uav")) type = UnitType.UnmannedAerialVehicle;
				else if (designation.Contains("cyber")) return (UnitType.ElectronicWarfare, "#ffffff");
				else if (designation.Contains("space ")) break;
				else type = UnitType.AviationFixedWing;

				if (designation.Contains("recon")) {
					if (type == UnitType.AviationFixedWing) type = UnitType.AviationFixedWingReconnaissance;
					else if (type == UnitType.Aviation) type = UnitType.AviationReconnaissance;
				}
				return (type, colour);
			default:
				break;
		}

		//Name matching
		#region Maneuvre
		if (Regex.IsMatch(designation, ".*(?:guard.*|yeomenry.*|reserve(?!.*preserv).*|personnel$|airborne$)")) type = UnitType.Infantry;
		else if (designation.Contains("combined")) type = UnitType.CombinedArms;
		else if (designation.Contains("weapons")) {
			if (Regex.IsMatch(designation, ".*(?:mechanized|armoured).*")) type = UnitType.InfantryMechanized;
			else if (Regex.IsMatch(designation, ".*motorized.*")) type = UnitType.InfantryMotorized;
			else if (Regex.IsMatch(designation, ".*(?:marine|amphi).*")) type = UnitType.InfantryAmphibious;
			else type = UnitType.Infantry;
		} else if (Regex.IsMatch(designation, ".*(?:infantry|vehicle$).*")) {
			if (designation.Contains("motorized")) type = UnitType.InfantryMotorized;
			else if (Regex.IsMatch(designation, ".*(?:mechanized|vehicle).*")) type = UnitType.InfantryMechanized;
			else type = UnitType.Infantry;
		} else if (Regex.IsMatch(designation, ".*(?:marine$|amphibious).*")) {
			if (Regex.IsMatch(designation, ".*(?:mechanized|armoured|medium|heavy).*")) type = UnitType.ArmouredAmphibious;
			else type = UnitType.InfantryAmphibious;
		} else if (designation.Contains("sniper")) type = UnitType.Sniper;
		else if (designation.Contains("air assault")) type = UnitType.AirAssault;
		else if (designation.Contains("security")) {
			if (Regex.IsMatch(designation, ".*(?:air|aerial).*")) {
				return (UnitType.SecurityPoliceAir, "#80e0ff");
			} else if (designation.Contains("motorized")) {
				type = UnitType.SecurityMotorized;
			} else {
				type = UnitType.Security;
			}
		} else if (Regex.IsMatch(designation, ".*space$")) type = UnitType.Space;
		else if (designation.Contains("dog")) type = UnitType.Dog;
		else colour = "#ffd00b"; //Cavalry
								 //Colour matching
		if (Regex.IsMatch(designation, ".*(?:naval|guard|yeomenry).*")) colour = "#0065bd";
		else if (Regex.IsMatch(designation, ".*(?:aerial).*")) colour = "#80e0ff";
		if (type != UnitType.Empty) return (type, colour);
		#endregion
		#region Cavalry
		if (designation.Contains("cavalry")) type = UnitType.ReconnaissanceArmoured;
		else if (Regex.IsMatch(designation, ".*(?:hh|hq|headquarter|medical support adm).*")) {
			if (Regex.IsMatch(designation, ".*(?:armoured|mechanized).*")) type = UnitType.Armoured;
		} else if (Regex.IsMatch(designation, ".*reconnaissance$")) {
			if (designation.Contains("nbcr")) type = UnitType.Empty;
			else if (designation.Contains("armoured")) type = UnitType.ReconnaissanceArmoured;
			else if (designation.Contains("amph")) type = UnitType.ReconnaissanceAmphibious;
			else if (designation.Contains("motor")) type = UnitType.ReconnaissanceMotorized;
			else type = UnitType.Reconnaissance;
		} else if (Regex.IsMatch(designation, "^armoured$")) type = UnitType.Armoured;
		else if (Regex.IsMatch(designation, ".*fire.*liason.*")) type = UnitType.NavalFireLiason;
		else if (Regex.IsMatch(designation, "^surveil")) type = UnitType.ReconnaissanceSurveillance;
		else colour = "#ff3333"; //Arty
								 //Colour matching
		if (Regex.IsMatch(designation, ".*(?:naval).*")) colour = "#0065bd";
		else if (Regex.IsMatch(designation, ".*(?:aerial).*")) colour = "#80e0ff";
		if (type != UnitType.Empty) return (type, colour);
		#endregion
		#region Artillery
		if (designation.Contains("anti tank")) {
			if (designation.Contains("motorized")) type = UnitType.AntiTankMotorized;
			else if (designation.Contains("mechanized")) type = UnitType.AntiTankMechanized;
			else type = UnitType.AntiTank;
		} else if (designation.Contains("light air defence")) type = UnitType.AirDefense;
		else if (Regex.IsMatch(designation, ".*(?:air.defence|missile.defence).*")) type = UnitType.AirDefenseMissile;
		else if (Regex.IsMatch(designation, ".*(?:sph|self-propelled).*")) type = UnitType.ArtillerySPH;
		else if (Regex.IsMatch(designation, ".*(?:artillery|fires).*")) type = UnitType.Artillery;
		else if (designation.Contains("target acq")) type = UnitType.ArtilleryTargetAcquisition;
		else if (designation.Contains("forward obs")) type = UnitType.ArtilleryForwardObserver;
		else if (Regex.IsMatch(designation, ".*(?:anti.ship|coastal def).*")) type = UnitType.Missile;
		else if (designation.Contains("mortar")) {
			if (designation.Contains("motorized")) type = UnitType.MortarWheeled;
			else if (designation.Contains("mechanized")) type = UnitType.MortarArmoured;
			else if (Regex.IsMatch(designation, ".*(towed|light).*")) type = UnitType.MortarTowed;
			else type = UnitType.Mortar;
		} else colour = "#ffffff"; //CS
								   //Colour matching
		if (Regex.IsMatch(designation, ".*(?:naval).*")) colour = "#0065bd";
		else if (Regex.IsMatch(designation, ".*(?:aerial).*")) colour = "#80e0ff";
		if (type != UnitType.Empty) return (type, colour);

		#endregion
		#region CS
		if (Regex.IsMatch(designation, ".*(?:special service|special operation).*")) {
			if (designation.Contains("surface")) type = UnitType.SpecialOperationsForcesGround;
			else if (designation.Contains("naval")) type = UnitType.SpecialOperationsForcesNaval;
			else if (designation.Contains("diving")) type = UnitType.SpecialOperationsForcesSubmarine;
			else type = UnitType.SpecialOperationsForces;
			return (type, "#ffffff");
		} else if (designation.Contains("special forces")) type = UnitType.SpecialForces;
		else if (designation.Contains("underwater demolition")) type = UnitType.UnderwaterDemolitionTeam;
		else if (Regex.IsMatch(designation, ".*medical($|.*tr(e|an).*|.*evac.*)")) type = UnitType.Medical;
		else if (designation.Contains("hospital")) type = UnitType.MedicalTreatmentFacility;
		else if (designation.Contains("mine")) {
			if (designation.Contains("clear")) type = UnitType.MineClearing;
			else if (designation.Contains("throw")) type = UnitType.MineLaunching;
			else if (designation.Contains("lay")) type = UnitType.MineLaying;
			else type = UnitType.Mine;
		} else if (Regex.IsMatch(designation, ".*(?:engineer|technical|installation|integrat|construc|infrastr).*")) {
			if (designation.Contains("motorized")) type = UnitType.EngineerMotorized;
			else if (Regex.IsMatch(designation, ".*(?:mechanized|armoured).*")) type = UnitType.EngineerMechanized;
			else if (Regex.IsMatch(designation, ".*(?:survey|recon).*")) type = UnitType.EngineerSurvey;
			else type = UnitType.Engineer;
		}
		else if (designation.Contains("drilling")) type = UnitType.Drilling;
		else if (designation.Contains("pipe")) type = UnitType.Pipeline;
		else if (Regex.IsMatch(designation, ".*(?:combat support).*")) type = UnitType.Supply;

		else if (Regex.IsMatch(designation, ".*(?:explosive|eod).*")) type = UnitType.EOD;
		else if (designation.Contains("police")) type = UnitType.MilitaryPolice;
		else if (designation.Contains("geospat")) type = UnitType.Geospatial;
		else if (designation.Contains("meteo")) type = UnitType.Meteorological;
		else if (designation.Contains("topog")) type = UnitType.Topographic;
		else if (designation.Contains("survey")) type = UnitType.Survey;
		else if (Regex.IsMatch(designation, ".*(?:air.traffic).*")) type = UnitType.AirTrafficServices;

		else if (Regex.IsMatch(designation, ".*(?:nbcr|decontam).*")) {
			if (designation.Contains("motorized")) type = UnitType.NBCRMotorized;
			else if (Regex.IsMatch(designation, ".*(?:mechanized|armoured).*")) type = UnitType.NBCRMechanized;
			else type = UnitType.NBCR;

			if (Regex.IsMatch(designation, ".*(?:recon|detection).*")) {
				if (type == UnitType.NBCRMotorized) type = UnitType.NBCRReconnaissanceMotorized;
				else if (type == UnitType.NBCRMechanized) type = UnitType.NBCRReconnaissanceMechanized;
				else type = UnitType.NBCRReconnaissance;
			}

		} 
		else if (designation.Contains("analysis") && designation.Contains("electronic war")) type = UnitType.ElectronicWarfareAnalysis;
		else if (designation.Contains("analysis")) type = UnitType.Analysis;
		else if (Regex.IsMatch(designation, ".*counter.intel.*")) type = UnitType.CounterIntelligence;
		else if (Regex.IsMatch(designation, ".*(?:^space intelligence|sattelite|aerospace surve).*")) type = UnitType.SignalTacticalSatellite;
		else if (Regex.IsMatch(designation, ".*(?:intel|missile warn|operations s|radar post|a.*control|space comm|combat info).*")) type = UnitType.MilitaryIntelligence;
		else if (designation.Contains("direction finding")) type = UnitType.ElectronicWarfareDirectionFinding;
		else if (designation.Contains("electronic rang")) type = UnitType.ElectronicRanging;
		else if (designation.Contains("electronic interc")) type = UnitType.ElectronicWarfareIntercept;
		else if (designation.Contains("electronic jamm")) type = UnitType.ElectronicWarfareJamming;
		else if (designation.Contains("electronic surv")) type = UnitType.ElectronicWarfareSearch;
		else if (Regex.IsMatch(designation, ".*(?:(?:netw|electr|cyber).*war).*")) type = UnitType.ElectronicWarfare;
		else if (designation.Contains("sensor")) type = UnitType.ElectronicWarfare;

		else if (Regex.IsMatch(designation, ".*(?:signal|coord|networking).*")) type = UnitType.Signal;
		else if (Regex.IsMatch(designation, ".*(?:relay|retrainsmission).*")) type = UnitType.RadioRelay;
		else if (Regex.IsMatch(designation, ".*radio.*station.*")) type = UnitType.RadioTeletypeCentre;
		else if (designation.Contains("radio")) type = UnitType.Radio;
		
		else if (designation.Contains("information op")) type = UnitType.InformationOperations;
		else if (designation.Contains("psychological oper")) type = UnitType.BroadcastTransmitterAntenna;

		else if (designation.Contains("band")) type = UnitType.ArmyMusic;
		else if (designation.Contains("liaison")) type = UnitType.Liaison;
		else if (designation.Contains("command and control")) type = UnitType.CommandAndControl;
		else colour = "#d87600"; //CSS
								 //Colour matching
		if (Regex.IsMatch(designation, ".*(?:naval).*")) colour = "#0065bd";
		else if (Regex.IsMatch(designation, ".*(?:aerial).*")) colour = "#80e0ff";
		if (type != UnitType.Empty) return (type, colour);
		#endregion
		#region CSS
		if (designation.Contains("food suppl")) type = UnitType.Class1;
		else if (designation.Contains("quartermaster suppl")) type = UnitType.Class2;
		else if (designation.Contains("fuel suppl")) type = UnitType.Class3;
		else if (designation.Contains("engineer suppl")) type = UnitType.Class4;
		else if (designation.Contains("ammunition suppl")) type = UnitType.Class5;
		else if (designation.Contains("personnal suppl")) type = UnitType.Class6;
		else if (designation.Contains("heavy suppl")) type = UnitType.Class7;
		else if (designation.Contains("medical suppl")) type = UnitType.Class8;
		else if (designation.Contains("parts suppl")) type = UnitType.Class9;
		else if (designation.Contains("civilian suppl")) type = UnitType.Class10;

		else if (designation.Contains("water purif")) type = UnitType.WaterPurification;
		else if (designation.Contains("water")) type = UnitType.Water;
		else if (designation.Contains("quartermaster")) type = UnitType.Quartermaster;
		else if (designation.Contains("fuel")) type = UnitType.PetroleumOilLubricants;
		else if (designation.Contains("ammunition")) type = UnitType.Ammunition;
		else if (designation.Contains("personnel services")) type = UnitType.PersonnelServices;
		else if (designation.Contains("ordnance")) type = UnitType.Ordnance;
		else if (designation.Contains("service support")) type = UnitType.CombatServiceSupport;
		else if (Regex.IsMatch(designation, ".*(?:supply|distribution|support|logistic).*")) type = UnitType.Supply;

		else if (designation.Contains("transport")) type = UnitType.Transportation;
		else if (designation.Contains("railway")) type = UnitType.Railhead;
		else if (Regex.IsMatch(designation, ".*(?:maint|repair|preserv).*")) type = UnitType.Maintenance;

		else if (designation.Contains("public rel")) type = UnitType.PublicAffairs;
		else if (Regex.IsMatch(designation, ".*civil affair.*[^a]")) type = UnitType.CivilAffairs;
		else if (designation.Contains("postal")) type = UnitType.Postal;
		else if (designation.Contains("financ")) type = UnitType.Finance;
		else if (designation.Contains("labour")) type = UnitType.Labour;
		else if (designation.Contains("laundry")) type = UnitType.Laundry;
		else if (Regex.IsMatch(designation, "^prison$")) type = UnitType.Prison;
		else if (designation.Contains("mortuary")) type = UnitType.Mortuary;
		else if (Regex.IsMatch(designation, ".*(?:legal|jud).*")) type = UnitType.JudgeAdvocateGeneral;
		else if (designation.Contains("fire fighting")) type = UnitType.Firefighting;
		else if (designation.Contains("welfare")) type = UnitType.MoraleWelfareRecreation;
		else if (designation.Contains("administrat")) type = UnitType.Administrative;
		#endregion
		if (type != UnitType.Empty) return (type, colour);
		if (designation.Contains("admiralty")) return (UnitType.Naval, "#0065bd");
		return (type, colour);

		//=UNIQUE(QUERY(B2:D1240, "SELECT C"))
	}

	public static string AddOrdinalSuffix(string num) {
		int number = int.Parse(num);
		if (number >= 11 && number <= 13) {
			return number + "th";
		}

		return (number % 10) switch {
			1 => number + "st",
			2 => number + "nd",
			3 => number + "rd",
			_ => number + "th",
		};
	}
}