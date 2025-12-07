using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using Origins.Items.Materials;
using Origins.Items.Weapons.Magic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Dyes {
    public class Anti_Gray_Dye : Dye_Item, ICustomWikiStat {
		public static int ID { get; private set; }
		public static int ShaderID { get; private set; }
		public string[] Categories => [
			WikiCategories.SpecialEffectDye
		];
		public override void SetStaticDefaults() {
			ID = Type;
			GameShaders.Armor.BindShader(Type, new AntiGrayArmorShaderData());
			ItemID.Sets.NonColorfulDyeItems.Add(Type);
			ShaderID = GameShaders.Armor.GetShaderIdFromItemId(Type);
			Item.ResearchUnlockCount = 3;
		}
		public override bool AddToDyeTrader(Player player) => false;
		public override void AddRecipes() {
			CreateRecipe(2)
			.AddIngredient<Tangela_Bud>()
			.AddTile(TileID.DyeVat)
			.Register();
		}
	}
}
