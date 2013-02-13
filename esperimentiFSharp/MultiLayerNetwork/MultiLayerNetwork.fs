﻿module MultiLayerNetwork

open System
open System.Data
open System.Collections.Generic
open NeuralTypes
open Neural
open TableUtilities
open Builder
open Parameter

type NeuralLayer() =   
   inherit ResizeArray<Neuron>()  
   member this.Activate() =   
     this  
     |> Seq.iter (fun n->n.Activate() |> ignore) 

type MultiLayerNetwork(trainingFun : TrainigFunctionType) =
    inherit SupervisedNeuralNetwork(trainingFun)

    let _inputLayer = new Dictionary<string, ConstantNeuron>(HashIdentity.Structural)   // Per ogni ingresso devo sapere il neurone associato (nomi delle colonne della tabella)
    let _hiddenLayers = new ResizeArray<NeuralLayer>()                                  
    let _outputLayer = new Dictionary<string, Neuron>(HashIdentity.Structural)          // Per ogni valore di un attributo nominal devo sapere il neurone associato (da dove li recupero -> ricavo la colonna dell'attributo (AttributeDataColumn))
                                                                                        // Se l'attributo da predire è numeric, la chiave è il nome dell'attributo
    member nn.HiddenLayers = _hiddenLayers
    member nn.InputLayer = _inputLayer
    member nn.OutputLayer = _outputLayer

    member nn.CreateNetork(trainingSet : DataTable, classAtt : string, seed:int option, hiddenLayers:(int*ActivationFunType*OutputFunType) list option, outputLayer:(ActivationFunType*OutputFunType) option) =    // Potrebbe accettare una lista di int che dicono quanti livelli nascosti ci devono essere e con quanti neuroni ciascuno
        nn.OutputLayer.Clear()                      // Elimino la rete precedente
        nn.HiddenLayers
        |> Seq.iter(fun el -> el.Clear())
        nn.HiddenLayers.Clear()
        nn.InputLayer.Clear()

        let seed = defaultArg seed DateTime.Now.Second
        let actFunOut, outFunOut  = defaultArg outputLayer (sumOfProducts, linear)

        // Recupero di dati che mi servono per costruire il layer di uscita e nascosto
        let attType = (trainingSet.Columns.[classAtt] :?> AttributeDataColumn).AttributeType
        let numClasses,nomList = match attType with
                                 | AttributeType.Nominal nomList -> List.length nomList, nomList
                                 | AttributeType.Numeric -> 1,[classAtt]
                                 | _ -> failwithf "Is not possible to create a neural network for an output type of '%A'" attType
        let numAttributes = trainingSet.Columns.Count

        // Insieme ai dati in ingresso (numero neuroni * funzione di attivazione * funzione di uscita) metto anche un indice (da 0) che mi serve dopo
        let hiddenLayers = defaultArg hiddenLayers [(numClasses + numAttributes)/2, sumOfProducts, sigmoid] |> List.mapi (fun i el -> (i, el))
        let random = new Random(seed)

        // Costruisco i neuroni di input
        for col in trainingSet.Columns do
            if col.ColumnName <> classAtt then
                let neuron = new ConstantNeuron()
                nn.InputLayer.Add(col.ColumnName, neuron)

        // Costruisco i neuroni e i livelli nascosti
        for layerIndex, (layerCount, actFun, outFun) in hiddenLayers do         // In hiddenLayer c'è anche l'indice 
            let hidLayer = new NeuralLayer()
            let prevLayer = if layerIndex = 0 then
                                nn.InputLayer.Values |> Seq.map (fun el -> el :> Neuron) |> Seq.toList
                            else 
                                nn.HiddenLayers.[layerIndex-1] |> Seq.toList
            for i in 1..layerCount do
                let hid = new Neuron()
                hid.ActivationFunction <- actFun
                hid.OutputFunction <- outFun
                for prevNeur in prevLayer do
                    hid.InputMap.Add(prevNeur, (random.NextDouble()-0.5))       // Per avere pesi random sia positivi che negativi
                hidLayer.Add(hid)
            nn.HiddenLayers.Add(hidLayer) 

        // Costruisco i neuroni di output
        for i in 1..numClasses do
            let out = new Neuron()
            out.ActivationFunction <- actFunOut
            out.OutputFunction <- outFunOut
            for prevNeur in nn.HiddenLayers.[nn.HiddenLayers.Count-1] do        // Prendo l'ultimo livello nascosto  
                out.InputMap.Add(prevNeur, (random.NextDouble()-0.5))
            nn.OutputLayer.Add(nomList.[i-1], out)
        ()

    member nn.Activate(row : DataRow) = 
        // Imposto gli ingressi nei neuroni dell'inputLayer
        // Devo skippare l'attributo da classificare
        for col in nn.TrainingTable.Columns do
            if col.ColumnName <> nn.TrainedAttribute then
                let element = toAttributeValue row col.Ordinal
                let inVal = match element with                          // Per ogni attributo ricavo il relativo valore e lo setto nei neuroni di input
                            | Numeric(n) -> n
                            | String(_) -> 0.0                          // Stringhe e valori missing vengono ignorati -> ingressi a zero
                            | Nominal(_, i) -> Convert.ToDouble(i)
                            | Missing -> 0.0 

                _inputLayer.[col.ColumnName].SetOutput(inVal)

        _hiddenLayers
        |> Seq.iter (fun layer -> layer.Activate())         // Attivo i layer in sequenza

        _outputLayer.Values
        |> Seq.iter (fun neur -> neur.Activate() |> ignore)

    override nn.Classify(row) =
        nn.Activate(row)
         
        if _outputLayer.Keys.Count = 1 then                         // L'attributo da classificare è numerico
            Numeric(_outputLayer.[nn.TrainedAttribute].Output)
        else                                                        // L'attributo da classificare è nominale
            let outputs = _outputLayer.Keys                                     // Costruisco una sequenza di tuple con nome del valore e uscita del relativo neurone
                            |> Seq.map (fun s -> (s, _outputLayer.[s].Output))
            let max = outputs                                                   // Ottengo una tupla con il nome del valore con l'uscita massima e la relativa uscita
                        |> Seq.maxBy (fun (_, value) -> value) 
            let attType = (nn.TrainingTable.Columns.[nn.TrainedAttribute] :?> AttributeDataColumn).AttributeType
            let nomList = match attType with
                            | AttributeType.Nominal(list) -> list
                            | _ -> failwith "never here!!!"
            let attVal = fst max                                                // Prendo il primo valore della tupla, ossia il valore nominale
            let index = (List.findIndex (fun el -> el = attVal) nomList) + 1    // Trovo l'indice della posizione del valore nella lista di valori nominali
            Nominal(attVal, index)     
            

// ----------------------------------------------------------------------------------------
// -------------------------------VISUALIZER-----------------------------------------------
// ----------------------------------------------------------------------------------------          
            
let private createGraphFromNetwork (nn:SupervisedNeuralNetwork) =
    if not (typeof<MultiLayerNetwork>.IsInstanceOfType(nn)) then
        failwithf "The visualizer cannot be built for the type '%s'" (nn.GetType().Name)

    let nn = nn :?> MultiLayerNetwork
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


// Selezione nodo o ramo-------------------------------------------
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
            

// ----------------------------------------------------------------------------------------
// -------------------------------BUILDER--------------------------------------------------
// ----------------------------------------------------------------------------------------

let private globalRules = new Dictionary<string, (string->obj->unit)>(HashIdentity.Structural)
globalRules.Add("SEED", (fun name input -> if input.GetType() <> typeof<int> then
                                                invalidArg name "Wrong type, expected 'int'" ))

let private actFunctions = new Dictionary<string, ActivationFunType>(HashIdentity.Structural) 
let private outFunctions = new Dictionary<string, OutputFunType>(HashIdentity.Structural) 
actFunctions.Add("sumOfProducts", sumOfProducts)
outFunctions.Add("sigmoid", sigmoid)
//outFunctions.Add("heavside", heavside)
outFunctions.Add("linear", linear)

let private aspectsRules = new Dictionary<string, Dictionary<string,(string->obj->unit)>>(HashIdentity.Structural)
let mutable private layerRules = new Dictionary<string, (string->obj->unit)>(HashIdentity.Structural)
layerRules.Add("ACTIVATION_FUNCTION", (fun name input ->    match actFunctions.TryGetValue(input.ToString()) with
                                                            | res,_ when res=true -> ()
                                                            | _,_ -> failwithf "The activation function '%s' is not valid" (input.ToString()) ))

//                                                            if not (typeof<ActivationFunType>.IsInstanceOfType(input)) then
//                                                                invalidArg name "The activation function must be a function of type 'seq<double * double> -> double'" ))

layerRules.Add("OUTPUT_FUNCTION", (fun name input ->  match outFunctions.TryGetValue(input.ToString()) with
                                                            | res,_ when res=true -> ()
                                                            | _,_ -> failwithf "The activation function '%s' is not valid" (input.ToString()) ))

//                                                            if not (typeof<OutputFunType>.IsInstanceOfType(input)) then
//                                                                invalidArg name "The activation function must be a function of type 'double -> double'" ))


aspectsRules.Add("OUTPUT_LAYER", layerRules)            // Il layer di uscita non specifica il numero di neuroni, dipende dall'atributo da predire

layerRules <- new Dictionary<string, (string->obj->unit)>(layerRules, HashIdentity.Structural)       // Copio gli elementi dall'altro dizionario
layerRules.Add("NEURONS", (fun name input -> if input.GetType() <> typeof<int> then
                                                invalidArg name "Wrong type, expected 'int'"   
                                             let value = unbox input
                                             if value <= 0 then
                                                invalidArg name "The number of neurons must be positive (>0)" ))
aspectsRules.Add("HIDDEN_LAYER", layerRules)


type MultiLayerNetworkBuilder() =
    inherit Builder<SupervisedNeuralNetwork>(globalRules, aspectsRules)

    let check (builder:Builder<'T>) =
        let numOfLocParams = builder.LocalParameters.ParameterValues |> Seq.length
        let numOfOutput = builder.GetAspects("OUTPUT_LAYER") |> Seq.length 
        if numOfLocParams > 1 then
            failwith "Only one global parameter can be setted in MultiLayerNetworkBuilder, SEED"
        if numOfOutput > 1 then
            failwith "Only one OUTPUT_LAYER aspect can be setted in MultiLayerNetworkBuilder"
        builder.GetAspects("HIDDEN_LAYER")
        |> Seq.iter (fun pStore ->  let nNeurons = pStore.GetValues("NEURONS") |> Seq.length
                                    let nActFun = pStore.GetValues("ACTIVATION_FUNCTION") |> Seq.length
                                    let nOutFun = pStore.GetValues("OUTPUT_FUNCTION") |> Seq.length
                                    if nNeurons = 0 || nNeurons > 1 then
                                        failwith "Exactly one 'NEURONS' specification can be added in an 'HIDDEN_LAYER' aspect"
                                    if nActFun = 0 || nActFun > 1 then
                                        failwith "Exactly one 'ACTIVATION_FUNCTION' specification can be added in an 'HIDDEN_LAYER' aspect"
                                    if nOutFun = 0 || nOutFun > 1 then
                                        failwith "Exactly one 'OUTPUT_FUNCTION' specification can be added in an 'HIDDEN_LAYER' aspect" ) 
        builder.GetAspects("OUTPUT_LAYER")
        |> Seq.iter (fun pStore ->  let nActFun = pStore.GetValues("ACTIVATION_FUNCTION") |> Seq.length
                                    let nOutFun = pStore.GetValues("OUTPUT_FUNCTION") |> Seq.length
                                    if nActFun > 1 then
                                        failwith "Only one 'ACTIVATION_FUNCTION' specification can be added in an 'OUTPUT_LAYER' aspect"
                                    if nOutFun > 1 then
                                        failwith "Only one 'OUTPUT_FUNCTION' specification can be added in an 'OUTPUT_LAYER' aspect" )

    let createHiddenLayersList (builder:Builder<'T>) =
        if builder.GetAspects("HIDDEN_LAYER") = Seq.empty then
            None
        else
            builder.GetAspects("HIDDEN_LAYER")
            |> Seq.map (fun pStore -> let neurons : int = pStore.GetValues("NEURONS") |> Seq.exactlyOne |> unbox  
                                      let actFunName = pStore.GetValues("ACTIVATION_FUNCTION") |> Seq.exactlyOne |> unbox                                  
                                      let actFun : ActivationFunType = actFunctions.[actFunName]
                                      let outFunName = pStore.GetValues("OUTPUT_FUNCTION") |> Seq.exactlyOne |> unbox            
                                      let outFun : OutputFunType = outFunctions.[outFunName]
                                      (neurons, actFun, outFun)  )
            |> Seq.toList 
            |> Some

    let createOutputLayer (builder:Builder<'T>) =
        if builder.GetAspects("OUTPUT_LAYER") = Seq.empty then
            None
        else
            let outStore = builder.GetAspects("OUTPUT_LAYER") |> Seq.exactlyOne                                     
            let actFunName = outStore.GetValues("ACTIVATION_FUNCTION") |> Seq.exactlyOne |> unbox                                  
            let actFun : ActivationFunType = actFunctions.[actFunName]
            let outFunName = outStore.GetValues("OUTPUT_FUNCTION") |> Seq.exactlyOne |> unbox            
            let outFun : OutputFunType = outFunctions.[outFunName]
            Some(actFun, outFun)

    let createSeed  (builder:Builder<'T>) =
        if builder.LocalParameters.GetValues("SEED") = Seq.empty then
            None
        else
            Some(builder.LocalParameters.GetValues("SEED") |> Seq.exactlyOne |> unbox)

    override this.Name = "MultiLayerNetwork"
    override this.Build(gobalParameters:ParameterStore) = 
        check this
        let trainingSet = gobalParameters.GetValues("TRAINING_TABLE") |> Seq.exactlyOne |> unbox    // Non controllo se c'è o meno il parametro, do per scontato che ci sia
        let classAtt = gobalParameters.GetValues("CLASS_ATTRIBUTE") |> Seq.exactlyOne |> unbox      
        let hiddenLayers = createHiddenLayersList this
        let outputLayer = createOutputLayer this
        let seed = createSeed this
        
        let NN = new MultiLayerNetwork((fun a b c -> ()))                                           // Il training algorithm lo creo da fuori e lo setto con l'opportuno builder
        NN.CreateNetork(trainingSet, classAtt, seed, hiddenLayers, outputLayer)
        NN :> SupervisedNeuralNetwork

    override this.GetVisualizer(net) =
        (createGraphFromNetwork net) :> Windows.Forms.Control
           