using NUnit.Framework;
using Gotcha.Levels.V64;

namespace Gotcha.Tests
{
    /// <summary>Livello 254: la sveglia del vicino (comodino pesante, biglia nella crepa, un suono forte).</summary>
    public class V64NeighboursAlarmTests
    {
        const float Dt = 1f / 60f;

        static MarbleEvent Roll(NeighboursAlarmLogic l, float tilt, float seconds)
        {
            MarbleEvent last = MarbleEvent.None;
            for (float t = 0f; t < seconds; t += Dt)
            {
                MarbleEvent e = l.StepMarble(tilt, Dt);
                if (e != MarbleEvent.None) last = e;
                if (e == MarbleEvent.Fell) break;
            }
            return last;
        }

        static void MoveTable(NeighboursAlarmLogic l, float fingersDx)
        {
            for (int i = 0; i < 20; i++) l.DragTable(fingersDx / 20f);
        }

        [Test]
        public void CorrectPath_Completes()
        {
            var l = new NeighboursAlarmLogic();
            MoveTable(l, 2f);
            Assert.AreEqual(AlarmStage.CrackOpen, l.Stage);
            Assert.IsTrue(l.CrackExposed);
            Assert.IsFalse(l.NeedsMic);
            Assert.AreEqual(MarbleEvent.Fell, Roll(l, 0.3f, 5f));
            Assert.AreEqual(AlarmStage.Neighbour, l.Stage);
            Assert.IsTrue(l.NeedsMic);
            Assert.IsTrue(l.Ringing);
            Assert.IsTrue(l.LoudSound());
            Assert.IsFalse(l.LoudSound());            // un secondo suono non riparte
            Assert.IsFalse(l.Ringing);
            Assert.IsTrue(l.TryComplete());
            Assert.IsFalse(l.TryComplete());
        }

        [Test]
        public void TableIsHeavy_AndStaysMoved()
        {
            var l = new NeighboursAlarmLogic();
            MoveTable(l, 1f);
            Assert.AreEqual(NeighboursAlarmLogic.TableWeight, l.TableOffset, 1e-4);   // si muove meno delle dita
            Assert.AreEqual(AlarmStage.Ringing, l.Stage);
            MoveTable(l, 1f);
            Assert.AreEqual(AlarmStage.CrackOpen, l.Stage);
            MoveTable(l, -5f);                                                           // non torna indietro
            Assert.AreEqual(NeighboursAlarmLogic.RevealOffset, l.TableOffset, 1e-4);
            Assert.IsTrue(l.CrackExposed);
            MoveTable(l, 10f);
            Assert.AreEqual(NeighboursAlarmLogic.TableMaxOffset, l.TableOffset, 1e-4);
        }

        [Test]
        public void TableCannotGoTowardsTheBed()
        {
            var l = new NeighboursAlarmLogic();
            MoveTable(l, -3f);
            Assert.AreEqual(0f, l.TableOffset);
            Assert.AreEqual(AlarmStage.Ringing, l.Stage);
        }

        [Test]
        public void MarbleBumpsIntoTable_WhenCrackHidden()
        {
            var l = new NeighboursAlarmLogic();
            Assert.AreEqual(MarbleEvent.Bump, Roll(l, 1f, 3f));
            Assert.AreEqual(AlarmStage.Ringing, l.Stage);
            Assert.LessOrEqual(l.MarbleX, NeighboursAlarmLogic.TableHomeX - NeighboursAlarmLogic.TableHalfWidth);
        }

        [Test]
        public void MarbleStaysInsideTheRoom()
        {
            var l = new NeighboursAlarmLogic();
            Roll(l, -1f, 5f);
            Assert.GreaterOrEqual(l.MarbleX, NeighboursAlarmLogic.WallLeft);
            Roll(l, 1f, 5f);
            Assert.Less(l.MarbleX, l.TableX);
        }

        [Test]
        public void TinyTilt_DoesNotMoveTheMarble()
        {
            var l = new NeighboursAlarmLogic();
            Roll(l, 0.04f, 5f);
            Assert.AreEqual(NeighboursAlarmLogic.MarbleStartX, l.MarbleX, 1e-4);
        }

        [Test]
        public void FastMarble_StillFallsIntoTheCrack()
        {
            var l = new NeighboursAlarmLogic();
            MoveTable(l, 3f);
            Roll(l, -1f, 3f);                                  // rincorsa da sinistra
            Assert.AreEqual(MarbleEvent.Fell, Roll(l, 1f, 4f)); // a tutta velocità
        }

        [Test]
        public void MarbleThatWasAgainstTheTable_FallsWhenItMoves()
        {
            var l = new NeighboursAlarmLogic();
            Roll(l, 1f, 3f);                                    // appoggiata al comodino
            MoveTable(l, 2f);
            Assert.AreEqual(MarbleEvent.Fell, Roll(l, 0.2f, 3f));
        }

        [Test]
        public void SoundBeforeTheNeighbour_DoesNothing()
        {
            var l = new NeighboursAlarmLogic();
            Assert.IsFalse(l.LoudSound());
            MoveTable(l, 2f);
            Assert.IsFalse(l.LoudSound());
            Assert.AreEqual(AlarmStage.CrackOpen, l.Stage);
        }

        [Test]
        public void ClockRedHerrings()
        {
            var l = new NeighboursAlarmLogic();
            int h = l.ClockHour, m = l.ClockMinute;
            Assert.IsTrue(l.TapClock());
            Assert.IsTrue(l.ClockHour != h || l.ClockMinute != m);
            Assert.AreEqual(2, l.Loudness);
            for (int i = 0; i < 10; i++) l.TapClock();
            Assert.AreEqual(NeighboursAlarmLogic.MaxLoudness, l.Loudness);
            Assert.That(l.ClockHour, Is.InRange(1, 12));
            Assert.That(l.ClockMinute, Is.InRange(0, 59));
            l.HoldClock(true);
            Assert.IsTrue(l.ClockHeld);
            Assert.IsTrue(l.Ringing);                        // continua dal pavimento
            l.HoldClock(false);
            l.SetPillowOnClock(true);
            Assert.IsTrue(l.Ringing);
            Assert.IsTrue(l.Shake());
            Assert.IsTrue(l.ClockFallen);
            Assert.IsFalse(l.PillowOnClock);
            Assert.IsFalse(l.Shake());                       // cade una volta sola
            Assert.IsTrue(l.Ringing);
            Assert.AreEqual(AlarmStage.Ringing, l.Stage);
        }

        [Test]
        public void ClockFallen_LevelStillSolvable()
        {
            var l = new NeighboursAlarmLogic();
            l.Shake();
            MoveTable(l, 2f);
            Assert.AreEqual(MarbleEvent.Fell, Roll(l, 0.5f, 4f));
            Assert.IsTrue(l.LoudSound());
        }

        [Test]
        public void Impulse_OneShot_IgnoresOngoingNoise_AndContinuousNoise()
        {
            var imp = new V64AImpulse();
            imp.Reset();
            // rumore forte già in corso quando la fase comincia: non conta
            int fired = 0;
            for (int i = 0; i < 30; i++) if (imp.Feed(0.9f, Dt)) fired++;
            Assert.AreEqual(0, fired);
            // silenzio, poi un battito di mani
            for (int i = 0; i < 30; i++) imp.Feed(0.03f, Dt);
            Assert.IsTrue(imp.Armed);
            Assert.IsTrue(imp.Feed(0.95f, Dt));
            // il suono che continua non riscatta
            for (int i = 0; i < 60; i++) Assert.IsFalse(imp.Feed(0.95f, Dt));
            // un suono moderato non basta
            for (int i = 0; i < 30; i++) imp.Feed(0.03f, Dt);
            Assert.IsFalse(imp.Feed(0.3f, Dt));
        }

        [Test]
        public void Reset_RestoresEverything()
        {
            var l = new NeighboursAlarmLogic();
            MoveTable(l, 2f);
            Roll(l, 1f, 4f);
            l.LoudSound();
            l.TryComplete();
            l.Reset();
            Assert.AreEqual(AlarmStage.Ringing, l.Stage);
            Assert.AreEqual(0f, l.TableOffset);
            Assert.AreEqual(NeighboursAlarmLogic.MarbleStartX, l.MarbleX);
            Assert.IsFalse(l.CompletionReported);
        }
    }
}
