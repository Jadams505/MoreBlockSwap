using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MoreBlockSwap
{
    public static class BlockSwapHooks
    {
        internal static bool Player_PlaceThing_ValidTileForReplacement(On.Terraria.Player.orig_PlaceThing_ValidTileForReplacement orig, Player self)
        {
            bool vanillaCall = orig(self);
            return vanillaCall || IsTileValidForMoreBlockSwapReplacement(self.HeldItem.createTile, self.HeldItem.placeStyle, Player.tileTargetX, Player.tileTargetY);
        }

        internal static bool WorldGen_WouldTileReplacementWork(On.Terraria.WorldGen.orig_WouldTileReplacementWork orig, ushort attemptingToReplaceWith, int x, int y)
        {
            bool vanillaCall = orig(attemptingToReplaceWith, x, y);
            return vanillaCall || IsTileValidForMoreBlockSwapReplacement(attemptingToReplaceWith, Main.LocalPlayer.HeldItem.placeStyle, x, y);
        }

        private static bool IsTileValidForMoreBlockSwapReplacement(int heldTile, int placeStyle, int targetX, int targetY)
        {
            Tile tileToReplace = Main.tile[targetX, targetY];
            TileObjectData data = TileObjectData.GetTileData(tileToReplace);

            if (data == null)
            {
                return false;
            }

            Point16 potentialTileEntityPos = BlockSwapUtil.TileEntityCoordinates(targetX, targetY, data.CoordinateWidth + data.CoordinatePadding, data.Width, data.Height);
            if (TileEntity.ByPosition.TryGetValue(potentialTileEntityPos, out _))
            {
                return false;
            }

            if (IsValidForCrossTypeReplacement(heldTile, placeStyle, targetX, targetY, tileToReplace))
            {
                return true;
            }

            if (tileToReplace.TileType == TileID.GemLocks && tileToReplace.TileFrameY >= 54)
            {
                return false; // prevents swappping gem locks when full to prevent networking issues
            }

            if (IsValidForReplacementCustom(heldTile, placeStyle, tileToReplace))
            {
                return true;
            }

            if (heldTile == tileToReplace.TileType && heldTile < TileID.Count)
            {
                int tileToReplaceItemPlaceStyle = BlockSwapUtil.GetItemPlaceStyleFromTile(tileToReplace);
                if (data.RandomStyleRange > 0)
                {
                    return placeStyle == tileToReplaceItemPlaceStyle; // allows for replacement into a different random style
                }
                return placeStyle != tileToReplaceItemPlaceStyle;
            }
            return false;
        }

        private static bool IsValidForReplacementCustom(int heldTileId, int heldPlaceStyle, Tile tileToReplace)
        {
            int tileToReplacePlaceStyle = BlockSwapUtil.GetItemPlaceStyleFromTile(tileToReplace);
            TileObjectData tileToReplaceData = TileObjectData.GetTileData(tileToReplace);
            int tileToReplaceRealStyle = TileObjectData.GetTileStyle(tileToReplace);
            int closeDoorId = TileLoader.CloseDoorID(tileToReplace);

            if (closeDoorId != -1)
            {
                if (heldTileId == closeDoorId)
                {
                    if (tileToReplacePlaceStyle != -1)
                    {
                        return heldPlaceStyle != tileToReplacePlaceStyle;
                    }
                }
            }

            if (heldTileId == TileID.Saplings && tileToReplace.TileType == TileID.Saplings)
            {
                return true;
            }

            if (heldTileId == TileID.GemSaplings && tileToReplace.TileType == TileID.GemSaplings)
            {
                return true;
            }

            return false;
        }

        private static bool IsValidForCrossTypeReplacement(int heldTileId, int heldPlaceStyle, int targetX, int targetY, Tile tileToReplace)
        {
            TileObjectData heldData = TileObjectData.GetTileData(heldTileId, heldPlaceStyle);
            TileObjectData replaceData = TileObjectData.GetTileData(tileToReplace);

            if (heldData != null && replaceData != null &&
                heldData.Width == replaceData.Width && heldData.Height == replaceData.Height &&
                heldTileId != tileToReplace.TileType)
            {
                Point replaceTopLeft = TopLeftOfMultiTile(targetX, targetY, tileToReplace);
                bool canPlace = TileObject.CanPlace(replaceTopLeft.X + heldData.Origin.X, replaceTopLeft.Y + heldData.Origin.Y, heldTileId, heldPlaceStyle, 0, out _, onlyCheck: false, checkStay: true);

                return canPlace;
            }

            return false;
        }

        internal static void WorldGen_MoveReplaceTileAnchor(On.Terraria.WorldGen.orig_MoveReplaceTileAnchor orig, ref int x, ref int y, ushort targetType, Tile t)
        {
            if (BlockSwapUtil.ShouldVanillaHandleSwap(targetType, t))
            {
                orig(ref x, ref y, targetType, t);
                return;
            }
            Point topLeftPos = TopLeftOfMultiTile(x, y, t);
            x = topLeftPos.X;
            y = topLeftPos.Y;
        }

        private static Point TopLeftOfMultiTile(int tilePosX, int tilePosY, Tile tile)
        {
            TileObjectData data = TileObjectData.GetTileData(tile);
            if (data != null)
            {
                int frameX = tile.TileFrameX;
                int frameY = tile.TileFrameY;

                int xAdjustment = frameX.SafeMod(data.CoordinateFullWidth).SafeDivide(data.CoordinateWidth + data.CoordinatePadding);
                int yAdjustment = frameY.SafeMod(data.CoordinateFullHeight).SafeDivide(data.CoordinateHeights[0] + data.CoordinatePadding);
                tilePosX -= xAdjustment;
                tilePosY -= yAdjustment;
            }
            return new Point(tilePosX, tilePosY);
        }

        internal static void WorldGen_KillTile_GetItemDrops(On.Terraria.WorldGen.orig_KillTile_GetItemDrops orig, int x, int y, Tile tileCache, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack, bool includeLargeObjectDrops)
        {
            orig(x, y, tileCache, out dropItem, out dropItemStack, out secondaryItem, out secondaryItemStack, includeLargeObjectDrops);
            if (dropItem > 0)
            {
                return;
            }
            if (includeLargeObjectDrops) // Only true when called in WorldGen.ReplaceTile
            {
                TileObjectData data = TileObjectData.GetTileData(tileCache);
                if (data != null)
                {
                    int style = BlockSwapUtil.GetItemPlaceStyleFromTile(tileCache);
                    int targetTileId = tileCache.TileType;

                    if(targetTileId == TileID.OpenDoor)
                    {
                        targetTileId = TileID.ClosedDoor;
                    }

                    int drop = BlockSwapUtil.GetItemDrop(targetTileId, style);
                    if (drop != -1)
                    {
                        dropItem = drop;
                    }
                }
            }
        }

        internal static void WorldGen_ReplaceTIle_DoActualReplacement(On.Terraria.WorldGen.orig_ReplaceTIle_DoActualReplacement orig, ushort targetType, int targetStyle, int topLeftX, int topLeftY, Tile t)
        {
            if (BlockSwapUtil.ShouldVanillaHandleSwap(targetType, t))
            {
                orig(targetType, targetStyle, topLeftX, topLeftY, t);
                return;
            }

            Tile topLeftTile = Main.tile[topLeftX, topLeftY];
            TileObjectData heldData = TileObjectData.GetTileData(targetType, targetStyle);
            TileObjectData replaceData = TileObjectData.GetTileData(topLeftTile);
            Point newTopLeftFrame = DetermineNewTileStart(targetType, targetStyle, topLeftX, topLeftY);

            if (topLeftTile.TileType == TileID.OpenDoor && targetType == TileID.ClosedDoor)
            {
                targetType = TileID.OpenDoor;
            }

            int newFrameX = newTopLeftFrame.X;
            for (int i = 0; i < replaceData.Width; ++i)
            {
                int newFrameY = newTopLeftFrame.Y;
                for (int j = 0; j < replaceData.Height; ++j)
                {
                    Tile tile = Framing.GetTileSafely(topLeftX + i, topLeftY + j);
                    tile.TileType = targetType;
                    tile.TileFrameX = (short)newFrameX;
                    tile.TileFrameY = (short)newFrameY;
                    tile.Clear(TileDataType.TilePaint);
                    newFrameY += heldData.CoordinateHeights[j] + heldData.CoordinatePadding;
                }
                newFrameX += heldData.CoordinateWidth + heldData.CoordinatePadding;
            }

            for (int i = 0; i < replaceData.Width; ++i)
            {
                for (int j = 0; j < replaceData.Height; ++j)
                {
                    WorldGen.SquareTileFrame(topLeftX + i, topLeftY + j);
                }
            }
        }

        private static Point DetermineNewTileStart(ushort targetType, int targetStyle, int topLeftX, int topLeftY)
        {
            Tile topLeftTile = Main.tile[topLeftX, topLeftY];
            TileObjectData heldData = TileObjectData.GetTileData(targetType, targetStyle);

            if(GetCustomTileStart(targetType, targetStyle, topLeftTile) is Point customFrame)
            {
                return customFrame;
            }

            Point newTopLeftFrame = new Point(topLeftTile.TileFrameX, topLeftTile.TileFrameY);
            if (heldData != null)
            {
                int xOffset = heldData != null ? heldData.Origin.X : 0;
                int yOffset = heldData != null ? heldData.Origin.Y : 0;
                bool canPlace = TileObject.CanPlace(topLeftX + xOffset, topLeftY + yOffset, targetType, targetStyle, 0, out TileObject placeData, onlyCheck: false, checkStay: true);
                
                int col = heldData.CalculatePlacementStyle(targetStyle, placeData.alternate, placeData.random);
                int row = 0;

                if (heldData.StyleWrapLimit > 0)
                {
                    row = col.SafeDivide(heldData.StyleWrapLimit) * heldData.StyleLineSkip;
                    col %= heldData.StyleWrapLimit;
                }

                if (heldData.StyleHorizontal)
                {
                    newTopLeftFrame.X = heldData.CoordinateFullWidth * col;
                    newTopLeftFrame.Y = heldData.CoordinateFullHeight * row;
                }
                else
                {
                    newTopLeftFrame.X = heldData.CoordinateFullWidth * row;
                    newTopLeftFrame.Y = heldData.CoordinateFullHeight * col;
                }
            }

            return newTopLeftFrame;
        }

        private static Point? GetCustomTileStart(ushort targetType, int targetStyle, Tile topLeftTile)
        {
            if (targetType == TileID.ClosedDoor && topLeftTile.TileType == TileID.OpenDoor)
            {
                if (BlockSwapUtil.GetPlaceStyleForDoor(topLeftTile, out int doorStyle))
                {
                    int normalizedReplaceStyle = doorStyle % 36;
                    int normalizedTargetStyle = targetStyle % 36;
                    int styleDiff = normalizedTargetStyle - normalizedReplaceStyle;

                    int replaceCol = doorStyle / 36;
                    int targetCol = targetStyle / 36;
                    int colDiff = targetCol - replaceCol;

                    int newTopLeftX = colDiff * 72 + topLeftTile.TileFrameX;
                    int newTopLeftY = styleDiff * 54 + topLeftTile.TileFrameY;

                    return new Point(newTopLeftX, newTopLeftY);
                }
            }
            return null;
        }
    }
}
