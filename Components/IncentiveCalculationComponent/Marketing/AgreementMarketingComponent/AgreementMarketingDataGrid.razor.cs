using System.Text.Json.Nodes;
using iFinancing360.UI.Helper;
using iFinancing360.UI.Helper.APIClient;
using iFinancing360.UI.Components;
using Microsoft.AspNetCore.Components;
using SixLabors.ImageSharp.Memory;

namespace IFinancing360_ICS_UI.Components.IncentiveCalculationComponent.Marketing.AgreementMarketingComponent
{
  public partial class AgreementMarketingDataGrid
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = default!;
    #endregion


    #region Component Field
    DataGrid<JsonObject> dataGrid = null!;
    [Parameter, EditorRequired] public string? ID { get; set; }
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
      BodyResponse<List<JsonObject>>? res = await IFINICSClient.GetRows<JsonObject>("AgreementIncentiveMarketing", "GetRowsByIncentiveID", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        incentiveID = ID
      });
      return res?.Data;
    }
    #endregion

    private string GetLink(JsonObject row)
    {
      
      return $"incentivecalculation/marketing/{ID}/agreement/{row["ID"]}";
      
    }

    #region Preview
    private async Task<byte[]> GetHTML()
    {
        var selectedData = dataGrid.selectedData;

        if (!selectedData.Any())
        {
            await NoDataSelectedAlert();
            return Array.Empty<byte>();
        }

        var row = selectedData.First();
        var ID = row["ID"]?.GetValue<string>();
        
        var result = await IFINICSClient.GetFileAsync("AgreementIncentiveMarketing", "GetHTMLPreview", new { ID = ID });
        
        return result?.Content ?? Array.Empty<byte>();
    }
    #endregion

    #region Print
    private async Task Print(string MimeType)
    {
        var selectedData = dataGrid.selectedData;

        if (!selectedData.Any())
        {
            await NoDataSelectedAlert();
            return;
        }

        bool? result = await Confirm();
        if (result == true)
        {
            Loading.Show();

            foreach (var row in selectedData)
            {
                var payload = new JsonObject
                {
                    ["ID"] = row["ID"]?.GetValue<string>(),
                    ["MimeType"] = MimeType
                };

                var result2 = await IFINICSClient.Post("AgreementMarketing", "PrintDocument", payload);
                if (result2?.Data != null)
                {
                    var data = result2.Data;

                    var Content = data["Content"]?.GetValueAsByteArray();
                    var FileName = data["Name"]?.GetValue<string>();
                    var MimeTypeResponse = data["MimeType"]?.GetValue<string>();

                    PreviewFile(Content, FileName, MimeTypeResponse);
                }
            }

            await dataGrid.Reload();
            Loading.Close();
        }
    }
    #endregion

  }
}
