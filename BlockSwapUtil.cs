using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MoreBlockSwap
{
    public static class BlockSwapUtil
    {
        /// <summary>
        /// Used to determine what item should drop from a given tile
        /// </summary>
        /// <param name="tile">The tile in the world that is going to be replaced and must drop an item</param>
        /// <returns>The placeStyle of the item that is going to drop from the tile</returns>
        public static int GetItemPlaceStyleFromTile(Tile tile)
        {
            TileObjectData data = TileObjectData.GetTileData(tile);
            if (data == null)
            {
                return 0;
            }

            int customStyle = GetCustomPlaceStyleFromTile(tile);
            if (customStyle != -1)
            {
                return customStyle;
            }

            int tileObjectStyle = TileObjectData.GetTileStyle(tile);
            int placementStyle = data.CalculatePlacementStyle(tileObjectStyle, 0, 0);

            if (data.RandomStyleRange > 0)
            {
                return placementStyle / data.RandomStyleRange; // normalize random style
            }

            if (data.StyleMultiplier > 1)
            {
                return tileObjectStyle;
            }

            int swl = data.StyleWrapLimit;

            if (swl > 0)
            {
                int startStyle = swl * placementStyle.SafeDivide(swl * data.StyleLineSkip);
                int styleOffset = placementStyle % swl;

                return startStyle + styleOffset;
            }

            int frameX = tile.TileFrameX;
            int frameY = tile.TileFrameY;
            int col = frameX.SafeDivide(data.CoordinateFullWidth);
            int row = frameY.SafeDivide(data.CoordinateFullHeight);

            return data.StyleHorizontal ? col : row;
        }

        public static bool GetPlaceStyleForDoor(Tile tile, out int style)
        {
            TileObjectData data = TileObjectData.GetTileData(tile);
            ModTile mTile = TileLoader.GetTile(tile.TileType);
            int frameX = tile.TileFrameX;
            int frameY = tile.TileFrameY;

            if ((mTile != null && mTile.OpenDoorID != -1) || tile.TileType == TileID.ClosedDoor)
            {
                int row = frameY.SafeDivide(data.CoordinateFullHeight);
                int col = frameX.SafeDivide(data.CoordinateFullWidth * 3);

                style =  row + col * data.StyleWrapLimit;
                return true;
            }
            else if ((mTile != null && mTile.CloseDoorID != -1) || tile.TileType == TileID.OpenDoor)
            {
                int row = frameY.SafeDivide(data.CoordinateFullHeight);
                int col = frameX.SafeDivide(data.CoordinateFullWidth * 2);
                style =  row + col * 36;
                return true;
            }
            style = 0;
            return false;
        }

        public static int GetCustomPlaceStyleFromTile(Tile tile)
        {
            TileObjectData data = TileObjectData.GetTileData(tile);
            int tileObjectStyle = 0, alterante = -1;
            TileObjectData.GetTileInfo(tile, ref tileObjectStyle, ref alterante);
            int calculatedStyle = data.CalculatePlacementStyle(tileObjectStyle, alterante, 0);

            if(GetPlaceStyleForDoor(tile, out int doorStyle))
            {
                return doorStyle;
            }

            int frameX = tile.TileFrameX;
            return tile.TileType switch
            {
                TileID.Saplings => 0, // makes all saplings drop acorns
                TileID.Statues => calculatedStyle % 165,
                TileID.Cannon => Math.Clamp(frameX.SafeDivide(data.CoordinateFullWidth), 0, 3),
                TileID.SillyBalloonTile => 2 * frameX.SafeDivide(data.CoordinateFullWidth * 2), // Purple = 0, Green = 2, Pink = 4
                TileID.Campfire => frameX.SafeDivide(data.CoordinateFullWidth), // Remove with 1.4.4

                TileID.GeyserTrap => 0,
                TileID.BubbleMachine => 0,
                TileID.MushroomStatue => 0,
                TileID.ItemFrame => 0,
                TileID.Fireplace => 0,
                TileID.Chimney => 0,
                TileID.Signs => 0,
                TileID.AnnouncementBox => 0,
                TileID.ArrowSign => 0,
                TileID.PaintedArrowSign => 0,
                TileID.TatteredWoodSign => 0,
                TileID.Lever => 0,

                _ => -1
            };
        }

        public static int GetItemDrop(int targetTileId, int targetStyle)
        {
            int customDrop = GetCustomDrop(targetTileId, targetStyle);
            if(customDrop != -1)
            {
                return customDrop;
            }

            for (int i = 0; i < ItemID.Count; ++i)
            {
                Item item = new Item();
                item.SetDefaults(i);
                if (item.createTile == targetTileId && item.placeStyle == targetStyle)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int GetCustomDrop(int targetTileId, int targetStyle)
        {
            switch (targetTileId)
            {
                case TileID.Sandcastles:
                    return ItemID.SandBlock;
            }
            return -1;
        }

        public static bool ShouldVanillaHandleSwap(int targetType, Tile tileToReplace)
        {
            TileObjectData toReplaceData = TileObjectData.GetTileData(tileToReplace);
            TileObjectData heldTileData = TileObjectData.GetTileData(targetType, 0);
            int replaceType = tileToReplace.TileType;
            return toReplaceData == null || heldTileData == null ||
                TileID.Sets.BasicChest[targetType] || TileID.Sets.BasicChest[replaceType] ||
                TileID.Sets.BasicDresser[targetType] || TileID.Sets.BasicDresser[replaceType] ||
                /*targetType == TileID.Campfire || replaceType == TileID.Campfire ||*/
                TileID.Sets.Platforms[targetType] || TileID.Sets.Platforms[replaceType] ||
                TileID.Sets.Torch[targetType] || TileID.Sets.Torch[replaceType];
        }

        public static Point16 TileEntityCoordinates(int tileCoordX, int tileCoordY, int size = 18, int width = 1, int height = 1)
        {
            Tile tile = Framing.GetTileSafely(tileCoordX, tileCoordY);
            int frameX = tile.TileFrameX;
            int frameY = tile.TileFrameY;
            int posXAdjusted = tileCoordX - frameX.SafeDivide(size).SafeMod(width);
            int posYAdjusted = tileCoordY - frameY.SafeDivide(size).SafeMod(height);

            return new Point16(posXAdjusted, posYAdjusted);
        }

        public static int SafeDivide(this int dividend, int divisor)
        {
            if(divisor == 0)
            {
                return dividend;
            }
            return dividend / divisor;
        }

        public static int SafeMod(this int lhs, int rhs)
        {
            if(rhs == 0)
            {
                return lhs;
            }
            return lhs % rhs;
        }
    }
}
