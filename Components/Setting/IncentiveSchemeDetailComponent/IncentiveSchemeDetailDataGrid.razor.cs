using System.Text.Json.Nodes;
using iFinancing360.UI.Helper.APIClient;
using iFinancing360.UI.Components;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.IncentiveSchemeDetailComponent
{
  public partial class IncentiveSchemeDetailDataGrid
  {
    #region Service
    [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
    #endregion

    #region Component Field
    public DataGrid<JsonObject> dataGrid = null!;
    #endregion

    #region Field

    #endregion
    #region Parameter
    [Parameter] public string? ParentMenuURL { get; set; }
    [Parameter] public string? SchemeID { get; set; }
    [Parameter, EditorRequired] public EventCallback ReloadParent { get; set; }

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
      var res = await IFINICSClient.GetRows<JsonObject>("IncentiveSchemeDetail", "GetRowsBySchemeID", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        SchemeID,
      });

      return res?.Data;
    }
    #endregion

    #region Add
    private async void Add()
    {
      Loading.Show();

      #region Insert
      if (SchemeID != null)
      {
        var res = await IFINICSClient.Post("IncentiveSchemeDetail", "Insert", new { IncentiveSchemeID = SchemeID });

        if (res?.Data != null)
        {
          await ReloadParent.InvokeAsync();
          await dataGrid.Reload();
        }
      }
      #endregion
      Loading.Close();
    }
    #endregion

    #region Save 
    private async void Save(JsonObject data)
    {
        Loading.Show();
        List<JsonObject> list = new();

        foreach (var (key, value) in data)
        {
            var id = key.Split("_").Last();
            var objKey = key.Split("_").First();

            if (string.IsNullOrEmpty(id)) continue;

            var row = list.Find(x => x["ID"]?.GetValue<string>() == id);

            if (row == null)
            {
                row = SetAuditInfo(new JsonObject
                {
                    ["ID"] = id,
                    ["IncentiveSchemeID"] = SchemeID 
                });

                list.Add(row);
            }

            row[objKey] = value?.DeepClone();
        }

        await IFINICSClient.Put("IncentiveSchemeDetail", "UpdateByID", list);

        await ReloadParent.InvokeAsync();
        await dataGrid.Reload();

        Loading.Close();
    }
    #endregion

    #region Delete
    private async void Delete()
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

        List<string?> id = dataGrid.selectedData.Select(row => row["ID"]?.GetValue<string>()).ToList();

        await IFINICSClient.Delete("IncentiveSchemeDetail", "DeleteByID", id,
        new
        {
            IncentiveSchemeID = SchemeID
        });

        await dataGrid.Reload();
        await ReloadParent.InvokeAsync();
        dataGrid.selectedData.Clear();

        Loading.Close();

        StateHasChanged();
      }
    }
    #endregion
  }
}
