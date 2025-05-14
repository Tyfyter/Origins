using Microsoft.Xna.Framework.Graphics;
using Origins.Tiles.Other;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other {
	[ReinitializeDuringResizeArrays]
	public abstract class ModLargeGem : ModItem {
		public static Asset<Texture2D>[] GemTextures { get; } = ItemID.Sets.Factory.CreateNamedSet($"{nameof(ModLargeGem)}_{nameof(GemTextures)}")
		.RegisterCustomSet<Asset<Texture2D>>(null,
			ItemID.LargeAmethyst,	TextureAssets.Gem[0] ?? Asset<Texture2D>.Empty,
			ItemID.LargeTopaz,		TextureAssets.Gem[1] ?? Asset<Texture2D>.Empty,
			ItemID.LargeSapphire,	TextureAssets.Gem[2] ?? Asset<Texture2D>.Empty,
			ItemID.LargeEmerald,	TextureAssets.Gem[3] ?? Asset<Texture2D>.Empty,
			ItemID.LargeRuby,		TextureAssets.Gem[4] ?? Asset<Texture2D>.Empty,
			ItemID.LargeDiamond,	TextureAssets.Gem[5] ?? Asset<Texture2D>.Empty,
			ItemID.LargeAmber,		TextureAssets.Gem[6] ?? Asset<Texture2D>.Empty
		);
		public static void AddCrossModLargeGem(ModItem item, string texture) {
			if (item is null) return;
			ModContent.RequestIfExists(texture, out GemTextures[item.Type]);
			if (GemTextures[item.Type] is null) GemTextures[item.Type] = Asset<Texture2D>.Empty;
		}
		public virtual string GemTexture => Texture + "_Gem";
		public override void AutoStaticDefaults() {
			base.AutoStaticDefaults();
			ModContent.RequestIfExists(GemTexture, out GemTextures[Type]);
			if (GemTextures[Type] is null) {
				GemTextures[Type] = Asset<Texture2D>.Empty;
				Mod.Logger.Warn($"Mod Large Gem {FullName} has no gem texture");
			}
		}
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.rare = ItemRarityID.Blue;
		}
		public override Color? GetAlpha(Color lightColor) {
			if (Main.item[Item.whoAmI] != Item) return null;
			return new Color(250, 250, 250, Main.mouseTextColor / 2) * (1f - Item.shimmerTime);
		}
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			scale = Main.mouseTextColor / 1000f + 0.8f;
			return true;
		}
	}
	public class ModLargeGemLayer : PlayerDrawLayer {
		public override Position GetDefaultPosition() => new Between(null, PlayerDrawLayers.CaptureTheGem);
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			if (drawInfo.shadow != 0f) return;

			bool flag = false;
			List<int> ownedLargeGems = drawInfo.drawPlayer.OriginPlayer().ownedLargeGems;
			float num = ownedLargeGems.Count;
			if (num <= 0) return;

			float num2 = 1f - num * 0.06f;
			float num3 = (num - 1f) * 4f;
			switch ((int)num) {
				case 2:
				num3 += 10f;
				break;
				case 3:
				num3 += 8f;
				break;
				case 4:
				num3 += 6f;
				break;
				case 5:
				num3 += 6f;
				break;
				case 6:
				num3 += 2f;
				break;
				case 7:
				num3 += 0f;
				break;
			}

			float num4 = drawInfo.drawPlayer.miscCounter / 300f * ((float)Math.PI * 2f);
			if (!(num > 0f))
				return;

			float num5 = (float)Math.PI * 2f / num;
			float num6 = 0f;
			Vector2 vector = new Vector2(1.3f, 0.65f);
			if (!flag)
				vector = Vector2.One;

			List<DrawData> list = new List<DrawData>();
			for (int j = 0; j < num; j++) {
				Vector2 vector2 = (num4 + num5 * (j - num6)).ToRotationVector2();
				float num7 = num2;
				if (flag)
					num7 = MathHelper.Lerp(num2 * 0.7f, 1f, vector2.Y / 2f + 0.5f);

				Texture2D value = ModLargeGem.GemTextures[ownedLargeGems[j]].Value;
				DrawData item = new DrawData(value, new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X + (float)(drawInfo.drawPlayer.width / 2)), (int)(drawInfo.Position.Y - Main.screenPosition.Y + (float)drawInfo.drawPlayer.height - 80f)) + vector2 * vector * num3, null, new Color(250, 250, 250, (int)Main.mouseTextColor / 2), 0f, value.Size() / 2f, ((float)(int)Main.mouseTextColor / 1000f + 0.8f) * num7, SpriteEffects.None);
				list.Add(item);
			}
			if (flag)
				list.Sort(DelegateMethods.CompareDrawSorterByYScale);

			drawInfo.DrawDataCache.AddRange(list);
		}
	}
	public class Large_Chambersite : ModLargeGem {
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Chambersite_Item>(15)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
