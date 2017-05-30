﻿using EloBuddy;
using EloBuddy.SDK;

namespace Sexsimiko7AIO.Yasuo
{
    class DamageHandler
    {
        public static float QDamage(Obj_AI_Base target)
        {
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical,
                (float) (new double[] {20, 40, 60, 80, 100}[Player.GetSpell(SpellSlot.Q).Level - 1]
                         + 1*Player.Instance.TotalAttackDamage));
        }
        public static float EDamage(Obj_AI_Base target)
        {
            var stacksPassive = Player.Instance.Buffs.Find(b => b.DisplayName.Equals("YasuoDashScalar"));
            var stacks = 1 + 0.25 * (stacksPassive?.Count ?? 0);
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new double[] { 60, 70, 80, 90, 100 }[Player.GetSpell(SpellSlot.E).Level - 1] + 0.2 * ObjectManager.Player.FlatPhysicalDamageMod * stacks
                        + 0.6 * (Player.Instance.FlatMagicDamageMod)));
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new double[] { 200, 300, 400 }[Player.GetSpell(SpellSlot.R).Level - 1]
                         + 1.5 * Player.Instance.FlatPhysicalDamageMod));
        }
    }
}
