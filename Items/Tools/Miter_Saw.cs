using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Miter_Saw : ModItem, ICustomDrawItem {
		static AutoCastingAsset<Texture2D> useTexture;
		public override void SetStaticDefaults() {
			useTexture = ModContent.Request<Texture2D>(Texture + "_Use");
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
		}
		public override void Unload() {
			useTexture = null;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WarAxeoftheNight);
			Item.damage = 18;
			Item.DamageType = DamageClass.Melee;
			Item.axe = 10;
			Item.width = 42;
			Item.height = 38;
			Item.useTime = 5;
			Item.useAnimation = 48;
			Item.knockBack = 0.8f;
			Item.value = Item.sellPrice(silver: 40);
			Item.UseSound = SoundID.Item23;
			Item.rare = ItemRarityID.Green;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
			if (player.ItemAnimationJustStarted) {
				noHitbox = true;
				return;
			}
			float itemRotation = player.compositeFrontArm.rotation;
			float size = 14 * player.GetAdjustedItemScale(Item);
			Vector2 pos = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, itemRotation)
				+ new Vector2(24, -4 * player.direction).RotatedBy(itemRotation + MathHelper.PiOver2)
				- new Vector2(size, size);
			size *= 2;
			hitbox = new(
				(int)pos.X,
				(int)pos.Y,
				(int)size,
				(int)size
			);
			itemHitbox = hitbox;
			int cd = player.itemAnimationMax / 8;
			if (player.attackCD > cd) player.attackCD = cd;
			player.ResetMeleeHitCooldowns();
		}
		Rectangle itemHitbox;
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			Rectangle overlap = Rectangle.Intersect(itemHitbox, target.Hitbox);
			Vector2 dir = new Vector2(4 * hit.HitDirection, 2).RotatedBy(player.compositeFrontArm.rotation);
			for (int i = 0; i < 4; i++) {
				Dust.NewDustPerfect(
					Main.rand.NextVector2FromRectangle(overlap),
					DustID.Torch,
					dir.RotatedByRandom(1f)
				).noGravity = true;
			}
		}
		public override void UseItemFrame(Player player) {
			float fact = (0.5f - (player.itemAnimation / (float)player.itemAnimationMax)) * 2;
			if (player.altFunctionUse == 2) {
				player.itemRotation -= player.direction * (fact * (System.Math.Abs(fact + 0.1f) - 0.65f) + 0.2f);
			} else {
				player.itemRotation += player.direction * fact * (System.Math.Abs(fact) - 0.5f);
			}
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation - MathHelper.PiOver2 * player.direction);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public bool DrawOverHand => true;
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player drawPlayer = drawInfo.drawPlayer;
			float itemRotation = drawPlayer.itemRotation - MathHelper.PiOver2;

			Texture2D texture = useTexture.Value;
			if (drawInfo.itemEffect == SpriteEffects.FlipHorizontally) {
				drawInfo.itemEffect = SpriteEffects.FlipVertically;
			}
			drawInfo.DrawDataCache.Add(new DrawData(
				texture,
				drawPlayer.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, drawPlayer.compositeFrontArm.rotation) - Main.screenPosition,
				texture.Frame(verticalFrames: 2, frameY: (drawPlayer.itemAnimation % 4) / 2),
				Item.GetAlpha(lightColor),
				itemRotation + (MathHelper.Pi * 0.75f) * drawPlayer.direction,
				new Vector2(11, drawPlayer.direction == 1 ? 27 : 7),
				drawPlayer.GetAdjustedItemScale(Item),
				drawInfo.itemEffect,
				1
			));
		}
	}
}
