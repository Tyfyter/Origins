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
		static List<(Func<bool> day, HolidayHairPassData pass)> shaders;
		public override HairShaderData ShaderData => new HolidayHairShaderData(Mod.Assets.Request<Effect>("Effects/HolidayHairDye"), "Default");
		public static HolidayHairPassData CurrentPass {
			get {
				return shaders[^2].pass;
				for (int i = 0; i < shaders.Count; i++) {
					if (shaders[i].day()) return shaders[i].pass;
				}
				return shaders[^1].pass;
			}
		}
		public override void Load() {
			if (!ModLoader.TryGetMod("HolidayLib", out Mod HolidayLib)) {
				shaders = [(() => true, new())];
				return;
			}
			Func<bool> Day(string name) => (Func<bool>)HolidayLib.Call("GETACTIVELOOKUP", name);
			Func<object[], object> _addHoliday = (Func<object[], object>)HolidayLib.Call("GETFUNC", "ADDHOLIDAY");
			Func<bool> AddHoliday(params object[] args) {
				if (_addHoliday(args) is Exception e) throw e;
				return Day((string)args[0]);
			}
			///<summary>Adds a holiday with the specified information using either the overlay or textured pass</summary>
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
				(AddHoliday("Autism Awareness Day", new DateTime(2007, 4, 2)), new HolidayHairPassData(
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
				SimpleHoliday("World Stroke Day", new DateTime(1999, 10, 29), new Color(33, 33, 33), "WorldStrokeDay_Hair", false),
				//SimpleHoliday("World Art Day", new DateTime(2012, 4, 15), new Color(33, 33, 33), "ArtDay_Hair", false),
				SimpleHoliday("Cerebral Palsy Awareness Day", new DateTime(2012, 3, 25), null, "CerebralPalsyAwarenessDay_Hair", true),
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
				)),
				(() => true, new())
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
