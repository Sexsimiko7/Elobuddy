﻿using System;
using System.Collections.Generic;
using System.Linq;          
using EloBuddy;
using SexsiPrediction.Extensions;
using SharpDX;

namespace SexsiPredictioner.SexsiPrediction.Extensions
{
    /// <summary>
    ///     Class UnitExtensions.
    /// </summary>
    public static class UnitExtensions
    {
        #region Static Fields

        private static readonly AIHeroClient Player = ObjectManager.Player;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Counts the ally heroes in range.
        /// </summary>
        /// <param name="unit">the unit.</param>
        /// <param name="range">The range.</param>
        /// <returns>How many ally heroes are inside a 'float' range from the starting 'unit' GameObject.</returns>
        public static int CountAllyHeroesInRange(this GameObject unit, float range)
        {
            return unit.Position.CountAllyHeroesInRange(range);
        }

        /// <summary>
        ///     Counts the enemy heroes in range.
        /// </summary>
        /// <param name="unit">the unit.</param>
        /// <param name="range">The range.</param>
        /// <param name="dontIncludeStartingUnit">The original unit.</param>
        /// <returns>How many enemy heroes are inside a 'float' range from the starting 'unit' GameObject.</returns>
        public static int CountEnemyHeroesInRange(
            this GameObject unit,
            float range,
            Obj_AI_Base dontIncludeStartingUnit = null)
        {
            return unit.Position.CountEnemyHeroesInRange(range, dontIncludeStartingUnit);
        }

        /// <summary>
        ///     Returns the 3D distance between two vectors.
        /// </summary>
        /// <param name="v1">The start vector.</param>
        /// <param name="v2">The end vector.</param>
        public static float Distance(this Vector3 v1, Vector3 v2)
        {
            return Vector3.Distance(v1, v2);
        }

        /// <summary>
        ///     Returns the 3D distance between a gameobject and a vector.
        /// </summary>
        /// <param name="gameObject">The GameObject.</param>
        /// <param name="v1">The vector.</param>
        public static float Distance(this GameObject gameObject, Vector3 v1)
        {
            return Vector3.Distance(((Obj_AI_Base)gameObject).ServerPosition, v1);
        }

        public static float Distance(this GameObject gameObject, Vector2 v1)
        {
            return Vector2.Distance((Vector2)((Obj_AI_Base)gameObject).ServerPosition, v1);
        }

        /// <summary>
        ///     Returns the 3D distance between two gameobjects.
        /// </summary>
        /// <param name="gameObj">The start GameObject.</param>
        /// <param name="gameObj1">The target GameObject.</param>
        public static float Distance(this GameObject gameObj, GameObject gameObj1)
        {
            return Vector3.Distance(((Obj_AI_Base)gameObj).ServerPosition, gameObj1.Position);
        }

        /// <summary>
        ///     Returns the 3D distance squared between two gameobjects.
        /// </summary>
        /// <param name="gameObj">The start GameObject.</param>
        /// <param name="gameObj1">The target GameObject.</param>
        public static float DistanceSqr(this GameObject gameObj, GameObject gameObj1)
        {
            return Vector3.DistanceSquared(((Obj_AI_Base)gameObj).ServerPosition, gameObj1.Position);
        }

        /// <summary>
        ///     Returns the 3D distance squared between two gameobjects.
        /// </summary>
        /// <param name="gameObj">The start GameObject.</param>
        /// <param name="gameObj1">The target position.</param>
        public static float DistanceSqr(this GameObject gameObj, Vector3 pos)
        {
            return Vector3.DistanceSquared(((Obj_AI_Base)gameObj).ServerPosition, pos);
        }

        /// <summary>
        ///     Returns a determined buff a determined unit has.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="buffName">The buff's stringname</param>
        public static BuffInstance GetBuff(this Obj_AI_Base unit, string buffName)
        {
            return unit.GetBuff(buffName);
        }

        /// <summary>
        ///     Returns how many stacks of the 'buffname' buff the target possesses.
        /// </summary>
        /// <param name="from">The target.</param>
        /// <param name="buffname">The buffname.</param>
        public static int GetBuffCount(this Obj_AI_Base from, string buffname)
        {
            return from.GetBuffCount(buffname);
        }

        /// <summary>
        ///     Gets the full attack range.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns>System.Single.</returns>
        public static float GetFullAttackRange(this Obj_AI_Base source, AttackableUnit target)
        {
            var baseRange = source.AttackRange + source.BoundingRadius;

            if (!target.IsValidTarget())
            {
                return baseRange;
            }

            if (!Player.ChampionName.Equals("Caitlyn"))
            {
                return baseRange + target.BoundingRadius;
            }

            var unit = target as Obj_AI_Base;

            if (unit != null && unit.HasBuff("caitlynyordletrapinternal"))
            {
                baseRange = 1300 + Player.BoundingRadius;
            }

            return baseRange + target.BoundingRadius;
        }

        public static List<Vector2> GetWaypoints(this Obj_AI_Base unit)
        {
            var result = new List<Vector2>();

            if (unit.IsVisible)
            {
                result.Add(unit.ServerPosition.To2D());
                var path = unit.Path;

                if (path.Length <= 0)
                {
                    return result;
                }

                var first = path[0].To2D();
                if (first.DistanceSquared(result[0]) > 40)
                {
                    result.Add(first);
                }

                for (var i = 1; i < path.Length; i++)
                {
                    result.Add(path[i].To2D());
                }
            }
            else if (WaypointTracker.StoredPaths.ContainsKey(unit.NetworkId))
            {
                var path = WaypointTracker.StoredPaths[unit.NetworkId];
                var timePassed = (Environment.TickCount - WaypointTracker.StoredTick[unit.NetworkId]) / 1000f;
                if (path.GetPathLength() >= unit.MoveSpeed * timePassed)
                {
                    result = path.CutPath((unit.MoveSpeed * timePassed));
                }
            }

            return result;
        }

        /// <summary>
        ///     Determines whether the specified target has a determined buff.
        /// </summary>
        /// <param name="from">The target.</param>
        /// <param name="buffname">The buffname.</param>
        /// <returns>
        ///     <c>true</c> if the specified target has the 'buffname' buff; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasBuff(this Obj_AI_Base from, string buffname)
        {
            return from.HasBuff(buffname);
        }

        /// <summary>
        ///     Determines whether the specified unit is affected by a determined bufftype.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="buffType">The buff type.</param>
        /// <returns>
        ///     <c>true</c> if the specified unit is affected by the 'buffType' BuffType; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasBuffOfType(this Obj_AI_Base unit, BuffType buffType)
        {
            return unit.HasBuffOfType(buffType);
        }

        /// <summary>
        ///     Determines whether the specified hero target has a determined item.
        /// </summary>
        /// <param name="from">The target.</param>
        /// <param name="itemId">The item's ID.</param>
        /// <returns>
        ///     <c>true</c> if the specified hero target has the 'itemId' item; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasItem(this AIHeroClient from, uint itemId)
        {
            return from.HasItem(itemId);
        }

        /// <summary>
        ///     Determines whether the specified target has a determined item.
        /// </summary>
        /// <param name="from">The target.</param>
        /// <param name="itemId">The item's ID.</param>
        /// <returns>
        ///     <c>true</c> if the specified target has the 'itemId' item; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasItem(this Obj_AI_Base from, uint itemId)
        {
            return from.HasItem(itemId);
        }

        /// <summary>
        ///     Returns the current health a determined unit has, in percentual.
        /// </summary>
        /// <param name="unit">The unit.</param>
        public static float HealthPercent(this Obj_AI_Base unit)
        {
            return unit.Health / unit.MaxHealth * 100;
        }

        /// <summary>
        ///     Determines whether the specified target is inside a determined range.
        /// </summary>
        /// <param name="unit">The target.</param>
        /// <param name="range">The range.</param>
        /// <returns>
        ///     <c>true</c> if the specified target is inside the specified range; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInRange(this Obj_AI_Base unit, float range)
        {
            if (unit != null)
            {
                return Vector3.Distance(unit.Position, Player.Position) <= range;
            }

            return false;
        }

        /// <summary>
        ///     Determines whether or not the specified unit is recalling.
        /// </summary>
        /// <param name="unit">The unit</param>
        /// <returns>
        ///     <c>true</c> if the specified unit is recalling; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRecalling(this AIHeroClient unit)
        {
            return unit.ValidActiveBuffs().Any(
                buff => buff.Name.ToLower().Contains("recall") && buff.Type == BuffType.Aura);
        }

        /// <summary>
        ///     Determines whether the target is a valid target in the auto attack range from the specified check range from
        ///     vector.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="allyIsValidTarget">if set to <c>true</c> allies will be set as valid targets.</param>
        /// <param name="checkRangeFrom">The check range from.</param>
        /// <returns><c>true</c> if the target is a valid target in the auto attack range; otherwise, <c>false</c>.</returns>
        public static bool IsValidAutoRange(
            this AttackableUnit target,
            bool allyIsValidTarget = false,
            Vector3 checkRangeFrom = default(Vector3))
        {
            if (target == null || !target.IsValid || target.IsDead || target.IsInvulnerable || !target.IsVisible
                || !target.IsTargetable)
            {
                return false;
            }

            if (!allyIsValidTarget && target.Team == Player.Team)
            {
                return false;
            }

            return target.Distance(checkRangeFrom != Vector3.Zero ? checkRangeFrom : Player.Position)
                   < Player.GetFullAttackRange(target);
        }

        /// <summary>
        ///     Determines whether the specified target is a valid target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="range">The range.</param>
        /// <param name="allyIsValidTarget">if set to <c>true</c> allies will be set as valid targets.</param>
        /// <param name="includeBoundingRadius"></param>
        /// <param name="checkRangeFrom">The check range from position.</param>
        /// <returns>
        ///     <c>true</c> if the specified target is a valid target; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidTarget(
            this AttackableUnit target,
            float range = float.MaxValue,
            bool allyIsValidTarget = false,
            bool includeBoundingRadius = true,
            Vector3 checkRangeFrom = default(Vector3))
        {
            if (target == null || !target.IsValid || target.IsDead || target.IsInvulnerable || !target.IsVisible
                || !target.IsTargetable)
            {
                return false;
            }

            if (!allyIsValidTarget && target.Team == Player.Team)
            {
                return false;
            }

            return target.Distance(checkRangeFrom != Vector3.Zero ? checkRangeFrom : Player.Position) < range
                   + (includeBoundingRadius ? Player.BoundingRadius + target.BoundingRadius : 0);
        }

        /// <summary>
        ///     Returns the current mana a determined unit has, in percentual.
        /// </summary>
        /// <param name="unit">The unit.</param>
        public static float ManaPercent(this Obj_AI_Base unit)
        {
            return unit.Mana / unit.MaxMana * 100;
        }

        /// <summary>
        ///     Gets the buffs of the unit which are valid and active
        /// </summary>
        /// <param name="buff">The unit.</param>
        public static BuffInstance[] ValidActiveBuffs(this Obj_AI_Base unit)
        {
            return unit.Buffs.Where(buff => buff.IsValid && buff.IsActive).ToArray();
        }

        #endregion

        /// <summary>
        ///     Waypoint Tracker data container.
        /// </summary>
        internal static class WaypointTracker
        {
            #region Static Fields

            /// <summary>
            ///     Stored Paths.
            /// </summary>
            public static readonly Dictionary<int, List<Vector2>> StoredPaths = new Dictionary<int, List<Vector2>>();

            /// <summary>
            ///     Stored Ticks.
            /// </summary>
            public static readonly Dictionary<int, int> StoredTick = new Dictionary<int, int>();

            #endregion
        }
    }
}
