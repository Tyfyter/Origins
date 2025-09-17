using Origins.Buffs;
using Origins.Dev;
using Origins.Journal;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Beard)]
	public class Forbidden_Voice : ModItem, IJournalEntrySource, ICustomWikiStat {
		public string[] Categories => [
		];
		public string EntryName => "Origins/" + typeof(Forbidden_Voice_Entry).Name;
		public override void SetDefaults() {
			Item.DefaultToAccessory(38, 32);
			Item.damage = 76;
			Item.DamageType = DamageClass.Melee;
			Item.knockBack = 7;
			Item.useTime = 15 * 60;
			Item.mana = 140;
			Item.shootSpeed = 4;
			Item.shoot = ModContent.ProjectileType<Forbidden_Voice_P>();
			Item.buffType = BuffID.Silenced;
			Item.rare = CursedRarity.ID;
			Item.master = true;
			Item.value = Item.sellPrice(gold: 5);
		}
		public override bool CanUseItem(Player player) => false;
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.cursedVoice = true;
			originPlayer.cursedVoiceItem = Item;
			if (originPlayer.cursedVoiceCooldownMax == 0) originPlayer.cursedVoiceCooldownMax = 1;
			player.endurance += (1 - player.endurance) * Math.Clamp(1 - originPlayer.cursedVoiceCooldown / (float)originPlayer.cursedVoiceCooldownMax, 0, 1) * 0.1f;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.SubstituteKeybind(Keybindings.ForbiddenVoice);
		}
		public override bool MeleePrefix() => true;
	}
	public class Forbidden_Voice_P : ModProjectile {
		public override string Texture => "Origins/Items/Accessories/Forbidden_Voice";
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PurificationPowder);
			Projectile.aiStyle = 0;
			Projectile.width = 25 * 16;
			Projectile.height = Projectile.width;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 7;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
		}
		public override void AI() {
			if (Projectile.soundDelay <= 0) {
				Projectile.soundDelay = Projectile.timeLeft + 1;
				SoundEngine.PlaySound(Main.rand.NextFromList(SoundID.Zombie121, SoundID.Zombie122, SoundID.Zombie123).WithPitchRange(0.2f, 0.35f).WithVolume(0.5f));
				Main.instance.CameraModifiers.Add(new CameraShakeModifier(
					Projectile.Center, 10f, 6f, 30, 1000f, 2f, nameof(Forbidden_Voice)
				));
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Silenced_Debuff.ID, 10 * 60);
			if (target.realLife != -1) {
				foreach (NPC other in Main.ActiveNPCs) {
					if (other.realLife == target.realLife) other.AddBuff(Silenced_Debuff.ID, 10 * 60);
				}
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(BuffID.Silenced, 10 * 60);
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			float factor = (Projectile.scale - 1) * 0.5f;
			hitbox.Inflate((int)(hitbox.Width * factor), (int)(hitbox.Height * factor));
		}
	}
	public class Forbidden_Voice_Entry : JournalEntry {
		public override string TextKey => nameof(Forbidden_Voice);
		public override ArmorShaderData TextShader => GameShaders.Armor.GetShaderFromItemId(ItemID.PurpleOozeDye);
		public override Color BaseColor => new(64, 64, 64);
	}
	public class CameraShakeModifier(Vector2 startPosition, float strength, float shakeSpeed, int framesToLast, float distanceFalloff = -1f, float timeFalloff = -1f, string uniqueIdentity = null) : ICameraModifier {
		Vector2 pos = Vector2.Zero;
		Vector2 target = Vector2.Zero;
		private int _framesLasted;
		public string UniqueIdentity { get; private set; } = uniqueIdentity;
		public bool Finished { get; private set; }
		public void Update(ref CameraInfo cameraInfo) {
			float distFactor;
			if (distanceFalloff == -1f) {
				distFactor = 1f;
			} else {
				distFactor = Utils.Remap(Vector2.Distance(startPosition, cameraInfo.OriginalCameraCenter), 0f, distanceFalloff, 1f, 0f);
			}
			float timeFactor;
			if (timeFalloff == -1f) {
				timeFactor = 1f;
			} else {
				timeFactor = MathF.Pow(Math.Max(1 - _framesLasted / (float)framesToLast, 0), timeFalloff);
			}
			if (MathUtils.LinearSmoothing(ref pos, target, shakeSpeed)) {
				if (_framesLasted >= framesToLast) {
					Finished = true;
				} else {
					target = Main.rand.NextVector2Unit() * strength * distFactor * timeFactor;
				}
			}
			cameraInfo.CameraPosition += pos * OriginClientConfig.Instance.ScreenShakeMultiplier;
			_framesLasted++;
			if (_framesLasted >= framesToLast) {
				target = Vector2.Zero;
			}
		}
	}
}
