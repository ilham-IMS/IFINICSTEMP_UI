using System.Text.Json.Nodes;
using Blazor.Diagrams;
using Blazor.Diagrams.Core.Anchors;
using Blazor.Diagrams.Core.Behaviors;
using Blazor.Diagrams.Core.Controls.Default;
using Blazor.Diagrams.Core.Models;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_ICS_UI.Components.Setting.FlowchartNodeComponent;

public partial class FlowchartNode
{
   [Inject] IFINICSClient IFINICSClient { get; set; } = null!;
   [Parameter] public string? DynamicButtonProcessRoleID { get; set; }
   [Parameter] public string? DynamicButtonProcessID { get; set; }
   private BlazorDiagram Diagram { get; set; } = new();
   private List<JsonObject> nodes = new();

   protected async override Task OnInitializedAsync()
   {
      Diagram.RegisterComponent<IfinNodeModel, IfinNodeWidget>();

      nodes = await GetRows(DynamicButtonProcessRoleID!);

      // Tambahkan node dan link berdasarkan daftar JsonObject
      AddNodeLink(nodes);

      // Tambahkan shortcut keyboard
      var ksb = Diagram.GetBehavior<KeyboardShortcutsBehavior>();
      //ksb?.SetShortcut("y", ctrl: true, shift: false, alt: false, CreateLink);
      ksb?.SetShortcut("-", ctrl: false, shift: false, alt: false, DeleteLink);
      ksb?.SetShortcut("m", ctrl: true, shift: false, alt: false, DeleteNode);
      base.OnInitialized();

   }

   private void AddNodeLink(List<JsonObject> nodes)
   {
      var nodeDictionary = new Dictionary<string, IfinNodeModel>();

      try
      {
         // Tambahkan node ke diagram
         foreach (var node in nodes)
         {
            var nodeName = node["NodeName"]?.GetValue<string>();
            var nodeModel = AddNode(nodeName!, node["XCoordinat"]?.GetValue<double>() ?? 0, node["YCoordinat"]?.GetValue<double>() ?? 0,
            node["ShortDescription"]?.GetValue<string>() ?? "");
            nodeDictionary[nodeName!] = nodeModel;
         }
         // new() {
         //    {"http", ""},
         // }
         // Tambahkan link antara node
         foreach (var node in nodes)
         {
            var fromNodeName = node["SourceLink"]?.GetValue<string>();
            var targetNodeName = node["TargetLink"]?.GetValue<string>();

            if (fromNodeName != null && targetNodeName != null)
            {
               // Console.WriteLine($"qwer6789 {nodes.IndexOf(node)} {nodeDictionary["JAKA"]}");
               var fromNode = nodeDictionary[fromNodeName];
               var targetNode = nodeDictionary[targetNodeName];
               // Console.WriteLine($"qwer6789 {nodes.IndexOf(node)} {fromNode}, {targetNode}");
               AddLink(fromNode, targetNode);
            }
         }

      }
      catch (System.Exception)
      {

         throw;
      }

   }

   protected async Task<List<JsonObject>> GetRows(string dynamicButtonProcessRoleID)
   {
      var res = await IFINICSClient.GetRows<JsonObject>("FlowchartNode", "GetRows", new
      {
         DynamicButtonProcessRoleID = dynamicButtonProcessRoleID
      });
      return res?.Data ?? [];
   }

   private IfinNodeModel AddNode(string label, double x, double y, string value)
   {
      var node = new IfinNodeModel(new Blazor.Diagrams.Core.Geometry.Point(x, y)) { Label = label, Value = value };
      Diagram.Nodes.Add(node);
      node.AddPort(PortAlignment.Top);
      node.AddPort(PortAlignment.Bottom);
      Diagram.Controls.AddFor(node).Add(new DragNewLinkControl(1, 0.5, offsetX: 20));
      return node;
   }

   private LinkModel AddLink(IfinNodeModel source, IfinNodeModel target)
   {
      var sourceAnchor = new ShapeIntersectionAnchor(source);
      var targetAnchor = new ShapeIntersectionAnchor(target);
      var link = new LinkModel(sourceAnchor, targetAnchor)
      {
         TargetMarker = LinkMarker.Arrow,
         SourceMarker = LinkMarker.Circle,
         Color = ColorGallery.PrimaryDark
      };
      Diagram.Links.Add(link);
      return link;
   }

   private async ValueTask CreateLink(Blazor.Diagrams.Core.Diagram diagram)
   {
      var selectedNodes = diagram.Nodes.Where(n => n.Selected).ToList();
      if (selectedNodes.Count == 2)
      {
         AddLink((IfinNodeModel)selectedNodes[0], (IfinNodeModel)selectedNodes[1]);
      }
      await ValueTask.CompletedTask;
   }

   private async ValueTask DeleteLink(Blazor.Diagrams.Core.Diagram diagram)
   {
      var selectedLinks = diagram.Links.Where(l => l.Selected).ToList();

      foreach (var link in selectedLinks)
      {
         // Coba lakukan casting ke IfinNodeModel
         var sourceNode = link.Source.Model as IfinNodeModel;

         if (sourceNode != null)
         {
            foreach (var node in nodes)
            {
               // Gunakan == untuk perbandingan
               if (sourceNode.Label == node["NodeName"]?.GetValue<string>())
               {
                  node["TargetLink"] = null; // Set TargetLink menjadi null
               }
            }
         }

         // Hapus link dari diagram
         diagram.Links.Remove(link);
      }

      await ValueTask.CompletedTask;
   }

   private async ValueTask DeleteNode(Blazor.Diagrams.Core.Diagram diagram)
   {
      // Buat list untuk menyimpan ID nodes yang akan dihapus
      List<JsonObject> idsToDelete = new();

      // Dapatkan node yang dipilih untuk dihapus
      var selectedNodes = diagram.Nodes.Where(n => n.Selected).ToList();

      foreach (var selectedNode in selectedNodes)
      {
         if (selectedNode is IfinNodeModel ifinNode)
         {
            // Cari objek JSON yang cocok di dalam nodes berdasarkan Label
            var matchingJsonNode = nodes.FirstOrDefault(n => n["NodeName"]?.GetValue<string>() == ifinNode.Label);

            if (matchingJsonNode != null)
            {
               // Ambil ID dari matchingJsonNode dan tambahkan ke daftar idsToDelete
               string? id = matchingJsonNode["ID"]?.GetValue<string>();
               if (id != null)
               {
                  idsToDelete.Add(SetAuditInfo(new JsonObject
                  {
                     ["ID"] = id
                  }));
               }
            }

            // Hapus semua link yang terhubung dengan node dari diagram
            var relatedLinks = diagram.Links
                .Where(l => l.Source.Model == selectedNode || l.Target.Model == selectedNode)
                .ToList();

            foreach (var link in relatedLinks)
            {
               diagram.Links.Remove(link);
            }

            // Hapus node dari diagram
            diagram.Nodes.Remove(selectedNode);
         }
      }

      // Panggil API delete dengan idsToDelete sebagai array
      var res = await IFINICSClient.Delete("FlowchartNode", "DeleteByID", idsToDelete.ToArray());
      //  if(res.Result == 1)
      //  {
      //    await Update();
      //  }

      await ValueTask.CompletedTask;
   }





   private void Print()
   {
      foreach (var link in Diagram.Links)
      {
         var sourceNode = link.Source.Model as IfinNodeModel;
         var targetNode = link.Target.Model as IfinNodeModel;

         if (sourceNode != null && targetNode != null)
         {
            Console.WriteLine($"Link from {sourceNode.Label} to {targetNode.Label}");
         }
      }
   }

   private void PrintNodeTexts()
   {
      foreach (var node in Diagram.Nodes)
      {
         if (node is IfinNodeModel ifinNode)
         {
            Console.WriteLine($"Node {ifinNode.Label} has text: {ifinNode.Value}");
         }
      }
   }

   private async Task Update()
   {
      Loading.Show();

      foreach (var node in Diagram.Nodes)
      {
         if (node is IfinNodeModel ifinNode)
         {
            var jsonNode = nodes.FirstOrDefault(n => n["NodeName"]?.GetValue<string>() == ifinNode.Label);
            if (jsonNode != null)
            {
               jsonNode["XCoordinat"] = ifinNode.Position.X;
               jsonNode["YCoordinat"] = ifinNode.Position.Y;
            }
         }
      }

      foreach (var link in Diagram.Links)
      {
         if (link.Source.Model is IfinNodeModel sourceNode && link.Target.Model is IfinNodeModel targetNode)
         {
            var jsonNode = nodes.FirstOrDefault(n => n["NodeName"]?.GetValue<string>() == sourceNode.Label);
            if (jsonNode != null)
            {
               jsonNode["SourceLink"] = sourceNode.Label;
               jsonNode["TargetLink"] = targetNode.Label;
            }
         }
      }
      await IFINICSClient.Put("FlowchartNode", "Update", nodes);
      NavigationManager.Refresh();
      Loading.Close();
   }

   private void Back()
   {
      NavigationManager.NavigateTo("flowchart");
   }
   private void Add()
   {
      NavigationManager.NavigateTo($"systemsetting/dynamicbuttonprocess/{DynamicButtonProcessID}/dynamicbuttonprocessrole/{DynamicButtonProcessRoleID}/flowchartnode/add");
   }
}

