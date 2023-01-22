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
            Tile tileToReplace = Framing.GetTileSafely(x, y);
            if(tileToReplace.TileType == attemptingToReplaceWith && attemptingToReplaceWith < TileID.Count)
            {
                TileObjectData data = TileObjectData.GetTileData(tileToReplace);
                if(data != null)
                {
                    return true;
                }
            }
            return orig(attemptingToReplaceWith, x, y);
        }

        private void WorldGen_MoveReplaceTileAnchor(On.Terraria.WorldGen.orig_MoveReplaceTileAnchor orig, ref int x, ref int y, ushort targetType, Tile t)
        {
            TileObjectData data = TileObjectData.GetTileData(t);
            if(data != null)
            {
                int xAdjustment = t.TileFrameX % data.CoordinateFullWidth / (data.CoordinateWidth + data.CoordinatePadding);
                int yAdjustment = t.TileFrameY % data.CoordinateFullHeight / (data.CoordinateHeights[0] + data.CoordinatePadding);
                x -= xAdjustment;
                y -= yAdjustment;
                return;
            }
            orig(ref x, ref y, targetType, t);
        }

        private void WorldGen_KillTile_GetItemDrops(On.Terraria.WorldGen.orig_KillTile_GetItemDrops orig, int x, int y, Tile tileCache, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack, bool includeLargeObjectDrops)
        {
            orig(x, y, tileCache, out dropItem, out dropItemStack, out secondaryItem, out secondaryItemStack, includeLargeObjectDrops);
            if (includeLargeObjectDrops)
            {
                TileObjectData data = TileObjectData.GetTileData(tileCache);
                if (data != null)
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
            TileObjectData data = TileObjectData.GetTileData(t);
            if(data != null)
            {
                bool canPlace = TileObject.CanPlace(topLeftX, topLeftY, targetType, targetStyle, 0, out TileObject placeData, onlyCheck: false, checkStay: true);

                int newTopLeftX;
                int newTopLeftY;
                int style = data.CalculatePlacementStyle(targetStyle, 0, placeData.random);
                //style -= data.Style;
                int adjustedStyle = 0;

                if(data.StyleWrapLimit > 0)
                {
                    adjustedStyle = style / data.StyleWrapLimit * data.StyleLineSkip;
                    style %= data.StyleWrapLimit;
                }

                if (data.StyleHorizontal)
                {
                    newTopLeftX = data.CoordinateFullWidth * style;
                    newTopLeftY = data.CoordinateFullHeight * adjustedStyle;
                }
                else
                {
                    newTopLeftX = data.CoordinateFullWidth * adjustedStyle;
                    newTopLeftY = data.CoordinateFullHeight * style;
                }

                Tile topLeftTile = Framing.GetTileSafely(topLeftX, topLeftY);
                int deltaX = newTopLeftX - topLeftTile.TileFrameX;
                int deltaY = newTopLeftY - topLeftTile.TileFrameY;

                for(int i = 0; i < data.Width; ++i)
                {
                    for(int j = 0; j < data.Height; ++j)
                    {
                        Tile tile = Framing.GetTileSafely(topLeftX + i, topLeftY + j);
                        tile.TileFrameX += (short)deltaX;
                        tile.TileFrameY += (short)deltaY;
                    }
                }

                for (int i = 0; i < data.Width; ++i)
                {
                    for (int j = 0; j < data.Height; ++j)
                    {
                        WorldGen.SquareTileFrame(topLeftX + i, topLeftY + j);
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

            if (heldTile == tileToReplace.TileType && heldTile < TileID.Count)
            {
                int tileToReplaceStyle = TileObjectData.GetTileStyle(tileToReplace);
                return tileToReplaceStyle != -1 && placeStyle != tileToReplaceStyle;
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