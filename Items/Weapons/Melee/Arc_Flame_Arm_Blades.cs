using Humanizer;
using Origins.Buffs;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Dusts;
using Origins.Items.Vanity.Dev;
using Origins.Items.Weapons.Melee;
using Origins.Layers;
using Origins.NPCs;
using PegasusLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Rei_Set : DevSet<Arc_Flame_Arm_Blades> {
		public override IEnumerable<ItemTypeDropRuleWrapper> GetDrops() {/*
			yield return ModContent.ItemType<First_Dream>();
			yield return ModContent.ItemType<Chew_Toy>();*/
			yield return new(ItemDropRule.ByCondition(DropConditions.HardmodeBossBag, ModContent.ItemType<Arc_Flame_Arm_Blades>()));
		}
	}
	[AutoloadEquip(EquipType.HandsOn, EquipType.HandsOff)]
	public class Arc_Flame_Arm_Blades : ModItem, ICustomWikiStat {
		public static int[] Debuffs = [];
		public static int SoundTime = 0;
		public string[] Categories => [
			WikiCategories.Sword,
			WikiCategories.DeveloperItem
		];
		public static string GenerateEmptyTag(int buffID) => $"[buffhint/dn\u200B:{(BuffID.Search.TryGetName(buffID, out string name) ? name : buffID)}]";
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(string.Join("", Debuffs.Skip(1).Select(GenerateEmptyTag)), CritType.ModEnabled ? string.Empty : this.GetLocalization("NoCSOTooltip"));
		public override void SetStaticDefaults() {
			Debuffs = [ModContent.BuffType<Arc_Burn_Debuff>(), ModContent.BuffType<Weak_Debuff>(), BuffID.OnFire3, BuffID.ShadowFlame];
			Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMasks(Item, EquipType.HandsOn, EquipType.HandsOff);
			//PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = Debuffs;
		}
		public override void SetDefaults() {
			Item.CloneDefaultsKeepSlots(ItemID.Arkhalis);/*
			Item.damage = 18;
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.useTime = 14;
			Item.useAnimation = 14;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 6;
			Item.autoReuse = true;
			Item.useTurn = true;*/
			//Item.shootSpeed = 50;
			Item.crit = 16;
			Item.shoot = ModContent.ProjectileType<Arc_Flame_Arm_Blades_Slash>();
			//Item.rare = ItemRarityID.Cyan;
			//Item.UseSound = SoundID.Item1;
		}
		public override void HoldStyle(Player player, Rectangle heldItemFrame) {
			player.handon = Item.handOnSlot;
			player.handoff = Item.handOffSlot;
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			player.handon = Item.handOnSlot;
			player.handoff = Item.handOffSlot;
		}
		public override bool AltFunctionUse(Player player) => !player.HasBuff(Blade_Dance_Cooldown_Debuff.ID);
		public override bool CanShoot(Player player) => player.altFunctionUse != 2;
		public override bool CanUseItem(Player player) {
			if (player.altFunctionUse == 2) {
				BladeDance(player);
				return false;
			}
			return true;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = Debuffs;
			for (int i = 0; i < tooltips.Count; i++) {
				switch (tooltips[i].Name) {
					case "CritChance":
					tooltips[i] = new(Mod, "DebuffChance", this.GetLocalizedValue("CritTooltip").FormatWith(tooltips[i].Text.Split(' ')[0]));
					break;
					case "PrefixCritChance":
					tooltips[i].Text = this.GetLocalizedValue("CritTooltip").FormatWith(tooltips[i].Text.Split(' ')[0]);
					break;
				}
			}
			if (!CritType.ModEnabled) {
				int index = tooltips.FindLastIndex(tip => tip.Name.StartsWith("Tooltip")) + 1;
				tooltips.Insert(index, new(Mod, "CritCondition", Language.GetTextValue("Mods.Origins.CritType.Arc_Flame_Arm_Blades_Crit_Type")) { OverrideColor = new(255, 255, 181) });
			}
		}
		public static void BladeDance(Player player) {
			SoundTime = 1;
			player.AddBuff(Blade_Dance_Buff.ID, 5 * 60);
			player.AddBuff(Blade_Dance_Cooldown_Debuff.ID, 5 * 60);
			SoundEngine.PlaySound(SoundID.Item37 with { PitchRange = (0.3f, 0.6f) }, player.Center);
		}
	}
	public class Arc_Flame_Arm_Blades_Slash : ModProjectile {
		static ref int[] Debuffs => ref Arc_Flame_Arm_Blades.Debuffs;
		static RangeRandom rand;
		public override void SetStaticDefaults() {
			rand = new(Main.rand, 0, Debuffs.Length);
			Main.projFrames[Type] = 14;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Arkhalis);
			Projectile.hide = true;
			Projectile.ContinuouslyUpdateDamageStats = true;
		}
		public override void AI() {
			float offsetRot = 0f;
			if (Projectile.spriteDirection == -1) offsetRot = (float)Math.PI;

			if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;

			Projectile.soundDelay--;
			if (Projectile.soundDelay <= 0) {
				SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
				Projectile.soundDelay = 12;
			}

			if (Main.myPlayer == Projectile.owner && Projectile.TryGetOwner(out Player player)) {
				Vector2 playerPos = player.RotatedRelativePoint(player.MountedCenter);
				if (player.channel && !player.noItems && !player.CCed) {
					float speed = 1f;
					if (player.inventory[player.selectedItem].shoot == Type)
						speed = player.inventory[player.selectedItem].shootSpeed * Projectile.scale;

					Vector2 velocity = Main.MouseWorld - playerPos;
					velocity.Normalize();
					if (velocity.HasNaNs()) velocity = Vector2.UnitX * player.direction;

					velocity *= speed;
					if (velocity.X != Projectile.velocity.X || velocity.Y != Projectile.velocity.Y)
						Projectile.netUpdate = true;

					Projectile.velocity = velocity;
				} else Projectile.Kill();

				Projectile.position = player.RotatedRelativePoint(player.MountedCenter, addGfxOffY: false) - Projectile.Size / 2f;
				Projectile.rotation = Projectile.velocity.ToRotation() + offsetRot;
				Projectile.position += Projectile.velocity.Normalized(out _) * 10;
				Projectile.spriteDirection = Projectile.direction;
				Projectile.timeLeft = 2;
				player.ChangeDir(Projectile.direction);
				player.heldProj = Projectile.whoAmI;
				player.SetDummyItemTime(2);
				player.itemRotation = MathHelper.WrapAngle((float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction));
			}

			Vector2 dustPos = Projectile.Center + Projectile.velocity * 3f;
			Color color = Main.rand.Next(3) switch {
				1 => new(187, 10, 251),
				2 => new(82, 103, 255),
				_ => new(212, 0, 95)
			};
			Lighting.AddLight(dustPos, color.ToVector3());
			if (Main.rand.NextBool(3)) {
				Dust dust = Dust.NewDustDirect(dustPos - Projectile.Size / 2f, Projectile.width, Projectile.height, ModContent.DustType<Tintable_Torch_Dust>(), Projectile.velocity.X, Projectile.velocity.Y, 100, color, 2f);
				dust.noGravity = true;
				dust.position -= Projectile.velocity;
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (!CritType.ModEnabled) {
				if (target.HasBuff<Arc_Burn_Debuff>()) {
					modifiers.CritDamage *= 5;
					modifiers.SetCrit();
				} else modifiers.DisableCrit();
			}
		}
		public void NonCritEffect(NPC target) {
			float chance = Projectile.CritChance;
			while (Main.rand.Next(100) < chance) {
				rand.Reset();
				for (int i = 0; i < Debuffs.Length; i++) {
					if (Debuffs[i] == Arc_Burn_Debuff.ID || Debuffs[i] == Weak_Debuff.ID || target.HasBuff(Debuffs[i]))
						rand.Multiply(i, i + 1, 0.3);
					if (target.buffImmune[Debuffs[i]])
						rand.Multiply(i, i + 1, 0);
				}
				int selectedDebuff = Debuffs[rand.Get()];
				target.AddBuff(selectedDebuff, 3 * 60);
				chance -= 100;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (!CritType.ModEnabled) {
				if (target.HasBuff<Arc_Burn_Debuff>()) target.DelBuff<Arc_Burn_Debuff>();
				else NonCritEffect(target);
			} else {
				if (hit.Crit && Projectile.ai[0] == 1 && target.HasBuff<Arc_Burn_Debuff>()) target.DelBuff<Arc_Burn_Debuff>();
				else NonCritEffect(target);
			}
			Projectile.ai[0] = 0;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			float chance = Projectile.CritChance;
			while (Main.rand.Next(100) < chance) {
				rand.Reset();
				for (int i = 0; i < Debuffs.Length; i++) {
					if (Debuffs[i] == Arc_Burn_Debuff.ID || Debuffs[i] == Weak_Debuff.ID || target.HasBuff(Debuffs[i]))
						rand.Multiply(i, i + 1, 0.3);
					if (target.buffImmune[Debuffs[i]])
						rand.Multiply(i, i + 1, 0);
				}
				int selectedDebuff = Debuffs[rand.Get()];
				if (selectedDebuff == Weak_Debuff.ID) selectedDebuff = BuffID.Weak;
				target.AddBuff(selectedDebuff, 3 * 60);
				chance -= 100;
			}
		}
	}
	public class Arc_Flame_Arm_Blades_Crit_Type : CritType<Arc_Flame_Arm_Blades> {
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) {
			if (target.HasBuff<Arc_Burn_Debuff>()) {
				projectile.ai[0] = 1;
				return true;
			}
			return false;
		}
		public override float CritMultiplier(Player player, Item item) => 10 / (1f + player.GetWeaponCrit(item) / 100f);
	}
}
namespace Origins.Buffs {
	public class Arc_Burn_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Buff_Hint_Handler.ModifyTip(Type, 15);
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().arcBurn = true;
			Color color = Main.rand.Next(3) switch {
				1 => new(187, 10, 251),
				2 => new(82, 103, 255),
				_ => new(212, 0, 95)
			};
			if (Main.rand.NextBool(3, 4)) {
				Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, ModContent.DustType<Tintable_Torch_Dust>(), npc.velocity.X, npc.velocity.Y, 100, color, 2f);
				dust.noGravity = true;
				dust.velocity *= 1.8f;
				dust.velocity.Y -= 0.5f;
				if (Main.rand.NextBool(4)) {
					dust.noGravity = false;
					dust.scale *= 0.5f;
				}
			}
			Lighting.AddLight(npc.Center, color.ToVector3());
		}

		public override void Update(Player player, ref int buffIndex) {
			player.OriginPlayer().arcBurn = true;
			Color color = Main.rand.Next(3) switch {
				1 => new(187, 10, 251),
				2 => new(82, 103, 255),
				_ => new(212, 0, 95)
			};
			if (Main.rand.NextBool(3, 4)) {
				Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, ModContent.DustType<Tintable_Torch_Dust>(), player.velocity.X, player.velocity.Y, 100, color, 2f);
				dust.noGravity = true;
				dust.velocity *= 1.8f;
				dust.velocity.Y -= 0.5f;
				if (Main.rand.NextBool(4)) {
					dust.noGravity = false;
					dust.scale *= 0.5f;
				}
			}
			Lighting.AddLight(player.Center, color.ToVector3());
		}
	}
	public class Blade_Dance_Buff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Buff_Hint_Handler.ModifyTip(Type, 0, this.GetLocalization("EffectDescription").Key);
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetDamage(DamageClass.Melee) += 0.5f;
		}
	}
	public class Blade_Dance_Cooldown_Debuff : ModBuff {
		public override string Texture => typeof(Arc_Flame_Arm_Blades).GetDefaultTMLName();
		public static int ID { get; private set; }
		public static ref int SoundTime => ref Arc_Flame_Arm_Blades.SoundTime;
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			BuffID.Sets.TimeLeftDoesNotDecrease[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			bool hasBuff = player.HasBuff(Blade_Dance_Buff.ID);
			Main.buffNoTimeDisplay[Type] = hasBuff;
			BuffID.Sets.TimeLeftDoesNotDecrease[Type] = hasBuff;
			if (SoundTime >= 1 && SoundTime.CycleUp(10)) {
				SoundEngine.PlaySound(SoundID.Item37 with { PitchRange = (0.3f, 0.6f) }, player.Center);
			}
		}
	}
}
