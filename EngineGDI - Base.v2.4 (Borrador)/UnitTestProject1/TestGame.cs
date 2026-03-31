using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineGDI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class TestGame
    {
        [TestMethod]
        public void TestGetDamage()
        {
            Character c = new Character(new Vector2(0, 0), 95, 101, null);
            c.DamageLife(5);

            Assert.AreEqual(95.0f, c.Life);
        }

        [TestMethod]
        public void TestGetDamage2()
        {
            Character1 c = new Character1(new Vector2(0, 0), 95, 101, null);
            c.DamageLife(6);

            Assert.AreNotEqual(95.0f, c.Life);
        }
    }
}
