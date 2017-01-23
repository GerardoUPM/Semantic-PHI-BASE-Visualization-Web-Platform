function init() {
    if (window.goSamples) goSamples();  // init for these samples -- you don't need to call this
    var $ = go.GraphObject.make;  // for conciseness in defining templates

    myFullDiagram =
      $(go.Diagram, "fullDiagram",  // each diagram refers to its DIV HTML element by id
        {
            initialAutoScale: go.Diagram.Uniform,  // automatically scale down to show whole tree
            contentAlignment: go.Spot.Center,  // center the tree in the viewport
            isReadOnly: true,  // don't allow user to change the diagram
            layout: $(go.TreeLayout,
                      { angle: 60, sorting: go.TreeLayout.SortingAscending }),
            maxSelectionCount: 1,  // only one node may be selected at a time in each diagram
            // when the selection changes, update the myLocalDiagram view
            "ChangedSelection": showLocalOnFullClick
        });

    myLocalDiagram =  // this is very similar to the full Diagram
      $(go.Diagram, "localDiagram",
        {
            autoScale: go.Diagram.Uniform,
            contentAlignment: go.Spot.Center,
            isReadOnly: true,
            layout: $(go.TreeLayout,
                      { angle: 60, sorting: go.TreeLayout.SortingAscending }),
            maxSelectionCount: 1,
            // when the selection changes, update the contents of the myLocalDiagram
            "ChangedSelection": showLocalOnLocalClick
        });

    // Define a node template that is shared by both diagrams
    var myNodeTemplate =
      $(go.Node, "Auto",
        { locationSpot: go.Spot.Center },
        new go.Binding("text", "key"),
        new go.Binding("text", "texto"),// for sorting
        $(go.Shape, "RoundedRectangle",
          new go.Binding("fill", "color"),
          { stroke: "gray" }),
        $(go.TextBlock,
          { margin: 1 },
          new go.Binding("text", "key"))
      );
    myFullDiagram.nodeTemplate = myNodeTemplate;
    myLocalDiagram.nodeTemplate = myNodeTemplate;

    // Define a basic link template, not selectable, shared by both diagrams
    var myLinkTemplate =
      $(go.Link,
        { routing: go.Link.Normal, selectable: false },
        $(go.Shape,
          { strokeWidth: 1 })
      );
    myFullDiagram.linkTemplate = myLinkTemplate;
    myLocalDiagram.linkTemplate = myLinkTemplate;

    // Create the full tree diagram
    setupDiagram();

    // Create a part in the background of the full diagram to highlight the selected node
    highlighter =
      $(go.Part, "Auto",
        {
            layerName: "Background",
            selectable: false,
            isInDocumentBounds: false,
            locationSpot: go.Spot.Center
        },
        $(go.Shape, "Ellipse",
          {
              fill: $(go.Brush, "Radial", { 0.0: "white", 1.0: "orange" }),
              stroke: null,
              desiredSize: new go.Size(100, 100)
          })
        );
    myFullDiagram.add(highlighter);

    // Start by focusing the diagrams on the node at the top of the tree.
    // Wait until the tree has been laid out before selecting the root node.
    myFullDiagram.addDiagramListener("InitialLayoutCompleted", function (e) {
        var str = " ";
        var node1 = document.getElementById("Nodo").value;
        var nodeTot = node1.trim();
        var nodeTotal = str + nodeTot;
        myFullDiagram.findPartForKey(nodeTotal).isSelected = true;
        showLocalOnFullClick();
    });
}

// Make the corresponding node in the full diagram to that selected in the local diagram selected,
// then call showLocalOnFullClick to update the local diagram.
function showLocalOnLocalClick() {
    document.getElementById("txtTabla").value = "";
    var selectedLocal = myLocalDiagram.selection.first();
    if (selectedLocal !== null) {
        // there are two separate Nodes, one for each Diagram, but they share the same key value
        myFullDiagram.select(myFullDiagram.findPartForKey(selectedLocal.data.key));
        var resultado = selectedLocal.data.texto;
        if (resultado !== null && resultado !== undefined && resultado !== "") {
            document.getElementById("txtTabla").value = resultado;
        } else {
            document.getElementById("txtTabla").value = "No hay información disponible";
        }
    }
}

function showLocalOnFullClick() {
    var node = myFullDiagram.selection.first();
    if (node !== null) {
        // move the large yellow node behind the selected node to highlight it
        highlighter.location = node.location;
        // create a new model for the local Diagram
        var model = new go.TreeModel();
        // add the selected node and its children and grandchildren to the local diagram
        var nearby = node.findTreeParts(3);  // three levels of the (sub)tree
        // add parent and grandparent
        var parent = node.findTreeParentNode();
        if (parent !== null) {
            nearby.add(parent);
            var grandparent = parent.findTreeParentNode();
            if (grandparent !== null) {
                nearby.add(grandparent);
            }
        }
        // create the model using the same node data as in myFullDiagram's model
        nearby.each(function (n) {
            if (n instanceof go.Node) model.addNodeData(n.data);
        });
        myLocalDiagram.model = model;
        // select the node at the diagram's focus
        myLocalDiagram.findPartForKey(node.data.key).isSelected = true;
    }
}
// Crea el diagrama con los nodos
function setupDiagram() {
    myFullDiagram.model = go.Model.fromJson(document.getElementById("mySavedModel").value);
}

function fun() {
    if(document.getElementById("fullDiagram").className =="visible")
    {
        document.getElementById("fullDiagram").style.display = 'none';
        document.getElementById("fullDiagram").className = "";
    } else {
        document.getElementById("fullDiagram").style.display = 'block';
        document.getElementById("fullDiagram").className = "visible";
    }
}



