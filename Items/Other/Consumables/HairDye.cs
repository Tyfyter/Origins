using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
		(HairShaderData shader, Func<bool> day)[] shaders;
		public override HairShaderData ShaderData {
			get {
				for (int i = 0; i < shaders.Length; i++) {
					if (shaders[i].day()) return shaders[i].shader;
				}
				return shaders[^1].shader;
			}
		}
		public override void Load() {
			Ref<Effect> effect = new Ref<Effect>(Mod.Assets.Request<Effect>("Effects/HolidayHairDye", AssetRequestMode.ImmediateLoad).Value);
			Mod HolidayLib = ModLoader.GetMod("HolidayLib");
			Func<bool> GetHolidayCheck(string day) => (Func<bool>)HolidayLib.Call("GETACTIVELOOKUP", day);
			shaders = new (HairShaderData, Func<bool>)[] {
				(new HairShaderData(effect, "SummerSolstace").UseImage("Images/Misc/noise"), GetHolidayCheck("Summer Solstice")),
				(new WinterSolstaceHairShaderData(effect, "WinterSolstace").UseImage("Images/Misc/noise"), GetHolidayCheck("Winter Solstice")),
				(new HairShaderData(effect, "Default"), () => true)
			};
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.Green;
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
			Item.value = Item.buyPrice(gold: 5);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item3;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useTurn = true;
			Item.useAnimation = 17;
			Item.useTime = 17;
			Item.consumable = true;
		}
	}
	public class WinterSolstaceHairShaderData : HairShaderData {
		public WinterSolstaceHairShaderData(Ref<Effect> shader, string passName) : base(shader, passName) { }
		public override Color GetColor(Player player, Color lightColor) => lightColor;
		public override void Apply(Player player, DrawData? drawData = null) {
			if (drawData.HasValue) {
				UseTargetPosition(Main.screenPosition + drawData.Value.position);
			}
			//Shader.Parameters["zoom"].SetValue(Main.GameViewMatrix.TransformationMatrix);
			base.Apply(player, drawData);
		}
	}
}
