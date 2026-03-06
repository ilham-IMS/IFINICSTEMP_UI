
using System.Text.Json;
using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicReportTableComponent
{
	public partial class DynamicReportTableDataGrid
	{
		#region Service
		[Inject] IFINICSClient IFINICSClient { get; set; } = null!;
		#endregion

		#region Parameter
		[Parameter] public string? DynamicReportID { get; set; }
		[Parameter] public string? ParentMenuURL { get; set; }
		#endregion

		#region Component Field
		DataGrid<JsonObject> dataGrid = null!;
		#endregion

		#region Field
		public JsonObject rowDynamicReport { get; set; } = [];
		public JsonObject ActiveRow = new();

		public bool IsPublished
		{
			get
			{
				return rowDynamicReport["IsPublished"]?.GetValue<int>() == 1;
			}
		}
		public bool IsReadOnly
		{
			get
			{
				return IsPublished;
			}
		}
		#endregion

		#region OnInitialized
		protected override async Task OnInitializedAsync()
		{
			await GetRowrowDynamicReport();
			await base.OnInitializedAsync();
		}
		#endregion

		#region LoadData
		protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
		{
			var res = await IFINICSClient.GetRows<JsonObject>("DynamicReportTable", "GetRows", new
			{
				Keyword = args.Keyword,
				Offset = args.Offset,
				Limit = args.Limit,
				DynamicReportID
			});

			return res?.Data;
		}
		#endregion

		#region GetRowrowDynamicReport
		public async Task GetRowrowDynamicReport()
		{
			Loading.Show();
			var res = await IFINICSClient.GetRow<JsonObject>("DynamicReport", "GetRowByID", new
			{
				ID = DynamicReportID
			});

			if (res?.Data != null)
			{
				rowDynamicReport = res.Data;
			}

			Loading.Close();
			StateHasChanged();
		}
		#endregion

		#region LoadLookup
		#endregion

		#region Add
		private void Add()
		{
			NavigationManager.NavigateTo($"{ParentMenuURL}/dynamicreportsetting/{DynamicReportID}/dynamicreporttable/add");
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

				await IFINICSClient.Delete("DynamicReportTable", "DeleteByID", id);

				await dataGrid.Reload();
				dataGrid.selectedData.Clear();

				Loading.Close();

				StateHasChanged();
			}
		}
		#endregion
	}
}