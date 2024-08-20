using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Terraria;
using Terraria.Graphics.Shaders;

namespace Origins.Items.Other.Dyes {
    public class Amber_Dye : Dye_Item, ICustomWikiStat {
		public static int ID { get; private set; }
		public static int ShaderID { get; private set; }
        public string[] Categories => [
            "SpecialEffectDye"
        ];
        public override void SetStaticDefaults() {
			ID = Type;
			GameShaders.Armor.BindShader(Type, new ArmorShaderData(Main.Assets.Request<Effect>("PixelShader"), "ArmorStardust"))
			.UseImage("Images/Misc/noise")
			.UseColor(1.5f, 0.8f, 0.4f)
			.UseSecondaryColor(2.0f, 1.2f, 0.4f)
			.UseSaturation(1f);
			ShaderID = GameShaders.Armor.GetShaderIdFromItemId(Type);
			Item.ResearchUnlockCount = 3;
		}
	}
}
