using MonoPatcherLib;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Objects.Island;
using Sims3.SimIFace;
using Sims3.SimIFace.RouteDestinations;
using System;

//DESCRIPTION:
/*
 * Fixes IP Houseboats freezing the game/simulation when Sims and Pets deal with entering and exiting.
 * Something with LAND/WATER pathing is freezing the game/simulation when dealing with Houseboats.
 * Side effect of this fix is pathing to Houseboats seems have been fixed as well.
 * Who has thought that two routing options by EA/The Sims Team be the problem? 
 * I believe this wasn't their intent as in LAND to WATER / WATER to LAND / Whatever is causing pathing be so bugged.
*/

//CREDITS:
/*
 * foulplay (Foul Play) - Finding the workaround/fix.
 * phantomsimmer (phantom99) - Very huge helping with a method of debugging the freeze via teleportation of a sim in game.
 * I would have spent very long time finding a method of debugging the freeze without their huge help.
 * thesammy58 (Sam) - Proving my silly thinking wrong on which I thought another Method of code were the issue.
 * unjust_harry ("Just Harry") - Finding the problematic Methods via grep.
 * fleshtexture (shapes) - For his S3MM inspector, I would not have found a static object that allowed unjust_harry ("Just Harry") to find the broken code.
 * lazyduchess (Lazy Duchess) - For their incredible MonoPatcher which allowed me to replace the broken Methods.
 * anime_boom (Anime_Boom) - Very huge help with extensive testing with other mods to make sure it works with other core and script mods.
 * Also testing mod framework folders to see if it works in both Overrides and Packages. I wouldn't have known about this at all. 
 * They also tested with Pets and Mermaids.
 * destrospean (Destrospean) - Helping with code I can use for future projects.
 * EA/The Sims Team - For their spaghetti code.
 */

namespace fp.houseboatfix
{
    [Plugin]
    public class Main
    {
        public Main()
        {
            //Say to MonoPatcher to use our Methods.
            MonoPatcher.PatchAll();
        }

        //Part 1 of the fix. Allow Water Planning to stop the freezing when using Houseboats.
        [ReplaceMethod(typeof(SimRoutingComponent), nameof(SimRoutingComponent.ExitHouseboatThroughBoat))]
        public static bool ExitHouseboatThroughBoat(GameObject routingObject, Route r, Lot houseboatLot, Houseboat houseboat, HouseboatJig houseboatJig)
        {
            Route route = houseboat.CreateGetInBoatRoute(routingObject as Sim);
            if (route == null || !route.PlanResult.Succeeded())
            {
                return false;
            }
            route.InsertPortalSubPathAtIndex(route.GetNumPaths(), houseboat.ObjectId);
            r.SetOption(Route.RouteOption.EnableWaterPlanning, true); //Our fix from false to true.
            Vector3 positionOfSlot = houseboat.GetPositionOfSlot(Slot.RoutingSlot_0);
            r.ReplanFromPoint(positionOfSlot);
            r.InsertRouteSubPathsAtIndex(0u, route);
            r.ResetCurrentPath();
            route.ClearPaths();
            return true;
        }

        //Part 2/Main of the fix. Allow Water Planning and Boat Planning to stop the freezing when using Houseboats and
        //stop them swimming to another Houseboats while they're far away.
        [ReplaceMethod(typeof(SimRoutingComponent), nameof(SimRoutingComponent.EnterHouseboatThroughGangway))]
        public static bool EnterHouseboatThroughGangway(Vector3 startPos, GameObject routingObject, Route r, Lot houseboatLot, Houseboat houseboat, HouseboatJig houseboatJig, bool fromFixup)
        {
            Gangway portGangway = houseboat.GetPortGangway();
            if (portGangway == null)
            {
                return false;
            }
            Route route = routingObject.CreateRoute();
            Vector3 position = houseboat.Position;
            if (Terrain.ArePointsConnectedByLand(startPos, position))
            {
                route.SetOption(Route.RouteOption.EnableWaterPlanning, true); //Our fix from false to true. *THIS IS THE MAIN FIX/WORKAOUND.
                route.SetOption2(Route.RouteOption2.EnablePlanningAsBoat, true); //Fix them swimming to another far away Houseboat.
            }
            Vector3 vector;
            if (fromFixup)
            {
                vector = routingObject.Position;
            }
            else
            {
                vector = portGangway.GetPositionOfSlot(Slot.RoutingSlot_0);
                if (!route.PlanToPointFromPoint(startPos, vector).Succeeded())
                {
                    return false;
                }
                route.CreateSegmentAtDistanceBeforeEndOfRoute(5f);
            }
            Vector3 slotPosition = houseboatJig.GetSlotPosition(Slot.RoutingSlot_2);
            Vector3 outPos;
            World.GetWorldToHouseboatDisplayPosition(houseboatLot.LotId, slotPosition, out outPos);
            Vector3[] points = new Vector3[2] { vector, outPos };
            route.InsertCustomPathAtIndex(route.GetNumPaths(), points, false, true, PathType.SimPath);
            route.InsertPortalSubPathAtIndex(route.GetNumPaths(), houseboatJig.ObjectId);
            r.AddObjectToIgnoreForRoute(houseboatJig.ObjectId);
            r.ReplanFromPoint(slotPosition);
            r.InsertRouteSubPathsAtIndex(0u, route);
            r.ResetCurrentPath();
            route.ClearPaths();
            if (r.PlanResult.Succeeded())
            {
                r.RegisterCallback(portGangway.EnterHouseboatRouteCallback, RouteCallbackType.TriggerWhileTrue, RouteCallbackConditions.SegmentComplete);
                if (fromFixup)
                {
                    portGangway.FixupEnterRoute(r);
                }
            }
            return true;
        }

        //Part 3 of the fix. Allow Water Planning to stop the freezing when using Houseboats.
        [ReplaceMethod(typeof(SimRoutingComponent), nameof(SimRoutingComponent.EnterHouseboatThroughBoat))]
        public static bool EnterHouseboatThroughBoat(Vector3 startPos, GameObject routingObject, Route r, Lot houseboatLot, Houseboat houseboat, HouseboatJig houseboatJig)
        {
            Route route = routingObject.CreateRoute();
            Vector3 positionOfSlot = houseboat.GetPositionOfSlot(Slot.RoutingSlot_0);
            if ((positionOfSlot - routingObject.Position).LengthSqr() > Houseboat.kBoatToHouseboatMaxRadius * Houseboat.kBoatToHouseboatMaxRadius)
            {
                route.SetOption(Route.RouteOption.MakeDynamicObjectAdjustments, true);
                route.SetOption(Route.RouteOption.DoNotEmitDegenerateRoutesForRadialRangeGoals, true);
                route.SetOption(Route.RouteOption.DoLineOfSightCheckUserOverride, false);
                route.SetOption(Route.RouteOption.CheckForFootprintsNearGoals, true);
                route.SetOption2(Route.RouteOption2.EnablePlanningAsBoat, true);
                route.SetOption2(Route.RouteOption2.EndAsBoat, true);
                route.SetOption2(Route.RouteOption2.DestinationMustBeInDeepWater, true);
                route.SetOption2(Route.RouteOption2.DestinationMustBeOnLand, false);
                if (World.ArePointsInSameBodyOfWater(routingObject.Position, positionOfSlot))
                {
                    route.SetOption2(Route.RouteOption2.DisablePlanningOnLand, true);
                }
                RadialRangeDestination radialRangeDestination = new RadialRangeDestination();
                radialRangeDestination.mCenterPoint = positionOfSlot;
                radialRangeDestination.mfMinRadius = Houseboat.kBoatToHouseboatMinRadius;
                radialRangeDestination.mfMaxRadius = Houseboat.kBoatToHouseboatMaxRadius;
                radialRangeDestination.mfPreferredSpacing = 0.15f;
                radialRangeDestination.ScoreFunctionWeights[2] = 1f;
                radialRangeDestination.mConeVector = -houseboat.GetForwardOfSlot(Slot.RoutingSlot_0);
                radialRangeDestination.mfConeAngle = (float)Math.PI / 2f;
                route.AddDestination(radialRangeDestination);
                if (!route.PlanFromPoint(startPos).Succeeded())
                {
                    return false;
                }
            }
            route.InsertPortalSubPathAtIndex(route.GetNumPaths(), houseboat.ObjectId);
            Vector3 positionOfSlot2 = houseboatJig.GetPositionOfSlot(Slot.RoutingSlot_2);
            r.AddObjectToIgnoreForRoute(houseboatJig.ObjectId);
            r.SetOption(Route.RouteOption.EnableWaterPlanning, true); //Our fix from false to true.
            r.SetOption2(Route.RouteOption2.EnablePlanningAsBoat, false);
            r.SetOption2(Route.RouteOption2.DestinationMustBeInDeepWater, false);
            r.SetOption2(Route.RouteOption2.DestinationMustBeOnLand, true);
            r.SetOption2(Route.RouteOption2.EndAsBoat, false);
            r.ReplanFromPoint(positionOfSlot2);
            r.InsertRouteSubPathsAtIndex(0u, route);
            r.ResetCurrentPath();
            route.ClearPaths();
            return true;
        }
    }
}