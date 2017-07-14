using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;

namespace YasuoMemeBender.Skills
{
    internal static class SteelTempest
    {
        public static Spell.Skillshot Q = new Spell.Skillshot(SpellSlot.Q, 475, SkillShotType.Linear);
        public static Spell.Skillshot QEmp = new Spell.Skillshot(SpellSlot.Q, 900, SkillShotType.Linear);
        public static Spell.Active QDash = new Spell.Active(SpellSlot.Q, 0);
        public static bool Empowered => ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name == "yasuoq3w";

        public static void CastQ(Obj_AI_Base target)
        {
            if (ObjectManager.Player.IsDashing())
            {
                if (target.IsValidTarget(375))
                {
                    QDash.Cast();
                }
            }
            else
            {
                if (target.IsValidTarget(QEmp.Range))
                {
                    QEmp.Cast(target);
                }
            }
        }
        public static void LastHitQ()
        {
            if (!Q.IsReady())
            {
                return;
            }
            var minion = EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both, EntityManager.UnitTeam.Enemy,ObjectManager.Player.ServerPosition,
                    Empowered ? QEmp.Range : Q.Range).Where(m => Q.GetHealthPrediction(m) < Q.GetSpellDamage(m))
                .OrderBy(m => m.Distance(ObjectManager.Player, true)).FirstOrDefault();

            if (minion == null)
            {
                return;
            }

            if ((!Empowered && Config.lasthitMenu["ylm.lasthit.useq"].Cast<CheckBox>().CurrentValue)
                || (Empowered && Config.lasthitMenu["ylm.lasthit.useq3"].Cast<CheckBox>().CurrentValue))
            {
                CastQ(minion);

            }
        }

        public static void ClearQ(bool jungleClear = false)
        {
            if (!Q.IsReady())
            {
                return;
            }
            if (jungleClear && !((!Empowered && Config.jungleclearMenu["ylm.jungleclear.useq"].Cast<CheckBox>().CurrentValue) || (Empowered && Config.jungleclearMenu["ylm.jungleclear.useq3"].Cast<CheckBox>().CurrentValue)))
            {
                return;
            }
            var qRange = Empowered ? QEmp.Range : Q.Range;
            var minions = EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Both, EntityManager.UnitTeam.Enemy, ObjectManager.Player.ServerPosition,
                Empowered ? QEmp.Range : Q.Range);
            // Console.WriteLine("Empowered: {0}, useQ: {1}, useQ3: {2}", Empowered, Config.Param<bool>("ylm.laneclear.useq"), Config.Param<bool>("ylm.laneclear.useq3"));

            var objAiMinions = minions as List<Obj_AI_Minion> ?? minions.ToList();
            if (!objAiMinions.Any())
            {
                return;
            }
            if (!Empowered)
            {
                if (!Config.laneclearMenu["ylm.laneclear.useq"].Cast<CheckBox>().CurrentValue)
                {
                    return;
                }
                if (!ObjectManager.Player.IsDashing())
                {
                    //mini
                    //var Position = SteelTempest.Q.GetLineFarmLocation(minions).Position;
                    Q.Cast(Q.GetBestLinearCastPosition(objAiMinions).CastPosition);
                    // MinionManager.GetBestLineFarmLocation(minions, SteelTempest.Q.Width, SteelTempest.Q.Range);
                }
                else
                {
                    objAiMinions.RemoveAll(m => m.Distance(ObjectManager.Player, true) > 375);
                    if (objAiMinions.Count > 0)
                    {
                        QDash.Cast(ObjectManager.Player);
                    }
                }
            }
            else if (Config.laneclearMenu["ylm.laneclear.useq3"].Cast<CheckBox>().CurrentValue)
            {

                if (ObjectManager.Player.IsDashing())
                {
                    objAiMinions.RemoveAll(m => m.Distance(ObjectManager.Player, true) > 375);
                    if (objAiMinions.Count > 0)
                    {
                        QDash.Cast(ObjectManager.Player);
                    }
                }
                else
                {
                    var mode = Config.laneclearMenu["ylm.laneclear.modeq3"].Cast<ComboBox>().CurrentValue;
                    var bestFarmLocation = QEmp.GetBestLinearCastPosition(objAiMinions);
                    if (mode == 0 ||
                        bestFarmLocation.HitNumber >= Config.laneclearMenu["ylm.laneclear.modeq3amount"].Cast<Slider>().CurrentValue)
                    {
                        QEmp.Cast(bestFarmLocation.CastPosition);
                    }
                }
            }
            // minions.
        }


    }
}
