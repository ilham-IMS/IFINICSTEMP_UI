using System.Text.Json.Nodes;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.IncentiveCalculationComponent.Collection
{
  public partial class IncentiveCollectionForm
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Parameter
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
      var res = await IFINICSClient.GetRow<JsonObject>("IncentiveCollection", "GetRowByID", new
      {
        ID = ID
      });

      if (res?.Data != null)
      {
        row = res.Data;
      }

      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region OnSubmit
    private async Task OnSubmit(JsonObject data)
    {
      Loading.Show();

      data = SetAuditInfo(data);

      data = row.Merge(data);

      #region Insert
      if (ID == null)
      {
        var res = await IFINICSClient.Post("IncentiveCollection", "Insert", data);

        if (res?.Data != null)
        {
          NavigationManager.NavigateTo($"{ParentMenuURL}/incentivecollection/{res.Data["ID"]}");
        }
      }
      #endregion

      #region Update
      else
      {
        var res = await IFINICSClient.Put("IncentiveCollection", "UpdateByID", data);
        if (res?.Data != null)
        {
          await GetRow();
        }
      }

      Loading.Close();
      StateHasChanged();
      #endregion
    }
    #endregion

    #region Back
    private void Back()
    {
      NavigationManager.NavigateTo($"{ParentMenuURL}/collection");
    }
    #endregion

    #region GetHTMLPreview
    private async Task<string> GetHTMLPreview()
    {

        bool? result = await Confirm();
        if (result == true)
        {
          Loading.Show();

          var ids = new List<string> { row["ID"]?.GetValue<string>() };
          
          var result2 = await IFINICSClient.Post(
              "IncentiveCollection",
              "GetHTMLPreview", 
              ids
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
        

        bool? result = await Confirm();
        if (result == true)
        {
            Loading.Show();
            
            var ids = new List<string> { row["ID"]?.GetValue<string>() };
        
            var result2 = await IFINICSClient.Post(
                "IncentiveCollection",
                "PrintDocument",
                ids,
                new
                {
                    mimeType = mimeType,
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
