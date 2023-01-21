using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MoreBlockSwap
{
    public class MoreBlockSwap : Mod
    {
        public override void Load()
        {
            On.Terraria.WorldGen.WouldTileReplacementWork += WorldGen_WouldTileReplacementWork;
            On.Terraria.WorldGen.MoveReplaceTileAnchor += WorldGen_MoveReplaceTileAnchor;
            On.Terraria.WorldGen.KillTile_GetItemDrops += WorldGen_KillTile_GetItemDrops;
            On.Terraria.WorldGen.ReplaceTIle_DoActualReplacement += WorldGen_ReplaceTIle_DoActualReplacement;
            On.Terraria.Player.PlaceThing_ValidTileForReplacement += Player_PlaceThing_ValidTileForReplacement;
        }

        public override void Unload()
        {
            On.Terraria.WorldGen.WouldTileReplacementWork -= WorldGen_WouldTileReplacementWork;
            On.Terraria.WorldGen.MoveReplaceTileAnchor -= WorldGen_MoveReplaceTileAnchor;
            On.Terraria.WorldGen.KillTile_GetItemDrops -= WorldGen_KillTile_GetItemDrops;
            On.Terraria.WorldGen.ReplaceTIle_DoActualReplacement -= WorldGen_ReplaceTIle_DoActualReplacement;
            On.Terraria.Player.PlaceThing_ValidTileForReplacement -= Player_PlaceThing_ValidTileForReplacement;
        }

        private bool WorldGen_WouldTileReplacementWork(On.Terraria.WorldGen.orig_WouldTileReplacementWork orig, ushort attemptingToReplaceWith, int x, int y)
        {
            Tile tile = Framing.GetTileSafely(x, y);
            if(tile.TileType == attemptingToReplaceWith)
            {
                if(tile.TileType == TileID.Sinks)
                {
                    return true;
                }
            }
            return orig(attemptingToReplaceWith, x, y);
        }

        private void WorldGen_MoveReplaceTileAnchor(On.Terraria.WorldGen.orig_MoveReplaceTileAnchor orig, ref int x, ref int y, ushort targetType, Tile t)
        {
            orig(ref x, ref y, targetType, t);
            if (t.TileType == TileID.Sinks)
            {
                x -= t.TileFrameX % 36 / 18;
                y -= t.TileFrameY % 36 / 18;
            }
        }

        private void WorldGen_KillTile_GetItemDrops(On.Terraria.WorldGen.orig_KillTile_GetItemDrops orig, int x, int y, Tile tileCache, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack, bool includeLargeObjectDrops)
        {
            orig(x, y, tileCache, out dropItem, out dropItemStack, out secondaryItem, out secondaryItemStack, includeLargeObjectDrops);
            if (includeLargeObjectDrops)
            {
                if(tileCache.TileType == TileID.Sinks)
                {
                    int style = TileObjectData.GetTileStyle(tileCache);
                    int drop = GetItemDrop(tileCache.TileType, style);
                    if(drop != -1)
                    {
                        dropItem = drop;
                    }
                }
            }
        }

        private void WorldGen_ReplaceTIle_DoActualReplacement(On.Terraria.WorldGen.orig_ReplaceTIle_DoActualReplacement orig, ushort targetType, int targetStyle, int topLeftX, int topLeftY, Tile t)
        {
            if(t.TileType == TileID.Sinks)
            {
                for(int i = 0; i < 2; ++i)
                {
                    for(int j = 0; j < 2; ++j)
                    {
                        Tile tile = Main.tile[topLeftX + i, topLeftY + j];
                        tile.TileType = targetType;
                        tile.TileFrameY = (short)(targetStyle * 38 + j * 18);
                    }
                }

                for (int k = 0; k < 2; k++)
                {
                    for (int l = 0; l < 2; l++)
                    {
                        WorldGen.SquareTileFrame(topLeftX + k, topLeftY + l);
                    }
                }
            }
            else
            {
                orig(targetType, targetStyle, topLeftX, topLeftY, t);
            }
        }

        private bool Player_PlaceThing_ValidTileForReplacement(On.Terraria.Player.orig_PlaceThing_ValidTileForReplacement orig, Player self)
        {
            int heldTile = self.HeldItem.createTile;
            int placeStyle = self.HeldItem.placeStyle;

            Tile tileToReplace = Main.tile[Player.tileTargetX, Player.tileTargetY];

            if (heldTile == tileToReplace.TileType && heldTile == TileID.Sinks)
            {
                int tileToReplaceStyle = TileObjectData.GetTileStyle(tileToReplace);
                return placeStyle != tileToReplaceStyle;
            }
            return orig(self);
        }

        public static int GetItemDrop(int targetTileId, int targetStyle)
        {
            for(int i = 0; i < ItemID.Count; ++i)
            {
                Item item = new Item();
                item.SetDefaults(i);
                if(item.createTile == targetTileId && item.placeStyle == targetStyle)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}