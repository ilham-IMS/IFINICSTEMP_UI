
using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.DynamicReportComponent
{
	public partial class DynamicReportPublishedDataGrid
	{
		#region Service
		[Inject] IFINICSClient IFINICSClient { get; set; } = null!;
		#endregion

		#region Component Field
		DataGrid<JsonObject> dataGrid = null!;
		#endregion

		#region Field

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
			var res = await IFINICSClient.GetRows<JsonObject>("DynamicReport", "GetRowsPublishedByUser", new
			{
				Keyword = args.Keyword,
				Offset = args.Offset,
				Limit = args.Limit,
				UserCode = GetCurrentUser()
			});

			return res?.Data;
		}
		#endregion

	}
}