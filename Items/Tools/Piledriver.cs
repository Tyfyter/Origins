using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Melee;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Piledriver : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"ToolWeapon"
		];
		public static int Pick => 70;
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TitaniumDrill);
			Item.damage = 46;
			Item.pick = 0;
			Item.width = 28;
			Item.height = 26;
			Item.useTime = Item.useAnimation = 60;
			Item.knockBack = 9f;
			Item.shootSpeed = 4f;
			Item.shoot = ModContent.ProjectileType<Piledriver_P>();
			Item.value = Item.sellPrice(silver: 40);
			Item.rare = ItemRarityID.Blue;
			Item.tileBoost = 0;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = 0; i < tooltips.Count; i++) {
				if (tooltips[i].Name == "Knockback") {
					tooltips.Insert(i + 1, new TooltipLine(Mod, "PickPower", Pick + Lang.tip[26].Value));
					break;
				}
			}
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<NE8>(), 6)
            .AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 12)
            .AddTile(TileID.Anvils)
			.Register();
		}
		public override bool AltFunctionUse(Player player) => true;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.direction == 1) {
				Vector2 offset = velocity.SafeNormalize(default) * 4;
				position += new Vector2(-offset.Y, offset.X);
			}
			Projectile.NewProjectile(
				source,
				position,
				velocity,
				type,
				damage,
				knockback,
				player.whoAmI,
				player.itemTimeMax * (player.altFunctionUse == 2 ? 1.5f : 1),
				ai2: player.altFunctionUse
			);
			return false;
		}
		public override bool MeleePrefix() => true;
	}
	public class Piledriver_P : ModProjectile {
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.TitaniumDrill);
			Projectile.friendly = false;
		}
		public override bool PreAI() {
			Player owner = Main.player[Projectile.owner];
			if (owner.controlUseTile) {
				owner.channel = true;
			}
			return true;
		}
		public override void AI() {
			Projectile.rotation += MathHelper.PiOver2;
			if (Projectile.ai[1] >= Projectile.ai[0]) {
				Projectile.ai[1] -= Projectile.ai[0];
				Projectile.frame = 3;
				if (Projectile.owner == Main.myPlayer) {
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						Projectile.velocity,
						ModContent.ProjectileType<Piledriver_Mine_P>(),
						Projectile.originalDamage,
						Projectile.knockBack,
						Main.myPlayer,
						ai2: Projectile.ai[2]
					);
				}
			} else {
				Projectile.ai[1]++;
				Projectile.frame = (int)((Projectile.ai[1] / Projectile.ai[0]) * 3);
				if (Projectile.ai[1] <= 1) Projectile.frame = 3;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				texture.Frame(verticalFrames: 4, frameY: Projectile.frame),
				lightColor,
				Projectile.rotation + MathHelper.Pi,
				new Vector2(8, 14 - (3 * (Projectile.direction + 1))),
				Projectile.scale,
				Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically
			);
			return false;
		}
	}
	public class Piledriver_Mine_P : ModProjectile {
		public override string Texture => "Origins/Items/Tools/Piledriver_P";
		public override void SetDefaults() {
			Projectile.timeLeft = 24;
			Projectile.extraUpdates = 100;
			Projectile.width = Projectile.height = 0;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = 1;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				float scale = itemUse.Player.GetAdjustedItemScale(itemUse.Item);
				Projectile.timeLeft = (int)(Projectile.timeLeft * scale);
				Projectile.scale *= scale + scale - 1;
			}
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			hitbox.Inflate(4, 4);
			//Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Vector2.Zero).noGravity = true;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (Projectile.ai[2] == 2 && !CritType.ModEnabled) {
				modifiers.CritDamage *= 1 + Projectile.CritChance / 50f;
				modifiers.SetCrit();
			}
		}
		public override void AI() {
			Point pos = Projectile.position.ToTileCoordinates();
			if ((pos.X == Player.tileTargetX && pos.Y == Player.tileTargetY) || Framing.GetTileSafely(pos).HasFullSolidTile()) {
				Mine();
			}
		}
		void Mine() {
			Vector2 velocity = Projectile.velocity;
			velocity /= MathF.Max(MathF.Abs(velocity.X) / 16, MathF.Abs(velocity.Y) / 16);
			Vector2 pos = Projectile.position;
			Player owner = Main.player[Projectile.owner];
			int pick = Piledriver.Pick;
			bool stronk = Projectile.ai[2] == 2;
			float stronkth = stronk ? 0 : 0.4f;
			Vector2 perp = new Vector2(velocity.Y, -velocity.X);
			static void MineTile(Player owner, int x, int y, int pick, float extraHits = 0) {
				int tileType = Framing.GetTileSafely(x, y).TileType;
				bool altar = false;
				if (tileType == TileID.DemonAltar) altar = true;
				if (tileType >= TileID.Count) {
					int[] adj = TileLoader.GetTile(tileType).AdjTiles;
					for (int i = 0; i < adj.Length; i++) if (adj[i] == TileID.DemonAltar) {
						altar = true;
						break;
					}
				}
				if (altar) {
					owner.Hurt(PlayerDeathReason.ByOther(4), owner.statLife / 2, -owner.direction);
					return;
				}
				if (Main.tileAxe[tileType]) pick = 0;
				if (Main.tileHammer[tileType]) pick /= 10;
				owner.PickTile(x, y, pick);
				for (int i = 0; i < extraHits; i++) {
					owner.PickTile(x, y, (int)(pick * Math.Min(extraHits, 1)));
				}
			}
			Point tilePos = pos.ToTileCoordinates();
			int minSolidness = Framing.GetTileSafely(tilePos).TileSolidness();
			if (minSolidness <= 0) return;
			for (int i = 0; i < 5; i++) {
				if (Framing.GetTileSafely(tilePos).TileSolidness() < minSolidness) break;
				MineTile(owner, tilePos.X, tilePos.Y, pick, stronkth);
				if (stronk) {
					tilePos = (pos + perp).ToTileCoordinates();
					MineTile(owner, tilePos.X, tilePos.Y, pick, stronkth);
					tilePos = (pos - perp).ToTileCoordinates();
					MineTile(owner, tilePos.X, tilePos.Y, pick, stronkth);
				}
				pos += velocity;
				tilePos = pos.ToTileCoordinates();
			}
			Projectile.Kill();
		}
		public override void OnKill(int timeLeft) {
			if (timeLeft > 0) {
				Vector2 pos = Projectile.position;
				for (int i = 0; i < 3; i++) Dust.NewDust(pos, 0, 0, DustID.Torch);
			}
		}
	}
	public class Piledriver_Crit_Type : CritType<Piledriver> {
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) => projectile?.ai[2] == 2;
		public override float CritMultiplier(Player player, Item item) => 1.8f + (player.GetWeaponCrit(item) / 100f);
	}
}
