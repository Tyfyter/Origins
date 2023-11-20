using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Reflection;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Holiday_Hair_Dye : HairDye {
		public override string Texture => "Terraria/Images/Item_" + ItemID.TwilightHairDye;
		static List<(Func<bool> day, HolidayHairPassData pass)> shaders;
		public override HairShaderData ShaderData => new HolidayHairShaderData(new Ref<Effect>(Mod.Assets.Request<Effect>("Effects/HolidayHairDye", AssetRequestMode.ImmediateLoad).Value), "Default");
		public static HolidayHairPassData CurrentPass {
			get {
				for (int i = 0; i < shaders.Count; i++) {
					if (shaders[i].day()) return shaders[i].pass;
				}
				return shaders[^1].pass;
			}
		}
		public override void Load() {
			Mod HolidayLib = ModLoader.GetMod("HolidayLib");
			Func<bool> Day(string name) => (Func<bool>)HolidayLib.Call("GETACTIVELOOKUP", name);
			Func<object[], object> _addHoliday = (Func<object[], object>)HolidayLib.Call("GETFUNC", "ADDHOLIDAY");
			void AddHoliday(params object[] args) {
				if (_addHoliday(args) is Exception e) throw e;
			}
			const string lunarNewYear = "Lunar New Year";
			const string autismAwareness = "Autism Awareness Day";
			AddHoliday(lunarNewYear, (Func<int>)(() => {
				return new ChineseLunisolarCalendar().GetDayOfYear(DateTime.Now) == 1 ? 1 : 0;
			}));
			AddHoliday(autismAwareness, new DateTime(2007, 4, 2));
			shaders = new() {
				(Day(autismAwareness), new HolidayHairPassData(
					  PassName: "AutismAwareness",
					  ColorFunc: (hairColor, lightColor) => Color.Lerp(lightColor, Color.White, 0.1f)
				)),
				(Day(lunarNewYear), new HolidayHairPassData(
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
				(() => true, new())
			};
		}
		public override void Unload() => shaders = null;
		public override void SetDefaults() {
			base.SetDefaults();
			Item.value = Item.sellPrice(gold: 6);
		}
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("HolidayLib");
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
	public struct HairColorWrapper {
		readonly Vector3 color;
		readonly Func<Player, Vector3> colorFunc;
		public readonly bool isFunc;
		public readonly bool defined;
		public Vector3 GetColor(Player player) => isFunc ? colorFunc(player) : color;
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
		public static implicit operator HairColorWrapper(Func<Player, Vector3> color) => new(color);
	}
	public class HolidayHairShaderData : HairShaderData {
		readonly Func<object[], object> checkVersion;
		int lastVersion = -1;
		public HolidayHairShaderData(Ref<Effect> shader, string passName) : base(shader, passName) {
			checkVersion = (Func<object[], object>)ModLoader.GetMod("HolidayLib").Call("GETFUNC", "FORCEDHOLIDAYVERSION");
		}
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
					if (currentPass.uSecondaryColor.defined && !currentPass.uSecondaryColor.isFunc) _uColor = currentPass.uSecondaryColor.GetColor(player);
				}
			}
			if (_shaderDisabled) return;
			if (currentPass.uColor.defined && currentPass.uColor.isFunc) _uColor = currentPass.uColor.GetColor(player);
			if (currentPass.uSecondaryColor.defined && currentPass.uSecondaryColor.isFunc) _uColor = currentPass.uSecondaryColor.GetColor(player);
			if (drawData.HasValue) {
				UseTargetPosition(Main.screenPosition + drawData.Value.position);
			}
			//Shader.Parameters["zoom"].SetValue(Main.GameViewMatrix.TransformationMatrix);
			base.Apply(player, drawData);
		}
	}
}
