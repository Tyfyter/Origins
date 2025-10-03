namespace Origins.Tiles.BossDrops {
	public class Defiled_Amalgamation_Trophy : TrophyTileBase { }
	public class World_Cracker_Trophy : TrophyTileBase { }
	public class Fiberglass_Weaver_Trophy : TrophyTileBase { }
	public class Lost_Diver_Trophy : TrophyTileBase { }
	public class Shimmer_Construct_Trophy : TrophyTileBase { }
	public class Trenchmaker_Trophy : TrophyTileBase {
		public override string Texture => base.Texture.Replace("Trenchmaker", "Boss") + "_Empty";
	}
}
