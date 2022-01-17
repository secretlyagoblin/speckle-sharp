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
  public class Topography : Base, IDisplayMesh, IDisplayValues
  {
    public Mesh baseGeometry { get; set; } = new Mesh();

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

    public Topography()
    {
      this.displayMesh = new Mesh();
    }

    [SchemaInfo("Topography", "Creates a Speckle topography", "BIM", "Architecture")]
    public Topography([SchemaMainParam] Mesh displayMesh)
    {
      this.displayMesh = displayMesh;
    }
  }
}

namespace Objects.BuiltElements.Revit
{
  public class RevitTopography : Topography
  {
    public string elementId { get; set; }
    public Base parameters { get; set; }

    public RevitTopography() { }

    [SchemaInfo("RevitTopography", "Creates a Revit topography", "Revit", "Architecture")]
    public RevitTopography([SchemaMainParam] Mesh displayMesh, List<Parameter> parameters = null)
    {
      this.displayMesh = displayMesh;
      this.parameters = parameters.ToBase();
    }
  }
}
