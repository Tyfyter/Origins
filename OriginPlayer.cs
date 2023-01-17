using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Accessories;
using Origins.Items.Accessories.Eyndum_Cores;
using Origins.Items.Armor.Vanity.Terlet.PlagueTexan;
using Origins.Items.Materials;
using Origins.Items.Other.Fish;
using Origins.Items.Other.Testing;
using Origins.Items.Tools;
using Origins.Items.Weapons.Defiled;
using Origins.Items.Weapons.Explosives;
using Origins.Items.Weapons.Summon;
using Origins.Journal;
using Origins.Layers;
using Origins.NPCs;
using Origins.Projectiles;
using Origins.Projectiles.Misc;
using Origins.Questing;
using Origins.UI;
using Origins.Water;
using Origins.World;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using static Origins.Items.OriginGlobalItem;
using static Origins.OriginExtensions;

namespace Origins {
    public class OriginPlayer : ModPlayer {
		#region variables and defaults
		public const float rivenMaxMult = 0.3f;
        public float rivenMult => (1f-rivenMaxMult)+Math.Max((Player.statLife/(float)Player.statLifeMax2)*(rivenMaxMult*2), rivenMaxMult);
        
        #region set bonuses
        public bool fiberglassSet = false;
        public bool cryostenSet = false;
        public bool cryostenHelmet = false;
        public bool felnumSet = false;
        public float felnumShock = 0;
        public float oldFelnumShock = 0;
        public bool celestineSet = false;
        public bool minerSet = false;
        public bool defiledSet = false;
        public bool rivenSet = false;
        public bool riftSet = false;
        public bool eyndumSet = false;
        public bool mimicSet = false;
        public int mimicSetChoices = 0;
        public int setActiveAbility = 0;
        public int setAbilityCooldown = 0;
        #endregion set bonuses

        #region accessories
        public bool bombHandlingDevice = false;
        public bool destructiveClaws = false;
        public bool dimStarlight = false;
        public int dimStarlightCooldown = 0;
        public bool madHand = false;
        public bool fiberglassDagger = false;
        public bool advancedImaging = false;
        public bool rasterize = false;
        public bool decayingScale = false;
        public bool lazyCloakVisible = false;
        public bool amebicVialVisible = false;
        public byte amebicVialCooldown = 0;
        public bool entangledEnergy = false;
        public bool asylumWhistle = false;
        public int asylumWhistleTarget = -1;
        public bool mitosis = false;
        public Item mitosisItem = null;
        public int mitosisCooldown = 0;
        public bool reshapingChunk = false;
        public float mysteriousSprayMult = 1;
        public bool protozoaFood = false;
        public int protozoaFoodCooldown = 0;
        public Item protozoaFoodItem = null;
        public bool symbioteSkull = false;
        public byte parasiticInfluenceCooldown = 0;
        public bool gunGlove = false;
        public Item gunGloveItem = null;
        public int gunGloveCooldown = 0;
        public bool guardedHeart = false;
        public bool razorwire = false;
        public Item razorwireItem = null;
        #endregion

        #region explosive stats
        public float explosiveThrowSpeed = 1;
        public float explosiveSelfDamage = 1;
        #endregion

        #region biomes
        public bool ZoneVoid { get; internal set; } = false;
        public float ZoneVoidProgress = 0;
        public float ZoneVoidProgressSmoothed = 0;

        public float ZoneDefiledProgress = 0;
        public float ZoneDefiledProgressSmoothed = 0;

        public float ZoneRivenProgress = 0;
        public float ZoneRivenProgressSmoothed = 0;

        public bool ZoneBrine { get; internal set; } = false;
        public float ZoneBrineProgress = 0;
        public float ZoneBrineProgressSmoothed = 0;

        public bool ZoneFiberglass { get; internal set; } = false;
        public float ZoneFiberglassProgress = 0;
        public float ZoneFiberglassProgressSmoothed = 0;
        #endregion

        #region buffs
        public int rapidSpawnFrames = 0;
        public int rasterizedTime = 0;
        public bool toxicShock = false;
        public bool tornDebuff = false;
        public bool flaskBile = false;
        public bool flaskSalt = false;
        public int tornTime = 0;
        public int tornTargetTime = 180;
        public float tornTarget = 0.7f;
        #endregion

        #region keybinds
        public bool controlTriggerSetBonus = false;
        public bool releaseTriggerSetBonus = false;
        #endregion

        public float statSharePercent = 0f;

        public bool journalUnlocked = false;
        public Item journalDye = null;

        public bool itemLayerWrench = false;
        public bool plagueSight = false;
        public bool plagueSightLight = false;

        public Ref<Item> eyndumCore = null;

        internal static bool ItemChecking = false;
        public int cryostenLifeRegenCount = 0;
        internal byte oldBonuses = 0;
        public const int minionSubSlotValues = 3;
        public float[] minionSubSlots = new float[minionSubSlotValues];
        public int wormHeadIndex = -1;
        public int heldProjectile = -1;
        public int lastMinionAttackTarget = -1;
        public int hookTarget = -1;
        bool rivenWet = false;
        public HashSet<string> unlockedJournalEntries = new();
        public override void ResetEffects() {
            oldBonuses = 0;
            if(fiberglassSet||fiberglassDagger)oldBonuses|=1;
            if(felnumSet)oldBonuses|=2;
            fiberglassSet = false;
            cryostenSet = false;
            cryostenHelmet = false;
            oldFelnumShock = felnumShock;
            if(!felnumSet) {
                felnumShock = 0;
            } else {
                if(felnumShock > Player.statLifeMax2) {
                    if(Main.rand.NextBool(20)) {
                        Vector2 pos = new Vector2(Main.rand.Next(4, Player.width-4), Main.rand.Next(4, Player.height-4));
                        Projectile proj = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.position + pos, Main.rand.NextVector2CircularEdge(3,3), Felnum_Shock_Leader.ID, (int)(felnumShock*0.1f), 0, Player.whoAmI, pos.X, pos.Y);
                        if(proj.ModProjectile is Felnum_Shock_Leader shock) {
                            shock.Parent = Player;
                            shock.OnStrike += () => felnumShock *= 0.9f;
                        }
                    }
                    felnumShock -= (felnumShock - Player.statLifeMax2) / Player.statLifeMax2 * 5 + 1;
                }
            }
            felnumSet = false;
            celestineSet = false;
            minerSet = false;
            defiledSet = false;
            reshapingChunk = false;
            rivenSet = false;
            riftSet = false;
            eyndumSet = false;
            mimicSet = false;
            setActiveAbility = 0;
            if (setAbilityCooldown > 0) {
                setAbilityCooldown--;
                if (setAbilityCooldown == 0) {
                    SoundEngine.PlaySound(SoundID.MaxMana.WithPitch(-1).WithVolume(0.5f));
                    for (int i = 0; i < 5; i++) {
                        int dust = Dust.NewDust(Player.position, Player.width, Player.height, DustID.PortalBoltTrail, 0f, 0f, 255, Color.Black, (float)Main.rand.Next(20, 26) * 0.1f);
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity *= 0.5f;
                    }
                }
			}
            bombHandlingDevice = false;
            destructiveClaws = false;
            dimStarlight = false;
            madHand = false;
            fiberglassDagger = false;
            advancedImaging = false;
            rasterize = false;
            decayingScale = false;
            lazyCloakVisible = false;
            amebicVialVisible = false;
            mitosis = false;
            mitosisItem = null;
            entangledEnergy = false;
            mysteriousSprayMult = 1;
            protozoaFood = false;
            protozoaFoodItem = null;
            symbioteSkull = false;
            toxicShock = false;
            gunGlove = false;
            gunGloveItem = null;
            razorwire = false;
            razorwireItem = null;

            flaskBile = false;
            flaskSalt = false;
            explosiveThrowSpeed = 1f;
            explosiveSelfDamage = 1f;
            statSharePercent = 0f;
            if (cryostenLifeRegenCount>0)
                cryostenLifeRegenCount--;
            
            if (dimStarlightCooldown>0)
                dimStarlightCooldown--;
            if (amebicVialCooldown > 0)
                amebicVialCooldown--;
            if (protozoaFoodCooldown > 0)
                protozoaFoodCooldown--;
            if (parasiticInfluenceCooldown > 0)
                parasiticInfluenceCooldown--;
            if (gunGloveCooldown > 0)
                gunGloveCooldown--;
            if (mitosisCooldown > 0)
                mitosisCooldown--;

            if (rapidSpawnFrames>0)
                rapidSpawnFrames--;
            if (!tornDebuff && tornTime > 0 && --tornTime <= 0) {
                tornTargetTime = 180;
                tornTarget = 0.7f;
            }
            tornDebuff = false;
            int rasterized = Player.FindBuffIndex(Rasterized_Debuff.ID);
            if (rasterized >= 0) {
                rasterizedTime = Math.Min(Math.Min(rasterizedTime + 1, 8), Player.buffTime[rasterized] - 1);
            }
			if (Player.breath > Player.breathMax) {
                Player.breath = Player.breathMax;
			}
            Player.breathMax = 200;
            plagueSight = false;
            plagueSightLight = false;
            minionSubSlots = new float[minionSubSlotValues];
			if (lastMinionAttackTarget != Player.MinionAttackTargetNPC) {
				if (asylumWhistle) {
                    if (Player.MinionAttackTargetNPC == -1) {
                        Player.MinionAttackTargetNPC = asylumWhistleTarget;
					} else {
                        asylumWhistleTarget = lastMinionAttackTarget;
					}
				}
                lastMinionAttackTarget = Player.MinionAttackTargetNPC;
            }
			if (!asylumWhistle) {
                asylumWhistleTarget = -1;
			} else if(asylumWhistleTarget > -1) {
                NPC possibleTarget = Main.npc[asylumWhistleTarget];
                if (!possibleTarget.CanBeChasedBy() || possibleTarget.Hitbox.Distance(Player.Center) > 3000f) {
                    asylumWhistleTarget = -1;
                } else if(Player.HeldItem.CountsAsClass(DamageClass.Summon)) {
                    Vector2 center = possibleTarget.Center;
                    float count = Player.miscCounter / 60f;
                    float offset = MathHelper.TwoPi / 3f;
                    for (int i = 0; i < 3; i++) {
                        int dust = Dust.NewDust(center, 0, 0, DustID.WitherLightning, 0f, 0f, 100, default(Color), 0.35f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity = Vector2.Zero;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].position = center + (count * MathHelper.TwoPi + offset * i).ToRotationVector2() * 6f;
                    }
                }
            }
            asylumWhistle = false;
        }
		#endregion
		public override void PreUpdateMovement() {
            if (hookTarget >= 0) {//ropeVel.HasValue&&
                Player.fallStart = (int)(Player.position.Y / 16f);
                Projectile projectile = Main.projectile[hookTarget];
				if (projectile.type == Amoeba_Hook_Projectile.ID) {
                    Vector2 diff = Player.Center - projectile.Center;
                    Vector2 normDiff = diff.SafeNormalize(default);
                    float dot = Vector2.Dot(normDiff, Player.velocity.SafeNormalize(default));
                    Player.velocity = Vector2.Lerp(normDiff * -16, Player.velocity, 0.85f + dot * 0.1f);
					if (diff.LengthSquared() > 64) {
                        Player.GoingDownWithGrapple = true;
					}
                    Player.RefreshMovementAbilities();
                }
            }
            //endCustomMovement:
            hookTarget = -1;
        }
		public override void PreUpdate() {
            if (rivenWet) {
                Player.gravity = 0.25f;
            }
        }
		public override void PostUpdate() {
            heldProjectile = -1;
            if (rasterizedTime > 0) {
                Player.velocity = Vector2.Lerp(Player.velocity, Player.oldVelocity, rasterizedTime * 0.06f);
                Player.position = Vector2.Lerp(Player.position, Player.oldPosition, rasterizedTime * 0.06f);
            }
            Player.oldVelocity = Player.velocity;
            rivenWet = false;
            if (Player.wet && !(Player.lavaWet || Player.honeyWet) && LoaderManager.Get<WaterStylesLoader>().Get(Main.waterStyle) is Riven_Water_Style) {
                rivenWet = true;
                int duration = 30;
                int targetTime = 1440;
                float targetSeverity = 0f;
                bool hadTorn = Player.HasBuff(Torn_Buff.ID);
                Player.AddBuff(Torn_Buff.ID, duration);
                if (hadTorn || targetSeverity < tornTarget) {
                    tornTargetTime = targetTime;
                    tornTarget = targetSeverity;
                }
                Player.velocity.X *= 0.975f;
            }
        }
        public override void PostUpdateMiscEffects() {
            if(cryostenHelmet) {
                if (Player.statLife != Player.statLifeMax2 && (int)Main.time % (cryostenLifeRegenCount > 0 ? 5 : 15) == 0) {
                    for (int i = 0; i < 10; i++) {
                        int num6 = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Frost);
                        Main.dust[num6].noGravity = true;
                        Main.dust[num6].velocity *= 0.75f;
                        int num7 = Main.rand.Next(-40, 41);
                        int num8 = Main.rand.Next(-40, 41);
                        Main.dust[num6].position.X += num7;
                        Main.dust[num6].position.Y += num8;
                        Main.dust[num6].velocity.X = -num7 * 0.075f;
                        Main.dust[num6].velocity.Y = -num8 * 0.075f;
                    }
                }
            }
			if (protozoaFood && protozoaFoodCooldown <= 0 && Player.ownedProjectileCounts[Mini_Protozoa_P.ID] < Player.maxMinions) {
                Item item = protozoaFoodItem;
                int damage = Player.GetWeaponDamage(item);
                Projectile.NewProjectileDirect(
                    Player.GetSource_Accessory(item),
                    Player.Center,
                    OriginExtensions.Vec2FromPolar(Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi), Main.rand.NextFloat(1, 8)),
                    Mini_Protozoa_P.ID,
                    damage,
                    Player.GetWeaponKnockback(item),
                    Player.whoAmI
                ).originalDamage = damage;
                protozoaFoodCooldown = item.useTime;
            }
			if (statSharePercent != 0f) {
                foreach (DamageClass damageClass in DamageClasses.All) {
					if (damageClass == DamageClass.Generic) {
                        continue;
					}
                    Player.GetArmorPenetration(DamageClass.Generic) += Player.GetArmorPenetration(damageClass) * statSharePercent;
                    Player.GetArmorPenetration(damageClass) -= Player.GetArmorPenetration(damageClass) * statSharePercent;

                    Player.GetDamage(DamageClass.Generic) = Player.GetDamage(DamageClass.Generic).CombineWith(Player.GetDamage(damageClass).MultiplyBonuses(statSharePercent));
                    Player.GetDamage(damageClass) = Player.GetDamage(damageClass).MultiplyBonuses(1f - statSharePercent);

                    Player.GetAttackSpeed(DamageClass.Generic) += (Player.GetAttackSpeed(damageClass) - 1) * statSharePercent;
                    Player.GetAttackSpeed(damageClass) -= (Player.GetAttackSpeed(damageClass) - 1) * statSharePercent;

                    float crit = Player.GetCritChance(damageClass) * statSharePercent;
                    Player.GetCritChance(DamageClass.Generic) += crit;
                    Player.GetCritChance(damageClass) -= crit;
                }
            }
            if (hookTarget >= 0) {
                Projectile projectile = Main.projectile[hookTarget];
                if (projectile.type == Amoeba_Hook_Projectile.ID) {
                    if (projectile.ai[1] < 5 || Player.controlJump || (Player.Center - projectile.Center).LengthSquared() > 2304) {
                        Player.GoingDownWithGrapple = true;
                    }
                }
            }
        }
        public override void PostUpdateEquips() {
            if (eyndumSet) {
                ApplyEyndumSetBuffs();
                if (eyndumCore?.Value?.ModItem is ModItem equippedCore) {
                    equippedCore.UpdateEquip(Player);
                }
            }
            Player.buffImmune[Rasterized_Debuff.ID] = Player.buffImmune[BuffID.Cursed];
			if (tornDebuff) {
				if (tornTime < tornTargetTime) {
                    tornTime++;
				}
			}
			if (tornTime > 0) {
                Player.statLifeMax2 = (int)(Player.statLifeMax2 * (1 - ((1 - tornTarget) * (tornTime / (float)tornTargetTime))));
				if (Player.statLifeMax2 <= 0) {
                    Player.KillMe(PlayerDeathReason.ByOther(0), 1, 0);
				}
            }
        }
		public override void PostUpdateBuffs() {
            if (Player.whoAmI == Main.myPlayer) {
                foreach (var quest in Quest_Registry.Quests.Values) {
                    if (!quest.SaveToWorld && quest.PreUpdateInventoryEvent is not null) {
                        quest.PreUpdateInventoryEvent();
                    }
                }
            }
        }
		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
            tornTime = 0;
            tornTargetTime = 180;
            tornTarget = 0.7f;
        }
		public override void UpdateVisibleVanityAccessories() {
		}
		public override void ProcessTriggers(TriggersSet triggersSet) {
            releaseTriggerSetBonus = !controlTriggerSetBonus;
            controlTriggerSetBonus = Origins.SetBonusTriggerKey.Current;
			if (controlTriggerSetBonus && releaseTriggerSetBonus) {
                TriggerSetBonus();
			}
        }
        public void ApplyEyndumSetBuffs() {
            #region movement
            float speedMult = (Player.moveSpeed - 1) * 0.5f;
            Player.runAcceleration += (Player.runAcceleration / Player.moveSpeed) * speedMult;
            Player.maxRunSpeed += (Player.maxRunSpeed / Player.moveSpeed) * speedMult;
            Player.extraFall += Player.extraFall / 2;
            Player.wingTimeMax += Player.wingTimeMax / 2;
            Player.jumpSpeedBoost += Player.jumpSpeedBoost * 0.5f;
            if (Player.spikedBoots == 1) Player.spikedBoots = 2;
            #endregion
            #region defense
            Player.statLifeMax2 += (Player.statLifeMax2 - Player.statLifeMax) / 2;
            Player.statDefense += Player.statDefense / 2;
            Player.endurance += Player.endurance * 0.5f;
            Player.lifeRegen += Player.lifeRegen / 2;
            Player.thorns += Player.thorns * 0.5f;
            Player.lavaMax += Player.lavaMax / 2;
            #endregion
            #region damage
			foreach (DamageClass damageClass in DamageClasses.All) {
                Player.GetArmorPenetration(damageClass) += Player.GetArmorPenetration(damageClass) * 0.5f;

                Player.GetDamage(damageClass) = Player.GetDamage(damageClass).MultiplyBonuses(1.5f);

                Player.GetAttackSpeed(damageClass) += (Player.GetAttackSpeed(damageClass) - 1) * 0.5f;
            }

            Player.arrowDamage = Player.arrowDamage.MultiplyBonuses(1.5f);
            Player.bulletDamage = Player.bulletDamage.MultiplyBonuses(1.5f);
            Player.rocketDamage = Player.rocketDamage.MultiplyBonuses(1.5f);

            //explosiveDamage += (explosiveDamage - 1) * 0.5f;
            //explosiveThrowSpeed += (explosiveThrowSpeed - 1) * 0.5f;
            //explosiveSelfDamage += (explosiveSelfDamage - 1) * 0.5f;
            #endregion
            #region resources
            Player.statManaMax2 += (Player.statManaMax2 - Player.statManaMax) / 2;
            Player.manaCost += (Player.manaCost - 1) * 0.5f;
            Player.maxMinions += (Player.maxMinions - 1) / 2;
            Player.maxTurrets += (Player.maxTurrets - 1) / 2;
            Player.manaRegenBonus += Player.manaRegenBonus / 2;
            Player.manaRegenDelayBonus += Player.manaRegenDelayBonus / 2;
            #endregion
            #region utility
            Player.wallSpeed += (Player.wallSpeed - 1) * 0.5f;
            Player.tileSpeed += (Player.tileSpeed - 1) * 0.5f;
            Player.pickSpeed *= (Player.pickSpeed - 1) * 0.5f;
            Player.aggro += Player.aggro / 2;
            Player.blockRange += Player.blockRange / 2;
            #endregion
        }
        public void TriggerSetBonus() {
            if (setAbilityCooldown > 0) return;
			switch (setActiveAbility) {
                case 1: {
                        Vector2 speed = Vector2.Normalize(Main.MouseWorld - Player.MountedCenter) * 14;
                        int type = ModContent.ProjectileType<Infusion_P>();
						for (int i = -5; i < 6; i++) {
                            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.MountedCenter + speed.RotatedBy(MathHelper.PiOver2) * i * 0.25f + speed * (5 - Math.Abs(i)) * 0.75f, speed, type, 40, 7, Player.whoAmI);
                        }
                        setAbilityCooldown = 30;
                        if(Player.manaRegenDelay < 60) Player.manaRegenDelay = 60;
				}
                break;
                case 2: {
                    if (Player.CheckMana((int)(40 * Player.manaCost), true)) {
                        setAbilityCooldown = 1800;
                        Player.AddBuff(Mimic_Buff.ID, 600);
                        Player.AddBuff(BuffID.Heartreach, 30);
                    }
                }
                break;
                case 3:
                break;
                default:
                break;
			}
        }
        public override void UpdateLifeRegen() {
            if(cryostenHelmet)Player.lifeRegenCount+=cryostenLifeRegenCount>0 ? 180 : 1;
        }
		#region attacks
		public override bool? CanAutoReuseItem(Item item) {
            if (destructiveClaws && item.CountsAsClass(DamageClasses.Explosive)) return true;
			return null;
		}
		public override void MeleeEffects(Item item, Rectangle hitbox) {
            if (flaskBile) {
                Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, DustID.BloodWater, newColor:Color.Black);
            } else if (flaskSalt) {
                Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, DustID.GoldFlame, newColor: Color.Lime);
            }
			if (gunGlove && gunGloveCooldown <= 0) {
                if (Player.PickAmmo(gunGloveItem, out int projToShoot, out float speed, out int damage, out float knockback, out int usedAmmoItemId, ItemID.Sets.gunProj[gunGloveItem.type])) {
					if (CombinedHooks.CanShoot(Player, gunGloveItem)) {
                        Vector2 position = Player.itemLocation;
                        Vector2 velocity = Vec2FromPolar(Player.direction == 1 ? Player.itemRotation : Player.itemRotation + MathHelper.Pi, speed);
                            
                        CombinedHooks.ModifyShootStats(Player, gunGloveItem, ref position, ref velocity, ref projToShoot, ref damage, ref knockback);
                        EntitySource_ItemUse_WithAmmo source = (EntitySource_ItemUse_WithAmmo)Player.GetSource_ItemUse_WithPotentialAmmo(gunGloveItem, usedAmmoItemId);
						if (CombinedHooks.Shoot(Player, gunGloveItem, source, position, velocity, projToShoot, damage, knockback)) {
                            Projectile.NewProjectile(source, position, velocity, projToShoot, damage, knockback, Player.whoAmI);
                            SoundEngine.PlaySound(gunGloveItem.UseSound, position);
                        }
					}
                    gunGloveCooldown = CombinedHooks.TotalUseTime(gunGloveItem.useTime, Player, gunGloveItem);
                }
			}
        }
		public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit) {
            if(felnumShock>29) {
                damage+=(int)(felnumShock/15);
                felnumShock = 0;
                SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), target.Center);
            }
        }
        public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            if (advancedImaging) {
                velocity *= 1.38f;
            }
            if (item.CountsAsClass(DamageClasses.Explosive)) {
                if (item.useAmmo == 0 && item.CountsAsClass(DamageClass.Throwing)) {
                    velocity *= explosiveThrowSpeed;
                }
            }
            if (item.shoot > ProjectileID.None && felnumShock > 29) {
                Projectile p = new();
                p.SetDefaults(type);
                OriginGlobalProj.felnumEffectNext = true;
                if (
                    (p.CountsAsClass(DamageClass.Melee) || 
                    p.CountsAsClass(DamageClass.Summon) ||
                    ProjectileID.Sets.IsAWhip[type] ||
                    Origins.DamageModOnHit[type] ||
                    p.aiStyle == ProjAIStyleID.WaterJet) &&
                    !Origins.ForceFelnumShockOnShoot[type]) return;
                damage += (int)(felnumShock / 15);
                felnumShock = 0;
                SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), position);
            }
        }
		public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            if(item.CountsAsClass(DamageClasses.Explosive)) {
                if(riftSet) {
                    Fraction dmg = new Fraction(2, 2);
                    int c = (madHand ? 1 : 0) + (Main.rand.NextBool(2)? 1 : 0);
                    dmg.D+=c;
                    damage *= dmg;
                    double rot = Main.rand.NextBool(2)?-0.1:0.1;
                    Vector2 _position;
                    Vector2 _velocity;
                    int _type;
                    int _damage;
                    float _knockBack;
                    for(int i = c; i-->0;) {
                        _position = position;
                        _velocity = velocity.RotatedBy(rot);
                        _type = type;
                        _damage = damage;
                        _knockBack = knockback;
                        if(ItemLoader.Shoot(item, Player, source, _position, _velocity, _type, _damage, _knockBack)) {
                            Projectile.NewProjectile(source, _position, _velocity, _type, _damage, _knockBack, Player.whoAmI);
                        }
                        rot = -rot;
                    }
                }
            }
            return true;
        }
        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(Origins.DamageModOnHit[proj.type]) {
                bool shouldReplace = Origins.ExplosiveBaseDamage.TryGetValue(proj.type, out int dam);
                float baseDamage = Player.GetTotalDamage(proj.DamageType).ApplyTo(shouldReplace ? dam : damage);
                damage = shouldReplace ? Main.DamageVar(baseDamage) : (int)baseDamage;
            }
            if((proj.CountsAsClass(DamageClass.Melee) || proj.CountsAsClass(DamageClass.Summon) || ProjectileID.Sets.IsAWhip[proj.type]) && felnumShock > 29) {
                damage+=(int)(felnumShock / 15);
                felnumShock = 0;
                SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), proj.Center);
            }
            if(proj.minion&&rivenSet) {
                damage = (int)(damage*rivenMult);
            }
        }
        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit) {
            OnHitNPCGeneral(item, target, damage, knockback, crit);
            if (entangledEnergy && item.ModItem is IElementalItem elementalItem && (elementalItem.Element & Elements.Fiberglass) != 0) {
                Projectile.NewProjectile(
                    Player.GetSource_OnHit(target),
                    target.Center,
                    default,
                    ModContent.ProjectileType<Entangled_Energy_Lifesteal>(),
                    0,
                    0,
                    Player.whoAmI,
                    ai1:damage / 10
                );
            }
			if (item.CountsAsClass(DamageClass.Melee)) {//flasks
                if (flaskBile) {
                    target.AddBuff(Rasterized_Debuff.ID, Rasterized_Debuff.duration * 2);
                }
                if (flaskSalt) {
                    OriginGlobalNPC.InflictTorn(target, 300, 180, 0.8f);
                }
            }
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) {
            OnHitNPCGeneral(proj, target, damage, knockback, crit);
			if (proj.CountsAsClass(DamageClass.Melee) || ProjectileID.Sets.IsAWhip[proj.type]) {//flasks
                if (flaskBile) {
                    target.AddBuff(Rasterized_Debuff.ID, Rasterized_Debuff.duration * 2);
                }
                if (flaskSalt) {
                    OriginGlobalNPC.InflictTorn(target, 300, 180, 0.8f);
                }
            }
        }
        public void OnHitNPCGeneral(Entity entity, NPC target, int damage, float knockback, bool crit) {
            Entity sourceEntity = entity is Projectile ? entity: Player;
            if (crit) {
                if (celestineSet)
                    Item.NewItem(sourceEntity.GetSource_OnHit(target, "SetBonus_Celestine"), target.Hitbox, Main.rand.Next(Origins.celestineBoosters));
                if (dimStarlight && dimStarlightCooldown < 1) {
                    Item.NewItem(sourceEntity.GetSource_OnHit(target, "Accessory"), target.position, target.width, target.height, ItemID.Star);
                    dimStarlightCooldown = 300;
                }
            }
            if (rasterize) {
                target.AddBuff(Rasterized_Debuff.ID, Rasterized_Debuff.duration);
            }
            if (symbioteSkull) {
                OriginGlobalNPC.InflictTorn(target, Main.rand.Next(50, 70), 60, 0.9f);
            }
            if (decayingScale) {
                target.AddBuff(Toxic_Shock_Debuff.ID, Toxic_Shock_Debuff.default_duration);
                target.AddBuff(Solvent_Debuff.ID, 300);
            }
			if (target.life <= 0) {
                foreach (var quest in Quest_Registry.Quests.Values) {
                    if (!quest.SaveToWorld && quest.KillEnemyEvent is not null) {
                        quest.KillEnemyEvent(target);
                    }
                }
            }
        }

        public override void ModifyHitByProjectile(Projectile proj, ref int damage, ref bool crit) {
            if(proj.owner == Player.whoAmI && proj.friendly && proj.CountsAsClass(DamageClasses.Explosive)) {
                /*float damageVal = damage;
                if(minerSet) {
                    explosiveSelfDamage-=0.2f;
                    float inverseDamage = Player.GetDamage(DamageClasses.Explosive).ApplyTo(damage);
                    damageVal -= inverseDamage - damage;
                    //damage = (int)(damage/explosiveDamage);
                    //damage-=damage/5;
                }
                damage = (int)(damageVal * explosiveSelfDamage);*/
            }
        }
        public override void OnHitByNPC(NPC npc, int damage, bool crit) {
            if(!Player.noKnockback && damage!=0) {
                Player.velocity.X *= MeleeCollisionNPCData.knockbackMult;
            }
            MeleeCollisionNPCData.knockbackMult = 1f;
        }
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
            if (entangledEnergy && item.ModItem is IElementalItem elementalItem && (elementalItem.Element & Elements.Fiberglass) != 0) {
                damage.Flat += Player.statDefense / 2;
            }
            damage.Flat *= Origins.FlatDamageMultiplier[item.type];
        }
        /// <param name="target">the potential target</param>
        /// <param name="targetPriorityMultiplier"></param>
        /// <param name="isPriorityTarget">whether or not this npc is a "priority" target (i.e. a manually selected target)</param>
        /// <param name="foundTarget">whether or not a target has already been found</param>
        public delegate void Minion_Selector(NPC target, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget);
        public bool GetMinionTarget(Minion_Selector selector) {
            bool foundTarget = false;
            if (Player.MinionAttackTargetNPC > -1) selector(Main.npc[Player.MinionAttackTargetNPC], 1f, true, ref foundTarget);
            if (asylumWhistleTarget > -1) selector(Main.npc[asylumWhistleTarget], 1f, true, ref foundTarget);
            if (!foundTarget) {
                for (int i = 0; i < Main.maxNPCs; i++) {
                    selector(Main.npc[i], 1f, false, ref foundTarget);
                }
            }
            return foundTarget;
		}
        #endregion
        internal static FieldInfo _sourcePlayerIndex;
        static FieldInfo SourcePlayerIndex => _sourcePlayerIndex ??= typeof(PlayerDeathReason).GetField("_sourcePlayerIndex", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static FieldInfo _sourceProjectileIndex;
        static FieldInfo SourceProjectileIndex => _sourceProjectileIndex ??= typeof(PlayerDeathReason).GetField("_sourceProjectileIndex", BindingFlags.Instance | BindingFlags.NonPublic);
        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter) {
            if(Player.HasBuff(Solvent_Debuff.ID) && Main.rand.Next(9)<3) {
                crit = true;
            }
			if ((int)SourcePlayerIndex.GetValue(damageSource) == Player.whoAmI) {
                Projectile sourceProjectile = Main.projectile[(int)SourceProjectileIndex.GetValue(damageSource)];
                if (sourceProjectile.owner == Player.whoAmI && sourceProjectile.CountsAsClass(DamageClasses.Explosive)) {
                    float damageVal = damage;
                    if (minerSet) {
                        explosiveSelfDamage -= 0.2f;
                        float inverseDamage = Player.GetDamage(DamageClasses.Explosive).ApplyTo(damage);
                        damageVal -= inverseDamage - damage;
						if (explosiveSelfDamage < 0) {
                            explosiveSelfDamage = 0;
                        }
                        if (damageVal < 0) {
                            damageVal = 0;
                        }
                        //damage = (int)(damage/explosiveDamage);
                        //damage-=damage/5;
                    }
                    damage = (int)(damageVal * explosiveSelfDamage);
                }
            }
            if(defiledSet) {
                float manaDamage = damage;
                float costMult = 3;
                float costMult2 = reshapingChunk ? 0.25f : 0.15f;
                float costMult3 = (float)Math.Pow(reshapingChunk ? 0.25f : 0.15f, Player.manaCost);
                if (Player.magicCuffs) {
                    costMult = 1;
                    Player.magicCuffs = false;
                }
                if (Player.statMana < manaDamage*costMult*costMult2) {
                    manaDamage = Player.statMana / (costMult * costMult2);
                }
                if (manaDamage * costMult * costMult2 >= 1f) {
                    Player.ManaEffect((int)-(manaDamage * costMult * costMult2));
                }
                Player.CheckMana((int)Math.Floor(manaDamage * costMult * costMult2), true);
                damage = (int)(damage - (manaDamage * costMult3));
                Player.AddBuff(ModContent.BuffType<Defiled_Exhaustion_Buff>(), 50);
            }else if (reshapingChunk) {
                damage -= damage / 20;
            }
			if (toxicShock) {
                damage += Player.statDefense / 10;
			}
            return damage > 0;
        }
        public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter) {
            if(guardedHeart) {
                Player.AddBuff(Guarded_Heart_Buff.ID, 60 * 8);
            }
			if (razorwire) {
                const float maxDist = 96 * 96;
                const int armorPenetration = 0;
                double totalDamage = damage * 0.67f;
                List<(int id, float weight)> targets = new();
                NPC npc;
                for (int i = 0; i < Main.maxNPCs; i++) {
                    npc = Main.npc[i];
                    if (npc.active && npc.damage > 0 && !npc.friendly) {
                        Vector2 currentPos = npc.Hitbox.ClosestPointInRect(Player.MountedCenter);
                        Vector2 diff = currentPos - Player.MountedCenter;
                        float dist = diff.LengthSquared();
                        if (dist > maxDist) continue;
                        float currentWeight = (1.5f - Vector2.Dot(npc.velocity, diff.SafeNormalize(default))) * (dist / maxDist);
						if (totalDamage / 3 - npc.checkArmorPenetration(armorPenetration) > npc.life) {
                            currentWeight = 0;
						}
						if (targets.Count >= 3) {
                            for (int j = 0; j < 3; j++) {
                                if (targets[j].weight < currentWeight) {
                                    targets.Insert(j, (i, currentWeight));
                                    break;
                                }
                            }
						} else {
                            targets.Add((i, currentWeight));
                        }
                    }
                }
                for (int i = 0; i < 3; i++) {
                    if (i >= targets.Count) break;
                    Vector2 currentPos = Main.npc[targets[i].id].Hitbox.ClosestPointInRect(Player.MountedCenter);
                    Projectile.NewProjectile(
                        Player.GetSource_Accessory(razorwireItem),
                        Player.MountedCenter,
                        (currentPos - Player.MountedCenter).WithMaxLength(12),
                        ProjectileID.JestersArrow,// proper projectile not implemented yet, unless it's not supposed to have a visual effect it'll probably need one
                        (int)totalDamage,
                        10,
                        Player.whoAmI
                    );
                }
            }
        }
        public override void PostSellItem(NPC vendor, Item[] shopInventory, Item item) {
            if (vendor.type == NPCID.Demolitionist && item.type == ModContent.ItemType<Peat_Moss>()) {
                OriginSystem originWorld = ModContent.GetInstance<OriginSystem>();
                if (originWorld.peatSold < 999) {
                    if (item.stack >= 999 - originWorld.peatSold) {
                        item.stack -= 999 - originWorld.peatSold;
                        originWorld.peatSold = 999;
                        int nextSlot = 0;
                        for (; ++nextSlot < shopInventory.Length && !shopInventory[nextSlot].IsAir;);
                        if (nextSlot < shopInventory.Length) shopInventory[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Grenade>());
                        if (nextSlot < shopInventory.Length) shopInventory[nextSlot++].SetDefaults(ModContent.ItemType<Impact_Bomb>());
                        if (nextSlot < shopInventory.Length) shopInventory[nextSlot].SetDefaults(ModContent.ItemType<Impact_Dynamite>());
                    } else {
                        originWorld.peatSold += item.stack;
                        item.TurnToAir();
                    }
                }
            }
        }
        public bool DisplayJournalTooltip(IJournalEntryItem journalItem) {
			if (!journalUnlocked) {
                return true;
			}
            bool unlockedEntry = unlockedJournalEntries.Contains(journalItem.EntryName);
			if (Origins.InspectItemKey.JustPressed) {
                if (!unlockedEntry) unlockedJournalEntries.Add(journalItem.EntryName);
				if (OriginClientConfig.Instance.OpenJournalOnUnlock) {
                    Origins.OpenJournalEntry(journalItem.EntryName);
                }
                return false;
			}
            return !unlockedEntry;
        }
        public static void InflictTorn(Player player, int duration, int targetTime = 180, float targetSeverity = 0.7f) {
            player.AddBuff(Torn_Buff.ID, duration);
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            if (targetSeverity < originPlayer.tornTarget) {
                originPlayer.tornTargetTime = targetTime;
                originPlayer.tornTarget = targetSeverity;
            }
        }

        public override void LoadData(TagCompound tag) {
            if (tag.SafeGet<Item>("EyndumCore") is Item eyndumCoreItem) {
                eyndumCore = new Ref<Item>(eyndumCoreItem);
            }
            if (tag.SafeGet<int>("MimicSetSelection") is int mimicSetSelection) {
                mimicSetChoices = mimicSetSelection;
            }
            if (tag.SafeGet<Item>("JournalDye") is Item journalDyeItem) {
                journalDye = journalDyeItem;
            }
            if (tag.SafeGet<List<string>>("UnlockedJournalEntries") is List<string> journalEntries) {
                unlockedJournalEntries = journalEntries.ToHashSet();
            }
            if (tag.ContainsKey("journalUnlocked")) {
                journalUnlocked = tag.Get<bool>("journalUnlocked");
            }
            questsTag = tag.SafeGet<TagCompound>("Quests");
        }
        TagCompound questsTag;
        public override void OnEnterWorld(Player player) {
            questsTag ??= new TagCompound();
            TagCompound worldQuestsTag = ModContent.GetInstance<OriginSystem>().questsTag ?? new TagCompound();
            foreach (var quest in Quest_Registry.Quests.Values) {
                if (!quest.SaveToWorld) {
                    quest.LoadData(questsTag.SafeGet<TagCompound>(quest.FullName) ?? new TagCompound());
				} else {
                    quest.LoadData(worldQuestsTag.SafeGet<TagCompound>(quest.FullName) ?? new TagCompound());
                }
            }
        }
		public override void SaveData(TagCompound tag) {
            if (eyndumCore is not null) {
                tag.Add("EyndumCore", eyndumCore.Value);
            }
            tag.Add("MimicSetSelection", mimicSetChoices);
            tag.Add("journalUnlocked", journalUnlocked);
            if (journalDye is not null) {
                tag.Add("JournalDye", journalDye);
            }
            if (unlockedJournalEntries is not null) {
                tag.Add("UnlockedJournalEntries", unlockedJournalEntries.ToList());
            }
            TagCompound questsTag = new TagCompound();
            foreach (var quest in Quest_Registry.Quests.Values) {
                if (!quest.SaveToWorld) {
                    TagCompound questTag = new TagCompound();
                    quest.SaveData(questTag);
                    if (questTag.Count > 0) questsTag.Add(quest.FullName, questTag);
                }
            }
            if (questsTag.Count > 0) {
                tag.Add("Quests", questsTag);
			}
        }
		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition) {
            bool zoneDefiled = Player.InModBiome<Defiled_Wastelands>();
            bool zoneRiven = Player.InModBiome<Riven_Hive>();
            bool junk = (itemDrop >= ItemID.OldShoe && itemDrop < ItemID.MinecartTrack);
			if (zoneDefiled && zoneDefiled) {
				if (Main.rand.NextBool()) {
                    zoneDefiled = false;
                } else {
                    zoneRiven = false;
				}
			}
            if (zoneDefiled) {
                if (attempt.crate) {
                    if (attempt.rare && !(attempt.veryrare || attempt.legendary)) {
                        itemDrop = ModContent.ItemType<Chunky_Crate>();
                    }
                } else if (attempt.legendary && Main.hardMode && Main.rand.NextBool(2)) {
                    itemDrop = ModContent.ItemType<Knee_Slapper>();
                } else if (attempt.uncommon && !attempt.rare) {
                    itemDrop = ModContent.ItemType<Prikish>();
                }
            } else if (zoneRiven) {
                if (attempt.crate) {
                    if (attempt.rare && !(attempt.veryrare || attempt.legendary)) {
                        itemDrop = ModContent.ItemType<Crusty_Crate>();
                    }
                } else if (attempt.legendary && Main.hardMode && Main.rand.NextBool(2)) {
                    itemDrop = ModContent.ItemType<Knee_Slapper>();
                } else if (attempt.uncommon && !attempt.rare) {
                    itemDrop = ModContent.ItemType<Prikish>();
                }
                if (Main.rand.NextBool(4)) {
                    if (junk) {
                        itemDrop = ModContent.ItemType<Tire>();
                    }
                }
            }
        }
		public override bool PreItemCheck() {
            ItemChecking = true;
            return true;
        }
        public override void PostItemCheck() {
            ItemChecking = false;
        }
		public override void HideDrawLayers(PlayerDrawSet drawInfo) {
            Item item = drawInfo.heldItem;
            if (
                (
                    drawInfo.drawPlayer.ItemAnimationActive && (
                        (item.useStyle == ItemUseStyleID.Shoot && item.ModItem is ICustomDrawItem) || 
                        (item.useStyle == ItemUseStyleID.Swing && item.ModItem is AnimatedModItem)
                    )
                ) ||
                Origins.isDrawingShadyDupes) PlayerDrawLayers.HeldItem.Hide();
		}
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
            if (plagueSight) drawInfo.colorEyes = IsDevName(Player.name, 1) ? new Color(43, 185, 255) : Color.Gold;
			if (mysteriousSprayMult != 1f) {
                float lightSaturationMult = (float)Math.Pow(mysteriousSprayMult, 2f);
                float saturationMult = 1f - (float)Math.Pow(1f - mysteriousSprayMult, 1.5f);
                drawInfo.colorArmorHead = OriginExtensions.Desaturate(drawInfo.colorArmorHead, lightSaturationMult);
                drawInfo.colorArmorBody = OriginExtensions.Desaturate(drawInfo.colorArmorBody, lightSaturationMult);
                drawInfo.colorArmorLegs = OriginExtensions.Desaturate(drawInfo.colorArmorLegs, lightSaturationMult);
                drawInfo.floatingTubeColor = OriginExtensions.Desaturate(drawInfo.floatingTubeColor, lightSaturationMult);
                drawInfo.itemColor = OriginExtensions.Desaturate(drawInfo.itemColor, lightSaturationMult);

                drawInfo.headGlowColor = OriginExtensions.Desaturate(drawInfo.headGlowColor, saturationMult);
                drawInfo.armGlowColor = OriginExtensions.Desaturate(drawInfo.armGlowColor, saturationMult);
                drawInfo.bodyGlowColor = OriginExtensions.Desaturate(drawInfo.bodyGlowColor, saturationMult);
                drawInfo.legsGlowColor = OriginExtensions.Desaturate(drawInfo.legsGlowColor, saturationMult);

                drawInfo.colorElectricity = OriginExtensions.Desaturate(drawInfo.colorElectricity, saturationMult);
                drawInfo.ArkhalisColor = OriginExtensions.Desaturate(drawInfo.ArkhalisColor, saturationMult);

                drawInfo.colorHair = OriginExtensions.Desaturate(drawInfo.colorHair, saturationMult);
                drawInfo.colorHead = OriginExtensions.Desaturate(drawInfo.colorHead, saturationMult);
                drawInfo.colorEyes = Color.Lerp(drawInfo.colorEyes, Color.White, 1f - saturationMult);
                drawInfo.colorEyeWhites = Color.Lerp(drawInfo.colorEyeWhites, Color.Black, 1f - saturationMult);
                drawInfo.colorBodySkin = OriginExtensions.Desaturate(drawInfo.colorBodySkin, saturationMult);

            }
        }
        public override void FrameEffects() {
            for(int i = 13; i < 18+Player.extraAccessorySlots; i++) {
                if(Player.armor[i].type==Plague_Texan_Sight.ID)Plague_Texan_Sight.ApplyVisuals(Player);
            }
        }
        public void SetMimicSetChoice(int level, int choice) {
            mimicSetChoices = (mimicSetChoices & ~(3 << level * 2)) | ((choice & 3) << level * 2);
        }
        public int GetMimicSetChoice(int level) {
            return (mimicSetChoices >> level * 2) & 3;
        }
    }
}
