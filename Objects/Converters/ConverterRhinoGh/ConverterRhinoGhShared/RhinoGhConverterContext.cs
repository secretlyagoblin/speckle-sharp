using Rhino;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;
using Speckle.Core.Kits;
using System;
using System.Collections.Generic;
using System.Text;

namespace Objects.Converter.RhinoGh
{
  public class RhinoGhConverterContext
  {
    public RhinoGhConverterContext(RhinoDoc doc)
    {
      HasDocument = doc is RhinoDoc;
      InnerDocument = doc;
      ModelAbsoluteTolerance = doc?.ModelAbsoluteTolerance ?? 0.000001;
      ModelUnits = UnitToSpeckle(doc?.ModelUnitSystem ?? UnitSystem.None);      
      Path = doc?.Path ?? string.Empty;
      DocumentMeshParameterSettings = HasDocument ? MeshingParameters.DocumentCurrentSetting(doc) : MeshingParameters.Default;

    }

    public double ModelAbsoluteTolerance { get; }
    public string ModelUnits { get; }
    public string Path { get; }
    public bool HasDocument { get; }
    public RhinoDoc InnerDocument { get; }
    public MeshingParameters DocumentMeshParameterSettings { get; }
    public string Notes => InnerDocument?.Notes ?? string.Empty;

    private string UnitToSpeckle(UnitSystem us)
    {
      switch (us)
      {
        case UnitSystem.None:
          return Units.Meters;
        //case UnitSystem.Angstroms:
        //  break;
        //case UnitSystem.Nanometers:
        //  break;
        //case UnitSystem.Microns:
        //  break;
        case UnitSystem.Millimeters:
          return Units.Millimeters;
        case UnitSystem.Centimeters:
          return Units.Centimeters;
        //case UnitSystem.Decimeters:
        //  break;
        case UnitSystem.Meters:
          return Units.Meters;
        //case UnitSystem.Dekameters:
        //  break;
        //case UnitSystem.Hectometers:
        //  break;
        case UnitSystem.Kilometers:
          return Units.Kilometers;
        //case UnitSystem.Megameters:
        //  break;
        //case UnitSystem.Gigameters:
        //  break;
        //case UnitSystem.Microinches:
        //  break;
        //case UnitSystem.Mils:
        //  break;
        case UnitSystem.Inches:
          return Units.Inches;
        case UnitSystem.Feet:
          return Units.Feet;
        case UnitSystem.Yards:
          return Units.Yards;
        case UnitSystem.Miles:
          return Units.Miles;
        //case UnitSystem.PrinterPoints:
        //  break;
        //case UnitSystem.PrinterPicas:
        //  break;
        //case UnitSystem.NauticalMiles:
        //  break;
        //case UnitSystem.AstronomicalUnits:
        //  break;
        //case UnitSystem.LightYears:
        //  break;
        //case UnitSystem.Parsecs:
        //  break;
        //case UnitSystem.CustomUnits:
        //  break;
        case UnitSystem.Unset:
          return Units.Meters;
        default:
          throw new Speckle.Core.Logging.SpeckleException($"The Unit System \"{us}\" is unsupported.");
      }
    }
  }
}
