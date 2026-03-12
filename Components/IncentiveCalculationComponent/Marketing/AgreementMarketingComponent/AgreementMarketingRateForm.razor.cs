using System.Text.Json.Nodes;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.IncentiveCalculationComponent.Marketing.AgreementMarketingComponent
{
  public partial class AgreementMarketingRateForm
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    [Inject] IFINLOSClient IFINLOSClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter, EditorRequired] public string? IncentiveID { get; set; }
    [Parameter, EditorRequired] public string? ID { get; set; }
    [Parameter] public string? ParentMenuURL { get; set; }
    #endregion

    #region Component Field
    
    #endregion

    #region Field
    public JsonObject row = new();
    #endregion

    #region OnInitialized
    protected override async Task OnParametersSetAsync()
    {
      
      if (ID != null)
      {
        await GetRow();
      }
      else
      {
        
      }
      
      await base.OnParametersSetAsync();
    }
    #endregion

    #region GetRow
    public async Task GetRow()
    {
      Loading.Show();
      var res = await IFINICSClient.GetRow<JsonObject>("AgreementIncentiveMarketing", "GetRowByID", new
      {
        ID = ID
      });

      if (res?.Data != null)
      {
        row = res.Data;
      }

      var resApp = await IFINLOSClient.GetRow<JsonObject>("ApplicationFee", "GetRowByApplicationMainIDFeeCode", new
      {
        ApplicationMainID = row["ApplicationMainID"]?.GetValue<string>(),
        FeeCode = "PROV"
      });
      row["BPETotalAmount"] = row["TotalRefundAmount"]?.GetValue<decimal>() + resApp?.Data?["FeeAmount"]?.GetValue<decimal>();
      row["BPETotal"] = (row["TotalRefundAmount"]?.GetValue<decimal>() + resApp?.Data?["FeeAmount"]?.GetValue<decimal>()) / row["NetFinance"]?.GetValue<decimal>(); 
      row["BPERatio"] = (row["BPETotalAmount"]?.GetValue<decimal>() - resApp?.Data?["FeeAmount"]?.GetValue<decimal>()) / row["TotalInsurancePremiAmount"]?.GetValue<decimal>();
      row["BPEIncomeIncentiveExpense"] = ((row["CommissionRate"]?.GetValue<decimal>() * row["TotalInsurancePremiAmount"]?.GetValue<decimal>()) + resApp?.Data?["FeeAmount"]?.GetValue<decimal>()) - row["BPETotalAmount"]?.GetValue<decimal>();
      row["BPEEffect"] = row["BPEIncomeIncentiveExpense"]?.GetValue<decimal>() / (row["InterestMargin"]?.GetValue<decimal>() * row["InterestMarginAmount"]?.GetValue<decimal>());

      var resFeesNon = await IFINICSClient.GetRows<JsonObject>("AgreementFee", "GetRowsByAgreementID", new
      {
        Keyword = "", 
        Offset = 0, 
        Limit = int.MaxValue,
        AgreementID = row["ID"]?.GetValue<string>(),
        IsInternalIncome = -1
      });

      row["NonInterestExpense"] = resFeesNon?.Data?.Where(fee => fee["FeeAmount"] != null).Sum(fee => fee["FeeAmount"]?.GetValue<decimal>() ?? 0);

      var resFeesInt = await IFINICSClient.GetRows<JsonObject>("AgreementFee", "GetRowsByAgreementID", new
      {
        Keyword = "", 
        Offset = 0, 
        Limit = int.MaxValue,
        AgreementID = row["ID"]?.GetValue<string>(),
        IsInternalIncome = 1
      });

      row["NonInterestIncome"] = resFeesInt?.Data?.Where(fee => fee["FeeAmount"] != null).Sum(fee => fee["FeeAmount"]?.GetValue<decimal>() ?? 0);

      row["NonInterestEffectAmount"] = row["NonInterestIncome"]?.GetValue<decimal>() - row["NonInterestExpense"]?.GetValue<decimal>();
      row["NonInterestEffect"] = row["NonInterestEffectAmount"]?.GetValue<decimal>() / (row["InterestMargin"]?.GetValue<decimal>() * row["InterestMarginAmount"]?.GetValue<decimal>());

      var totalInterestMargin = row["InterestMargin"]?.GetValue<decimal>() + row["BPEEffect"]?.GetValue<decimal>() + row["NonInterestEffect"]?.GetValue<decimal>();

      var profitBeforeMarketingIncentive = row["InterestMarginAmount"]?.GetValue<decimal>() + row["BPEIncomeIncentiveExpense"]?.GetValue<decimal>() + row["NonInterestEffectAmount"]?.GetValue<decimal>();

      row["MarketingIncentiveRatio"] = profitBeforeMarketingIncentive * 0.0384m;

      row["NetInterestMarginAfterCost"] = profitBeforeMarketingIncentive - row["MarketingIncentiveRatio"]?.GetValue<decimal>();
      Loading.Close();
      StateHasChanged();
    }
    #endregion
  }
}
