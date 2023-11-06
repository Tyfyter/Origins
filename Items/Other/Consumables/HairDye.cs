using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Reflection;
using ReLogic.Content;
using System;
using System.Collections.Generic;
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
			shaders = new() {
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
			Item.ResearchUnlockCount = 3;
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
	public record HolidayHairPassData(string PassName = "Default", bool UsesLighting = true, bool UsesHairColor = true, Asset<Texture2D> Image = null);
	public class HolidayHairShaderData : HairShaderData {
		readonly Func<object[], object> checkVersion;
		int lastVersion = -1;
		public HolidayHairShaderData(Ref<Effect> shader, string passName) : base(shader, passName) {
			checkVersion = (Func<object[], object>)ModLoader.GetMod("HolidayLib").Call("GETFUNC", "FORCEDHOLIDAYVERSION");
		}
		public override Color GetColor(Player player, Color lightColor) {
			Vector4 color = Vector4.One;
			currentPass ??= new();
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
			if (drawData.HasValue) {
				UseTargetPosition(Main.screenPosition + drawData.Value.position);
			}
			int currentVersion = (int)checkVersion(null);
			if (currentVersion != lastVersion || currentPass is null) {
				lastVersion = currentVersion;
				HolidayHairPassData newPass = Holiday_Hair_Dye.CurrentPass;
				if (currentPass.PassName != newPass.PassName) {
					currentPass = newPass;
					ShaderDataMethods._passName.SetValue(this, newPass.PassName);
					_uImage = currentPass.Image;
				}
			}
			//Shader.Parameters["zoom"].SetValue(Main.GameViewMatrix.TransformationMatrix);
			base.Apply(player, drawData);
		}
	}
}
