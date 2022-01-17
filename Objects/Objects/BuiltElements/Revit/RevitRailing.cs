using System;
using Objects.Geometry;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using System.Linq;
using Speckle.Newtonsoft.Json;

namespace Objects.BuiltElements.Revit
{
  public class RevitRailing : Base, IDisplayMesh, IDisplayValues
  {
    //public string family { get; set; }
    public string type { get; set; }
    public Level level { get; set; }
    public Polycurve path { get; set; }
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

    public RevitRailing() { }

    [SchemaInfo("Railing", "Creates a Revit railing by base curve.", "Revit", "Architecture")]
    public RevitRailing(string type, [SchemaMainParam] Polycurve baseCurve, Level level, bool flipped = false)
    {
      this.type = type;
      this.path = baseCurve;
      this.level = level;
      this.flipped = flipped;
    }
  }
}
