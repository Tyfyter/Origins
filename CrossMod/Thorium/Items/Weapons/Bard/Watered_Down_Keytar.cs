using Origins.Dev;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items;

namespace Origins.CrossMod.Thorium.Items.Weapons.Bard {
	#region without thorium
	public class Watered_Down_Keytar : ModItem {
		public static int ID { get; internal set; }
		public override bool IsLoadingEnabled(Mod mod) => !ModLoader.HasMod("ThoriumMod");
		public static SoundStyle BassSound = new("Origins/Sounds/Custom/KeytarBass", SoundType.Sound);
		public static SoundStyle SynthSound = new("Origins/Sounds/Custom/KeytarSynth", SoundType.Sound);
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ID = Type;
		}
		public static void SetSharedDefaults(Item item, out int cost) {
			item.damage = 54;
			item.DamageType = DamageClass.Generic;
			item.knockBack = 1f;
			item.useAnimation = 16;
			item.useTime = 16;
			item.useStyle = ItemUseStyleID.Shoot;
			item.noMelee = true;
			item.shootSpeed = 18;
			item.value = Item.sellPrice(gold: 2);
			item.rare = ItemRarityID.Pink;
			cost = 20;
		}
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) Item.UseSound = BassSound;
			else Item.UseSound = SynthSound;
			
			SoundEngine.PlaySound(Item.UseSound.Value.WithPitchOffset(
				Math.Min(((Main.MouseWorld - player.Center) / new Vector2(Main.screenWidth * 0.4f, Main.screenHeight * 0.4f)).Length(), 1) * 2 - 1
			), player.Center);
			return null;
		}
		public override Vector2? HoldoutOffset() => new Vector2(-6, 0);
		public override bool AltFunctionUse(Player player) => true;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				type = ModContent.ProjectileType<Watered_Down_Keytar_Bass>();
			}
		}
		public override void SetDefaults() {
			SetSharedDefaults(Item, out Item.mana);
			Item.shoot = ModContent.ProjectileType<Watered_Down_Keytar_Synth>();
		}
	}
	public class Watered_Down_Keytar_Synth : Keytar_Synth {
		public override string Texture => "Origins/CrossMod/Thorium/Items/Weapons/Bard/Keytar_Synth";
	}
	public class Watered_Down_Keytar_Bass : Keytar_Bass {
		public override string Texture => "Origins/CrossMod/Thorium/Items/Weapons/Bard/Keytar_Bass";
	}
	#endregion without thorium
	#region with thorium
	[ExtendsFromMod("ThoriumMod")]
	public class Watered_Down_Keytar_Thorium : BardItem, ICustomWikiStat {
		public override string Name => base.Name[..^"_Thorium".Length];
		public override BardInstrumentType InstrumentType => BardInstrumentType.Electronic;
		public override void SetStaticDefaults() {
			Empowerments.AddInfo<EmpowermentProlongation>(2);
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
			Watered_Down_Keytar.ID = Type;
		}
		public override void SetBardDefaults() {
			Watered_Down_Keytar.SetSharedDefaults(Item, out int cost);
			Item.shoot = ModContent.ProjectileType<Watered_Down_Keytar_Synth_Thorium>();
			InspirationCost = cost / 10;
		}
		public override void BardUseAnimation(Player player) {
			if (player.altFunctionUse == 2) {
				Item.UseSound = Watered_Down_Keytar.BassSound;
			} else {
				Item.UseSound = Watered_Down_Keytar.SynthSound;
			}
		}
		public override Vector2? HoldoutOffset() => new Vector2(-6, 0);
		public override bool AltFunctionUse(Player player) => true;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				type = ModContent.ProjectileType<Watered_Down_Keytar_Bass_Thorium>();
			}
		}
		public string CustomStatPath => WikiPageExporter.GetWikiName(this) + "_Thorium";
	}
	[ExtendsFromMod("ThoriumMod")]
	public class Watered_Down_Keytar_Synth_Thorium : Keytar_Synth_Thorium {
		public override string Texture => "Origins/CrossMod/Thorium/Items/Weapons/Bard/Keytar_Synth";
	}
	[ExtendsFromMod("ThoriumMod")]
	public class Watered_Down_Keytar_Bass_Thorium : Keytar_Bass_Thorium {
		public override string Texture => "Origins/CrossMod/Thorium/Items/Weapons/Bard/Keytar_Bass";
	}
	#endregion with thorium
}