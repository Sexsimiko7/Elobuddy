using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EloBuddy;
using EloBuddy.SDK;                    
using SexsiPrediction.Skillshots;
using SharpDX;                                   
using Prediction = SexsiPrediction.Skillshots.Prediction;
using GameObjects = SexsiPredictioner.SexsiPrediction.Util.Cache.GameObjects;

namespace SexsiPrediction.Collision
{
    public static class Collision
    {
        #region Constructors and Destructors

        static Collision()
        {
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        #endregion

        #region Properties

        private static int WallCastedT { get; set; }

        private static Vector2 YasuoWallPosition { get; set; }

        #endregion

        #region Public Methods and Operators

        public static List<Obj_AI_Base> GetCollision(List<Vector3> positions, PredictionInput input)
        {
            var result = new List<Obj_AI_Base>();

            foreach (var position in positions)
            {
                if (input.CollisionObjects.HasFlag(CollisionableObjects.Minions))
                {
                    foreach (var minion in GameObjects.EnemyMinions.Where(
                        minion => minion.IsValidTarget(
                            Math.Min(input.Range + input.Radius + 100, 2000),
                            checkRangeFrom: input.RangeCheckFrom)))
                    {
                        input.Unit = minion;
                        var minionPrediction = Prediction.GetPrediction(input, false, false);
                        if (((Vector2)minionPrediction.UnitPosition).DistanceSquared(
                                (Vector2)input.From,
                                (Vector2)position,
                                true) <= Math.Pow(input.Radius + 50 + minion.BoundingRadius, 2))
                        {
                            result.Add(minion);
                        }
                    }
                }

                if (input.CollisionObjects.HasFlag(CollisionableObjects.Heroes))
                {
                    foreach (var hero in GameObjects.EnemyHeroes.Where(
                        hero => hero.IsValidTarget(
                            Math.Min(input.Range + input.Radius + 100, 2000),
                            checkRangeFrom: input.RangeCheckFrom)))
                    {
                        input.Unit = hero;
                        var prediction = Prediction.GetPrediction(input, false, false);
                        if (((Vector2)prediction.UnitPosition).DistanceSquared(
                                (Vector2)input.From,
                                (Vector2)position,
                                false) <= Math.Pow(input.Radius + 50 + hero.BoundingRadius, 2))
                        {
                            result.Add(hero);
                        }
                    }
                }

                if (input.CollisionObjects.HasFlag(CollisionableObjects.Walls))
                {
                }

                if (!input.CollisionObjects.HasFlag(CollisionableObjects.YasuoWall))
                {
                    continue;
                }

                if (Environment.TickCount - WallCastedT > 4000)
                {
                    continue;
                }

                GameObject wall = null;
                foreach (var gameObject in GameObjects.AllGameObjects.Where(
                    x => x.IsValid && Regex.IsMatch(
                             x.Name,
                             "_w_windwall_enemy_0.\\.troy",
                             RegexOptions.IgnoreCase)))
                {
                    wall = gameObject;
                }

                if (wall == null)
                {
                    break;
                }

                var level = wall.Name.Substring(wall.Name.Length - 6, 1);
                var wallWidth = 300 + 50 * Convert.ToInt32(level);

                var wallDirection = ((Vector2)wall.Position - YasuoWallPosition).Normalized().Perpendicular();
                var wallStart = (Vector2)wall.Position + wallWidth / 2f * wallDirection;
                var wallEnd = wallStart - wallWidth * wallDirection;

                if (!wallStart.Intersection(wallEnd, (Vector2)position, (Vector2)input.From).Intersects)
                {
                    continue;
                }

                var t = Environment.TickCount
                        + (wallStart.Intersection(wallEnd, (Vector2)position, (Vector2)input.From).Point
                               .Distance(input.From) / input.Speed + input.Delay) * 1000;

                if (t < WallCastedT + 4000)
                {
                    result.Add(ObjectManager.Player);
                }
            }

            return result.Distinct().ToList();
        }

        #endregion

        #region Methods

        private static void Obj_AI_Hero_OnProcessSpellCast(
            Obj_AI_Base sender,
            GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsValid || sender.Team == ObjectManager.Player.Team
                || args.SData.Name != "YasuoWMovingWall")
            {
                return;
            }

            WallCastedT = Environment.TickCount;
            YasuoWallPosition = (Vector2)sender.ServerPosition;
        }

        #endregion
    }
}
