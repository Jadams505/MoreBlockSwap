using Terraria.ModLoader;

namespace MoreBlockSwap
{
    public class MoreBlockSwap : Mod
    {
        public static MoreBlockSwap Instance => ModContent.GetInstance<MoreBlockSwap>();

        public override void Load()
        {
            Terraria.On_WorldGen.WouldTileReplacementWork += BlockSwapHooks.WorldGen_WouldTileReplacementWork;
            Terraria.On_WorldGen.MoveReplaceTileAnchor += BlockSwapHooks.WorldGen_MoveReplaceTileAnchor;
            Terraria.On_WorldGen.KillTile_GetItemDrops += BlockSwapHooks.WorldGen_KillTile_GetItemDrops;
            Terraria.On_WorldGen.ReplaceTIle_DoActualReplacement += BlockSwapHooks.WorldGen_ReplaceTIle_DoActualReplacement;
            Terraria.On_Player.PlaceThing_ValidTileForReplacement += BlockSwapHooks.Player_PlaceThing_ValidTileForReplacement;
            Terraria.On_WorldGen.ReplaceTile += BlockSwapHooks.WorldGen_ReplaceTile;
            Terraria.On_WorldGen.KillTile_DropItems += BlockSwapHooks.WorldGen_KillTile_DropItems;
            Terraria.IL_Player.PlaceThing_TryReplacingTiles += BlockSwapHooks.IL_Player_PlaceThing_TryReplacingTiles;
        }

        public override void Unload()
        {
            Terraria.On_WorldGen.WouldTileReplacementWork -= BlockSwapHooks.WorldGen_WouldTileReplacementWork;
            Terraria.On_WorldGen.MoveReplaceTileAnchor -= BlockSwapHooks.WorldGen_MoveReplaceTileAnchor;
            Terraria.On_WorldGen.KillTile_GetItemDrops -= BlockSwapHooks.WorldGen_KillTile_GetItemDrops;
            Terraria.On_WorldGen.ReplaceTIle_DoActualReplacement -= BlockSwapHooks.WorldGen_ReplaceTIle_DoActualReplacement;
            Terraria.On_Player.PlaceThing_ValidTileForReplacement -= BlockSwapHooks.Player_PlaceThing_ValidTileForReplacement;
            Terraria.On_WorldGen.ReplaceTile -= BlockSwapHooks.WorldGen_ReplaceTile;
            Terraria.On_WorldGen.KillTile_DropItems -= BlockSwapHooks.WorldGen_KillTile_DropItems;
            Terraria.IL_Player.PlaceThing_ValidTileForReplacement -= BlockSwapHooks.IL_Player_PlaceThing_TryReplacingTiles;
        }
    }
}
