namespace Origins.Tiles.BossDrops {
	public class Defiled_Amalgamation_Relic : RelicTileBase { }
	public class World_Cracker_Relic : RelicTileBase { }
	public class Fiberglass_Weaver_Relic : RelicTileBase { }
	public class Lost_Diver_Relic : RelicTileBase { }
	public class Shimmer_Construct_Relic : RelicTileBase { }
	public class Trenchmaker_Relic : RelicTileBase {
		public override string RelicTextureName => base.RelicTextureName.Replace("Trenchmaker_Relic", "Boss_Trophy_Empty");
	}
}
