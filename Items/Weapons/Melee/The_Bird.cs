using Origins.Items.Accessories;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Microsoft.Xna.Framework;
using Origins.NPCs;

namespace Origins.Items.Weapons.Melee {
	public class The_Bird : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Sword",
			"DeveloperItem",
			"ReworkExpected"
		];
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WoodenSword);
			Item.useStyle = ItemUseStyleID.Swing;
			Item.damage = 99;
			Item.crit = 44;
			Item.useAnimation = Item.useTime = 20;
			Item.rare = ItemRarityID.Cyan;
			Item.knockBack = 16;
		}
		public override void AddRecipes() {
			//Recipe.Create(Type)
			//.AddIngredient(ModContent.ItemType<Baseball_Bat>())
			//.AddIngredient(ModContent.ItemType<Razorwire>())
			//.Register();
		}
		public override bool AltFunctionUse(Player player) => true;
		public override void UseItemFrame(Player player) {
			int frame = player.bodyFrame.Y / player.bodyFrame.Height;
			float progress = player.itemAnimation / (float)player.itemAnimationMax;
			if (player.altFunctionUse == 2) {
				progress *= progress;
				frame = 2 + (int)(progress * 4f);
				if (frame == 5) frame = 0;
				switch (frame) {
					case 0:
					player.itemLocation = player.MountedCenter + new Vector2(-4 * player.direction, 4);
					break;
					case 4:
					player.itemLocation = player.MountedCenter + new Vector2(4 * player.direction, 8);
					break;
					case 3:
					player.itemLocation = player.MountedCenter + new Vector2(6 * player.direction, 8);
					break;
					case 2:
					player.itemLocation = player.MountedCenter + new Vector2(6 * player.direction, -8);
					break;
				}
				player.itemRotation = (progress * 3 - 0.5f) * player.direction;
			}
			player.bodyFrame.Y = player.bodyFrame.Height * frame;
		}
		public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
			if (player.altFunctionUse == 2) {
				switch (player.bodyFrame.Y / player.bodyFrame.Height) {
					case 0:
					hitbox.Y += 48;
					break;
					case 2 or 3 or 4:
					hitbox = player.Hitbox;
					hitbox.X += player.direction * 24;
					hitbox.Inflate(8, (80 - player.height) / 2);
					break;
				}
			}
		}
		public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = 0;
			if (target.knockBackResist != 0) {
				OriginGlobalNPC global = target.GetGlobalNPC<OriginGlobalNPC>();
				global.birdedTime = 1;
			}
		}
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			Vector2 knockback = hit.GetKnockbackFromHit(false, false, yMult: 1);
			if (player.altFunctionUse == 2) {
				target.velocity = new(knockback.Y * player.direction * 0.25f, -knockback.X);
			} else {
				target.velocity = new(knockback.X * player.direction, knockback.Y * (target.noGravity ? -0.5f : -0.75f));
			}
			if (hit.Knockback != 0) {
				OriginGlobalNPC global = target.GetGlobalNPC<OriginGlobalNPC>();
				global.birdedTime = 90;
				global.birdedDamage = hit.SourceDamage;
			}
		}
	}
}
