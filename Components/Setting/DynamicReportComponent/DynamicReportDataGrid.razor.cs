
using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicReportComponent
{
	public partial class DynamicReportDataGrid
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
		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
		}
		#endregion

		#region LoadData
		protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
		{
			var res = await IFINICSClient.GetRows<JsonObject>("DynamicReport", "GetRows", new
			{
				Keyword = args.Keyword,
				Offset = args.Offset,
				Limit = args.Limit,
				User = GetCurrentUser()
			});

			return res?.Data;
		}
		#endregion

		#region Add
		private void Add()
		{
			NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicreportsetting/add");
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

				await IFINICSClient.Delete("DynamicReport", "DeleteByID", id);

				await dataGrid.Reload();
				dataGrid.selectedData.Clear();

				Loading.Close();

				StateHasChanged();
			}
		}
		#endregion

		#region print
		protected async Task Print()
		{

			Console.WriteLine("Print");

			var file = await IFINICSClient.GetRow<JsonObject>("DynamicReportUser", "GetReportData", null);


			if (file?.Data != null)
			{
				var data = file.Data;

				var content = data["Content"]?.GetValueAsByteArray();
				var fileName = data["Name"]?.GetValue<string>();
				var mimeType = data["MimeType"]?.GetValue<string>();

				PreviewFile(content, fileName, mimeType);
			}
		}
		#endregion print
	}
}