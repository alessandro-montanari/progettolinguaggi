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