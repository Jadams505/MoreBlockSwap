using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MoreBlockSwap
{
    public static class BlockSwapHooks
    {
        internal static bool WorldGen_ReplaceTile(Terraria.On_WorldGen.orig_ReplaceTile orig, int x, int y, ushort targetType, int targetStyle)
        {
            Tile replaceTile = Framing.GetTileSafely(x, y);
            ushort heldTile = targetType;

            if (BlockSwapUtil.IsConversionCase(replaceTile.TileType, targetType, out int typeOverride, out _))
            {
                ReplacementUtil.ReplaceSingleTile(x, y, (ushort)typeOverride, targetStyle);
                return true;
            }

            if (SwapValidityUtil.IsValidFramedTileCase(heldTile, targetType, replaceTile))
            {
                ReplacementUtil.ReplaceSingleTile(x, y, targetType, targetStyle);
                return true;
            }

            return orig(x, y, targetType, targetStyle);
        }

        internal static void WorldGen_KillTile_DropItems(Terraria.On_WorldGen.orig_KillTile_DropItems orig, int x, int y, Tile tileCache, bool includeLargeObjectDrops, bool includeAllModdedLargeObjectDrops)
        {
            // Only the case when swapping
            if (includeLargeObjectDrops) // || includeAllModdedLargeObjectDrops
            {
                ItemDropUtil.DropItems(x, y, tileCache, null, includeLargeObjectDrops);
                return;
            }
            orig(x, y, tileCache, includeLargeObjectDrops, includeAllModdedLargeObjectDrops);
        }

        // Called only on the client
        internal static bool Player_PlaceThing_ValidTileForReplacement(Terraria.On_Player.orig_PlaceThing_ValidTileForReplacement orig, Player self)
        {
            bool vanillaCall = orig(self);
            return vanillaCall || IsTileValidForMoreBlockSwapReplacement(self, self.HeldItem.createTile, self.HeldItem.placeStyle, Player.tileTargetX, Player.tileTargetY);
        }

        // Called on the client when used in PlaceThing_ValidTileForReplacement
        // Called on the client and server when used in ReplaceTile
        // In the ReplaceTile case it only gets called on the server if true on the client
        internal static bool WorldGen_WouldTileReplacementWork(Terraria.On_WorldGen.orig_WouldTileReplacementWork orig, ushort attemptingToReplaceWith, int x, int y)
        {
            bool vanillaCall = orig(attemptingToReplaceWith, x, y);
            if (vanillaCall)
            {
                return true;
            }

            // This was already true on the client so it is assumed true on the server
            // It's risky but seems to work
            // The alternative is a full rewrite of ReplaceTile because for some stubborn reason it doesn't pass targetType and targetStyle to its method calls
            if (Main.dedServ)
            {
                return true; 
            }

            // Check made by the client so Main.LocalPlayer can be used
            return IsTileValidForMoreBlockSwapReplacement(Main.LocalPlayer, attemptingToReplaceWith, Main.LocalPlayer.HeldItem.placeStyle, x, y);
        }

        private static bool IsTileValidForMoreBlockSwapReplacement(Player player, int heldTile, int placeStyle, int targetX, int targetY)
        {
            Tile tileToReplace = Main.tile[targetX, targetY];
            TileObjectData data = TileObjectData.GetTileData(tileToReplace);

            if (BlockSwapUtil.GetPlaceDataForRubblemaker(player, out int rubblePlaceTile, out int rubblePlaceStyle))
            {
                heldTile = rubblePlaceTile;
                placeStyle = rubblePlaceStyle;
            }

            if (!tileToReplace.HasTile)
            {
                return false;
            }

            if (SwapValidityUtil.IsInvalidForReplacement(tileToReplace))
            {
                return false;
            }

            if (SwapValidityUtil.IsInvalidBlockedByPlayers(targetX, targetY, heldTile, placeStyle))
            {
                return false;
            }

            if (SwapValidityUtil.IsInvalidTileEntityLikeTile(heldTile, placeStyle, targetX, targetY))
            {
                return false;
            }

            if(SwapValidityUtil.IsValidFramedTileCase(heldTile, placeStyle, tileToReplace))
            {
                return true;
            }

            if (BlockSwapUtil.IsConversionCase(tileToReplace.TileType, heldTile, out _, out _))
            {
                return true;
            }

            if (data == null)
            {
                return false;
            }

            if (SwapValidityUtil.IsValidForCrossTypeReplacement(heldTile, placeStyle, targetX, targetY, tileToReplace))
            {
                return true;
            }

            if (SwapValidityUtil.IsValidForOpenDoorReplacement(heldTile, placeStyle, tileToReplace))
            {
                return true;
            }

            return SwapValidityUtil.IsValidForSameTypeReplacement(heldTile, placeStyle, tileToReplace);
        }

        internal static void WorldGen_MoveReplaceTileAnchor(Terraria.On_WorldGen.orig_MoveReplaceTileAnchor orig, ref int x, ref int y, ushort targetType, Tile t)
        {
            if (BlockSwapUtil.ShouldVanillaHandleSwap(targetType, t))
            {
                orig(ref x, ref y, targetType, t);
                return;
            }
            Point topLeftPos = BlockSwapUtil.TopLeftOfMultiTile(x, y, t);
            x = topLeftPos.X;
            y = topLeftPos.Y;
        }

        internal static void WorldGen_KillTile_GetItemDrops(Terraria.On_WorldGen.orig_KillTile_GetItemDrops orig, int x, int y, Tile tileCache, out int dropItem, out int dropItemStack, out int secondaryItem, out int secondaryItemStack, bool includeLargeObjectDrops)
        {
            orig(x, y, tileCache, out dropItem, out dropItemStack, out secondaryItem, out secondaryItemStack, includeLargeObjectDrops);
            
            if (includeLargeObjectDrops) // Only true when called in WorldGen.ReplaceTile
            {
                int targetTileId = tileCache.TileType;

                if (dropItem > 0)
                {
                    return;
                }

                if (BlockSwapUtil.IsOpenDoor(targetTileId))
                {
                    targetTileId = BlockSwapUtil.ClosedDoorId(targetTileId);
                }

                int style = BlockSwapUtil.GetItemPlaceStyleFromTile(tileCache);
                int drop = ItemDropUtil.GetItemDrop(targetTileId, style, x, y);
                if (drop != -1)
                {
                    dropItem = drop;
                }
            }
        }

        internal static void WorldGen_ReplaceTIle_DoActualReplacement(Terraria.On_WorldGen.orig_ReplaceTIle_DoActualReplacement orig, ushort targetType, int targetStyle, int topLeftX, int topLeftY, Tile t)
        {
            if (BlockSwapUtil.ShouldVanillaHandleSwap(targetType, t))
            {
                orig(targetType, targetStyle, topLeftX, topLeftY, t);
                return;
            }

            if(SwapValidityUtil.IsValidForOpenDoorReplacement(targetType, targetStyle, t))
            {
                int openDoor = BlockSwapUtil.OpenDoorId(closedDoor: targetType);
                if(openDoor != -1)
                {
                    targetType = (ushort)openDoor;
                }
            }

            ReplacementUtil.MultiTileSwap(targetType, targetStyle, topLeftX, topLeftY);

            // Manually consume the rubblemaker item because replacement ignores the usual use item code
            if (!Main.dedServ)
            {
                if (BlockSwapUtil.IsRubblemakerTile(targetType))
                    Main.LocalPlayer.PlaceThing_Tiles_PlaceIt_ConsumeFlexibleWandMaterial();
            }
        }

        internal static void IL_Player_PlaceThing_TryReplacingTiles(MonoMod.Cil.ILContext il)
        {
            var c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.After, i => i.MatchCall(typeof(Player), "PlaceThing_Tiles_GetAutoAccessoryCache")))
            {
                MoreBlockSwap.Instance.Logger.Info("Failed to Find IL for PlaceThing_Tiles_GetAutoAccessoryCache");
                return;
            }

            if(!c.TryGotoNext(MoveType.After, i => i.MatchLdfld(typeof(Item), "createTile")))
            {
                MoreBlockSwap.Instance.Logger.Info("Failed to Find IL for createTile");
                return;
            }

            // ldfld System.Int32 Terraria.Item::createTile
            c.EmitDelegate(CustomPlaceTile);
            // stloc.s V_7

            if (!c.TryGotoNext(MoveType.After, i => i.MatchLdfld(typeof(Item), "placeStyle")))
            {
                MoreBlockSwap.Instance.Logger.Info("Failed to Find IL for placeStyle");
                return;
            }

            // ldfld System.Int32 Terraria.Item::placeStyle
            c.EmitDelegate(CustomPlaceStyle);
            // stloc.s V_8
        }

        private static int CustomPlaceStyle(int placeStyle)
        {
            if (BlockSwapUtil.GetPlaceDataForRubblemaker(Main.LocalPlayer, out _, out int rubblePlaceStyle))
                placeStyle = rubblePlaceStyle;

            return placeStyle;
        }

        private static int CustomPlaceTile(int placeTile)
        {
            if (BlockSwapUtil.GetPlaceDataForRubblemaker(Main.LocalPlayer, out int rubblePlaceTile, out _))
                placeTile = rubblePlaceTile;

            return placeTile;
        }
    }
}
