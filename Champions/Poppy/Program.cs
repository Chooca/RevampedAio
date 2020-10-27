using System;
using System.Linq;
using System.Collections.Generic;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using EnsoulSharp.SDK.Utility;
using EnsoulSharp;
using SharpDX;
using Color = System.Drawing.Color;
using static EnsoulSharp.SDK.Items;
using SharpDX.Direct3D9;
using SPrediction;
using static RVMP.RevampedLibary.Extensions.Commons.Extensions;

//DO NOT USE// not done yet !!!
//DO NOT USE// not done yet !!!
//DO NOT USE// not done yet !!!
//DO NOT USE// not done yet !!!
//DO NOT USE// not done yet !!!
//DO NOT USE// not done yet !!!
//DO NOT USE// not done yet !!!
//DO NOT USE// not done yet !!!
//DO NOT USE// not done yet !!!

namespace PoppyRVMP
{
    static class Program
    {
        private static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell _Q;
        public static Spell _W;
        public static Spell _E;
        public static Spell _R;

        private static Menu StartMenu, ComboMenu, LastHitM, DebugC, DrawingsMenu, JungleMenu, ClearMenu, UtilityMenu, RSet, ESet, WSet, QSet, otheroptions;


      
        public static void PoppyRVMP()
        {
            if (!_Player.CharacterName.Contains("Poppy"))
            {
                return;
            }
            Game.Print("PoppyRVMP ready!", System.Drawing.Color.Blue);


            _Q = new Spell(SpellSlot.Q, 400);
            _Q.SetSkillshot(0.55f, 90f, float.MaxValue, false, SkillshotType.Line);
            _W = new Spell(SpellSlot.W, 400);
            _E = new Spell(SpellSlot.E, 500);
            _R = new Spell(SpellSlot.R);
            _R.SetSkillshot(0.5f, 90f, 1400, true, SkillshotType.Line);
            _R.SetCharged("PoppyR", "PoppyR", 425, 1400, 1.0f);

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Interrupter.OnInterrupterSpell += Interruptererer;
            Dash.OnDash += Dash_OnDash;
        }

        public static float ComboDamage(AIHeroClient hero)
        {
            double damage = 0;
            if (_Q.IsReady())
            {
                damage += Damage.GetSpellDamage(_Player, hero, SpellSlot.Q);
            }
            if (_E.IsReady())
            {
                damage += (float)Damage.GetSpellDamage(_Player, hero, SpellSlot.E);
            }
            if (_R.IsReady() || _R.IsCharging)
            {
                damage += (float)Damage.GetSpellDamage(_Player, hero, SpellSlot.R);
            }
            return (float)damage;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling() || MenuGUI.IsChatOpen || ObjectManager.Player.IsWindingUp)
            {
                return;
            }
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Combo:
                    Combo();
                    break;
                case OrbwalkerMode.Harass:
                    Harass();
                    break;
                case OrbwalkerMode.LastHit:
                    LastHit();
                    break;
                case OrbwalkerMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
            }

            ImmobileQ();
            KillSteal();

        }
        public static void LastHit()

        {
            var minion = GameObjects.EnemyMinions.Where(a => a.Distance(ObjectManager.Player) <= _E.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (minion != null)
            {
                if (ClearMenu["UseELH"].GetValue<MenuBool>().Enabled && _E.IsReady() && minion.IsValidTarget(_E.Range) && minion.Health < ObjectManager.Player.GetSpellDamage(minion, SpellSlot.E))
                {
                    _E.Cast(minion);
                }
            }
               var MHR = GameObjects.EnemyMinions.Where(a => a.Distance(ObjectManager.Player) <= _Q.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (MHR != null)
            {



                if (ClearMenu["UseQLH"].GetValue<MenuBool>().Enabled && _Q.IsReady() && ObjectManager.Player.ManaPercent > ClearMenu["ClearMana"].GetValue<MenuSlider>().Value && MHR.IsValidTarget(_Q.Range) &&
                    ObjectManager.Player.GetSpellDamage(MHR, SpellSlot.Q) >= MHR.Health)

                {
                    _Q.Cast(MHR.Position);
                }


                if (ClearMenu["UseWLH"].GetValue<MenuBool>().Enabled && _W.IsReady() && ObjectManager.Player.GetSpellDamage(MHR, SpellSlot.W) >= MHR.Health &&
                    ObjectManager.Player.ManaPercent > ClearMenu["ClearMana"].GetValue<MenuSlider>().Value)
                {
                    _W.Cast(MHR.Position);
                }
            }
        }

        public static void JungleClear()
        {
            var MHR = GameObjects.Jungle.Where(a => a.Distance(ObjectManager.Player) <= _Q.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (MHR != null)
            {
                if (_Q.IsReady() && _Player.ManaPercent > JungleMenu["ClearManaJ"].GetValue<MenuSlider>().Value && JungleMenu["UseJQCL"].GetValue<MenuBool>().Enabled && MHR.IsValidTarget(_Q.Range))
                {
                    _Q.Cast(MHR);
                }
            }

            if (_W.IsReady() && _Q.IsReady() == false && _Player.ManaPercent > JungleMenu["ClearManaJ"].GetValue<MenuSlider>().Value && JungleMenu["UseJWCL"].GetValue<MenuBool>().Enabled && MHR.IsValidTarget(_W.Range))
            {
                _W.Cast(MHR.Position);
            }
            if (_E.IsReady() && _Player.ManaPercent > JungleMenu["ClearManaJ"].GetValue<MenuSlider>().Value && JungleMenu["UseJECL"].GetValue<MenuBool>().Enabled && MHR.IsValidTarget(_E.Range))
            {
                _E.Cast(MHR);
            }
        }

        public static void LaneClear()

        {
            if (_Q.IsReady() && ClearMenu["UseQCL"].GetValue<MenuBool>().Enabled)
            {
                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(_Q.Range))
                {

                    if (minion.IsValidTarget(_Q.Range) && minion != null && ClearMenu["UseQCL"].GetValue<MenuBool>().Enabled)
                    {
                        _Q.CastOnUnit(minion);
                    }
                }
            }
            var MHR = GameObjects.EnemyMinions.Where(a => a.Distance(ObjectManager.Player) <= _Q.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (MHR != null)

                if (ClearMenu["UseWCL"].GetValue<MenuBool>().Enabled)
                {
                    if (_W.IsReady())
                    {
                        _W.Cast(MHR.Position);
                    }

                }

            if (ClearMenu["UseECL"].GetValue<MenuBool>().Enabled && _E.IsReady() && ObjectManager.Player.ManaPercent > ClearMenu["ClearMana"].GetValue<MenuSlider>().Value && MHR.IsValidTarget(_E.Range))

            {
                _E.Cast(MHR);
            }
            foreach (var minion in GetEnemyLaneMinionsTargetsInRange(_E.Range))
            {

                if (minion.Health <= GameObjects.Player.GetSpellDamage(minion, SpellSlot.E))
                {
                    if (ClearMenu["UseELH"].GetValue<MenuBool>().Enabled)
                    {
                        if (minion.Distance(GameObjects.Player) > 250)
                        {
                            _E.CastOnUnit(minion);
                        }
                    }
                    if (ClearMenu["UseELH"].GetValue<MenuBool>().Enabled)
                    {
                        _E.CastOnUnit(minion);
                    }

                }

            }

        }

        public static void Harass()
        {
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            if (target == null) return;
            if (ESet["UseEH"].GetValue<MenuBool>().Enabled)
            {
                if (!target.IsValidTarget(_E.Range) && !_E.IsReady())
                    return;
                {
                    if (_E.IsReady() && ESet["UseES"].GetValue<MenuBool>().Enabled)
                    {

                        _E.Cast(target);
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                        if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                        {
                            Game.Print("Casting E with speedup");
                            Console.WriteLine(Game.Time + "Casting E with Speedup");
                        }

                    }
                    if (_E.IsReady() && !ESet["UseES"].GetValue<MenuBool>().Enabled)
                    {
                        _E.Cast(target);
                        if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                        {
                            Game.Print("Casting E with normal");
                            Console.WriteLine(Game.Time + "Casting E with Speedup");
                        }
                    }
                }
            }
            if (WSet["UseWH"].GetValue<MenuBool>().Enabled)
            {
                if (!_W.IsReady() && _Player.Distance(target) >= 500) return;
                {

                    var Wpred = _W.GetPrediction(target);
                    if (Wpred.Hitchance >= HitChance.High && target.IsValidTarget(_W.Range))
                    {
                        if (WSet["UseW2"].GetValue<MenuBool>().Enabled)
                        {
                            var Enemys = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(_W.Range));
                            if (Enemys != null)
                            {
                                if (Enemys.Count() >= 2)
                                {
                                    _W.Cast(target.Position);
                                    if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                                    {

                                        Game.Print("Casting W Found more than >= 2 People ");
                                        Console.WriteLine("Casting W Found more than >= 2 People");
                                    }
                                }
                                else if (Enemys.Count() >= 1)
                                {
                                    _W.Cast(target.Position);
                                    if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                                    {

                                        Game.Print("Casting W Found more than >= 1 People ");
                                        Console.WriteLine("Casting W Found more than >= 1 People");
                                    }
                                }
                            }
                        }
                    }

                }
            }
            if (!WSet["UseW2"].GetValue<MenuBool>().Enabled)
            {
                _W.Cast(target.Position);
                if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                {

                    Game.Print("Casting W ");
                    Console.WriteLine("Casting W");
                }
            }

            if (QSet["UseQH"].GetValue<MenuBool>().Enabled)
            {
                if (_Q.IsReady())
                {
                    var canHitMoreThanOneTarget =
                      GameObjects.EnemyHeroes.OrderByDescending(x => x.CountEnemyHeroesInRange(_Q.Width))
                      .FirstOrDefault(x => x.IsValidTarget(_Q.Range) && x.CountEnemyHeroesInRange(_Q.Width) >= 1);
                    if (canHitMoreThanOneTarget != null)
                    {
                        var getAllTargets = GameObjects.EnemyHeroes.Find(x => x.IsValidTarget() && x.IsValidTarget(_Q.Width));
                        var Qpred = _Q.GetPrediction(target);
                        if (Qpred.Hitchance >= HitChance.High && target.IsValidTarget(_Q.Range))
                        {
                            _Q.Cast(target);
                            if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                            {

                                Game.Print("FOUND 1 PEOPLE FOR Q ");
                                Console.WriteLine("FOUND 1 PEOPLE FOR Q");
                            }
                        }
                    }
                }
            }
            if (QSet["UseQH"].GetValue<MenuBool>().Enabled)
            {

                if (!target.IsValidTarget(_Q.Range))
                    return;
                {
                    if (_Q.IsReady() && QSet["UseS"].GetValue<MenuBool>().Enabled)
                    {
                        var Qpred = _Q.GetPrediction(target);
                        if (Qpred.Hitchance >= HitChance.High && target.IsValidTarget(_Q.Range))
                        {
                            if (!target.PoisonWillExpire(250))
                                return;
                            {
                                _Q.Cast(target.Position);
                                if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                                {

                                    Game.Print("Casting Q with HIGH pred saver ");
                                    Console.WriteLine("Casting Q with HIGH pred saver ");
                                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,Game.CursorPos);
                                }
                            }
                        }

                    }

                }
            }
        }


        private static void Drawing_OnDraw(EventArgs args)
        {
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            var Combo = Orbwalker.ActiveMode == OrbwalkerMode.Combo;
            var LastHit = Orbwalker.ActiveMode == OrbwalkerMode.LastHit;
            var LaneClear = Orbwalker.ActiveMode == OrbwalkerMode.LaneClear;
            var Harass = Orbwalker.ActiveMode == OrbwalkerMode.Harass;

            if (DrawingsMenu["DQ"].GetValue<MenuBool>().Enabled && _Q.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _Q.Range, Color.Lime, 1);
            }
            if (DrawingsMenu["DE"].GetValue<MenuBool>().Enabled && _E.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _E.Range, Color.Lime, 1);
            }
            if (DrawingsMenu["DR"].GetValue<MenuBool>().Enabled && _R.IsReady())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, _R.Range, Color.Lime, 1);
            }
            if (DrawingsMenu["QPred"].GetValue<MenuBool>().Enabled && _Q.IsReady())
            {
                if (target == null)
                    return;
                Drawing.DrawCircle(_Q.GetPrediction(target).CastPosition, _Q.Width, System.Drawing.Color.Violet);

            }
            if (ComboMenu["DrawStatus"].GetValue<MenuBool>().Enabled)

            {
                if (Harass && !Combo && LaneClear && LastHit)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.93f, System.Drawing.Color.White, "[ Orbwalker Mode: LastHit ]");
                }
                if (Harass && Combo && !LaneClear && LastHit)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Combo ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.93f, System.Drawing.Color.White, "[ Orbwalker Mode: LastHit ]");
                }
                if (Harass && Combo && LaneClear && !LastHit)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Combo ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.93f, System.Drawing.Color.White, "[ Orbwalker Mode: LaneClear ]");
                }
                if (Harass && Combo && LaneClear && LastHit)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Combo ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.93f, System.Drawing.Color.White, "[ Orbwalker Mode: LastHit ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.95f, System.Drawing.Color.White, "[ Orbwalker Mode: LaneClear ]");
                }
                if (Harass && !Combo && !LaneClear && LastHit)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: LastHit ]");
                }
                if (Harass && Combo && !LaneClear && !LastHit)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Combo ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                }

                if (Harass && LaneClear && !Combo && !LastHit)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: LaneClear ]");
                }
                if (Harass && !LaneClear && !Combo && !LastHit)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Harass ]");
                }
                if (LaneClear && LastHit && !Combo && !Harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: LastHit ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: LaneClear ]");
                }
                if (LaneClear && Combo && !LastHit && !Harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Combo ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: LaneClear ]");
                }
                if (LaneClear && LastHit && Combo && !Harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Combo ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: LastHit ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.93f, System.Drawing.Color.White, "[ Orbwalker Mode: LaneClear ]");
                }
                if (LaneClear && !LastHit && !Combo && !Harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: LaneClear ]");
                }

                if (LastHit && Combo && !LaneClear && !Harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Combo ]");
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.91f, System.Drawing.Color.White, "[ Orbwalker Mode: LastHit ]");
                }
                if (LastHit && !Combo && !LaneClear && !Harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: LastHit ]");
                }

                if (Combo && !LastHit && !LaneClear && !Harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: Combo ]");
                }

                else if (!Combo && !LastHit && !LaneClear && !Harass)
                {
                    Drawing.DrawText(Drawing.Width * 0.72f, Drawing.Height * 0.89f, System.Drawing.Color.White, "[ Orbwalker Mode: None ]");
                }
            }
        }
        private static void Combo()
        {

            var HighP = ComboMenu["PredHit"].GetValue<MenuList>().Index == 0;
            var MediumP = ComboMenu["PredHit"].GetValue<MenuList>().Index == 1;
            var LowP = ComboMenu["PredHit"].GetValue<MenuList>().Index == 2;
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            var targetQ2 = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }
            if (ComboMenu["DisableAA"].GetValue<MenuBool>().Enabled && Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                Orbwalker.AttackState = true;
            }
            if (!ComboMenu["DisableAA"].GetValue<MenuBool>().Enabled && Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                Orbwalker.AttackState = false;
            }
            if (Orbwalker.ActiveMode != OrbwalkerMode.Combo)
            {
                Orbwalker.AttackState = false;
            }
            if (HighP)
            {
                if (ESet["UseE"].GetValue<MenuBool>().Enabled)
                {
                    if (!target.IsValidTarget(_E.Range) && !_E.IsReady())
                        return;
                    {
                        if (_E.IsReady() && ESet["UseES"].GetValue<MenuBool>().Enabled)
                        {

                            _E.Cast(target);
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,Game.CursorPos);
                            if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                            {
                                Game.Print("Casting E with speedup");
                                Console.WriteLine(Game.Time + "Casting E with Speedup");
                            }

                        }
                        if (_E.IsReady() && !ESet["UseES"].GetValue<MenuBool>().Enabled)
                        {
                            _E.Cast(target);
                            if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                            {
                                Game.Print("Casting E with normal");
                                Console.WriteLine(Game.Time + "Casting E with Speedup");
                            }
                        }
                    }
                }

                if (WSet["UseW"].GetValue<MenuBool>().Enabled)
                {
                    if (!_W.IsReady() && _Player.Distance(target) >= 500) return;
                    {

                        var Wpred = _W.GetPrediction(target);
                        if (Wpred.Hitchance >= HitChance.High && target.IsValidTarget(_W.Range))
                        {
                            if (WSet["UseW2"].GetValue<MenuBool>().Enabled)
                            {
                                var Enemys = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(_W.Range));
                                if (Enemys != null)
                                {
                                    if (Enemys.Count() >= 2)
                                    {
                                        _W.Cast(target.Position);
                                        if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                                        {

                                            Game.Print("Casting W Found more than >= 2 People ");
                                            Console.WriteLine("Casting W Found more than >= 2 People");
                                        }
                                    }
                                    else if (Enemys.Count() >= 1)
                                    {
                                        _W.Cast(target.Position);
                                        if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                                        {

                                            Game.Print("Casting W Found more than >= 1 People ");
                                            Console.WriteLine("Casting W Found more than >= 1 People");
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
                if (!WSet["UseW2"].GetValue<MenuBool>().Enabled)
                {
                    _W.Cast(target.Position);
                    if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                    {

                        Game.Print("Casting W ");
                        Console.WriteLine("Casting W");
                    }
                }


                if (QSet["UseQ"].GetValue<MenuBool>().Enabled)
                {
                    if (_Q.IsReady())
                    {
                        var canHitMoreThanOneTarget =
                          GameObjects.EnemyHeroes.OrderByDescending(x => x.CountEnemyHeroesInRange(_Q.Width))
                          .FirstOrDefault(x => x.IsValidTarget(_Q.Range) && x.CountEnemyHeroesInRange(_Q.Width) >= 1);
                        if (canHitMoreThanOneTarget != null)
                        {
                            var getAllTargets = GameObjects.EnemyHeroes.Find(x => x.IsValidTarget() && x.IsValidTarget(_Q.Width));
                            var Qpred = _Q.GetPrediction(target);
                            if (Qpred.Hitchance >= HitChance.High && target.IsValidTarget(_Q.Range))
                            {
                                _Q.Cast(target);
                                if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                                {

                                    Game.Print("FOUND 1 PEOPLE FOR Q ");
                                    Console.WriteLine("FOUND 1 PEOPLE FOR Q");
                                }
                            }
                        }
                    }
                }
                if (QSet["UseQ"].GetValue<MenuBool>().Enabled)
                {

                    if (!target.IsValidTarget(_Q.Range))
                        return;
                    {
                        if (_Q.IsReady() && QSet["UseS"].GetValue<MenuBool>().Enabled)
                        {
                            var Qpred = _Q.GetPrediction(target);
                            if (Qpred.Hitchance >= HitChance.High && target.IsValidTarget(_Q.Range))
                            {
                                if (!target.PoisonWillExpire(250))
                                    return;
                                {
                                    _Q.Cast(target.Position);
                                    if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                                    {

                                        Game.Print("Casting Q with HIGH pred saver ");
                                        Console.WriteLine("Casting Q with HIGH pred saver ");
                                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,Game.CursorPos);
                                    }
                                }
                            }

                        }

                    }
                }

                if (!QSet["UseS"].GetValue<MenuBool>().Enabled && QSet["UseQ"].GetValue<MenuBool>().Enabled)
                {
                    if (_Q.IsReady())
                    {

                        var Qpred = _Q.GetPrediction(target);
                        if (Qpred.Hitchance >= HitChance.High && target.IsValidTarget(_Q.Range))
                        {
                            _Q.Cast(target);
                            if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                            {

                                Game.Print("Casting Q with HIGH pred ");
                                Console.WriteLine("Casting Q with HIGH pred ");
                                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,Game.CursorPos);
                            }
                        }
                    }



                }

                if (RSet["UseR"].GetValue<MenuBool>().Enabled && RSet["UseRG"].GetValue<MenuBool>().Enabled && _R.IsReady())
                {
                    var Enemys = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(_R.Range - 25));
                    if (Enemys != null)
                    {
                        if (Enemys.Count() >= RSet["UseRGs"].GetValue<MenuSlider>().Value && target.IsFacing(_Player) && RSet["UseRFace"].GetValue<MenuBool>().Enabled)
                        {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, target);
                            _R.Cast(target);
                        }
                        if (Enemys.Count() >= RSet["UseRGs"].GetValue<MenuSlider>().Value && !RSet["UseRFace"].GetValue<MenuBool>().Enabled)
                        {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, target);
                            _R.Cast(target);
                        }
                    }

                }

                if (RSet["UseR"].GetValue<MenuBool>().Enabled && _R.IsReady())
                {
                    if (!_R.IsReady()) return;
                    {
                        if (target.IsFacing(_Player) && target.IsValidTarget(_R.Range) && RSet["UseRFace"].GetValue<MenuBool>().Enabled)
                        {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, target);
                            _R.Cast(target.Position);
                        }
                    }
                    if (target.IsValidTarget(_R.Range) && !RSet["UseRFace"].GetValue<MenuBool>().Enabled)
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, target);
                        _R.Cast(target.Position);
                    }

                }
            }

            if (MediumP)
            {
                if (ESet["UseE"].GetValue<MenuBool>().Enabled)
                {
                    if (!target.IsValidTarget(_E.Range) && !_E.IsReady())
                        return;
                    {
                        if (_E.IsReady() && ESet["UseES"].GetValue<MenuBool>().Enabled)
                        {

                            _E.Cast(target);
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,Game.CursorPos);
                            if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                            {
                                Game.Print("Casting E with speedup");
                                Console.WriteLine(Game.Time + "Casting E with Speedup");
                            }

                        }
                        if (_E.IsReady() && !ESet["UseES"].GetValue<MenuBool>().Enabled)
                        {
                            _E.Cast(target);
                            if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                            {
                                Game.Print("Casting E with normal");
                                Console.WriteLine(Game.Time + "Casting E with Speedup");
                            }
                        }
                    }
                }

                if (WSet["UseW"].GetValue<MenuBool>().Enabled)
                {
                    if (!_W.IsReady() && _Player.Distance(target) >= 500) return;
                    {

                        var Wpred = _W.GetPrediction(target);
                        if (Wpred.Hitchance >= HitChance.Medium && target.IsValidTarget(_W.Range))
                        {
                            if (WSet["UseW2"].GetValue<MenuBool>().Enabled)
                            {
                                var Enemys = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(_W.Range));
                                if (Enemys != null)
                                {
                                    if (Enemys.Count() >= 2)
                                    {
                                        _W.Cast(target.Position);
                                        if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                                        {

                                            Game.Print("Casting W Found more than >= 2 People ");
                                            Console.WriteLine("Casting W Found more than >= 2 People");
                                        }
                                    }
                                    else if (Enemys.Count() >= 1)
                                    {
                                        _W.Cast(target.Position);
                                        if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                                        {


                                            Game.Print("Casting W Found more than >= 1 People ");
                                            Console.WriteLine("Casting W Found more than >= 1 People");
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
                if (!WSet["UseW2"].GetValue<MenuBool>().Enabled)
                {
                    _W.Cast(target.Position);
                    if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                    {

                        Game.Print("Casting W ");
                        Console.WriteLine("Casting W");
                    }
                }
                if (QSet["UseQ"].GetValue<MenuBool>().Enabled)
                {
                    if (_Q.IsReady())
                    {
                        var canHitMoreThanOneTarget =
                          GameObjects.EnemyHeroes.OrderByDescending(x => x.CountEnemyHeroesInRange(_Q.Width))
                          .FirstOrDefault(x => x.IsValidTarget(_Q.Range) && x.CountEnemyHeroesInRange(_Q.Width) >= 1);
                        if (canHitMoreThanOneTarget != null)
                        {
                            var getAllTargets = GameObjects.EnemyHeroes.Find(x => x.IsValidTarget() && x.IsValidTarget(_Q.Width));
                            //var center = getAllTargets.Aggregate(Vector3.Zero, (current, x) => current + x.Position) / getAllTargets.Count
                            var Qpred = _Q.GetPrediction(target);
                            if (Qpred.Hitchance >= HitChance.Medium && target.IsValidTarget(_Q.Range))
                            {
                                _Q.Cast(target);
                                if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                                {

                                    Game.Print("FOUND 1 PEOPLE FOR Q ");
                                    Console.WriteLine("FOUND 1 PEOPLE FOR Q");
                                }
                            }
                        }

                    }

                }
                if (QSet["UseQ"].GetValue<MenuBool>().Enabled)
                {

                    if (!target.IsValidTarget(_Q.Range))
                        return;
                    {
                        if (_Q.IsReady() && QSet["UseS"].GetValue<MenuBool>().Enabled)
                        {
                            var Qpred = _Q.GetPrediction(target);
                            if (Qpred.Hitchance >= HitChance.Medium && target.IsValidTarget(_Q.Range))
                            {
                                if (!target.PoisonWillExpire(250))
                                    return;
                                {
                                    _Q.Cast(target.Position);
                                    if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                                    {

                                        Game.Print("Casting Q with HIGH pred saver ");
                                        Console.WriteLine("Casting Q with HIGH pred saver ");
                                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,Game.CursorPos);
                                    }
                                }

                            }


                        }

                    }
                }

                if (!QSet["UseS"].GetValue<MenuBool>().Enabled && QSet["UseQ"].GetValue<MenuBool>().Enabled)
                {
                    if (_Q.IsReady())
                    {

                        var Qpred = _Q.GetPrediction(target);
                        if (Qpred.Hitchance >= HitChance.Medium && target.IsValidTarget(_Q.Range))
                        {
                            _Q.Cast(target);
                            if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                            {

                                Game.Print("Casting Q with HIGH pred ");
                                Console.WriteLine("Casting Q with HIGH pred ");
                                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,Game.CursorPos);
                            }
                        }

                    }



                }

                if (RSet["UseR"].GetValue<MenuBool>().Enabled && RSet["UseRG"].GetValue<MenuBool>().Enabled && _R.IsReady())
                {
                    var Enemys = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(_R.Range - 25));
                    if (Enemys != null)
                    {
                        if (Enemys.Count() >= RSet["UseRGs"].GetValue<MenuSlider>().Value && target.IsFacing(_Player) && RSet["UseRFace"].GetValue<MenuBool>().Enabled)
                        {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, target);
                            _R.Cast(target);
                        }
                        if (Enemys.Count() >= RSet["UseRGs"].GetValue<MenuSlider>().Value && !RSet["UseRFace"].GetValue<MenuBool>().Enabled)
                        {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, target);
                            _R.Cast(target);
                        }
                    }


                }

                if (RSet["UseR"].GetValue<MenuBool>().Enabled && _R.IsReady())
                {
                    if (!_R.IsReady()) return;
                    {
                        if (target.IsFacing(_Player) && target.IsValidTarget(_R.Range) && RSet["UseRFace"].GetValue<MenuBool>().Enabled)
                        {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, target);
                            _R.Cast(target.Position);
                        }
                    }
                    if (target.IsValidTarget(_R.Range) && !RSet["UseRFace"].GetValue<MenuBool>().Enabled)
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, target);
                        _R.Cast(target.Position);
                    }

                }
            }
            if (LowP)
            {
                if (ESet["UseE"].GetValue<MenuBool>().Enabled)
                {
                    if (!target.IsValidTarget(_E.Range) && !_E.IsReady())
                        return;
                    {
                        if (_E.IsReady() && ESet["UseES"].GetValue<MenuBool>().Enabled)
                        {

                            _E.Cast(target);
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,Game.CursorPos);
                            if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                            {
                                Game.Print("Casting E with speedup");
                                Console.WriteLine(Game.Time + "Casting E with Speedup");
                            }


                        }
                        if (_E.IsReady() && !ESet["UseES"].GetValue<MenuBool>().Enabled)
                        {
                            _E.Cast(target);
                            if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                            {
                                Game.Print("Casting E with normal");
                                Console.WriteLine(Game.Time + "Casting E with Speedup");
                            }
                        }
                    }
                }

                if (WSet["UseW"].GetValue<MenuBool>().Enabled)
                {
                    if (!_W.IsReady() && _Player.Distance(target) >= 500) return;
                    {

                        var Wpred = _W.GetPrediction(target);
                        if (Wpred.Hitchance >= HitChance.Low && target.IsValidTarget(_W.Range))
                        {
                            if (WSet["UseW2"].GetValue<MenuBool>().Enabled)
                            {
                                var Enemys = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(_W.Range));
                                if (Enemys != null)
                                {
                                    if (Enemys.Count() >= 2)
                                    {
                                        _W.Cast(target.Position);
                                        if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                                        {

                                            Game.Print("Casting W Found more than >= 2 People ");
                                            Console.WriteLine("Casting W Found more than >= 2 People");
                                        }
                                    }
                                    else if (Enemys.Count() >= 1)
                                    {
                                        _W.Cast(target.Position);
                                        if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                                        {

                                            Game.Print("Casting W Found more than >= 1 People ");
                                            Console.WriteLine("Casting W Found more than >= 1 People");
                                        }
                                    }

                                }
                            }
                        }

                    }
                }
                if (!WSet["UseW2"].GetValue<MenuBool>().Enabled)
                {
                    _W.Cast(target.Position);
                    if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                    {

                        Game.Print("Casting W ");
                        Console.WriteLine("Casting W");
                    }
                }
//radi is gay again
                if (QSet["UseQ"].GetValue<MenuBool>().Enabled && QSet["UseQ2"].GetValue<MenuBool>().Enabled)
                {
                    if (_Q.IsReady())
                    {
                        var canHitMoreThanOneTarget =
                          GameObjects.EnemyHeroes.OrderByDescending(x => x.CountEnemyHeroesInRange(_Q.Width))
                          .FirstOrDefault(x => x.IsValidTarget(_Q.Range) && x.CountEnemyHeroesInRange(_Q.Width) >= 1);
                        if (canHitMoreThanOneTarget != null)
                        {
                            var getAllTargets = GameObjects.EnemyHeroes.Find(x => x.IsValidTarget() && x.IsValidTarget(_Q.Width));
                            //var center = getAllTargets.Aggregate(Vector3.Zero, (current, x) => current + x.Position) / getAllTargets.Count;
                            var Qpred = _Q.GetPrediction(target);
                            if (Qpred.Hitchance >= HitChance.Low && target.IsValidTarget(_Q.Range))
                            {
                                _Q.Cast(target);
                                if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                                {

                                    Game.Print("FOUND 1 PEOPLE FOR Q ");
                                    Console.WriteLine("FOUND 1 PEOPLE FOR Q");
                                }
                            }
                        }
                    }
                }
                if (QSet["UseQ"].GetValue<MenuBool>().Enabled)
                {

                    if (!target.IsValidTarget(_Q.Range))
                        return;
                    {
                        if (_Q.IsReady() && QSet["UseS"].GetValue<MenuBool>().Enabled)
                        {
                            var Qpred = _Q.GetPrediction(target);
                            if (Qpred.Hitchance >= HitChance.Low && target.IsValidTarget(_Q.Range))
                            {
                                if (!target.PoisonWillExpire(250))
                                    return;
                                {
                                    _Q.Cast(target.Position);
                                    if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                                    {

                                        Game.Print("Casting Q with HIGH pred saver ");
                                        Console.WriteLine("Casting Q with HIGH pred saver ");
                                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,Game.CursorPos);
                                    }
                                }
                            }

                        }

                    }
                }

                if (!QSet["UseS"].GetValue<MenuBool>().Enabled && QSet["UseQ"].GetValue<MenuBool>().Enabled)
                {
                    if (_Q.IsReady())
                    {

                        var Qpred = _Q.GetPrediction(target);
                        if (Qpred.Hitchance >= HitChance.Low && target.IsValidTarget(_Q.Range))
                        {
                            _Q.Cast(target);
                            if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                            {

                                Game.Print("Casting Q with HIGH pred ");
                                Console.WriteLine("Casting Q with HIGH pred ");
                                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo,Game.CursorPos);
                            }
                        }
                    }



                }

                if (RSet["UseR"].GetValue<MenuBool>().Enabled && RSet["UseRG"].GetValue<MenuBool>().Enabled && _R.IsReady())
                {
                    var Enemys = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(_R.Range - 25));
                    if (Enemys != null)
                    {
                        if (Enemys.Count() >= RSet["UseRGs"].GetValue<MenuSlider>().Value && target.IsFacing(_Player) && RSet["UseRFace"].GetValue<MenuBool>().Enabled)
                        {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, target);
                            _R.Cast(target);
                        }
                        if (Enemys.Count() >= RSet["UseRGs"].GetValue<MenuSlider>().Value && !RSet["UseRFace"].GetValue<MenuBool>().Enabled)
                        {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, target);
                            _R.Cast(target);
                        }
                    }

                }

                if (RSet["UseR"].GetValue<MenuBool>().Enabled && _R.IsReady())
                {
                    if (!_R.IsReady()) return;
                    {
                        if (target.IsFacing(_Player) && target.IsValidTarget(_R.Range) && RSet["UseRFace"].GetValue<MenuBool>().Enabled)
                        {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, target);
                            _R.Cast(target.Position);
                        }
                    }
                    if (target.IsValidTarget(_R.Range) && !RSet["UseRFace"].GetValue<MenuBool>().Enabled)
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, target);
                        _R.Cast(target.Position);
                    }

                }
            }
        }
        private static void Interruptererer(AIBaseClient sender, Interrupter.InterruptSpellArgs args)
        {
            var RintTarget = TargetSelector.GetTarget(_R.Range, DamageType.Magical);
            if (RintTarget == null) return;
            if (_R.IsReady() && sender.IsValidTarget(_R.Range) && ComboMenu["Rint"].GetValue<MenuBool>().Enabled)
                _R.Cast(RintTarget);

        }
        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserArgs e)
        {
            if (!ComboMenu["RGapClose"].GetValue<MenuBool>().Enabled) return;
            if (sender.IsEnemy)
                _R.Cast(sender);
        }

        private static void Dash_OnDash(AIBaseClient sender, Dash.DashArgs e)
        {
            if (!QSet["QComboDash"].GetValue<MenuBool>().Enabled) return;
            if (!sender.IsEnemy) return;
            if (!_Q.IsReady()) return;
            if (e.EndPos.IsValid())
                _Q.Cast(e.EndPos);
        }
        public static void KillSteal()
        {
            var targetQ = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            var targetE = TargetSelector.GetTarget(_W.Range, DamageType.Magical);
            if (targetQ == null)
            {
                return;
            }
            if (targetE == null)
            {
                return;
            }
            if (QSet["UseQPok"].GetValue<MenuBool>().Enabled)
            {
                var Qpred = _Q.GetPrediction(targetQ);
                if (Qpred.Hitchance >= HitChance.High && targetQ.IsValidTarget(_Q.Range))
                {
                    if (targetQ.Health + targetQ.PhysicalShield < _Player.GetSpellDamage(targetQ, SpellSlot.Q))
                    {
                        if (!targetQ.IsValidTarget(_Q.Range) && !_Q.IsReady()) return;
                        {
                            _Q.Cast(targetQ);
                        }
                    }
                }
            }

            if (ESet["UseEK"].GetValue<MenuBool>().Enabled)
            {
                if (targetE.Health + targetE.PhysicalShield < _Player.GetSpellDamage(targetE, SpellSlot.E))
                {
                    if (!targetE.IsValidTarget(_E.Range) && !_E.IsReady()) return;
                    {
                        {
                            _E.Cast(targetE);
                        }
                    }
                }

            }
        }
        private static void ImmobileQ()
        {
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }
            if (QSet["UseQ"].GetValue<MenuBool>().Enabled && QSet["UseQI"].GetValue<MenuBool>().Enabled)

            {
                if (_Q.IsReady())
                {

                    var Qpred = _Q.GetPrediction(target);
                    if (Qpred.Hitchance >= HitChance.Immobile && target.IsValidTarget(_Q.Range))
                    {
                        _Q.Cast(target);
                        if (DebugC["Debug"].GetValue<MenuBool>().Enabled)
                        {

                            Game.Print("Casting Q for immobile enemy");
                            Console.WriteLine("Casting Q for immobile enemy ");
                        }
                    }
                }
            }

        }

    }

}