using SAP2000v1;
using Objects.Structural.Geometry;
using Objects.Structural.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Objects.Converter.SAP2000
{
  public partial class ConverterSAP2000
  {
    public ResultSet1D AllResultSet1dToSpeckle(List<string> frameNames)
    {
      ResultSet1D frameResults = new ResultSet1D();
      ResultSet1D pierResults = new ResultSet1D();
      ResultSet1D spandrelResults = new ResultSet1D();

      List<Result1D> combinedResults = new List<Result1D>();

      foreach (var frameName in frameNames)
      {
        frameResults = FrameResultSet1dToSpeckle(frameName);
        combinedResults.AddRange(frameResults.results1D);
      }

    

      return new ResultSet1D() { results1D = combinedResults };
    }

    public ResultSet1D FrameResultSet1dToSpeckle(string elementName)
    {
      List<Result1D> results = new List<Result1D>();

      SetLoadCombinationsForResults();

      // Reference variables for SAP2000 API
      int numberOfResults = 0;
      string[] obj, elm, loadCase, stepType;
      obj = elm = loadCase = stepType = new string[1];
      double[] objSta, elmSta, stepNum, p, v2, v3, t, m2, m3;
      objSta = elmSta = stepNum = p = v2 = v3 = t = m2 = m3 = new double[1];

      Model.Results.FrameForce(elementName, eItemTypeElm.ObjectElm, ref numberOfResults, ref obj, ref objSta, ref elm, ref elmSta, ref loadCase, ref stepType, ref stepNum, ref p, ref v2, ref v3, ref t, ref m2, ref m3);

      // Value used to normalized output station of forces between 0 and 1
      var lengthOf1dElement = objSta.Max();

      for (int i = 0; i < numberOfResults; i++)
      {
        Result1D result = new Result1D()
        {
          //element = FrameToSpeckle(elementName),
          position = (float)(objSta[i] / lengthOf1dElement),
          permutation = loadCase[i],
          dispX = 0, // values eventually populated by element.Node.{displacements}
          dispY = 0, // values eventually populated by element.Node.{displacements}
          dispZ = 0, // values eventually populated by element.Node.{displacements}
          rotXX = 0, // values eventually populated by element.Node.{displacements}
          rotYY = 0, // values eventually populated by element.Node.{displacements}
          rotZZ = 0, // values eventually populated by element.Node.{displacements}
          forceX = (float)v3[i],
          forceY = (float)v2[i],
          forceZ = (float)p[i],
          momentXX = (float)m3[i],
          momentYY = (float)m2[i],
          momentZZ = (float)t[i],
          axialStress = 0, // values eventually populated when element1d.section values are available
          shearStressY = 0, // values eventually populated when element1d.section values are available
          shearStressZ = 0, // values eventually populated when element1d.section values are available
          bendingStressYPos = 0, // values eventually populated when element1d.section values are available
          bendingStressYNeg = 0, // values eventually populated when element1d.section values are available
          bendingStressZPos = 0, // values eventually populated when element1d.section values are available
          bendingStressZNeg = 0, // values eventually populated when element1d.section values are available
          combinedStressMax = 0, // values eventually populated when element1d.section values are available
          combinedStressMin = 0 // values eventually populated when element1d.section values are available
        };

        results.Add(result);
      }

      return new ResultSet1D() { results1D = results };



    }

  

    public void SetLoadCombinationsForResults()
    {
      int numberOfLoadCombinations = 0;
      string[] loadCombinationNames = new string[1];

      Model.RespCombo.GetNameList(ref numberOfLoadCombinations, ref loadCombinationNames);

      foreach (var loadCombination in loadCombinationNames)
      {
        Model.Results.Setup.SetComboSelectedForOutput(loadCombination);
      }
    }
  }
}