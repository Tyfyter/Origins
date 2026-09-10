using Fargowiltas.Items.Summons.SwarmSummons;
using Origins.CrossMod.Fargos.NPCs;
using Origins.Items.Other.Consumables;
using Origins.NPCs;
using Origins.NPCs.Ashen.Boss;
using Origins.NPCs.Brine.Boss;
using Origins.NPCs.Defiled.Boss;
using Origins.NPCs.Fiberglass;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.NPCs.Riven.World_Cracker;
using PegasusLib.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Origins.NPCs.Ashen.Boss.Spawn_Trenchmaker_Action;

namespace Origins.CrossMod.Fargos.Items {
	#region Base Classes
	[ExtendsFromMod("Fargowiltas")]
	public abstract class TOSwarmSummon<TNPC, TMaterial> : TOSwarmSummon where TNPC : ModNPC where TMaterial : ModItem {
		public override int SwarmType => ModContent.NPCType<TNPC>();
		public override int NonSwarmItem => ModContent.ItemType<TMaterial>();
	}
	[ExtendsFromMod("Fargowiltas")]
	public abstract class TOSwarmSummon : ModItem {
		public abstract int SwarmType { get; }
		public abstract int SortingPriority { get; }
		public abstract int NonSwarmItem { get; }
		public virtual bool UseHardmodeScaling { get; }
		public virtual LocalizedText SwarmName => NPCLoader.GetNPC(SwarmType).DisplayName;
		public virtual bool ExtraUseConditions(Player player) => true;
		public virtual void ExtraSpawn(NPC npc) { }
		public override LocalizedText Tooltip => Language.GetText("Mods.Origins.CrossMod.Fargos.Items.GenericTooltip.OverloadBoss").WithFormatArgs(SwarmName);

		public override void SetStaticDefaults() {
			this.GetLocalization("SummonText", () => "<PH> Summoned Overloaded Boss Text");
			ItemID.Sets.SortingPriorityBossSpawns[Type] = SortingPriority;
		}
		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = 100;
			Item.value = 10000;
			Item.rare = ItemRarityID.Blue;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.consumable = true;
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(SoundID.Roar, player.Center);
			new TOSwarms_Action(player, Type).Perform();
			return true;
		}
		public override bool CanUseItem(Player player) {
			//return true;
			return !Fargowiltas.Fargowiltas.SwarmActive && !TOEnergizedGlobalNPC.SwarmActive && ExtraUseConditions(player);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(NonSwarmItem)
			.AddIngredient<Overloader>()
			.AddTile(TileID.DemonAltar)
			.Register();
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			bool hasEnergizer = ModCompatSets.EnergizedBossItems[SwarmType].Energizer > 0;
			int count = Math.Min(Item.stack, 10);
			int bags = 5 * count;
			int trophies = (count - (count % 3)) / 3;
			int energizers = hasEnergizer && count == 10 ? 1 : 0;
			string line = Language.GetTextValue("Mods.Fargowiltas.Items.OverloaderRewards", bags, trophies, energizers);
			tooltips.Add(new TooltipLine(Mod, "SwarmSummon", line));
			if (!hasEnergizer)
				tooltips.Add(new(Mod, "NoEnergizer", Language.GetTextValue("Mods.Origins.CrossMod.Fargos.Items.GenericTooltip.OverloadNoEnergizer", SwarmName)));
		}
	}

	[ExtendsFromMod("Fargowiltas")]
	public record class TOSwarms_Action(Player Player, int Type) : SyncedAction {
		public override bool ServerOnly => true;
		public TOSwarms_Action() : this(default, default) { }
		public override SyncedAction NetReceive(BinaryReader reader) => this with {
			Player = Main.player[reader.ReadInt16()],
			Type = reader.ReadInt32()
		};
		public override void NetSend(BinaryWriter writer) {
			writer.Write(Player.whoAmI);
			writer.Write(Type);
		}
		protected override void Perform() {
			if (Player.HeldItem.type == Type && Player.HeldItem.ModItem is TOSwarmSummon SwarmItem) {
				TOEnergizedGlobalNPC.SwarmActive = true;
				Fargowiltas.Fargowiltas.SwarmActive = true;
				int usedItems = Math.Min(Player.HeldItem.stack, 10);

				TOEnergizedGlobalNPC.SwarmItemsUsed = usedItems;
				TOEnergizedGlobalNPC.UseHardmodeScaling = SwarmItem.UseHardmodeScaling;

				NPC boss = NPC.NewNPCDirect(new EntitySource_BossSpawn(Player, OriginsModIntegrations.SwarmContext), Player.Center.RandomPosAround(-1000, 1000, -1000, -400), SwarmItem.SwarmType);
				SwarmItem.ExtraSpawn(boss);

				Player.HeldItem.stack -= usedItems - 1;

				ChatHelper.BroadcastChatMessage(NetworkText.FromKey(SwarmItem.GetLocalizationKey("SummonText")), new Color(175, 75, 255));
			}
		}
	}
	#endregion

	#region Bosses
	[ExtendsFromMod("Fargowiltas")]
	public class OverloadTrenchmaker : TOSwarmSummon<Trenchmaker, SummonTM> {
		public override string Texture => typeof(Distress_Beacon).GetDefaultTMLName();
		public override LocalizedText SwarmName => NPCLoader.GetNPC(SwarmType).GetLocalization($"{nameof(DisplayName)}Generic");
		public override int SortingPriority => 3;
		public override void SetDefaults() {
			base.SetDefaults();
			Item.color = Color.Red with { A = 220 };
		}
		public override void ExtraSpawn(NPC npc) {
			npc.position.Y -= 400;
			(npc.ModNPC as Trenchmaker).SetAIState(StateBossMethods<Trenchmaker>.StateIndex<Spawning_Jets_State>());
		}
	}
	[ExtendsFromMod("Fargowiltas")]
	public class OverloadDefiled : TOSwarmSummon<Defiled_Amalgamation, SummonDA> {
		public override string Texture => typeof(Nerve_Impulse_Manipulator).GetDefaultTMLName();
		public override int SortingPriority => 3;
		public override void SetDefaults() {
			base.SetDefaults();
			Item.color = Color.Red with { A = 220 };
		}
	}
	[ExtendsFromMod("Fargowiltas")]
	public class OverloadCracker : TOSwarmSummon<World_Cracker_Head, SummonWC> {
		public override string Texture => typeof(Sus_Ice_Cream).GetDefaultTMLName();
		public override int SortingPriority => 3;
		public override void SetDefaults() {
			base.SetDefaults();
			Item.color = Color.Red with { A = 220 };
		}
	}
	[ExtendsFromMod("Fargowiltas")]
	public class OverloadFiberglass : TOSwarmSummon<Fiberglass_Weaver, Glass_Webbing> {
		public override string Texture => typeof(Shaped_Glass).GetDefaultTMLName();
		public override int SortingPriority => 5;
		public override void SetDefaults() {
			base.SetDefaults();
			Item.color = Color.Red with { A = 220 };
		}
	}
	[ExtendsFromMod("Fargowiltas")]
	public class OverloadShimmer : TOSwarmSummon<Shimmer_Construct, Aether_Orb> {
		public override string Texture => typeof(Aether_Orb).GetDefaultTMLName();
		public override int SortingPriority => 5;
		public override void SetDefaults() {
			base.SetDefaults();
			Item.color = Color.Red with { A = 220 };
		}
	}
	[ExtendsFromMod("Fargowiltas")]
	public class OverloadDiver : TOSwarmSummon<Lost_Diver, SummonLD> {
		public override string Texture => typeof(Lost_Picture_Frame).GetDefaultTMLName();
		public override bool UseHardmodeScaling => true;
		public override int SortingPriority => 3;
		public override void SetDefaults() {
			base.SetDefaults();
			Item.color = Color.Red with { A = 220 };
		}
	}
	#endregion
}
