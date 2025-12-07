using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace Origins.Items.Other.Dyes {
    public class Acrid_Dye : Dye_Item, ICustomWikiStat {
		public static int ID { get; private set; }
		public static int ShaderID { get; private set; }
		public string[] Categories => [
			"SpecialEffectDye"
		];
		public override void SetStaticDefaults() {
			ID = Type;
			GameShaders.Armor.BindShader(Type, new ArmorShaderData(
				Mod.Assets.Request<Effect>("Effects/Solvent"),
				"Dissolve"
			))
			.UseImage(Origins.cellNoiseTexture.asset);
			ItemID.Sets.NonColorfulDyeItems.Add(Type);
			ShaderID = GameShaders.Armor.GetShaderIdFromItemId(Type);
			Item.ResearchUnlockCount = 3;
		}
	}
}
