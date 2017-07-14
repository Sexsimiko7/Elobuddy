using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace YasuoMemeBender.Skills
{
    internal static class SweepingBlade
    {
        public static Spell.Targeted E = new Spell.Targeted(SpellSlot.E, 475);

        public static int Stacks
        {
            get
            {
                var buff = ObjectManager.Player.Buffs.First(b => b.Name == "yasuodashscalar");
                return buff?.Count ?? 0;
            }
        }

        public static bool CanCastE(Obj_AI_Base target, bool ignoreMenu = false)
        {
            var endPos = EndPos(target);
            if ((endPos.IsUnderTurret() && YasuoUtils.DangerousTurret(YasuoUtils.GetNearestTurret(endPos))) ||
                target.Distance(ObjectManager.Player, true) >= E.RangeSquared || target.HasBuff("YasuoDashWrapper"))
            {
                return false;
            }
            if (!Config.spellEMenu["ylm.spelle.range"].Cast<CheckBox>().CurrentValue || ignoreMenu) return true;
            var eRangeC = Config.spellEMenu["ylm.spelle.rangeslider"].Cast<Slider>().CurrentValue;
            return (target.Distance(endPos, true) < eRangeC * eRangeC);
        }

        public static Vector3 EndPos(Vector3 position)
        {
            return ObjectManager.Player.ServerPosition.Extend(position, E.Range).To3D();
        }

        public static Vector3 EndPos(Obj_AI_Base unit)
        {
            return EndPos(unit.ServerPosition);
        }

        public static void JungleClearE()
        {

            if (!E.IsReady() || !Config.jungleclearMenu["ylm.jungleclear.usee"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }

            var minion =
                EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Monster, EntityManager.UnitTeam.Both, ObjectManager.Player.ServerPosition, E.Range
                    ).FirstOrDefault(m => CanCastE(m) && E.GetHealthPrediction(m) > 0);

            if (minion == null)
            {
                return;
            }
            E.Cast(minion);

        }

        public static void LaneE(bool lastHit = false)
        {
            if (!E.IsReady() || (!Config.laneclearMenu["ylm.laneclear.usee"].Cast<CheckBox>().CurrentValue && !Config.lasthitMenu["ylm.lasthit.usee"].Cast<CheckBox>().CurrentValue))
            {
                return;
            }
            lastHit = lastHit && Config.lasthitMenu["ylm.lasthit.usee"].Cast<CheckBox>().CurrentValue;
            var minion =
                EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Minion, EntityManager.UnitTeam.Both, ObjectManager.Player.ServerPosition, E.Range)
                    .FirstOrDefault(m => CanCastE(m) /*&& E.GetHealthPrediction(m) > 0*/);

            if (minion == null || !lastHit && !Config.laneclearMenu["ylm.laneclear.usee"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }

            var mode = Config.laneclearMenu["ylm.laneclear.modee"].Cast<ComboBox>().CurrentValue;
            if (((lastHit || mode == 0) && E.GetHealthPrediction(minion) <= E.GetSpellDamage(minion))
                || mode == 1)
            {
                E.Cast(minion);
            }
        }

        public static void GapClose(Obj_AI_Base target = null, bool escape = false)
        {
            if (!Config.gapcloseMenu["ylm.gapclose.on"].Cast<CheckBox>().CurrentValue
                ||
                (Config.gapcloseMenu["ylm.gapclose.hpcheck"].Cast<CheckBox>().CurrentValue &&
                 Config.gapcloseMenu["ylm.gapclose.hpcheck2"].Cast<Slider>().CurrentValue > ObjectManager.Player.HealthPercent))
            {
                return;
            }

            if (target == null)
            {
                target = TargetSelector.GetTarget(Config.gapcloseMenu["ylm.gapclose.limit"].Cast<Slider>().CurrentValue,
                    DamageType.Magical);
                if (target == null)
                {
                    return;
                }
            }
            if (ObjectManager.Player.IsDashing() && Config.gapcloseMenu["ylm.gapclose.stackQ"].Cast<CheckBox>().CurrentValue && !SteelTempest.Empowered
                && !CanCastE(target))
            {
                SteelTempest.QDash.Cast();
            }
            var distTarget = ObjectManager.Player.Distance(target, true);

            if (!E.IsReady())
            {
                return;
            }
            if (escape)
            {
                E.Cast(target);
            }
            if (distTarget <= ObjectManager.Player.AttackRange * ObjectManager.Player.AttackRange)
            {
                return;
            }
            /*var dashUnit = (from o in ObjectManager.Get<Obj_AI_Base>()
                             where o.IsValidTarget() && o.IsEnemy && (o.IsMinion || o is AIHeroClient)
                             let distance = o.Distance(ObjectManager.Player, true)
                             let endPos = EndPos(o.ServerPosition)
                             where
                                distance < E.RangeSquared && !endPos.UnderTurret(true) && o.Distance(target, true) < distance
                                && endPos.Distance(target.ServerPosition, true) < distTarget && !o.HasBuff("YasuoDashWrapper")
                             select o).FirstOrDefault();*/ 

            foreach (
                var unit in
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(o => o.IsEnemy && o.IsValidTarget() && (o.IsMinion || o is AIHeroClient)))
            {
                var distance = unit.Distance(ObjectManager.Player, true);
                var endPos = EndPos(unit.ServerPosition);

                if (distance < E.RangeSquared &&
                    (!endPos.IsUnderTurret() || !YasuoUtils.DangerousTurret(YasuoUtils.GetNearestTurret(endPos))) &&
                    unit.Distance(target, true) < distance
                    && endPos.Distance(target.ServerPosition, true) < distTarget && !unit.HasBuff("YasuoDashWrapper"))
                {
                    E.Cast(unit);
                    return;
                }
            }
        }

        public static void RunAway(Obj_AI_Base from)
        {
            var minions =
                EntityManager.MinionsAndMonsters.Get(EntityManager.MinionsAndMonsters.EntityType.Monster, EntityManager.UnitTeam.Both, null, E.Range)
                    .Where(m => !EndPos(m).IsUnderTurret() && !m.HasBuff("YasuoDashWrapper"));
            minions.OrderBy(m => m.Distance(from, true));
            var furthestMinion = minions.LastOrDefault();
            if (furthestMinion != null)
            {
                E.Cast(from);
            }
        }
    }
}
