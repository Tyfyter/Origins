using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Weapons.Magic;
using Origins.Journal;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Salty_Seed : ModItem, ICustomWikiStat, IJournalEntrySource {
		public string[] Categories => [
			"Tool"
		];
		public string EntryName => "Origins/" + typeof(Salty_Seed_Entry).Name;
		public class Salty_Seed_Entry : JournalEntry {
			public override string TextKey => "Salty_Seed";
			public override JournalSortIndex SortIndex => new("Riven", 2);
		}
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Plasma_Cutter>()] = Type;
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Plasma_Cutter>();
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(16, 26);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 8);

			Item.useTime = 5;
			Item.useAnimation = 5;
			Item.shootSpeed = 5;
			Item.shoot = ModContent.ProjectileType<Mitosis_P>();
		}
	}
	public class Mitosis_P : ModProjectile {
		static readonly AutoLoadingAsset<Texture2D> seedTexture = typeof(Mitosis_P).GetDefaultTMLName() + "_Seed";
		public static List<int> mitosises = [];
		public static List<int> nextMitosises = [];
		public override void Unload() {
			mitosises = null;
			nextMitosises = null;
		}
		public const int minion_duplicate_duration = 300;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 10;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.DamageType = DamageClass.Default;
			Projectile.aiStyle = ProjAIStyleID.Hook;
			Projectile.penetrate = -1;
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 900;
			Projectile.alpha = 0;
		}
		public override void AI() {
			Projectile.rotation = 0;
			Projectile.aiStyle = 0;
			Projectile.velocity *= 0.95f;
			nextMitosises.Add(Projectile.whoAmI);
			if (++Projectile.frameCounter >= 6) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 5;
			}
		}
		public override bool? CanUseGrapple(Player player) {
			//if (!player.CheckMana()) return false;
			if (player.ownedProjectileCounts[Type] > 0) {
				foreach (Projectile proj in Main.ActiveProjectiles) {
					if (proj.type == Type && proj.owner == player.whoAmI) {
						proj.Kill();
						break;
					}
				}
			}
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = seedTexture;
			Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition + Vector2.UnitY,
				frame,
				lightColor,
				Projectile.rotation,
				frame.Size() * 0.5f,
				1,
				Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None 
			);
			lightColor = Riven_Hive.GetGlowAlpha(lightColor);
			return true;
		}
	}
}
