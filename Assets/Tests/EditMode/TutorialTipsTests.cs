using System.Collections.Generic;
using NUnit.Framework;
using Gotcha.UI;

namespace Gotcha.Tests
{
    /// <summary>Il mini tutorial dei primi 8 livelli: quale consiglio, quando si mostra e le curve della scheda.</summary>
    public class TutorialTipsTests
    {
        static readonly string[] FirstEight = { "key_door", "tilt_maze", "do_nothing", "slice_apple", "beep_code", "defuse", "shush", "hold_both" };

        [Test]
        public void OneTipForEachOfTheFirstEightLevels_InOrder_WithDistinctIcons()
        {
            Assert.AreEqual(8, TutorialTips.All.Count);
            var icons = new HashSet<TipIcon>();
            for (int i = 0; i < FirstEight.Length; i++)
            {
                Assert.IsTrue(TutorialTips.TryGet(FirstEight[i], out TutorialTip tip), FirstEight[i]);
                Assert.AreEqual(FirstEight[i], TutorialTips.All[i].LevelId);
                Assert.AreNotEqual(TipIcon.None, tip.Icon);
                Assert.IsTrue(icons.Add(tip.Icon), "icona ripetuta: " + tip.Icon);
                Assert.IsFalse(string.IsNullOrEmpty(tip.Caption));
                Assert.AreEqual(tip.Caption.ToUpperInvariant(), tip.Caption, "le frasi sono in maiuscolo come il resto della UI");
                Assert.LessOrEqual(tip.Caption.Length, 40, "una riga corta");
            }
        }

        [Test]
        public void CaptionsAndIconsMatchTheOwnersRequest()
        {
            Expect("key_door", TipIcon.Pinch, "PINCH WITH TWO FINGERS");
            Expect("tilt_maze", TipIcon.Flat, "HOLD THE PHONE FLAT, SCREEN UP, AND TILT");
            Expect("do_nothing", TipIcon.Clue, "EVERYTHING ON SCREEN COULD BE A CLUE");
            Expect("slice_apple", TipIcon.Touch, "TAP, DRAG AND SWIPE THINGS");
            Expect("beep_code", TipIcon.Listen, "LISTEN! SOUND MATTERS");
            Expect("defuse", TipIcon.Time, "WATCH THE CLOCK: TIME MATTERS");
            Expect("shush", TipIcon.Buttons, "YOUR PHONE'S BUTTONS WORK TOO");
            Expect("hold_both", TipIcon.Multitouch, "MULTITOUCH: USE MORE THAN ONE FINGER");
        }

        static void Expect(string id, TipIcon icon, string caption)
        {
            Assert.IsTrue(TutorialTips.TryGet(id, out TutorialTip tip));
            Assert.AreEqual(icon, tip.Icon);
            Assert.AreEqual(caption, tip.Caption);
        }

        [Test]
        public void OtherLevelsAndBadIdsHaveNoTip()
        {
            Assert.IsFalse(TutorialTips.TryGet("bug_spray", out _));
            Assert.IsFalse(TutorialTips.TryGet("", out _));
            Assert.IsFalse(TutorialTips.TryGet(null, out _));
            Assert.IsFalse(TutorialTips.TryGet("KEY_DOOR", out _));
            Assert.IsFalse(TutorialTips.ShouldShow("bug_spray", false, out _));
        }

        [Test]
        public void ShownUntilTheLevelHasBeenSolvedOnce()
        {
            // dalla 0.76 "superato" è per identificativo (SaveData.IsCompleted), non per posizione
            Assert.IsTrue(TutorialTips.ShouldShow("key_door", false, out TutorialTip tip));
            Assert.AreEqual(TipIcon.Pinch, tip.Icon);
            Assert.IsTrue(TutorialTips.ShouldShow("key_door", false, out _));
            Assert.IsFalse(TutorialTips.ShouldShow("key_door", true, out _));
            for (int i = 0; i < FirstEight.Length; i++) Assert.IsFalse(TutorialTips.ShouldShow(FirstEight[i], true, out _));
            for (int i = 0; i < FirstEight.Length; i++) Assert.IsTrue(TutorialTips.ShouldShow(FirstEight[i], false, out _));
        }

        [Test]
        public void PopStartsSmall_OvershootsALittle_AndSettlesAtOne()
        {
            Assert.AreEqual(TutorialTips.PopFrom, TutorialTips.PopScale(0f), 1e-5f);
            Assert.AreEqual(TutorialTips.PopFrom, TutorialTips.PopScale(-1f), 1e-5f);
            Assert.AreEqual(1f, TutorialTips.PopScale(TutorialTips.PopSeconds), 1e-5f);
            Assert.AreEqual(1f, TutorialTips.PopScale(3f), 1e-5f);
            float max = 0f;
            for (float t = 0f; t <= TutorialTips.PopSeconds; t += 0.005f) max = System.Math.Max(max, TutorialTips.PopScale(t));
            Assert.Greater(max, 1.01f, "il rimbalzo");
            Assert.Less(max, 1.15f, "un rimbalzo piccolo");
        }

        [Test]
        public void AlphaFadesInStaysAndFadesOut()
        {
            float fade = TutorialTips.ShowSeconds;
            Assert.AreEqual(0f, TutorialTips.Alpha(0f, fade), 1e-5f);
            Assert.AreEqual(1f, TutorialTips.Alpha(1f, fade), 1e-5f);
            Assert.AreEqual(1f, TutorialTips.Alpha(4.9f, fade), 1e-5f);
            Assert.AreEqual(0.5f, TutorialTips.Alpha(fade + TutorialTips.FadeSeconds * 0.5f, fade), 1e-4f);
            Assert.AreEqual(0f, TutorialTips.Alpha(fade + TutorialTips.FadeSeconds, fade), 1e-5f);
            Assert.IsFalse(TutorialTips.Finished(fade, fade));
            Assert.IsTrue(TutorialTips.Finished(fade + TutorialTips.FadeSeconds, fade));
            Assert.Greater(TutorialTips.ShowSeconds, 4f);
            Assert.LessOrEqual(TutorialTips.ShowSeconds, 6f);
        }

        [Test]
        public void TappingTheCardStartsTheFadeAtOnce_ButNeverLater()
        {
            float fade = TutorialTips.FadeStartAfterTap(1.2f, TutorialTips.ShowSeconds);
            Assert.AreEqual(1.2f, fade, 1e-5f);
            Assert.IsTrue(TutorialTips.Finished(1.2f + TutorialTips.FadeSeconds, fade));
            // un secondo tocco durante la dissolvenza non la fa ripartire
            Assert.AreEqual(1.2f, TutorialTips.FadeStartAfterTap(1.4f, fade), 1e-5f);
            // toccata dopo la dissolvenza automatica: resta quella
            Assert.AreEqual(TutorialTips.ShowSeconds, TutorialTips.FadeStartAfterTap(5.2f, TutorialTips.ShowSeconds), 1e-5f);
        }
    }
}
