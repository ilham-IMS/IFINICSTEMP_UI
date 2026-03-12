using System.Text.Json.Nodes;
using iFinancing360.UI.Helper;
using iFinancing360.UI.Helper.APIClient;
using iFinancing360.UI.Components;
using Microsoft.AspNetCore.Components;
using SixLabors.ImageSharp.Memory;

namespace IFinancing360_ICS_UI.Components.IncentiveCalculationComponent.Marketing.AgreementMarketingCommissionComponent
{
  public partial class AgreementMarketingCommissionDataGrid
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = default!;
    [Inject] IFINLOSClient IFINLOSClient { get; set; } = default!;
    #endregion


    #region Component Field
    DataGrid<JsonObject> dataGrid = null!;
    [Parameter, EditorRequired] public string? ID { get; set; }
    [Parameter, EditorRequired] public string? AgreementID { get; set; }
    [Parameter] public string? ParentMenuURL { get; set; }
    #endregion

    #region Field
    #endregion

    #region OnInitialized
    protected override async Task OnParametersSetAsync()
    {

      await base.OnParametersSetAsync();
    }
    #endregion

    #region LoadData
    protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRow<JsonObject>("AgreementIncentiveMarketing", "GetRowByID", new
      {
        ID = AgreementID
      });
      
      var resApp = await IFINLOSClient.GetRow<JsonObject>("ApplicationFee", "GetRowByApplicationMainIDFeeCode", new
      {
        ApplicationMainID = res?.Data?["ApplicationMainID"]?.GetValue<string>(),
        FeeCode = "PROV"
      });
      
      return [
        new JsonObject
        {
          ["CommDesc"] = "Insurance",
          ["CommRate"] = res?.Data?["CommissionRate"]?.GetValue<decimal>(),
          ["CommAmount"] = res?.Data?["CommissionRate"]?.GetValue<decimal>() * res?.Data?["TotalInsurancePremiAmount"]?.GetValue<decimal>()
        },
        new JsonObject
        {
          ["CommDesc"] = "Provision",
          ["CommRate"] = null,
          ["CommAmount"] = resApp?.Data?["FeeAmount"]?.GetValue<decimal>()
        }
      ];
    }
    #endregion

  }
}
