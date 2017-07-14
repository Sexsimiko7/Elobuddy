using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace YasuoMemeBender.Skills
{
    static class LastBreath
    {
        public static Spell.Active R = new Spell.Active(SpellSlot.R, 1200);

        public static bool ShouldUlt(AIHeroClient target)
        {

            var mode =
                Config.spellRMenu[$"ylm.spellr.rtarget.{target.ChampionName.ToLower()}"].Cast<ComboBox>().CurrentValue;

            if (mode == 0 || !R.IsReady() || (!Config.spellRMenu["ylm.spellr.towercheck"].Cast<CheckBox>().CurrentValue && target.IsUnderEnemyturret())
               /* || !target.HasBuffOfType(BuffType.Knockup) || !target.HasBuffOfType(BuffType.Knockback)*/)
                return false;

            switch (mode)
            {
                case 1:
                    return R.GetSpellDamage(target) + SteelTempest.Q.GetSpellDamage(target) > target.Health;
                case 2:
                    return true;
                case 3:
                    if (Config.spellRMenu["ylm.spellr.targethealth"].Cast<CheckBox>().CurrentValue &&
                        (target.HealthPercent > Config.spellRMenu["ylm.spellr.targethealthslider"].Cast<Slider>().CurrentValue))
                        return false;

                    if (Config.spellRMenu["ylm.spellr.playerhealth"].Cast<CheckBox>().CurrentValue &&
                        (ObjectManager.Player.HealthPercent < Config.spellRMenu["ylm.spellr.playerhealthslider"].Cast<Slider>().CurrentValue))
                        return false;

                    return true;
            }
            return false;
        }

        public static void CastR(AIHeroClient target)
        {
            if (Config.spellRMenu["ylm.spellr.delay"].Cast<CheckBox>().CurrentValue)
            {

                var buff = target.Buffs.FirstOrDefault(b => b.Type == BuffType.Knockup || b.Type == BuffType.Knockback);
                if (buff == null)
                {
                    return;
                }
                var delayTime = buff.EndTime - Game.Time - Game.Ping / 2;
                delayTime = delayTime < 0 ? 0 : delayTime;
                Core.DelayAction(() => R.Cast(target), (int) delayTime);
                Console.WriteLine("r");
            }
            else
            {   
                R.Cast(target);
                Console.WriteLine("r2");
            }
        }

        public static void AutoUltimate()
        {
            if (!R.IsReady())
            {
                return;
            }
            var enemies = EntityManager.Heroes.Enemies
                .Where(h => h.HasBuffOfType(BuffType.Knockup) || h.HasBuffOfType(BuffType.Knockback) && R.IsInRange(h));
            if (!enemies.Any())
            {
                return;
            }
            var avgPoint = enemies.FirstOrDefault().ServerPosition;
            var bestAvg = avgPoint;
            int inRange = avgPoint.CountEnemyChampionsInRange(ObjectManager.Player.AttackRange);
            int bestInRange = inRange;
            foreach (var enemy in enemies)
            {
                avgPoint += enemy.ServerPosition;
                avgPoint /= 2;
                inRange = avgPoint.CountEnemyChampionsInRange(ObjectManager.Player.AttackRange);
                if (inRange > bestInRange)
                {
                    bestInRange = inRange;
                    bestAvg = avgPoint;
                }

            }
            if (Config.spellRMenu["ylm.spellr.targetnumber"].Cast<Slider>().CurrentValue <= bestInRange)
            {
                R.Cast(bestAvg);
            }
        }
    }
}
