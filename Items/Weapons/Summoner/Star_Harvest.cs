using Origins.Dev;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Projectiles;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Summoner {
	public class Star_Harvest : ModItem, ICustomWikiStat {
		public override void SetStaticDefaults() {
			ItemID.Sets.StaffMinionSlotsRequired[Item.type] = 1;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 160;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Star_Harvest_Buff.ID;
			Item.shoot = Star_Cell_Tracker.ID;
			Item.noMelee = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Star_Harvest_Buff.ID, 2);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, knockback);
			return false;
		}
	}
	public class Star_Harvest_Buff : MinionBuff {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StardustCellMinion;
		public override IEnumerable<int> ProjectileTypes() => [
			Star_Cell.ID,
			Small_Star_Cell.ID,
			Star_Cell_Tracker.ID
		];
		public static int ID { get; private set; }
		public override bool ShowSlots => true;
		protected override void SetBuffFlag(Player player) => player.OriginPlayer().starCellArtifact = true;
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Star_Cell : MinionBase, IArtifactMinion {
		public static int ID { get; private set; }
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StardustCellMinion;
		public int MaxLife { get; set; }
		public float Life { get; set; }
		public override void SetStaticDefaults() {
			this.SetIDProp();
			Main.projFrames[Type] = 4;
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Type] = true;
			OriginsSets.Projectiles.NoMultishot[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 0f;
			Projectile.penetrate = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
			Projectile.netImportant = true;
			MaxLife = 120;
		}
		public override void MoveTowardsTarget() {
			base.MoveTowardsTarget();
		}
		public override void OnKill(int timeLeft) {
			if (!Projectile.IsLocallyOwned() || ArtifactMinionSystem.IsDismissingMinion) return;
			int count = MaxLife / 30;
			int maxPerSlot = count * 2;
			int total = CountKin();
			int i = 0;
			for (; i < count && total < maxPerSlot; i++) {
				Projectile.NewProjectile(
					Projectile.GetSource_Death(),
					Projectile.Center,
					Main.rand.NextVector2Circular(1, 1),
					Small_Star_Cell.ID,
					Projectile.originalDamage,
					Projectile.knockBack,
					ai2: Projectile.ai[2]
				);
				total++;
			}
			if (i < count) {
				Projectile.NewProjectile(
					Projectile.GetSource_Death(),
					Projectile.Center,
					default,
					Star_Cell_Tracker.ID,
					Projectile.originalDamage,
					Projectile.knockBack,
					ai1: count - i
				);
			}
		}
		protected bool CheckTracked() {
			if (GetProjectile(Projectile.owner, (int)Projectile.ai[2]) is not { active: true, ModProjectile: Star_Cell_Tracker }) {
				Projectile.Kill();
				return false;
			}
			return true;
		}
		protected int CountKin() {
			int count = 0;
			foreach (Projectile minion in Main.ActiveProjectiles) {
				if (minion.owner != Projectile.owner) continue;
				if (minion.ModProjectile is not Star_Cell) continue;
				if (minion.ai[2] == minion.ai[2]) count++;
			}
			return count;
		}

		#region boilerplate
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => fallThrough = true;
		public override ref bool HasBuff(Player player) => ref player.OriginPlayer().starCellArtifact;
		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => true;
		#endregion
	}
	public class Small_Star_Cell : Star_Cell {
		public new static int ID { get; private set; }
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StardustCellMinionShot;
		public override bool SkipTargeting => Projectile.ai[0] < 150;
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.width = 18;
			Projectile.height = 18;
			MaxLife = 30;
		}
		public override void MoveTowardsTarget() {
			if (SkipTargeting) Projectile.velocity *= 0.99f;
			else base.MoveTowardsTarget();

			if (Projectile.ai[0].Warmup(180, SpeedModifier)) {
				Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center,
					Projectile.velocity,
					Star_Cell.ID,
					Projectile.originalDamage,
					Projectile.knockBack,
					ai2: Projectile.ai[2]
				);
				Projectile.active = false;
			}
		}
		public override bool MinionContactDamage() => Projectile.ai[0] > 10;
		public override void OnKill(int timeLeft) {

		}
	}
	public class Star_Cell_Tracker : MinionBase {
		public static int ID { get; private set; }
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StardustCellMinion;
		public override bool SkipTargeting => true;
		public override bool AutomaticRotationAndDirection => false;
		public override void SetStaticDefaults() {
			this.SetIDProp();
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.netImportant = true;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.penetrate = -1;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.timeLeft = 60;
			Projectile.aiStyle = ProjAIStyleID.DesertTigerBall;
			Projectile.hide = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse) Projectile.ai[0] = 1;
		}
		public override void MoveTowardsTarget() {
			if (Projectile.IsLocallyOwned()) {
				for (int i = 0; i < Projectile.ai[0]; i++)
					Projectile.NewProjectile(
						Projectile.GetSource_Death(),
						Projectile.Center,
						default,
						Star_Cell.ID,
						Projectile.originalDamage,
						Projectile.knockBack,
						ai2: Projectile.identity
					);
				for (int i = 0; i < Projectile.ai[1]; i++)
					Projectile.NewProjectile(
						Projectile.GetSource_Death(),
						Projectile.Center,
						Main.rand.NextVector2Circular(1, 1),
						Small_Star_Cell.ID,
						Projectile.originalDamage,
						Projectile.knockBack,
						ai2: Projectile.identity
					);
				Projectile.ai[0] = 0;
				Projectile.ai[1] = 0;
			}
			Projectile.position = Main.player[Projectile.owner].position;
			if (!Projectile.IsLocallyOwned()) return;
			foreach (Projectile minion in Main.ActiveProjectiles) {
				if (minion.owner != Projectile.owner) continue;
				if (minion.ModProjectile is not Star_Cell) continue;
				if (minion.ai[2] == Projectile.identity) return;
			}
			Projectile.Kill();
		}
		public override ref bool HasBuff(Player player) => ref player.OriginPlayer().starCellArtifact;
		public override bool? CanCutTiles() => false;
	}
}
