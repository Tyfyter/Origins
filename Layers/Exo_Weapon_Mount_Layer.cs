using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Armor.Fiberglass;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Exo_Weapon_Mount_Layer : PlayerDrawLayer {
		AutoLoadingAsset<Texture2D> texture = typeof(Exo_Weapon_Mount).GetDefaultTMLName("_Back_Real");
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.back == Exo_Weapon_Mount.BackID;
		}
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.BackAcc);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Texture2D fakeTexture = TextureAssets.AccBack[Exo_Weapon_Mount.BackID].Value;
			Player player = drawInfo.drawPlayer;
			int buffTime = player.buffTime.GetIfInRange(player.FindBuffIndex(ModContent.BuffType<Exo_Weapon_Mount_Buff>()));
			int frame = (Exo_Weapon_Mount.BuffTime - buffTime) / 2;
			if (frame >= 7) frame = 0;
			for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--) {
				DrawData data = drawInfo.DrawDataCache[i];
				if (data.texture != fakeTexture) continue;
				data.texture = texture;
				data.sourceRect = texture.Frame(verticalFrames: 7, frameY: frame);
				data.effect ^= SpriteEffects.FlipHorizontally;
				data.origin = data.effect.ApplyToOrigin(new(-4, 9), data.sourceRect.Value);
				drawInfo.DrawDataCache[i] = data;
				break;
			}
		}
	}
}
