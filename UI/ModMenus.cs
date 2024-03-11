using Microsoft.Xna.Framework.Graphics;
using Origins.Backgrounds;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.UI {
	public class Riven_Hive_Mod_Menu : ModMenu {
		public override Asset<Texture2D> Logo => TextureAssets.Logo2;
		public override int Music => Origins.Music.Riven;
		public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<Riven_Surface_Background>();
		public override string DisplayName => "Riven Hive";//Language.GetOrRegister(Mod.GetLocalizationKey("ModMenu.Riven_Hive")).Value;
	}
}
