using System;
using System.Collections.Generic;
using System.Text;
using Objects.Structural.Geometry;
using SAP2000v1;
using System.Linq;

namespace Objects.Converter.SAP2000
{
  public partial class ConverterSAP2000
  {

    public string ModelUnits()
    {
      var units = Model.GetDatabaseUnits();
      if (units != 0)
      {
        string[] unitsCat = units.ToString().Split('_');
        return unitsCat[1];
      }
      else
      {
        return null;
      }
    }
    public static List<string> GetAllFrameNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.FrameObj.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }

    public static List<string> GetAllAreaNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.AreaObj.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }

    public bool[] RestraintToNative(Restraint restraint)
    {
      bool[] restraints = new bool[6];

      var code = restraint.code;

      int i = 0;
      foreach (char c in code)
      {
        restraints[i] = c.Equals('F') ? true : false; // other assume default of released 
        i++;
      }

      return restraints;
    }

    public double[] PartialRestraintToNative(Restraint restraint)
    {
      double[] partialFix = new double[6];
      partialFix[0] = restraint.stiffnessX;
      partialFix[1] = restraint.stiffnessY;
      partialFix[2] = restraint.stiffnessZ;
      partialFix[3] = restraint.stiffnessXX;
      partialFix[4] = restraint.stiffnessYY;
      partialFix[5] = restraint.stiffnessZZ;
      return partialFix;
    }

    public Restraint RestraintToSpeckle(bool[] releases)
    {
      var code = new List<string>() { "R", "R", "R", "R", "R", "R" }; // default to free
      if (releases != null)
      {
        for (int i = 0; i < releases.Length; i++)
        {
          if (releases[i]) code[i] = "F";
        }
      }

      var restraint = new Restraint(string.Join("", code));
      return restraint;
    }
    public static List<string> GetAllPointNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.PointObj.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }

    }

    public enum SAP2000ConverterSupported
    {
      Node,
      Line,
      Element1D,
      Element2D,
      Model,
    }

    public enum SAP2000APIUsableTypes
    {
      Point,
      Frame,
      Area, // cAreaObj
      LoadPattern,
      Model,
      Column,
      Brace,
      Beam,
      Floor,
      Wall,
      Tendon,
      Links,
      Spandrel,
      Pier,
      Grids,
      BeamLoading,
      ColumnLoading,
      BraceLoading,
      FrameLoading,
      FloorLoading,
      AreaLoading,
      WallLoading,
      NodeLoading,
      //ColumnResults,
      //BeamResults,
      //BraceResults,
      //PierResults,
      //SpandrelResults
      AnalysisResults
    }
  }
}
