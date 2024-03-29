﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MoreBlockSwap
{
    public static class SwapValidityUtil
    {
        public static bool IsValidForSameTypeReplacement(int heldTile, int heldStyle, Tile tileToReplace)
        {
            if (heldTile == tileToReplace.TileType)
            {
                TileObjectData replaceData = TileObjectData.GetTileData(tileToReplace);
                int tileToReplaceItemPlaceStyle = BlockSwapUtil.GetItemPlaceStyleFromTile(tileToReplace);
                if (replaceData != null && replaceData.RandomStyleRange > 0)
                {
                    return heldStyle == tileToReplaceItemPlaceStyle; // allows for replacement into a different random style
                }
                return heldStyle != tileToReplaceItemPlaceStyle;
            }

            return false;
        }

        public static bool IsValidForOpenDoorReplacement(int closedDoor, int closedDoorStyle, Tile openDoor)
        {
            int openDoorStyle = BlockSwapUtil.GetItemPlaceStyleFromTile(openDoor);

            if (BlockSwapUtil.IsOpenDoor(openDoor.TileType) && BlockSwapUtil.IsClosedDoor(closedDoor))
            {
                int replacementOpenDoorId = BlockSwapUtil.OpenDoorId(closedDoor);
                TileObjectData replacemenOpenDoorData = TileObjectData.GetTileData(replacementOpenDoorId, 0);
                TileObjectData currentOpenDoorData = TileObjectData.GetTileData(openDoor);

                if (replacemenOpenDoorData == null || currentOpenDoorData == null ||
                    replacemenOpenDoorData.Width != currentOpenDoorData.Width ||
                    replacemenOpenDoorData.Height != currentOpenDoorData.Height)
                {
                    return false;
                }
                return replacementOpenDoorId != openDoor.TileType || closedDoorStyle != openDoorStyle;
            }

            return false;
        }

        public static bool IsValidForStandardReplacement(int type)
        {
            if (Main.tileSolid[type] && !Main.tileSolidTop[type])
            {
                if (!Main.tileRope[type])
                {
                    return !Main.tileFrameImportant[type];
                }

                return true;
            }

            return false;
        }

        public static bool IsValidFramedTileCase(int heldTile, int heldStyle, Tile tileToReplace)
        {
            int replaceTile = tileToReplace.TileType;

            if (heldTile == TileID.PlanterBox && tileToReplace.TileType == TileID.PlanterBox && heldStyle != tileToReplace.TileFrameY / 18)
            {
                return true;
            }

            if (heldTile != replaceTile)
            {
                // Treat planter boxes like platforms
                if ((heldTile == TileID.PlanterBox && (IsValidForStandardReplacement(replaceTile) || TileID.Sets.Platforms[replaceTile])) || 
                    (replaceTile == TileID.PlanterBox && (IsValidForStandardReplacement(heldTile) || TileID.Sets.Platforms[heldTile])))
                {
                    return true;
                }

                // Non solid tiles can swap with other non solid tiles
                return !Main.tileSolid[heldTile] && !Main.tileFrameImportant[heldTile] && !Main.tileSolid[replaceTile] && !Main.tileFrameImportant[replaceTile];
            }

            return false;
        }

        public static bool IsValidForCrossTypeReplacement(int heldTileId, int heldPlaceStyle, int targetX, int targetY, Tile tileToReplace)
        {
            TileObjectData heldData = TileObjectData.GetTileData(heldTileId, heldPlaceStyle);
            TileObjectData replaceData = TileObjectData.GetTileData(tileToReplace);

            if (heldData != null && replaceData != null && // Both multi-tiles
                heldData.Width == replaceData.Width && heldData.Height == replaceData.Height && // Both same size
                heldTileId != tileToReplace.TileType) // Are different types
            {
                Point replaceTopLeft = BlockSwapUtil.TopLeftOfMultiTile(targetX, targetY, tileToReplace);
                bool canPlace = TileObject.CanPlace(replaceTopLeft.X + heldData.Origin.X, replaceTopLeft.Y + heldData.Origin.Y, heldTileId, heldPlaceStyle, 0, out _, onlyCheck: false, checkStay: true);
                
                for (int i = 1; i < heldData.AlternatesCount && canPlace == false; ++i)
                {
                    TileObjectData altData = TileObjectData.GetTileData(heldTileId, heldPlaceStyle, i);
                    canPlace = TileObject.CanPlace(replaceTopLeft.X + altData.Origin.X, replaceTopLeft.Y + altData.Origin.Y, heldTileId, heldPlaceStyle, 0, out _, onlyCheck: false, checkStay: true);
                }

                return canPlace;
            }

            return false;
        }

        // This is used to stop swapping to and from tile entity like tiles
        // This is because they require clean up and set up that simple swapping can't handle
        public static bool IsInvalidTileEntityLikeTile(int heldTile, int heldStyle, int x, int y)
        {
            Tile tileToReplace = Framing.GetTileSafely(x, y);
            TileObjectData replaceData = TileObjectData.GetTileData(tileToReplace);
            TileObjectData heldData = TileObjectData.GetTileData(heldTile, heldStyle);
            Point16 potentialTileEntityPos = new Point16(x, y);

            // Holding a tile entity tile
            if (heldData != null && heldData.HookPostPlaceMyPlayer.hook != null)
            {
                return true;
            }

            if(replaceData != null)
            {
                // Vanilla tile entities/chests/dressers use this
                // Chests and dressers are not tile entities so this is important
                if (replaceData.HookPostPlaceMyPlayer.hook != null)
                {
                    return true;
                }

                potentialTileEntityPos = BlockSwapUtil.TileEntityCoordinates(x, y);
            }

            // Secondary check for actual tile entities
            if (TileEntity.ByPosition.TryGetValue(potentialTileEntityPos, out _))
            {
                return true;
            }

            return false;
        }

        public static bool IsInvalidForReplacement(Tile tileToReplace)
        {
            return tileToReplace.TileType switch
            {
                TileID.DemonAltar => true,
                TileID.GemLocks => tileToReplace.TileFrameY >= 54, // prevents swappping gem locks when full to prevent networking issues
                TileID.Boulder => true, // forces you to break them and deal with consequences :)
                TileID.ChristmasTree => true, // Does strange things with its framing to account for christmas lights
                TileID.ClosedDoor => WorldGen.IsLockedDoor(tileToReplace), // stop swapping with locked temple door
                TileID.Cactus => true, // For some reason cactus is not in Main.tileFrameImportant
                var type when TileID.Sets.BreakableWhenPlacing[type] || Main.tileCut[type] => true, // These tiles are supposed to break so replacement is effectively already achieved
                var type when TileID.Sets.DoesntGetReplacedWithTileReplacement[type] => true, // Vanilla prevents against these. Future Consideration: Swapping with magic ice might be useful
                _ => false,
            };
        }

        // Prevents swapping into a solid tile if there is a player in the way
        public static bool IsInvalidBlockedByPlayers(int x, int y, int heldTile, int heldStyle)
        {
            Tile tile = Framing.GetTileSafely(x, y);
            Point topLeftPos = BlockSwapUtil.TopLeftOfMultiTile(x, y, tile);
            TileObjectData heldData = TileObjectData.GetTileData(heldTile, heldStyle);

            if (Main.tileSolid[heldTile])
            {
                int width = 16;
                int height = 16;

                if (heldData != null)
                {
                    width = heldData.Width * 16;
                    height = heldData.Height * 16;
                }

                Rectangle tileHitBox = new Rectangle(topLeftPos.X * 16, topLeftPos.Y * 16, width, height);
                for (int i = 0; i < 255; i++)
                {
                    Player player = Main.player[i];
                    if (player.active && !player.dead && !player.ghost && player.Hitbox.Intersects(tileHitBox))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
