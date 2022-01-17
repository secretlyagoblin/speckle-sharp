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
  public class FamilyInstance : Base, IDisplayMesh, IDisplayValues
  {
    public Point basePoint { get; set; }
    public string family { get; set; }
    public string type { get; set; }
    public string category { get; set; }
    public Level level { get; set; }
    public double rotation { get; set; }
    public bool facingFlipped { get; set; }
    public bool handFlipped { get; set; }
    public Base parameters { get; set; }
    public string elementId { get; set; }

    [DetachProperty]
    public List<Base> elements { get; set; }

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

    public FamilyInstance() { }

    [SchemaInfo("FamilyInstance", "Creates a Revit family instance", "Revit", "Families")]
    public FamilyInstance(Point basePoint, string family, string type, Level level,
      double rotation = 0, bool facingFlipped = false, bool handFlipped = false,
      List<Parameter> parameters = null)
    {
      this.basePoint = basePoint;
      this.family = family;
      this.type = type;
      this.level = level;
      this.rotation = rotation;
      this.facingFlipped = facingFlipped;
      this.handFlipped = handFlipped;
      this.parameters = parameters.ToBase();
    }
  }
}
