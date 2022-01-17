using System;
using Objects.Geometry;
using Objects.Utils;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using System.Linq;
using Speckle.Newtonsoft.Json;

namespace Objects.BuiltElements.Revit
{
  public class AdaptiveComponent : Base, IDisplayMesh, IDisplayValues
  {
    public string type { get; set; }
    public string family { get; set; }
    public List<Point> basePoints { get; set; }
    public bool flipped { get; set; }
    public string elementId { get; set; }
    public Base parameters { get; set; }

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

    public AdaptiveComponent() { }

    [SchemaInfo("AdaptiveComponent", "Creates a Revit adaptive component by points", "Revit", "Families")]
    public AdaptiveComponent(string type, string family, List<Point> basePoints, bool flipped = false, List<Parameter> parameters = null)
    {
      this.type = type;
      this.family = family;
      this.basePoints = basePoints;
      this.flipped = flipped;
      this.parameters = parameters.ToBase();
    }
  }
}