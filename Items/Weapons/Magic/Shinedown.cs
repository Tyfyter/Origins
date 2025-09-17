using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.NPCs;
using PegasusLib;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Shinedown : ModItem, ITornSource {
		public static float TornSeverity => 0.3f;
		float ITornSource.Severity => TornSeverity;
		public static float ExtraManaPerEnemyPercent => 0.6f;
		public static float FlatKnockbackAdjustment => 1f;
		public override void SetStaticDefaults() {
			Item.staff[Item.type] = true;
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [BuffID.CursedInferno, BuffID.Ichor, Rasterized_Debuff.ID, Torn_Debuff.ID];
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.DamageType = DamageClass.Magic;
			Item.useStyle = -1;
			Item.damage = 84;
			Item.noMelee = true;
			Item.width = 44;
			Item.height = 44;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.shoot = ModContent.ProjectileType<Shinedown_Staff_P>();
			Item.shootSpeed = 16 * 45;
			Item.mana = 7;
			Item.knockBack = FlatKnockbackAdjustment;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item132.WithPitch(1f);
			Item.autoReuse = false;
			Item.channel = true;
		}
		public static float GetSpeedMultiplier(Player player, Item item) {
			return (30f / item.useTime) / CombinedHooks.TotalUseTimeMultiplier(player, item);
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			LanguageTree locKey = this.GetLocalizationTree();
			float speed = GetSpeedMultiplier(Main.LocalPlayer, Item);
			if (Main.LocalPlayer.kbBuff) Item.knockBack /= 1.5f;
			for (int i = 0; i < tooltips.Count; i++) {
				switch (tooltips[i].Name) {
					case "Damage":
					tooltips[i].Text = locKey.Find("PerSecond").value.Format($"{Main.LocalPlayer.GetWeaponDamage(Item) * speed * 2:0.#}{Item.DamageType.DisplayName.Value}");
					break;
					case "CritChance":
					case "PrefixCritChance":
					tooltips.RemoveAt(i--);
					break;
					case "Speed":
					tooltips.RemoveAt(i--);
					break;
					case "Knockback":
					float statusRate = Main.LocalPlayer.GetWeaponKnockback(Item) * speed / FlatKnockbackAdjustment;
					string statusText = $"{statusRate:0.#}";
					LanguageTree tree = locKey.Find("StatusEffects");
					if (!tree.TryGetValue(statusText, out LanguageTree format)) format = tree.Find("Default");
					tooltips[i].Text = locKey.Find("PerSecond").value.Format(format.value.Format(statusText));
					break;
					case "PrefixKnockback":
					tooltips[i].Text = locKey.Find("StatusChance").value.Format(tooltips[i].Text.Split(' ')[0]);
					break;
					case "UseMana":
					float manaCost = Main.LocalPlayer.GetManaCost(Item) * speed;
					tooltips[i].Text = locKey.Find("ManaPerSecond").value.Format(manaCost, manaCost * ExtraManaPerEnemyPercent);
					break;
				}
			}
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			player.bodyFrame.Y = player.bodyFrame.Height * 2;
			player.itemLocation = player.MountedCenter + new Vector2(player.direction * -6, 6);
		}
	}
	[ReinitializeDuringResizeArrays]
	public class Shinedown_Staff_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MedusaHeadRay;
		public static bool[] InalidTargetNPCs = NPCID.Sets.Factory.CreateBoolSet(
			NPCID.TheDestroyer,
			NPCID.TheDestroyerBody,
			NPCID.TheDestroyerTail
		);
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.timeLeft = 5;
			Projectile.tileCollide = false;
			Projectile.ContinuouslyUpdateDamageStats = true;
		}
		public override bool ShouldUpdatePosition() => false;
		Aim[] aims;
		Aim[] decayingAims;
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) Projectile.originalDamage = itemUse.Item.damage;
			Projectile.ai[0] = 2;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.dead || !player.active) {
				Projectile.Kill();
				return;
			}
			float highestProgress = 0;
			ActiveSound sound;
			player.itemRotation = 0;//MathHelper.PiOver4 * -player.direction;
			Projectile.position = player.RotatedRelativePoint(player.MountedCenter + new Vector2(6 * player.direction, -48));
			if (Projectile.ai[2] == 1) {
				Projectile.ai[2] = 2;
				for (int i = 0; i < aims.Length; i++) {
					if (aims[i].active) {
						aims[i].active = false;
						AddDecayingAim(aims[i]);
					}
				}
			}
			if (Projectile.ai[2] == 2) {
				Projectile.aiStyle = -1;
				for (int i = 0; i < decayingAims.Length; i++) {
					decayingAims[i].UpdateDecaying(Projectile.ai[1]);
					if (decayingAims[i].active) Projectile.timeLeft = 2;
					highestProgress = float.Max(highestProgress, decayingAims[i].Progress);
				}
				if (Projectile.timeLeft == 2) player.SetDummyItemTime(2);
				if (SoundEngine.TryGetActiveSound(soundSlot, out sound)) {
					sound.Volume = highestProgress;
					sound.Position = Projectile.position;
				}
				if (SoundEngine.TryGetActiveSound(soundSlot2, out sound)) {
					sound.Volume = highestProgress;
					sound.Position = Projectile.position;
				}
				return;
			}
			if (Projectile.owner == Main.myPlayer) player.ChangeDir((Main.MouseWorld.X > player.Center.X).ToDirectionInt());
			Projectile.timeLeft = 2;
			player.SetDummyItemTime(2);
			aims ??= new Aim[Main.maxNPCs];
			decayingAims ??= new Aim[20];
			float maxLengthSQ = Projectile.velocity.LengthSquared();
			if (!player.noItems && !player.CCed) {
				if (--Projectile.ai[0] <= 0) {
					if (Main.myPlayer == Projectile.owner) {
						if (!player.channel) {
							Projectile.ai[2] = 1;
						} else {
							DoShoot(maxLengthSQ);
						}
					}
				}
			} else {
				Projectile.ai[2] = 1;
			}
			Vector2 center = Projectile.Center;
			int activeAims = 0;
			for (int i = 0; i < aims.Length; i++) {
				if (aims[i].active) {
					activeAims++;
					if (aims[i].Update(i, center, Projectile.ai[1], maxLengthSQ)) AddDecayingAim(aims[i]);
					highestProgress = float.Max(highestProgress, aims[i].Progress);
				}
			}
			for (int i = 0; i < decayingAims.Length; i++) {
				decayingAims[i].UpdateDecaying(Projectile.ai[1]);
				highestProgress = float.Max(highestProgress, decayingAims[i].Progress);
			}
			if (SoundEngine.TryGetActiveSound(soundSlot, out sound)) {
				sound.Volume = highestProgress;
				sound.Position = Projectile.position;
			} else if (highestProgress > 0) {
				int projType = Type;
				soundSlot = SoundEngine.PlaySound(SoundID.Pixie.WithPitch(-1), Projectile.Center, soundInstance => soundInstance.Volume > 0 && Projectile.active && Projectile.type == projType);
			}
			if (SoundEngine.TryGetActiveSound(soundSlot2, out sound)) {
				sound.Volume = highestProgress;
				sound.Position = Projectile.position;
			} else if (highestProgress > 0) {
				int projType = Type;
				soundSlot2 = SoundEngine.PlaySound(Origins.Sounds.LightningCharging.WithPitch(-1), Projectile.Center, soundInstance => soundInstance.Volume > 0 && Projectile.active && Projectile.type == projType);
			}
			Triangle hitTri;
			Vector2 perp;
			int totalDamage = 0;
			OriginPlayer originPlayer = player.OriginPlayer();
			foreach (NPC npc in Main.ActiveNPCs) {
				if (InalidTargetNPCs[npc.type] || !npc.CanBeChasedBy(Projectile)) continue;
				Rectangle npcHitbox = npc.Hitbox;
				for (int i = 0; i < aims.Length; i++) {
					if (!aims[i].active) continue;
					Vector2 motion = aims[i].Motion;
					if (motion == Vector2.Zero) continue;
					Vector2 norm = motion.SafeNormalize(Vector2.Zero);
					perp.X = norm.Y;
					perp.Y = -norm.X;
					hitTri = new(center + perp * 16, center - perp * 16, center + motion * Projectile.scale + norm * 16);
					if (hitTri.Intersects(npcHitbox)) {
						NPC.HitModifiers hitModifiers = npc.GetIncomingStrikeModifiers(Projectile.DamageType, 0);
						hitModifiers.Defense.Base = Math.Min(hitModifiers.Defense.Base, 8);
						int damage = hitModifiers.GetDamage(Projectile.damage, false);
						damage = Main.rand.RandomRound(damage * Projectile.ai[1]);
						OriginGlobalNPC globalNPC = npc.GetGlobalNPC<OriginGlobalNPC>();
						globalNPC.shinedownDamage += damage;
						globalNPC.shinedownSpeed = Projectile.ai[1];
						totalDamage += damage;
						if (Projectile.owner == Main.myPlayer && Main.rand.NextFloat() < (Projectile.knockBack * Projectile.ai[1] / Shinedown.FlatKnockbackAdjustment) / 60f) {
							switch (Main.rand.Next(4)) {
								case 0:
								npc.AddBuff(BuffID.CursedInferno, 60);
								break;

								case 1:
								npc.AddBuff(BuffID.Ichor, 60);
								break;

								case 2:
								npc.AddBuff(Rasterized_Debuff.ID, 20);
								break;

								case 3:
								OriginGlobalNPC.InflictTorn(npc, 60, 60, targetSeverity: Shinedown.TornSeverity, source: originPlayer);
								break;
							}
						}
						break;
					}
				}
			}
			if (Main.myPlayer == Projectile.owner) {
				Projectile.localAI[2] += (1 + activeAims * Shinedown.ExtraManaPerEnemyPercent) * player.GetManaCost(player.HeldItem) * Projectile.ai[1] / 60f;
				if (Projectile.localAI[2] > 1) {
					int cost = (int)Projectile.localAI[2];
					Projectile.localAI[2] -= cost;
					if (!player.CheckMana(player.HeldItem, cost, true)) {
						Projectile.ai[2] = 1;
						Projectile.netUpdate = true;
					}
				}
			}
			player.addDPS(Main.rand.RandomRound(totalDamage / 30f));
		}
		SlotId soundSlot;
		SlotId soundSlot2;
		void AddDecayingAim(Aim aim) {
			aim.active = true;
			float bestLength = float.PositiveInfinity;
			int bestDecaying = 0;
			for (int i = 0; i < decayingAims.Length; i++) {
				if (decayingAims[i].active) {
					float length = decayingAims[i].Motion.LengthSquared();
					if (bestLength > length) {
						bestLength = length;
						bestDecaying = i;
					}
				} else {
					bestDecaying = i;
					break;
				}
			}
			decayingAims[bestDecaying] = aim;
		}
		bool DoShoot(float maxLengthSQ) {
			Player player = Main.player[Projectile.owner];
			float bestAngle = 0.5f;
			Vector2 aimOrigin = Projectile.Center;
			Vector2 aimVector = aimOrigin.DirectionTo(Main.MouseWorld);
			List<byte> newAims = null;
			if (Main.netMode != NetmodeID.SinglePlayer) newAims = [];
			foreach (NPC npc in Main.ActiveNPCs) {
				if (aims[npc.whoAmI].active) continue;
				if (InalidTargetNPCs[npc.type] || !npc.CanBeChasedBy(Projectile)) continue;
				Vector2 diff = npc.Center - aimOrigin;
				float lengthSQ = diff.LengthSquared();
				if (lengthSQ > maxLengthSQ) continue;
				diff /= MathF.Sqrt(lengthSQ);
				float angle = Vector2.Dot(diff, aimVector);
				if (angle > bestAngle) {
					aims[npc.whoAmI].Set(npc);
					newAims?.Add((byte)npc.whoAmI);
				}
			}
			if (Main.netMode != NetmodeID.SinglePlayer && newAims.Count > 0) {
				ModPacket packet = Origins.instance.GetPacket();
				packet.Write(Origins.NetMessageType.shinedown_spawn_shadows);
				packet.Write((byte)Projectile.owner);
				packet.Write((ushort)Projectile.identity);
				packet.Write((byte)newAims.Count);
				packet.Write(newAims.ToArray());
				packet.Send();
			}

			Projectile.ai[1] = Shinedown.GetSpeedMultiplier(player, player.HeldItem);
			Projectile.ai[0] = Projectile.ai[1] * 20;
			Projectile.netUpdate = true;
			return true;
		}
		public void RecieveSync(byte[] indices) {
			for (int i = 0; i < indices.Length; i++) {
				byte index = indices[i];
				aims[indices[i]].Set(Main.npc[index]);
			}
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			overPlayers.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 position = Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
			Vector2 origin = texture.Frame().Bottom();
			float spriteLengthFactor = 1f / texture.Height;
			Color color = Color.Black * 0.4f;
			for (int i = 0; i < aims.Length; i++) {
				if (!aims[i].active) continue;
				Vector2 motion = aims[i].Motion;
				Main.EntitySpriteDraw(
					texture,
					position,
					null,
					color,
					motion.ToRotation() + MathHelper.PiOver2,
					origin,
					new Vector2(1f, motion.Length() * spriteLengthFactor),
					SpriteEffects.None
				);
			}
			for (int i = 0; i < decayingAims.Length; i++) {
				if (!decayingAims[i].active) continue;
				Vector2 motion = decayingAims[i].Motion;
				Main.EntitySpriteDraw(
					texture,
					position,
					null,
					color,
					motion.ToRotation() + MathHelper.PiOver2,
					origin,
					new Vector2(1f, motion.Length() * spriteLengthFactor),
					SpriteEffects.None
				);
			}
			return false;
		}
		struct Aim {
			int type;
			Vector2 motion;
			float progress;
			public bool active;
			public readonly float Progress => progress;
			public readonly Vector2 Motion => motion;
			public void Set(NPC target) {
				type = target.type;
				motion = default;
				active = true;
				progress = 0;
			}
			public bool Update(int index, Vector2 position, float speed, float maxLengthSQ) {
				NPC target = Main.npc[index];
				if (!target.active || target.type != type) target = null;
				if (target is null) {
					active = false;
					return true;
				}
				Vector2 diff = target.Center - position;
				if (diff.LengthSquared() > maxLengthSQ) {
					active = false;
					return true;
				}
				MathUtils.LinearSmoothing(ref progress, 1, 1 / 60f);
				speed *= progress + 1;
				MathUtils.LinearSmoothing(ref motion, diff, 4 * speed);
				motion = Utils.rotateTowards(Vector2.Zero, motion, diff, 0.3f * speed);
				return false;
			}
			public void UpdateDecaying(float speed) {
				MathUtils.LinearSmoothing(ref progress, 0, 1 / 60f);
				if (active) {
					speed *= 2 - progress;
					float length = Motion.Length();
					motion *= Math.Max(1 - (1 - 0.99f * ((length - 2) / length)) * speed, 0);
					active = length > 4;
				}
			}
		}
	}
}
