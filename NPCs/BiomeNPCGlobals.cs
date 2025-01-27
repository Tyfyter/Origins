using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.NPCs {
	public class BiomeNPCGlobals : ILoadable {
		public static List<IAssimilationProvider> assimilationProviders = [];
		public static Dictionary<int, Dictionary<IAssimilationProvider, AssimilationAmount>> assimilationDisplayOverrides = [];
		public static float CalcDryadDPSMult() {
			float damageMult = 1f;
			if (NPC.downedBoss1) {
				damageMult += 0.1f;
			}
			if (NPC.downedBoss2) {
				damageMult += 0.1f;
			}
			if (NPC.downedBoss3) {
				damageMult += 0.1f;
			}
			if (NPC.downedQueenBee) {
				damageMult += 0.1f;
			}
			if (Main.hardMode) {
				damageMult += 0.4f;
			}
			if (NPC.downedMechBoss1) {
				damageMult += 0.15f;
			}
			if (NPC.downedMechBoss2) {
				damageMult += 0.15f;
			}
			if (NPC.downedMechBoss3) {
				damageMult += 0.15f;
			}
			if (NPC.downedPlantBoss) {
				damageMult += 0.15f;
			}
			if (NPC.downedGolemBoss) {
				damageMult += 0.15f;
			}
			if (NPC.downedAncientCultist) {
				damageMult += 0.15f;
			}
			if (Main.expertMode) {
				damageMult *= Main.GameModeInfo.TownNPCDamageMultiplier;
			}
			return damageMult;
		}

		public void Load(Mod mod) {
			On_NPCStatsReportInfoElement.ProvideUIElement += On_NPCStatsReportInfoElement_ProvideUIElement;
			bestiaryStatBackground.LoadAsset();
		}
		static AutoLoadingAsset<Texture2D> bestiaryStatBackground = "Origins/UI/Bestiary_Stat_Background";
		private static UIElement On_NPCStatsReportInfoElement_ProvideUIElement(On_NPCStatsReportInfoElement.orig_ProvideUIElement orig, NPCStatsReportInfoElement self, BestiaryUICollectionInfo info) {
			UIElement element = orig(self, info);
			if (info.UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0) {
				int assimilationTypeCount = 0;
				foreach (IAssimilationProvider assimilationProvider in assimilationProviders) {
					if (assimilationProvider is GlobalNPC globalNPC && !ContentSamples.NpcsByNetId[self.NpcId].TryGetGlobalNPC(globalNPC, out _)) continue;
					if (assimilationProvider.AssimilationTexture is null) continue;
					string modName = (assimilationProvider as ModType)?.Mod?.Name ?? "Origins";
					AssimilationAmount assimilation;
					if (!(assimilationDisplayOverrides.TryGetValue(self.NpcId, out var @override) && @override.TryGetValue(assimilationProvider, out assimilation))) {
						assimilation = assimilationProvider.GetAssimilationAmount(ContentSamples.NpcsByNetId[self.NpcId]);
					}
					if (assimilation != default) {
						if (assimilationTypeCount % 2 == 0) {
							element.Height.Pixels += 35;
							bool foundSeparator = false;
							foreach (UIElement child in element.Children) {
								if (foundSeparator || child is UIHorizontalSeparator) {
									foundSeparator = true;
									child.Top.Pixels += 35;
								}
							}
						}
						UIImage uIImage = new((Asset<Texture2D>)bestiaryStatBackground) {
							Top = new StyleDimension(70, 0f),
							Left = new StyleDimension(3 + 99 * (assimilationTypeCount % 2), 0f)
						};
						uIImage.Append(new UIImageFramed(ModContent.Request<Texture2D>(assimilationProvider.AssimilationTexture), assimilationProvider.AssimilationTextureFrame) {
							HAlign = 0f,
							VAlign = 0.5f,
							Left = new StyleDimension(2, 0f),
							Top = new StyleDimension(0, 0f),
							IgnoresMouseInteraction = true
						});
						uIImage.Append(new UIText(assimilation.GetText().ToString()) {
							HAlign = 1f,
							VAlign = 0.5f,
							Left = new StyleDimension(-10, 0f),
							Top = new StyleDimension(0, 0f),
							IgnoresMouseInteraction = true
						});
						uIImage.OnUpdate += (element) => {
							if (element.IsMouseHovering) {
								Main.instance.MouseText(Language.GetTextValue($"Mods.{modName}.Assimilation.{assimilationProvider.AssimilationName}"), 0, 0);
							}
						};
						element.Append(uIImage);
						assimilationTypeCount++;
					}
				}
			}
			return element;
		}
		public void Unload() {

		}
	}
	public interface IAssimilationProvider {
		string AssimilationName { get; }
		string AssimilationTexture { get; }
		Rectangle AssimilationTextureFrame => new(0, 0, 30, 30);
		AssimilationAmount GetAssimilationAmount(NPC npc);
	}
	public record struct AssimilationData(int AssimilationType, AssimilationAmount Amount) {
		public readonly void Inflict(NPC attacker, Player victim) {
			victim.OriginPlayer().InflictAssimilation((ushort)AssimilationType, Amount.GetValue(attacker, victim));
		}
	}
	public readonly struct AssimilationAmount {
		public Func<NPC, Player, float> Function { get; init; }
		public float ClassicAmount { get; init; }
		public float? ExpertAmount { get; init; }
		public float? MasterAmount { get; init; }
		public AssimilationAmount(float classicAmount, float? expertAmount = null, float? masterAmount = null) {
			Function = null;
			ClassicAmount = classicAmount;
			ExpertAmount = expertAmount;
			MasterAmount = masterAmount;
		}
		public AssimilationAmount(Func<NPC, Player, float> function) {
			Function = function;
			ClassicAmount = 0;
			ExpertAmount = null;
			MasterAmount = null;
		}
		public readonly float GetValue(NPC attacker, Player victim) {
			if (Function is not null) {
				return Function(attacker, victim);
			}
			if (Main.masterMode && MasterAmount.HasValue) {
				return MasterAmount.Value;
			}
			if (Main.expertMode && ExpertAmount.HasValue) {
				return ExpertAmount.Value;
			}
			return ClassicAmount;
		}
		public readonly object GetText() {
			if (Function is not null) {
				return Language.GetOrRegister("Mods.Origins.Generic.VariableAssimilation");
			}
			if (Main.masterMode && MasterAmount.HasValue) {
				return $"{MasterAmount.Value:P0}";
			}
			if (Main.expertMode && ExpertAmount.HasValue) {
				return $"{ExpertAmount.Value:P0}";
			}
			return $"{ClassicAmount:P0}";
		}
		public static implicit operator AssimilationAmount(float value) => new(value, value * 1.3f, value * 1.5f);
		public static implicit operator AssimilationAmount((float classic, float expert) value) => new(value.classic, value.expert);
		public static implicit operator AssimilationAmount((float classic, float expert, float master) value) => new(value.classic, value.expert, value.master);
		public static implicit operator AssimilationAmount(Func<NPC, Player, float> function) => new(function);
		public static bool operator ==(AssimilationAmount a, AssimilationAmount b) {
			if (a.Function is not null) {
				return b.Function is not null && a.Function == b.Function;
			}
			return a.ClassicAmount == b.ClassicAmount && a.ExpertAmount == b.ExpertAmount && a.MasterAmount == b.MasterAmount;
		}
		public static bool operator !=(AssimilationAmount a, AssimilationAmount b) => !(a == b);
		public override bool Equals(object obj) => obj is AssimilationAmount other && this == other;
		public override int GetHashCode() => HashCode.Combine(ClassicAmount, ExpertAmount, MasterAmount, Function);
	}

}
