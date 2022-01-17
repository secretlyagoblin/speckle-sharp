using Objects.Geometry;
using Objects.Utils;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Speckle.Newtonsoft.Json;

namespace Objects.BuiltElements
{
  public class Beam : Base, IDisplayMesh, IDisplayValues<Mesh>
  {
    public ICurve baseLine { get; set; }

    #region DisplayValues
    [JsonIgnore, Obsolete("Use " + nameof(displayValues) + " instead")]
    public Mesh displayMesh {
      get => displayValues?.FirstOrDefault();
      set => displayValues = new List<Mesh> {value};
    }

    [DetachProperty]
    public List<Mesh> displayValues { get; set; }
    [JsonIgnore] IReadOnlyList<Base> IDisplayValues.displayValues => displayValues;
    #endregion


    public string units { get; set; }

    public Beam() { }

    [SchemaInfo("Beam", "Creates a Speckle beam", "BIM", "Structure")]
    public Beam([SchemaMainParam] ICurve baseLine)
    {
      this.baseLine = baseLine;
    }
  }
}

namespace Objects.BuiltElements.Revit
{
  public class RevitBeam : Beam
  {
    public string family { get; set; }
    public string type { get; set; }
    public Base parameters { get; set; }
    public string elementId { get; set; }
    public Level level { get; set; }

    public RevitBeam() { }

    [SchemaInfo("RevitBeam", "Creates a Revit beam by curve and base level.", "Revit", "Structure")]
    public RevitBeam(string family, string type, [SchemaMainParam] ICurve baseLine, Level level, List<Parameter> parameters = null)
    {
      this.family = family;
      this.type = type;
      this.baseLine = baseLine;
      this.parameters = parameters.ToBase();
      this.level = level;
    }
  }
}
