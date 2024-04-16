using Origins.Dev;
using Origins.Journal;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Beard)]
	public class Forbidden_Voice : ModItem, IJournalEntryItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"MasterAcc"
		};
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Whispers";
		public string EntryName => "Origins/" + typeof(Asylum_Whistle_Entry).Name;
		public override void SetDefaults() {
			Item.DefaultToAccessory(38, 32);
			Item.damage = 50;
			Item.DamageType = DamageClass.Generic;
			Item.knockBack = 7;
			Item.useTime = 48;
			Item.mana = 20;
			Item.shoot = ModContent.ProjectileType<Forbidden_Voice_P>();
			Item.rare = CursedRarity.ID;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.cursedVoice = true;
			originPlayer.cursedVoiceItem = Item;
		}
	}
	public class Forbidden_Voice_P : ModProjectile {
		public override string Texture => "Origins/Items/Accessories/Forbidden_Voice";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PurificationPowder);
			Projectile.aiStyle = 0;
			Projectile.width = 38;
			Projectile.height = 38;
			Projectile.DamageType = DamageClass.Generic;
			Projectile.timeLeft = 90;
			Projectile.extraUpdates = 90;
			Projectile.penetrate = 1;
			Projectile.friendly = true;
		}
	}
	public class Forbidden_Voice_Entry : JournalEntry {
		public override string TextKey => nameof(Forbidden_Voice);
		public override ArmorShaderData TextShader => GameShaders.Armor.GetShaderFromItemId(ItemID.ShadowflameHadesDye);
	}
}
