using MonoMod.Cil;
using Origins.Buffs;
using Origins.Dev;
using Origins.Projectiles;
using PegasusLib.Reflection;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Neck)]
	public class Faith_Beads : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Misc"
		];
		public override void Load() {
			MonoModHooks.Add(
				typeof(Player).GetProperty(nameof(Player.breathCDMax)).GetGetMethod(),
				(Func<Player, int> orig, Player self) => {
					int value = orig(self);
					if (self.breath == 0 && self.OriginPlayer().faithBeads) value *= 8;
					return value;
				}
			);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.damage = 20;
			Item.DamageType = DamageClasses.Explosive;
			Item.knockBack = 4;
			Item.shoot = ModContent.ProjectileType<Faith_Beads_Explosion>();
			Item.rare = ItemRarityID.Pink;
			Item.master = true;
			Item.value = Item.sellPrice(gold: 8);
		}
		public override void UpdateEquip(Player player) {
			player.buffImmune[BuffID.Suffocation] = true;
			player.ignoreWater = true;
			player.accFlipper = true;
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.faithBeads = true;
			originPlayer.faithBeadsItem = Item;
			originPlayer.explosiveBlastRadius += 0.4f;
		}
	}
	public class Faith_Beads_Explosion : ExplosionProjectile {
		public override DamageClass DamageType => DamageClasses.Explosive;
		public override int Size => 80;
		public override bool DealsSelfDamage => false;
		public override SoundStyle? Sound => SoundID.Item14;
		public override int FireDustAmount => 0;
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.usesLocalNPCImmunity = false;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
		}
	}
}
