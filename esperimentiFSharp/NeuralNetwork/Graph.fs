module Graph

open Neural

let createGraphFromNetwork (nn:MultiLayerNetwork) =
    let viewer = new Microsoft.Glee.GraphViewerGdi.GViewer();
    let graph = new Microsoft.Glee.Drawing.Graph("Neural Netowrk");
    graph.GraphAttr.Orientation <- Microsoft.Glee.Drawing.Orientation.Landscape
    graph.GraphAttr.LayerDirection <- Microsoft.Glee.Drawing.LayerDirection.LR
    graph.GraphAttr.LayerSep <- 150.0

    nn.OutputLayer
    |> Seq.iter (fun el ->  let targetNode = graph.AddEdge(el.Value.Id.ToString(), el.Key).TargetNode
                            
                            targetNode.Attr.Shape <- Microsoft.Glee.Drawing.Shape.Box
                            targetNode.Attr.Fillcolor <- Microsoft.Glee.Drawing.Color.Yellow
                            el.Value.InputMap.Keys
                                |> Seq.iter ( fun prevNeur -> graph.AddEdge(prevNeur.Id.ToString(), el.Value.Id.ToString()) |> ignore) )

    for layer in nn.HiddenLayers do
        layer
        |> Seq.iter (fun neur -> neur.InputMap.Keys
                                    |> Seq.iter ( fun prevNeur -> graph.AddEdge(prevNeur.Id.ToString(), neur.Id.ToString()) |> ignore) )

    nn.InputLayer
    |> Seq.iter ( fun el -> let sourceNode = graph.AddEdge(el.Key, el.Value.Id.ToString()).SourceNode
                            sourceNode.Attr.Shape <- Microsoft.Glee.Drawing.Shape.Box
                            sourceNode.Attr.Fillcolor <- Microsoft.Glee.Drawing.Color.LightGreen )


    viewer.Graph <- graph;
    viewer.Dock <- System.Windows.Forms.DockStyle.Fill;
    viewer


// Selezione nodo o ramo
//object selectedObjectAttr;
//    object selectedObject;
//    void gViewer_SelectionChanged(object sender, EventArgs e)
//    {
//
//      if (selectedObject != null)
//      {
//        if (selectedObject is Edge)
//          (selectedObject as Edge).Attr = selectedObjectAttr as EdgeAttr;
//        else if (selectedObject is Node)
//          (selectedObject as Node).Attr = selectedObjectAttr as NodeAttr;
//
//        selectedObject = null;
//      }
//
//      if (gViewer.SelectedObject == null)
//      {
//        label1.Text = "No object under the mouse";
//        this.gViewer.SetToolTip(toolTip1, "");
//
//      }
//      else
//      {
//        selectedObject = gViewer.SelectedObject;
//      
//        if (selectedObject is Edge)
//        {
//          selectedObjectAttr = (gViewer.SelectedObject as Edge).Attr.Clone();
//          (gViewer.SelectedObject as Edge).Attr.Color = Microsoft.Glee.Drawing.Color.Magenta;
//          (gViewer.SelectedObject as Edge).Attr.Fontcolor = Microsoft.Glee.Drawing.Color.Magenta;
//          Edge edge=gViewer.SelectedObject as Edge;
//       
//         
//
//
//        //here you can use e.Attr.Id or e.UserData to get back to you data
//          this.gViewer.SetToolTip(this.toolTip1, String.Format("edge from {0} {1}", edge.Source, edge.Target));
//     
//        }
//        else if (selectedObject is Node)
//        {
//
//          selectedObjectAttr = (gViewer.SelectedObject as Node).Attr.Clone();
//          (selectedObject as Node).Attr.Color = Microsoft.Glee.Drawing.Color.Magenta;
//          (selectedObject as Node).Attr.Fontcolor = Microsoft.Glee.Drawing.Color.Magenta;
//          //here you can use e.Attr.Id to get back to your data
//          this.gViewer.SetToolTip(toolTip1,String.Format("node {0}", (selectedObject as Node).Attr.Label));
//        }
//        label1.Text = selectedObject.ToString();
//      }
//      gViewer.Invalidate();
//    }