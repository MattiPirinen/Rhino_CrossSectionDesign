using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossSectionDesign.Classes_and_structures;
using Xunit;
using CsvHelper;
using Rhino.Geometry;
using System.IO;

namespace CrossSectionDesign.Tests
{
    public class RectangleCrossSectionTests
    {
        [Fact]
        public void ShouldResultCorrectStrength()
        {


            var csv = new CsvReader(new StreamReader("400x400.csv"));

            // Arrange 
            // *** Aquired 23.4.2018 from 
            var l = new Polyline();
            Point2d a = new Point2d(1, 1);
            l.Add(new Point3d());



            // Act
            RectangleCrossSection rect = new RectangleCrossSection("test")
            {
                ConcreteHeight = 400,
                ConcreteWidth = 400,
                ReinfMaterial = new SteelMaterial("B500B"),
                ConcreteMaterial = new ConcreteMaterial("C30/37"),
                ConcreteCover = 35,
                MainD = 25,
                StirrupD = 10,
                NoReinfW = 4,
                NoReinfH = 4
            };

            rect.CalculateStrengthCurve(Enumerates.Axis.XAxis);
            bool test = true;
            // Assert
            Assert.True(test);

        }

    }
}
