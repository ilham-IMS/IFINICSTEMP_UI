using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.FlowchartNodeComponent;

public partial class NodeForm
{
   [Parameter] public string DynamicButtonProcessRoleID { get; set; }
   [Parameter] public string DynamicButtonProcessID { get; set; }
   [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
   SingleSelectLookup<JsonObject>? masterProcessLookup;

   JsonObject row = new();


   protected override async Task OnParametersSetAsync()
   {
      await base.OnParametersSetAsync();
   }

   private async void OnSubmit(JsonObject data)
   {
      Loading.Show();
      data = SetAuditInfo(data);
      data["DynamicButtonProcessRoleID"] = DynamicButtonProcessRoleID;
      data = row.Merge(data);
      #region Insert

      System.Console.WriteLine( " Ini adalah data json yang dikirim " + data.ToJsonString());

      var res = await IFINICSClient.Post("FlowchartNode","Insert", data);

      if (res?.Data != null)
      {
         NavigationManager.NavigateTo($"systemsetting/dynamicbuttonprocess/{DynamicButtonProcessID}/dynamicbuttonprocessrole/{DynamicButtonProcessRoleID}/flowchartnode");
      }

      Loading.Close();
      StateHasChanged();
      #endregion
   }

   private void Back()
   {
      NavigationManager.NavigateTo($"systemsetting/dynamicbuttonprocess/{DynamicButtonProcessID}/dynamicbuttonprocessrole/{DynamicButtonProcessRoleID}");
   }

   #region Load Master Lookup
   private async Task<List<JsonObject>?> LoadMasterProcessLookup(DataGridLoadArgs args)
   {
      // data dari ifinLOS
      var res = await IFINICSClient.GetRows<JsonObject>("MasterDynamicButtonProcess", "GetRowsForLookup", new
      {
         args.Keyword,
         args.Offset,
         args.Limit,
      });

      return res?.Data;
   }
   #endregion

}

