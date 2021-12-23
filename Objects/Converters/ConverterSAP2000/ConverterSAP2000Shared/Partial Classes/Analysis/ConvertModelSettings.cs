using System;
using System.Collections.Generic;
using System.Text;
using Objects.Structural.Analysis;

namespace Objects.Converter.SAP2000
{
  public partial class ConverterSAP2000
  {
    public void modelSettingsToNative(ModelSettings modelSettings)
    {
      Model.DesignConcrete.SetCode(modelSettings.concreteCode);
      Model.DesignSteel.SetCode(modelSettings.steelCode);
      if (modelSettings.modelUnits != null) { UnitsToNative(modelSettings.modelUnits); }
    }
    public ModelSettings modelSettingsToSpeckle()
    {
      var speckleModelSettings = new ModelSettings();
      speckleModelSettings.modelUnits = UnitsToSpeckle();
      string concreteCode = "";
      Model.DesignConcrete.GetCode(ref concreteCode);
      speckleModelSettings.concreteCode = concreteCode;
      string steelCode = "";
      Model.DesignSteel.GetCode(ref steelCode);
      speckleModelSettings.steelCode = steelCode;
      return speckleModelSettings;
    }

  }
}
