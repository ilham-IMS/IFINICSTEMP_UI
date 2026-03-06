

using Microsoft.AspNetCore.Components;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using System.Text.Json.Nodes;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicButtonProcessComponent;

  public partial class DynamicButtonProcessForm
  {
    [Inject] IFINICSClient IFINICSClient { get; set; } = default!;

    #region Parameter
    [Parameter, EditorRequired] public string? ID { get; set; }
    [Parameter] public string ParentMenuURL { get; set; }
    #endregion

    #region Component Field
    SingleSelectLookup<JsonObject> moduleLookup = null!;
    SingleSelectLookup<JsonObject> parentLookup = null!;
    #endregion

    #region Field
    public Dictionary<string, string> menuTypes = new(){
      {"Link", "Link"},
      {"Parent", "Parent"},
      {"Child", "Child"}
    };
    private JsonObject row = new();
    JsonObject masterForm = new();
    List<FormControlsModel> controls = new();
    List<ExtendModel>? extend = new();
    #endregion

    #region RenderDynamicForm
    RenderFragment Form => builder =>
    {

      int seq = 0;
      foreach (var control in controls)
      {
        DynamicRenderForm(builder, ref seq, control);
      }

    };
    #endregion

    #region OnParametersSetAsync
    protected override async Task OnParametersSetAsync()
    {
   
      if (ID != null)
      {
        await GetRow();
      }
      else
      {
        row["IsActive"] = -1;
      }
      await base.OnParametersSetAsync();
    }
    #endregion

    protected async Task GetRow()
    {
      Loading.Show();
      var res = await IFINICSClient.GetRow<JsonObject>("DynamicButtonProcess", "GetRowByID", new { ID = ID });

      if (res?.Data != null)
      {
        row = res.Data;
      }
      Loading.Close();
    }

    protected async Task<List<JsonObject>?> LoadModuleLookup(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("SysModule", "GetRowsForLookup", new
      {
        Keyword = args.Keyword,
        Offset = args.Offset,
        Limit = args.Limit,
      });
      return res?.Data;
    }
    protected async Task<List<JsonObject>?> LoadParentLookup(DataGridLoadArgs args)
    {
      var res = await IFINICSClient.GetRows<JsonObject>("DynamicButtonProcess", "GetRowsForLookupParent", new
      {
        Keyword = args.Keyword,
        Offset = args.Offset,
        Limit = args.Limit,
        ModuleID = row["ModuleID"]?.GetValue<string>(),
      });
      return res?.Data;
    }


    private async void OnSubmit(JsonObject data)
    {
      Loading.Show();

      data = SetAuditInfo(data);
      data = row.Merge(data);

      #region Insert
      if (ID == null)
      {
        var res = await IFINICSClient.Post("DynamicButtonProcess", "Insert", data);

        if (res?.Data != null)
        {
          NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicbuttonprocess/{res.Data["ID"]}");
        }
      }
      #endregion

      #region Update
      else
      {
        var res = await IFINICSClient.Put("DynamicButtonProcess", "UpdateByID", data);
      }
      #endregion
      Loading.Close();
      StateHasChanged();
    }

    private void Back()
    {
      NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicbuttonprocess");
    }
  }
