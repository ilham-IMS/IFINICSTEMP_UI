using System.Text.Json.Nodes;
using iFinancing360.UI.Helper;
using iFinancing360.UI.Helper.APIClient;
using iFinancing360.UI.Components;
using Microsoft.AspNetCore.Components;
using SixLabors.ImageSharp.Memory;

namespace IFinancing360_ICS_UI.Components.IncentiveCalculationComponent.Collection
{
  public partial class IncentiveCollectionDataGrid
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = default!;
    #endregion


    #region Component Field
    DataGrid<JsonObject> dataGrid = null!;
    #endregion

    #region Field
    public JsonObject filter = new();
    #endregion

    #region OnInitialized
    protected override async Task OnParametersSetAsync()
    {

      filter["ProductOfferingID"] = "ALL";
      filter["ProductOfferingName"] = "ALL";

      await base.OnParametersSetAsync();
    }
    #endregion

    #region LoadData
    protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
    {
      BodyResponse<List<JsonObject>>? res = await IFINICSClient.GetRows<JsonObject>("IncentiveCollection", "GetRows", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        PeriodeFrom = filter["PeriodeFrom"]?.GetValue<string>(),
        PeriodeTo = filter["PeriodeTo"]?.GetValue<string>(),
      });
      return res?.Data;
    }
    #endregion

    private string GetLink(JsonObject row)
    {
      
      return $"incentivecalculation/collection/{row["ID"]}";
      
    }

    #region GetHTMLPreview
    private async Task<string> GetHTMLPreview()
    {
        var ids = dataGrid.Data
            .Select(row => row["ID"]?.GetValue<string>())
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();

        if (!ids.Any())
        {
            await NoDataSelectedAlert();
            return "";
        }

        bool? result = await Confirm();
        if (result == true)
        {
            Loading.Show();

            
            var result2 = await IFINICSClient.Post(
                "IncentiveCollection",
                "GetHTMLPreview", 
                ids,
                new {  
                  PeriodeFrom = filter["PeriodeFrom"]?.GetValue<string>(), 
                  PeriodeTo = filter["PeriodeTo"]?.GetValue<string>()
                  }
            );
            
            string html = result2?.Data["HTML"]?.GetValue<string>() ?? "<p>Default screen</p>";
            Loading.Close();

            return html;
        }
        return "";
    }
    #endregion

    #region PrintDocument
    private async Task PrintDocument(string mimeType)
    {
        var ids = dataGrid.Data
            .Select(row => row["ID"]?.GetValue<string>())
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();

        if (!ids.Any())
        {
            await NoDataSelectedAlert();
            return;
        }

        bool? result = await Confirm();
        if (result == true)
        {
            Loading.Show();
            
                var result2 = await IFINICSClient.Post(
                    "IncentiveCollection",
                    "PrintDocument",
                    ids,
                    new
                    {
                        mimeType = mimeType,
                        PeriodeFrom = filter["PeriodeFrom"]?.GetValue<string>(),
                        PeriodeTo = filter["PeriodeTo"]?.GetValue<string>()
                    });

                if (result2?.Data != null)
                {
                    var data = result2.Data;
                    var Content = data["Content"]?.GetValueAsByteArray();
                    var FileName = data["Name"]?.GetValue<string>();
                    var MimeType = data["MimeType"]?.GetValue<string>();

                    PreviewFile(Content, FileName, MimeType);
                }
            
            
            Loading.Close();
        }
    }
    #endregion
  }
}
