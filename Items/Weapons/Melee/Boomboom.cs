using Origins.Buffs;
using Origins.Dev;
using Origins.Journal;
using Origins.Projectiles.Weapons;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class Boomboom : ModItem, ICustomWikiStat, IJournalEntrySource {
		public string[] Categories => [
			"Boomerang"
		];
		public string EntryName => "Origins/" + typeof(Boomboom_Entry).Name;
		public class Boomboom_Entry : JournalEntry {
			public override string TextKey => "Boomboom";
			public override JournalSortIndex SortIndex => new("Brine_Fiend", 1);
		}
		public override void SetStaticDefaults() {
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.Venom, Toxic_Shock_Debuff.ID];
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ThornChakram);
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.damage = 60;
			Item.width = 34;
			Item.height = 34;
			Item.useTime = 14;
			Item.useAnimation = 14;
			Item.shoot = ModContent.ProjectileType<Boomboom_P>();
			Item.shootSpeed = 13f;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(gold: 2, silver: 30);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item1;
		}
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}
	}
	public class Boomboom_P : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 8;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.penetrate = -1;
			Projectile.width = 34;
			Projectile.height = 34;
			Projectile.scale = 1;
		}
		public override void AI() {
			if (++Projectile.ai[2] > 8) {
				Projectile.ai[2] = 0;
				if (Projectile.owner == Main.myPlayer) Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center,
					Main.rand.NextVector2CircularEdge(2, 2),
					ModContent.ProjectileType<Brine_Droplet>(),
					Projectile.damage / 2,
					Projectile.knockBack / 3,
					Projectile.owner
				);
			}
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (++Projectile.frameCounter > 2) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Toxic_Shock_Debuff.ID, 180);
			target.AddBuff(BuffID.Venom, 180);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 27;
			height = 27;
			return true;
		}
	}
}
