using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameInput;
using Origins.Dev;
using System.IO;
using Terraria.IO;
using PegasusLib.Graphics;
using ReLogic.Content;

namespace Origins.Items.Other.Testing; 
public class Procedural_Sprite_Generator : TestingItem {
	public override string Texture => "Terraria/Images/Item_" + ItemID.Paintbrush;
	public static readonly BlendState Erase = new() {
		ColorSourceBlend = Blend.Zero,
		AlphaSourceBlend = Blend.Zero,
		ColorDestinationBlend = Blend.InverseSourceAlpha,
		AlphaDestinationBlend = Blend.InverseSourceAlpha
	};
	[CloneByReference] Texture2D newSprite;
	public override void SetDefaults() {
		Item.width = 16;
		Item.height = 26;
		Item.value = 25000;
		Item.rare = ItemRarityID.Green;
		Item.useStyle = ItemUseStyleID.HoldUp;
		Item.useAnimation = 10;
		Item.useTime = 10;
	}
	public override bool AltFunctionUse(Player player) => true;
	public override bool? UseItem(Player player) {
		if (Main.myPlayer == player.whoAmI) {
			if (player.altFunctionUse == 2) {
				try {
					using FileStream stream = new(Path.Join(Path.Combine(Program.SavePathShared, "ModSources", "Origins", ""), "Procedural_Texture.png"), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
					newSprite.SaveAsPng(stream, newSprite.Width, newSprite.Height);
				} catch (Exception e) {
					Main.NewText($"Failed to save texture, {e}");
				}
			} else {
				newSprite?.Dispose();
				newSprite = Generate(DrawHoles, 512, 512);
			}
		}
		return true;
	}
	public static Texture2D Generate(Action<SpriteBatch, int, int> draw, int width, int height) => SpriteGenerator.Generate(sb => draw(sb, width, height), (width, height));
	public static void DrawHoles(SpriteBatch spriteBatch, int width, int height) {
		int yFrames = 1;
		int xFrames = 1;
		Texture2D hole = ModContent.Request<Texture2D>("Origins/Textures/Procedural/Hole", AssetRequestMode.ImmediateLoad).Value;
		spriteBatch.GraphicsDevice.Clear(Color.Black);
		spriteBatch.Restart(spriteBatch.GetState(), blendState: Erase);
		List<Vector2> holes = Main.rand.PoissonDiskSampling(new(2, 2, (width / xFrames) / 2 - (hole.Width + 2), (height / yFrames) / 2 - (hole.Height + 2)), 15);
		for (int i = 0; i < holes.Count; i++) {
			Point pos = holes[i].ToPoint();
			for (int j = 0; j < xFrames; j++) {
				for (int k = 0; k < yFrames; k++) {
					spriteBatch.Draw(hole, new Rectangle(
						pos.X * 2 + j * (width / xFrames),
						pos.Y * 2 + k * (height / yFrames),
						hole.Width * 2,
						hole.Height * 2
					), Color.White);
				}
			}
		}
	}
	public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
		if (newSprite is null) return;
		spriteBatch.Draw(newSprite, Main.ScreenSize.ToVector2() * 0.5f - newSprite.Size() * 0.5f, Color.White);
	}
}
