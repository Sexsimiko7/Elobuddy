using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Spells;
using SexsiPrediction.Collision;
using SexsiPrediction.Skillshots;
using HitChance = SexsiPrediction.Skillshots.HitChance;
using Prediction = EloBuddy.SDK.Prediction;


namespace SexsiPrediction
{
    class Program
    {
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Core.DelayAction(Menu, 5000);
            
        }

        private static void Menu()
        {
            foreach (KeyValuePair<string, List<Menu>> keyValuePair in MainMenu.MenuInstances)
            {
                var predmenu = keyValuePair.Value.FirstOrDefault(x => x.UniqueMenuId == "Prediction");
                if (predmenu != null)
                {
                    if (!predmenu["PredictionSelected"].IsVisible)
                    {
                        predmenu["PredictionSelected"].IsVisible = true;
                    }
                }
            }

            Prediction.Manager.Suscribe("Sexsi Prediction", GetSexsiPrediction);
        }

        private static Prediction.Manager.PredictionOutput GetSexsiPrediction(Prediction.Manager.PredictionInput input)
        {
            if (input == null || input.Target == null)
            {
                throw new ArgumentNullException("input");
            }
            var sexsiInput = new PredictionInput
            {
               CollisionObjects = CollisionTypesConverter(input.CollisionTypes),
               Delay = input.Delay,
               Radius = input.Radius,
               Speed = input.Speed,
               Range = input.Range,
               Target = input.Target,
               Unit = Player.Instance,
               SkillType = TypeConverter(input.Type)
            };
            var sexsiOutput = Skillshots.Prediction.Implementation.GetPrediction(sexsiInput);
            var output = new Prediction.Manager.PredictionOutput(input)
            {
                CastPosition = sexsiOutput.CastPosition,
                HitChance = HitchanceConverter(sexsiOutput.HitChance),
                PredictedPosition = sexsiOutput.PredictedPosition
            };
            output.CollisionGameObjects.AddRange(sexsiOutput.Collisions);
            Skillshots.Prediction.ResetImplementation();
            return output;
        }

        private static SkillType TypeConverter(SkillShotType type)
        {
            switch (type)
            {
                case SkillShotType.Linear:
                    return SkillType.Line;
                case SkillShotType.Circular:
                    return SkillType.Circle;
                case SkillShotType.Cone:
                    return SkillType.Cone;
                default:
                    return SkillType.Arc;
            }
        }

        private static EloBuddy.SDK.Enumerations.HitChance HitchanceConverter(Skillshots.HitChance hitchance)
        {
            switch (hitchance)
            {
                case HitChance.Medium:
                    return EloBuddy.SDK.Enumerations.HitChance.High;
                case HitChance.Collision:
                    return EloBuddy.SDK.Enumerations.HitChance.Collision;
                case HitChance.Dash:
                    return EloBuddy.SDK.Enumerations.HitChance.Dashing;
                case HitChance.Immobile:
                    return EloBuddy.SDK.Enumerations.HitChance.Immobile;
                case HitChance.Low:
                    return EloBuddy.SDK.Enumerations.HitChance.Low;
                case HitChance.OutOfRange:
                    return EloBuddy.SDK.Enumerations.HitChance.AveragePoint;
                case HitChance.High:
                    return EloBuddy.SDK.Enumerations.HitChance.High;
                case HitChance.Slowed:
                    return EloBuddy.SDK.Enumerations.HitChance.Unknown;
                default:
                    return EloBuddy.SDK.Enumerations.HitChance.Unknown;  
            }
        }

        private static CollisionableObjects CollisionTypesConverter(HashSet<CollisionType> collisionTypes)
        {
            foreach (var collisionType in collisionTypes)
            {
                switch (collisionType)
                {
                    case CollisionType.AiHeroClient:
                        return CollisionableObjects.Heroes;
                    case CollisionType.ObjAiMinion:
                        return CollisionableObjects.Minions;
                    case CollisionType.YasuoWall:
                        return CollisionableObjects.YasuoWall;
                    default:
                        return 0;
                }
            }
            return 0;
        }
    }
}
