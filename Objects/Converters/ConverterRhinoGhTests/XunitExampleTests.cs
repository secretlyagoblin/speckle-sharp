using System;

using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using Rhino;
using Xunit;


namespace ConverterRhinoGhTests
{
    /// <summary>
    /// XUnit tests
    /// </summary>
    [Collection("RhinoTestingCollection")]
    public class XunitExampleTests 
    {
        RhinoTestFixture _fixture;
        private RhinoDoc doc;
        
        public XunitExampleTests(RhinoTestFixture fixture)
        {
           fixture = _fixture;
           doc = Rhino.RhinoDoc.Create("meters");
        }
        
        [Fact]
        public void Brep_ToSpeckle()
        {
            
            var kit = Speckle.Core.Kits.KitManager.GetDefaultKit();
            var converter = kit.LoadConverter(Speckle.Core.Kits.Applications.Rhino7);
            converter.SetContextDocument(doc);
            
            // Arrange
            var bb = new BoundingBox(new Point3d(0, 0, 0), new Point3d(100, 100, 100));
            var brep = bb.ToBrep();
            var spcklBrep = converter.ConvertToSpeckle(brep);
            
            Assert.NotNull(spcklBrep);
            Assert.IsType<Objects.Geometry.Brep>(spcklBrep);
        }
    }
}