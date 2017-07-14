using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using YasuoMemeBender.Skills;

namespace YasuoMemeBender
{
    static class YasuoUtils
    {

        public static bool DecideKnockup(Obj_AI_Base target)
        {
            if (Config.comboMenu["ylm.combo.mode"].Cast<ComboBox>().CurrentValue == 0 || !target.IsValid)
            {
                return false;
            }
            return target.Distance(ObjectManager.Player, true) <= (475 + 200) * (475 + 200) && SteelTempest.Q.IsReady() && SteelTempest.Empowered
                   && SweepingBlade.E.IsReady() && !target.HasBuff("YasuoDashWrapper");
        }

        public static bool DangerousTurret(Obj_AI_Turret turret)
        {
            if (!Config.towerDiveMenu["ylm.towerdive.enabled"].Cast<CheckBox>().CurrentValue)
                return true;
            var units = ObjectManager.Get<Obj_AI_Base>().Where(o => (o.IsAlly && (o is AIHeroClient || o.IsMinion)) && turret.Distance(o, true) <= 900 * 900);
            return units.Count() >= Config.towerDiveMenu["ylm.towerdive.minAllies"].Cast<Slider>().CurrentValue;
        }

        public static Obj_AI_Turret GetNearestTurret(Vector3 pos)
        {
            var turrets = ObjectManager.Get<Obj_AI_Turret>().OrderBy(t => t.Distance(pos, true));
            return turrets.FirstOrDefault();
        }

        public static Obj_AI_Base BestQDashKnockupUnit()
        {
            //var heroes = ObjectManager.Get<Obj_AI_Hero>();
            var eRangeheroes = EntityManager.Heroes.Enemies
                .Where(h => SweepingBlade.CanCastE(h, true) && SweepingBlade.E.IsInRange(h));
            int max = 0;
            AIHeroClient bestUnit = null;
            foreach (var eHero in eRangeheroes)
            {
                var enemyCount = SweepingBlade.EndPos(eHero).CountEnemyChampionsInRange(375);
                if (enemyCount > max)
                {
                    max = enemyCount;
                    bestUnit = eHero;
                }
            }
            return bestUnit;
        }

        public static Obj_AI_Base ClosestMinion(Vector3 position)
        {
            var minion = EntityManager.MinionsAndMonsters
                .Get(EntityManager.MinionsAndMonsters.EntityType.Both, EntityManager.UnitTeam.Enemy, null,
                    Config.gapcloseMenu["ylm.gapclose.limit"].Cast<Slider>().CurrentValue)
                .OrderBy(m => m.Distance(position, true))
                .FirstOrDefault(m => position.Distance(ObjectManager.Player.ServerPosition, true) >
                                     m.Distance(position, true));
            return minion;
        }
    }
}
