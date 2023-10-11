public enum UnitTier {
	Empty = 0,
	Team = 11,
	Squad = 12,
	Section = 13,
	Platoon = 14,
	I = 15,
	II = 16,
	III = 17,
	X = 18,
	XX = 21,
	XXX = 22,
	XXXX = 23,
	XXXXX = 24,
	XXXXXX = 25,
	Command = 26
}
public enum UnitType {
	Empty = 000000,
	#region Maneuvre
	Combat = 120900,
	CombinedArms = 121000,
	Infantry = 121100,
	InfantryAmphibious = 121101,
	InfantryMechanized = 121102,
	InfantryMGS = 121103,
	InfantryMotorized = 121104,
	InfantryIFV = 121105,
	Parachute = 165500,
	Sniper = 121500,
	AirAssault = 120100,
	Security = 141700,
	SecurityArmoured = 141701,
	SecurityMotorized = 141702,
	Naval = 170100,
	CoastGuard = 201100,
	Diving = 140400,
	Space = 111300,
	Dog = 140500,
	Firefighting = 141000,
	#endregion
	#region Cavalry
	Amphibious = 120300,
	Armoured = 120500,
	ReconnaissanceArmoured = 120501,
	ArmouredAmphibious = 120502,
	Observer = 121200,
	Reconnaissance = 121300,
	ReconnaissanceSurveillance = 121301,
	ReconnaissanceAmphibious = 121302,
	ReconnaissanceMotorized = 121303,
	Surveillance = 121600,
	NavalFireLiason = 130200,
	#endregion
	#region Artillery
	AntiTank = 120400,
	AntiTankMechanized = 120401,
	AntiTankMotorized = 120402,
	AirDefense = 130100,
	AirDefenseMissile = 130102,
	Artillery = 130300,
	ArtillerySPH = 130301,
	ArtilleryTargetAcquisition = 130302,
	ArtilleryForwardObserver = 130400,
	Missile = 130700,
	Mortar = 130800,
	MortarArmoured = 130801,
	MortarWheeled = 130802,
	MortarTowed = 130803,
	#endregion
	#region Aerial
	Aviation = 120600,
	AviationReconnaissance = 120601,
	AviationComposite = 120700,
	AviationFixedWing = 120800,
	AviationFixedWingReconnaissance = 120801,
	UnmannedAerialVehicle = 121900,
	SecurityPoliceAir = 141900,
	#endregion
	#region CS
	SpecialForces = 121700,
	SpecialOperationsForces = 121800,
	SpecialOperationsForcesGround = 121802,
	SpecialOperationsForcesNaval = 121803,
	SpecialOperationsForcesSubmarine = 121804,
	UnderwaterDemolitionTeam = 121805,
	SpecialTroops = 111400,

	Medical = 161300,
	MedicalTreatmentFacility = 161400,

	Drilling = 140600,
	Engineer = 140700,
	EngineerMechanized = 140701,
	EngineerMotorized = 140702,
	EngineerSurvey = 140703,
	EOD = 140800,
	EngineerBaseConstruction = 140900,
	Pipeline = 162600,
	Geospatial = 141100,
	Meteorological = 130600,
	Topographic = 142100,
	MilitaryPolice = 141200,
	Mine = 141300,
	MineClearing = 141400,
	MineLaunching = 141500,
	MineLaying = 141600,

	Analysis = 150100,
	CounterIntelligence = 150200,
	DirectionFinding = 150300,
	ElectronicRanging = 150400,
	ElectronicWarfare = 150500,
	ElectronicWarfareAnalysis = 150501,
	ElectronicWarfareDirectionFinding = 150502,
	ElectronicWarfareIntercept = 150503,
	ElectronicWarfareJamming = 150504,
	ElectronicWarfareSearch = 150505,
	Interrogation = 150700,
	Jamming = 150800,
	JointIntelligenceCentre = 150900,
	MilitaryIntelligence = 151000,
	Search = 151100,
	Sensor = 151200,
	CommandAndControl = 110000,
	BroadcastTransmitterAntenna = 110601,
	InformationOperations = 110400,
	Liaison = 110500,
	MilitaryInformationSupportOperations = 110600,
	Radio = 110700,
	RadioRelay = 110800,
	RadioTeletypeCentre = 110900,
	Signal = 111000,
	SignalRadio = 111001,
	SignalRadioRelay = 111002,
	SignalRadioTeletypeCentre = 111003,
	SignalTacticalSatellite = 111004,
	SignalVideoImagery = 111005,

	AirTrafficServices = 120200,
	Survey = 130900,

	Band = 160500,
	ArmyMusic = 160501,
	#endregion
	#region CSS
	Class1 = 163700,
	Class2 = 163800,
	Class3 = 162000,
	Class4 = 164000,
	Class5 = 162200,
	Class6 = 164200,
	Class7 = 164300,
	Class8 = 164400,
	Class9 = 164500,
	Class10 = 164600,

	NBCR = 140100,
	NBCRMechanized = 140101,
	NBCRMotorized = 140102,
	NBCRReconnaissance = 140103,
	NBCRReconnaissanceMechanized = 140104,
	NBCRReconnaissanceMotorized = 140105,

	Ordnance = 162300,
	PersonnelServices = 162400,
	PetroleumOilLubricants = 162500,
	Postal = 162700,
	PublicAffairs = 162800,
	Quartermaster = 162900,
	Railhead = 163000,
	ReligiousSupport = 163100,
	SeaportOfDebarkation = 163300,
	Supply = 163400,
	Transportation = 163600,
	Water = 164700,
	WaterPurification = 164800,
	Support = 165200,
	MilitaryHistory = 151300,
	Sustainment = 160000,
	Administrative = 160100,
	AirportOfDebarkation = 160300,
	Ammunition = 160400,
	CombatServiceSupport = 160600,
	Finance = 160700,
	JudgeAdvocateGeneral = 160800,
	Labour = 160900,
	Laundry = 161000,
	Maintenance = 161100,
	Materiel = 161200,
	MoraleWelfareRecreation = 161500,
	Mortuary = 161600,
	DepartmentOFJustice = 200500,
	Prison = 200800,
	SearchAndRescue = 141800,

	CivilAffairs = 110200,
	CivilMilitaryCooperation = 110300,
	#endregion
	#region Naval
	CV = 1201000000,
	BBG = 1202010000,
	CCG = 1202020000,
	DDG = 1202030200,
	FFG = 1202041500,
	FF = 1202040000,
	K = 1202050400,
	KT = 1202051700,
	LL = 1202060000,
	LCC = 1203010000,
	LHA = 1203030000,
	LHD = 1203040000,
	LTH = 1203050000,
	LPD = 1203060000,
	LST = 1203070000,
	LCAC = 1203080011,
	ML = 1204010000,
	MS = 1204020000,
	MSD = 1204030014,
	MH = 1204040000,
	MCMV = 1204050000,
	PC = 1205010000,
	PG = 1205020000,
	USV = 1207000014,
	SIGINT = 1211000600,
	AE = 1301010006,
	AF = 1301020006,
	AGO = 1301050000,
	AGS = 1301060000,
	AH = 1301070000,
	AK = 1301080000,
	AKLOG = 1301080006,
	AOE = 1301090009,
	AO = 1301100006,
	AOH = 1301100002,
	AR = 1301110006,
	ARV = 1301110008,
	AS = 1301120000,
	ASH = 1301130002,
	YS = 1302020000,
	YT = 1302030000,
	YTH = 1302030002,
	YTM = 1302030004,
	YYD = 1302000005,
	#endregion
	#region Submarine
	SS = 1101000002,
	SSK = 1101000802,
	SSI = 1101000801,
	SSGI = 1101001001,
	SSN = 1101000806,
	SSBN = 1101000906,
	SSGN = 1101001006,
	SOFS = 1101001202,
	SOFSI = 1101001201,
	SOFSN = 1101001206,
	#endregion
}

public enum Modifier1 {
	Empty = 0, TacticalSattelite, Area, Attack, Biological, Border, Bridging, Chemical, Protection, Combat, C2, Communications, Construction, CrossCulturalCommunication, CrowdControl, Decon, Detention, DirectCommunication, Diving, Division, Dog, Drilling, ElectroOptical, Enganced, EOD, FDC, Force, FWD, GroundStation, LandingSupport, LargeExtensionNode, Maint, Meteo, MineCM, Missile, AdvisorSupport, MobileEquipment, MobilitySupport, MovementControlCentre, Multinational, MultinationalUnit, MLRS, MED1, MED2, MED3, MED4, Naval, UAV, Nuclear, Ops, Radar, RFID, Radiological, SAR, SEC, Sensor, Weapons, SIGINT, Armoured, RocketLauncher, Smoke, Sniper, SoundRanging, SOF,
	SWAT, Survey, TacticalExploitation, TargetAcquisition, Topographic, Utility, VideoImagery, MobilityAssault, AmphibiousShip, LHS, PLS, MEDEVAC, Ranger, Support, Aviation, RouteReconnaissanceAndClearance, TiltRotor, CommandPostNode, JointNetworkNode, RetransmissionSite, Assault, W, CriminalInvestigationDivision, Digital, Network, Aifield, Pipeline, Postal, Water, IndependentCommand, Theatre, Army, Corps, Brigade, HQ
}
public enum Modifier2 {
	Empty = 0, Airborne, Arctic, BattleDamageRepair, Bicycle, CasualtyStaging, Clearing, CloseRange, Control, Decontamination, Demolition, Dental, Digital, EnhancedPositionLocationReportingSystem, Equipment, Heavy, HighAltitude, Intermodal, IntensiveCare, Light, Laboratory, Launcher, LongRange, LowAltitude, Medium, MediumAltitude, MediumRange, Mountain, HighToMediumAltitude, MultiChannel, Optical, PackAnimal, Medevac, PreventiveMaint, Psychological, RadioRelay, Railroad, RecoveryUnmannedSystems, RecoveryMaintenance, RescueCoordinationCentre, Riverine, SingleChannel, Ski, ShortRange, Strategic, Support, Tactical, Towed, Troop, VSTOL, Veterinary, Wheeled, HighToLowAltitude, MediumToLowAltitude, Attack, Refuel, Utility, CSAR, Guerilla, AirAssault, Amphibious, VeryHeavy, Supply, Cyberspace, Tug, Barge, Launch, LC, LST, ServiceCraft, TugHarbour, TugOceangoing, SurfaceDeploymentDistributionCmd, Vessel, Composite, Shelter, LightMedium, Tracked
}

public class EnumUtils {
	public static string ParseTier(UnitTier tier) {
		switch (tier) {
			case UnitTier.Empty:
				return "";
			case UnitTier.Team:
				return "∅";
			case UnitTier.Squad:
				return "●";
			case UnitTier.Section:
				return "●●";
			case UnitTier.Platoon:
				return "●●●";
			case UnitTier.I:
				return "I";
			case UnitTier.II:
				return "II";
			case UnitTier.III:
				return "III";
			case UnitTier.X:
				return "X";
			case UnitTier.XX:
				return "XX";
			case UnitTier.XXX:
				return "XXX";
			case UnitTier.XXXX:
				return "XXXX";
			case UnitTier.XXXXX:
				return "XXXXX";
			case UnitTier.XXXXXX:
				return "XXXXXX";
			case UnitTier.Command:
				return "++";
			default:
				return "";
		}
	}
}
