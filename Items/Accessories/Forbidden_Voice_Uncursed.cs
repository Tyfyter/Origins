using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Weapons.Melee;
using Origins.Journal;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Beard)]
	public class Forbidden_Voice_Uncursed : Uncursed_Cursed_Item<Forbidden_Voice>, IJournalEntryItem, ICustomWikiStat {
		public string[] Categories => [
		];
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Whispers";
		public string EntryName => "Origins/" + typeof(Forbidden_Voice_Entry).Name;
		public override bool HasOwnTexture => true;
		public override void SetDefaults() {
			base.SetDefaults();
			Item.damage = 35;
			Item.DamageType = DamageClass.Generic;
			Item.knockBack = 4;
			Item.useTime = 48;
			Item.mana = 10;
			Item.shootSpeed = 4;
			Item.rare = ItemRarityID.Blue;
			Item.master = true;
			Item.value = Item.sellPrice(gold: 2);
			Item.UseSound = SoundID.NPCDeath9;
		}
		public override void UpdateEquip(Player player) {
			player.OriginPlayer().meleeScaleMultiplier *= 1.2f;
			player.GetDamage(DamageClass.Melee) *= 0.1f;
		}
	}
	public class Forbidden_Voice_Uncursed_P : ModProjectile {
		public override string Texture => "Origins/Items/Accessories/Forbidden_Voice";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PurificationPowder);
			Projectile.aiStyle = 0;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.DamageType = DamageClass.Generic;
			Projectile.timeLeft = 180;
			Projectile.extraUpdates = 0;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
			Projectile.tileCollide = true;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Soulhide_Weakened_Debuff.ID, 180);
		}
		public override bool PreDraw(ref Color lightColor) {
			Dust dust = Dust.NewDustDirect(Projectile.position + new Vector2(0, 2f), Projectile.width, Projectile.height, DustID.CorruptGibs, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 80, Scale: 1.3f);
			dust.velocity *= 0.3f;
			dust.noGravity = true;
			return false;
		}
		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 10; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position + new Vector2(0, 2f), Projectile.width, Projectile.height, DustID.CorruptGibs, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 80, Scale: 1.3f);
				dust.noGravity = true;
			}
			SoundEngine.PlaySound(SoundID.NPCDeath9, Projectile.Center);
		}
	}
}
