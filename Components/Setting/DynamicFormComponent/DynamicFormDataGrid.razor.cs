
using System.Text.Json;
using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicFormComponent
{
   public partial class DynamicFormDataGrid
   {
      #region Service
      [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
      #endregion

      #region Component Field
      DataGrid<JsonObject> dataGrid = null!;
      #endregion

      #region Field

      #endregion

      #region Parameter
      [Parameter] public string? ParentMenuURL { get; set; }
      #endregion

      #region OnInitialized

      #endregion

      #region LoadData
      protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
      {
         var res = await IFINICSClient.GetRows<JsonObject>("MasterForm", "GetRows", new
         {
            args.Keyword,
            args.Offset,
            args.Limit,
         });

         return res?.Data;
      }
      #endregion

      #region Add
      private void Add()
      {
         NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicform/add");
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

            await IFINICSClient.Delete("MasterForm", "DeleteByID", id);

            await dataGrid.Reload();
            dataGrid.selectedData.Clear();

            Loading.Close();

            StateHasChanged();
         }
      }
      #endregion
   }
}