using System;
using Objects.Geometry;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System.Collections.Generic;
using System.Linq;
using Speckle.Newtonsoft.Json;

namespace Objects.BuiltElements.Revit
{
  public class BuildingPad : Base, IDisplayMesh, IDisplayValues
  {
    public ICurve outline { get; set; }
    public List<ICurve> voids { get; set; } = new List<ICurve>();
    public string type { get; set; }
    public Level level { get; set; }
    public Base parameters { get; set; }
    public string elementId { get; set; }

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

    public BuildingPad() { }
  }
}
