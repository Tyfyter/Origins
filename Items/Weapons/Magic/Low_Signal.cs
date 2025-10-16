using Origins.CrossMod;
using Origins.Dev;
using Origins.Journal;
using Origins.Projectiles.Weapons;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Low_Signal : ModItem, ICustomWikiStat, IJournalEntrySource {
        public string[] Categories => [
            WikiCategories.Wand
        ];
		public string EntryName => "Origins/" + typeof(Low_Signal_Entry).Name;
		public class Low_Signal_Entry : JournalEntry {
			public override string TextKey => "Low_Signal";
			public override JournalSortIndex SortIndex => new("The_Defiled", 9);
		}
		public override void SetStaticDefaults() {
			Item.staff[Type] = true;
			Origins.AddGlowMask(this);
			CritType.SetCritType<Flak_Crit_Type>(Type);
		}
		public override void SetDefaults() {
			Item.damage = 48;
			Item.DamageType = DamageClass.Magic;
			Item.crit = 5;
			Item.mana = 9;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.width = 30;
			Item.height = 36;
			Item.useTime = 46;
			Item.useAnimation = 46;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 5;
			Item.shoot = ModContent.ProjectileType<Low_Signal_P>();
			Item.shootSpeed = 16f;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = Origins.Sounds.DefiledIdle.WithPitchRange(-0.6f, -0.4f);
			Item.autoReuse = true;
		}
	}
	public class Low_Signal_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Magic/Infusion_P";
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.timeLeft = 40;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 60;
			Projectile.aiStyle = 0;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.penetrate = 1;
			Projectile.hide = true;
		}
		public override void AI() {
			Dust.NewDustPerfect(Projectile.Center, DustID.AncientLight, default, newColor: Color.White, Scale: 0.5f + (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.15f);
		}
		public override void OnKill(int timeLeft) {
			Projectile.localNPCImmunity.CopyTo(Projectile.NewProjectileDirect(
				Projectile.GetSource_FromThis(),
				Projectile.Center,
				Vector2.Zero,
				ModContent.ProjectileType<Defiled_Spike_Explosion>(),
				Projectile.damage,
				0,
				Projectile.owner,
			7).localNPCImmunity.AsSpan());
		}
	}
}
