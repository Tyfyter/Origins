using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Reflection;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Globalization;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Holiday_Hair_Dye : HairDye {
		public const int January = 1;
		public const int February = 2;
		public const int March = 3;
		public const int April = 4;
		public const int May = 5;
		public const int June = 6;
		public const int July = 7;
		public const int August = 8;
		public const int September = 9;
		public const int October = 10;
		public const int November = 11;
		public const int December = 12;
		static List<(Func<bool> day, HolidayHairPassData pass)> shaders;
		public override HairShaderData ShaderData => new HolidayHairShaderData(Mod.Assets.Request<Effect>("Effects/HolidayHairDye"), "Default");
		public static HolidayHairPassData CurrentPass {
			get {
				for (int i = 0; i < shaders.Count; i++) {
					if (shaders[i].day()) return shaders[i].pass;
				}
				return defaultPass;
			}
		}
		public static HolidayHairPassData defaultPass = new();
		public override void Load() {
			if (!ModLoader.TryGetMod("HolidayLib", out Mod HolidayLib)) {
				shaders = [];
				return;
			}
			Func<bool> Day(string name) => (Func<bool>)HolidayLib.Call("GETACTIVELOOKUP", name);
			Func<object[], object> _addHoliday = (Func<object[], object>)HolidayLib.Call("GETFUNC", "ADDHOLIDAY");
			Func<bool> AddHoliday(params object[] args) {
				if (_addHoliday(args) is Exception e) throw e;
				return Day((string)args[0]);
			}
			///<summary>Adds a holiday with the specified information using either the overlay or textured pass</summary>
			///<param name="name">the name to use for the holiday</param>
			///<param name="date">the date to use for the holiday, year is ignored</param>
			///<param name="hairColor">the color to use for the hair, can be provided as a <see cref="Color">, <see cref="Vector3">, or <see cref="Func<Player, Vector3>"></param>
			///<param name="texture">the name of the texture, starting in the HolidayHairs folder</param>
			///<param name="overlay">whether to use the overlay or textured pass, the latter only draws where the hair would normally have pixels, and darkens the overlay where the hair texture is darker</param>
			///<param name="textureColor">the color to use for the texture, can be provided as a <see cref="Color">, <see cref="Vector3">, or <see cref="Func<Player, Vector3>"></param>
			(Func<bool> day, HolidayHairPassData pass) SimpleHoliday(string name, DateTime date, HairColorWrapper hairColor, string texture, bool overlay = true, HairColorWrapper textureColor = default) {
				AddHoliday(name, date);
				return (Day(name), new HolidayHairPassData(
					  PassName: overlay ? "Overlay" : "Textured",
					  //ColorFunc: (hairColor, lightColor) => lightColor,
					  uColor: hairColor.defined ? hairColor : new((player) => player.hairColor.ToVector3()),
					  uSecondaryColor: textureColor.defined ? textureColor : new(Color.White),
					  UsesHairColor: false,
					  Image: Mod.Assets.Request<Texture2D>("Items/Other/Consumables/HolidayHairs/" + texture)
				));
			}
			shaders = [
				(AddHoliday("Autism Awareness Day", new DateTime(2007, April, 2)), new HolidayHairPassData(
					PassName: "AutismAwareness",
					ColorFunc: (hairColor, lightColor) => Color.Lerp(lightColor, Color.White, 0.1f)
				)),
				(AddHoliday("Lunar New Year", () => new ChineseLunisolarCalendar().GetDayOfYear(DateTime.Now) == 1 ? 1 : 0), new HolidayHairPassData(
					PassName: "Textured",
					ColorFunc: (hairColor, lightColor) => lightColor,
					uColor: Color.Red,
					Image: Mod.Assets.Request<Texture2D>("Items/Other/Consumables/HolidayHairs/LunarNewYear_Hair")
				)),
				(Day("Saint Patrick's Day"), new HolidayHairPassData(
					PassName: "Overlay",
					ColorFunc: (hairColor, lightColor) => lightColor,
					uColor: Color.DarkGreen,
					Image: Mod.Assets.Request<Texture2D>("Items/Other/Consumables/HolidayHairs/StPatricks_Hair")
				)),
				(Day("Summer Solstice"), new HolidayHairPassData(
					PassName: "SummerSolstace"
				)),
				(Day("Winter Solstice"), new HolidayHairPassData(
					PassName: "WinterSolstace",
					UsesHairColor: false,
					Image: Main.Assets.Request<Texture2D>("Images/Misc/noise")
				)),
				SimpleHoliday("World Stroke Day", new DateTime(1999, October, 29), new Color(33, 33, 33), "WorldStrokeDay_Hair", false),
				SimpleHoliday("World Art Day", new DateTime(2012, April, 15), default, "ArtDay_Hair", false),
				SimpleHoliday("Cerebral Palsy Awareness Day", new DateTime(2012, March, 25), null, "CerebralPalsyAwarenessDay_Hair", true),
				SimpleHoliday("New Year's Day", new DateTime(2012, January, 1), default, "NewYears_Hair", false),
				SimpleHoliday("Programmers' Day", new DateTime(2012, January, 26), new Color(33, 33, 33), "Programmers_Hair", false),
				SimpleHoliday("Environment Day", new DateTime(2012, June, 5), default, "EnvironmentDay_Hair", true),
				SimpleHoliday("Groundhog Day", new DateTime(2012, February, 2), default, "GroundHog_Hair", true),
				SimpleHoliday("World Cancer Day", new DateTime(2012, February, 4), default, "WorldCancer_Hair", true),
				SimpleHoliday("World Art Day", new DateTime(2012, February, 11), default, "SickDay_Hair", true),
				SimpleHoliday("Saint Valentine's Day", new DateTime(2012, February, 14), default, "Valentines_Hair", true),
				SimpleHoliday("Women's Day", new DateTime(2012, March, 8), default, "WomensDay_Hair", true),
				SimpleHoliday("International Day of Happiness", new DateTime(2012, March, 20), default, "HappinessDay_Hair", true),
				SimpleHoliday("World Creativity and Innovation Day", new DateTime(2012, April, 21), default, "CreativityInnovation_Hair", true),
				SimpleHoliday("International Press Freedom Day", new DateTime(2012, May, 3), default, "PressFreedom_Hair", true),
				SimpleHoliday("International Nurses Day", new DateTime(2012, May, 12), default, "NurseDay_Hair", true),
				SimpleHoliday("International Day of Light", new DateTime(2012, May, 16), default, "LightDay_Hair", true),
				SimpleHoliday("World Ocean Day", new DateTime(2012, June, 8), default, "WorldOceanDay_Hair", true),
				SimpleHoliday("Fathers' Day", new DateTime(2012, June, 16), default, "FathersDay_Hair", true),
				SimpleHoliday("International Asteroid Day", new DateTime(2012, June, 30), default, "InternationalAsteroidDay_Hair", true),
				//SimpleHoliday("International Cooperatives Day", new DateTime(2012, April, 15), default, "InternationalCooperativesDay_Hair", false),
				SimpleHoliday("World Population Day", new DateTime(2012, July, 11), default, "WorldPopulationDay_Hair", true),
				SimpleHoliday("International Moon Day", new DateTime(2012, July, 20), default, "InternationalMoonDay_Hair", true),
				SimpleHoliday("International Friendship Day", new DateTime(2012, July, 30), default, "InternationalFriendshipDay_Hair", true),
				//SimpleHoliday("World Art Day", new DateTime(2012, April, 15), default, "MentalHealthAwarenessDay_Hair", false),// could not find such a day
				SimpleHoliday("Overdose Awareness Day", new DateTime(2012, August, 31), default, "OverdoseAwarenessDay_Hair", true),
				//SimpleHoliday("World Art Day", new DateTime(2012, April, 15), default, "ThanksgivingDay_Hair", false),
				SimpleHoliday("International Charity Day", new DateTime(2012, September, 5), default, "InternationalChairtyDay_Hair", true),
				SimpleHoliday("International Peace Day", new DateTime(2012, September, 21), default, "InternationalPeaceDay_Hair", true),
				SimpleHoliday("World Art Day", new DateTime(2012, November, 12), default, "WorldPneumoniaDay_Hair", true),
				SimpleHoliday("World Art Day", new DateTime(2012, November, 14), default, "WorldDiabetesDay_Hair", true),
				SimpleHoliday("World Art Day", new DateTime(2012, November, 21), default, "WorldPhilosphyDay_Hair", true),
				SimpleHoliday("World Art Day", new DateTime(2012, December, 10), default, "HumanRightsDay_Hair", true),
				SimpleHoliday("World Art Day", new DateTime(2012, December, 20), default, "WorldSoilDay_Hair", true),
				(() => Main.halloween, new HolidayHairPassData(
					PassName: "Overlay",
					uColor: new Color(33, 33, 33),
					UsesHairColor: false,
					  Image: Mod.Assets.Request<Texture2D>("Items/Other/Consumables/HolidayHairs/Halloween_Hair")
				)),
				(() => Main.xMas, new HolidayHairPassData(
					PassName: "Overlay",
					UsesHairColor: false,
					Image: Mod.Assets.Request<Texture2D>("Items/Other/Consumables/HolidayHairs/ChristmasDay_Hair")
				))
			];
		}
		public override void Unload() => shaders = null;
		public override void SetDefaults() {
			base.SetDefaults();
			Item.value = Item.sellPrice(gold: 6);
		}
	}
	public abstract class HairDye : ModItem {
		public abstract HairShaderData ShaderData { get; }
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GameShaders.Hair.BindShader(
					Item.type,
					ShaderData
				);
			}
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 26;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item3;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useTurn = true;
			Item.useAnimation = 17;
			Item.useTime = 17;
			Item.consumable = true;
		}
	}
	public record HolidayHairPassData(
		string PassName = "Default",
		bool UsesLighting = true,
		bool UsesHairColor = true,
		Asset<Texture2D> Image = null,
		Func<Color, Color, Color> ColorFunc = null,
		bool ShaderDisabled = false,
		HairColorWrapper uColor = default,
		HairColorWrapper uSecondaryColor = default
	);
	public readonly struct HairColorWrapper {
		readonly Vector3 color;
		readonly Func<Player, Vector3> colorFunc;
		public readonly bool isFunc;
		public readonly bool defined;
		public readonly Vector3 GetColor(Player player) => isFunc ? colorFunc(player) : color;
		public HairColorWrapper(Color color) {
			this.color = color.ToVector3();
			this.colorFunc = default;
			isFunc = false;
			defined = true;
		}
		public HairColorWrapper(Vector3 color) {
			this.color = color;
			this.colorFunc = default;
			isFunc = false;
			defined = true;
		}
		public HairColorWrapper(Func<Player, Vector3> colorFunc) {
			this.color = default;
			this.colorFunc = colorFunc;
			isFunc = true;
			defined = true;
		}
		public static implicit operator HairColorWrapper(Color color) => new(color);
		public static implicit operator HairColorWrapper(Vector3 color) => new(color);
		public static implicit operator HairColorWrapper(Func<Player, Vector3> color) => color is null ? default : new(color);
	}
	public class HolidayHairShaderData(Asset<Effect> shader, string passName) : HairShaderData(shader, passName) {
		readonly Func<object[], object> checkVersion = ModLoader.TryGetMod("HolidayLib", out Mod HolidayLib) ? (Func<object[], object>)HolidayLib.Call("GETFUNC", "FORCEDHOLIDAYVERSION") : (_) => -1;
		int lastVersion = -1;

		public override Color GetColor(Player player, Color lightColor) {
			Vector4 color = Vector4.One;
			currentPass ??= new();
			if (currentPass.ColorFunc is not null) return currentPass.ColorFunc(player.hairColor, lightColor);
			if (currentPass.UsesHairColor) {
				color *= player.hairColor.ToVector4();
			}
			if (currentPass.UsesLighting) {
				color *= lightColor.ToVector4();
			}
			return new Color(color);
		}

		HolidayHairPassData currentPass = new();
		public override void Apply(Player player, DrawData? drawData = null) {
			int currentVersion = (int)checkVersion(null);
			if (currentVersion != lastVersion || currentPass is null) {
				lastVersion = currentVersion;
				HolidayHairPassData newPass = Holiday_Hair_Dye.CurrentPass;
				if (currentPass.PassName != newPass.PassName) {
					currentPass = newPass;
					ShaderDataMethods._passName.SetValue(this, newPass.PassName);
					_uImage = currentPass.Image;
					_shaderDisabled = currentPass.ShaderDisabled;
					if (currentPass.uColor.defined && !currentPass.uColor.isFunc) _uColor = currentPass.uColor.GetColor(player);
					if (currentPass.uSecondaryColor.defined && !currentPass.uSecondaryColor.isFunc) _uSecondaryColor = currentPass.uSecondaryColor.GetColor(player);
				}
			}
			if (_shaderDisabled) return;
			if (currentPass.uColor.defined) {
				if (currentPass.uColor.isFunc) _uColor = currentPass.uColor.GetColor(player);
			} else {
				_uColor = player.hairColor.ToVector3();
			}
			if (currentPass.uSecondaryColor.defined) {
				if (currentPass.uSecondaryColor.isFunc) _uSecondaryColor = currentPass.uSecondaryColor.GetColor(player);
			} else {
				_uSecondaryColor = Vector3.One;
			}
			if (drawData.HasValue) UseTargetPosition(Main.screenPosition + drawData.Value.position);
			//Shader.Parameters["zoom"].SetValue(Main.GameViewMatrix.TransformationMatrix);
			base.Apply(player, drawData);
		}
	}
}
