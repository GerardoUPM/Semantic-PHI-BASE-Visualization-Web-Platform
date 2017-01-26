using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VDS.RDF;
using VDS.RDF.Query;

namespace appPlantaPatogeno.Controllers
{
    /* 
        @author: Alejandro Rofríguez González, Gerardo Lagunes García
        @version: Octubre 2016 - Enero 2017   
    */
    public class HomeController : Controller
    {
        //declaramos las variables para el metodo recursivo
        private int level = 0;
        private string padre = "";
        private int total = 0;
        private string json = "{\"class\": \"go.TreeModel\",\"nodeDataArray\": [";
        private string json2 = "";
        private string finTotal = "]}";
        public string sError = "";
        /* 
            @author: Alejandro Rofríguez González, Gerardo Lagunes García
            @version: Octubre 2016 - Enero 2017
            @description:
                sCorreccion: Agrega un filtro que elimina las URI inválidas. 
        */
        private string sCorreccion = "FILTER (!isURI(?valor) || strstarts(str(?valor), 'http'))";//" BIND( STRLEN(STR(?valor)) AS ?enlace ) FILTER( ?enlace > 15 ) ";
        Boolean bEntra = false;

        // GET: Home
        public ActionResult Index()
        {
            //Decclaramos todos los string necesarios para las consultas
            string url = Request.QueryString["interaction"];
            string nodo = Request.QueryString["class_type"];
            //Cargamos el prefijo para usarlo en todas las consultas
            string prefijos = "PREFIX RO: <http://www.obofoundry.org/ro/ro.owl#>PREFIX SIO: <http://semanticscience.org/resource/>PREFIX EDAM:  <http://edamontology.org/>PREFIX PHIO: <http://linkeddata.systems/ontologies/SemanticPHIBase#>PREFIX PUBMED:  <http://linkedlifedata.com/resource/pubmed/>PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>PREFIX up:  <http://purl.uniprot.org/core/>PREFIX foaf: <http://xmlns.com/foaf/0.1/>PREFIX skos: <http://www.w3.org/2004/02/skos/core#>";
            string inicio = "SELECT DISTINCT ?disn_1 ?label ?rel ?valor WHERE { ?disn_1 ?rel ?valor . ?disn_1 rdfs:label ?label FILTER(( ?disn_1 = <";
            //string fin = ">))}"; // @LnDelete
            string fin = ">))";
            string sfin = "} ORDER BY DESC(?rel)";
            Boolean defecto = true;
            //Se establece el endpoint para las consultas
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://linkeddata.systems:8890/sparql"));
            try
            {
                //Creamos la query de la interaction que proviene de la url
                string query = prefijos + inicio + url + fin + sCorreccion + sfin;
                //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\gerar\Documents\glg.txt", true)){file.WriteLine("public ActionResult Index() QUERY: " + query);}
                try
                { 
                    SparqlResultSet resultQuery1 = endpoint.QueryWithResultSet(query);
                    //recorremos los resultados de esta primera query
                    if (resultQuery1.Results != null)
                    {
                        metodoRecursivo(resultQuery1, padre, level);
                    }
                    json = json.TrimEnd(',');
                    json = json + finTotal;
                    ViewData["Nodos"] = json;
                    if (nodo != null)
                    {
                        string query2 = prefijos + inicio + nodo + fin;
                        SparqlResultSet resultQuery2 = endpoint.QueryWithResultSet(query2);
                        if (resultQuery2.Results != null)
                        {
                            metodoNodoCentral(resultQuery2, defecto);
                        }
                        ViewData["Nodo"] = json2;
                    }
                    if ((nodo == null) && resultQuery1.Results != null)
                    {
                        defecto = false;
                        metodoNodoCentral(resultQuery1, defecto);
                        ViewData["Nodo"] = json2;
                    }
                }
                catch (VDS.RDF.Parsing.RdfParseException parseEx)
                {
                    /*
                        @author: Alejandro Rofríguez González, Gerardo Lagunes García
                        @version: Octubre 2016 - Enero 2017
                        @description:
                            Actualización que permite mostrar un error en pantalla cuando ocurra un error de sintaxis 
                            en la consulta SPARQL. "There was an error executing the query so handle it here (ERROR PARSE)"   
                    */
                    //Console.WriteLine(parseEx.Message);// @LnDelete
                    sError = "Sorry for the inconvenience, this graph can not be displayed.";
                    ViewData["sError"] = sError; //Session["ERROR"] = sError;
                }
            }
            catch (RdfQueryException queryEx)
            {
                //There was an error executing the query so handle it here
                Console.WriteLine(queryEx.Message);
            }
            return View();
        }
        private SparqlResultSet metodoNodoCentral(SparqlResultSet n, Boolean d)
        {
            try
            {
                if (d)
                {
                    foreach (var linea in n.Results)
                    {
                        string todos = "";
                        string[] res = linea.ToString().Split(',');
                        todos = AsignaValores(res);
                        string[] res2 = todos.ToString().Split('·');

                        string disn = res2[0];
                        string label = res2[1];
                        string rel = res2[2];
                        string valor = res2[3];

                        string[] prueba = rel.Split('#');
                        if (prueba.Length > 1)
                        {
                            var prueba1 = prueba[1].ToString();
                            if (prueba1 == "label ")
                            {
                                string valorNodo = "";
                                string[] cadena = rel.Split('#');
                                string[] cadena2 = valor.Split('-');
                                string[] cadena3 = cadena2[0].Split('=');
                                if (cadena2.Length > 1)
                                {
                                    valorNodo = cadena3[1] + ":" + cadena2[1];
                                }
                                else
                                {
                                    valorNodo = cadena3[1];
                                }
                                json2 = valorNodo;
                            }
                        }
                    }

                }
                else {
                    foreach (var linea in n.Results)
                    {
                        string prueba1 = "";
                        string todos = "";
                        string[] res = linea.ToString().Split(',');
                        todos = AsignaValores(res);
                        string[] res2 = todos.ToString().Split('·');
                        string disn = res2[0];
                        string label = res2[1];
                        string rel = res2[2];
                        string valor = res2[3];
                        //string[] prueba = rel.Split('#');
                        //prueba1 = prueba[1].ToString();

                        if (rel.Contains('#'))
                        {
                            string[] prueba = rel.Split('#');
                            prueba1 = prueba[1].ToString();
                        }
                        else
                        {
                            string[] prueba = rel.Split('-');
                            prueba1 = prueba[1].ToString();
                            //prueba1 = prueba1.Replace("[","").Replace("]", "");
                        }


                        if (prueba1 == "label ")//valor
                        {
                            string valorNodo = "";
                            string[] cadena = rel.Split('#');
                            string[] cadena2 = valor.Split('-');
                            if (cadena2.Length == 3)
                            {
                                if (cadena2[1].Contains('['))
                                {
                                    string[] i = cadena2[1].Split('[');
                                    string[] j = i[1].Split(']');
                                    valorNodo = " Interaction:" + j[0];
                                }
                                else
                                {
                                    valorNodo = " Interaction:" + cadena2[1];
                                }

                            }
                            json2 = valorNodo;
                        }
                    }
                }
            }
            catch (RdfQueryException queryEx)
            {
                //There was an error executing the query so handle it here
                Console.WriteLine(queryEx.Message);
            }
            return n;
        }

        /*
	        @author: Alejandro Rofríguez González, Gerardo Lagunes García
	        @version: Octubre 2016 - Enero 2017
	        @description:
                Método recursivo que forma por medio de recursividad un JSON almacenado la variable "nodo"  
                @param:                    
                    SparqlResultSet n   : resultado de la consulta SPARQL ejecutada 
                    string padre        : el padre jerárquico del elemento JSON
                    int level           : nivel jerárquico del elemento JSON
                @return: SparqlResultSet (resultado de una consulta SPARQL)
        */
        private SparqlResultSet metodoRecursivo(SparqlResultSet n, string padre, int level)
        {
            try
            {
                if (level == 0)
                {
                    total = n.Count;
                }
                level++;
                int count = 0; ;
                if (level == 1)
                {
                    foreach (var linea in n.Results)
                    {
                        string prueba1 = "";
                        if (total > count)
                        {
                            int counter = 0;
                            string disn = "";
                            string label = "";
                            string rel = "";
                            string valor = "";
                            string[] res = linea.ToString().Split(',');
                            /*
	                            @author: Gerardo Lagunes García
	                            @version: Octubre 2016 - Enero 2017
	                            @description:
                                    Se actualizó ciclo para procesar arreglos con más de 5 elementos 
                            */
                            foreach (var linea2 in res)
                            {
                                if (counter == 0)
                                {
                                    disn = linea2.ToString();
                                }
                                if (counter == 1)
                                {
                                    if (res.Length >= 5)
                                    {
                                        label = res[1].ToString()+", "+res[2].ToString();
                                    }
                                    else
                                    {
                                        label = linea2.ToString();
                                    }
                                }
                                if (counter == 2)
                                {
                                    rel = linea2.ToString();
                                }
                                if (counter == 3)
                                {
                                    if (res.Length >= 5)
                                    {
                                        rel = linea2.ToString();
                                    }
                                    else
                                    {
                                        valor = linea2.ToString();
                                    }
                                }
                                if (res.Length >= 5)
                                {
                                    if (counter == 4)
                                    {
                                        valor = linea2.ToString();
                                    }
                                    else if(counter == 5)
                                    {
                                        valor = res[4].ToString() + res[5].ToString();
                                    }
                                }
                                counter++;
                            }
                            if (rel.Contains('#'))
                            {
                                string[] prueba = rel.Split('#');
                                prueba1 = prueba[1].ToString();
                            }
                            else {
                                string[] prueba = rel.Split('-');
                                prueba1 = prueba[1].ToString();
                                //prueba1 = prueba1.Replace("[","").Replace("]", ""); // @LnDelete
                            }
                            if ((prueba1 == "has_participant ") || (prueba1 == "has_unique_identifier ") || (prueba1 == "is_manifested_as ") || (prueba1 == "has_unique_identifier "))//relaciones   || (prueba1 == "participates_in ")
                            {
                                EjecutaSPARQL(valor, padre, level);
                            }
                            else if (prueba1 == "label ")//valor
                            {
                                string levelColor = "";
                                string valorNodo = "";
                                levelColor = ObtenerLevelColor(level);
                                
                                string[] cadena = rel.Split('#');
                                string[] cadena2 = valor.Split('-');
                                if (cadena2.Length == 3)
                                {
                                    if (cadena2[1].Contains('[')) {
                                        string[] i = cadena2[1].Split('[');
                                        string[] j = i[1].Split(']');
                                        valorNodo = " Interaction:" + j[0];
                                    }
                                    else {
                                        valorNodo = " Interaction:" + cadena2[1];
                                    }
                                }
                                string key = "{\"key\":\"";
                                string color = "\" ,\"color\":\"";
                                //string texto = "\" ,\"texto\": \""; // @LnDelete
                                string fin = "\"} ,";

                                string resultado = key + valorNodo + color + levelColor + fin;// + texto + fin;
                                json = json + resultado;
                                padre = valorNodo;
                            }
                            else if( (prueba1 == "participates_in "))
                            {
                                EjecutaSPARQL(valor, padre, level);
                            }
                            count++;
                        }

                    }
                }
                else
                {
                    string levelColor = "";
                    Boolean tipo2 = false;
                    Boolean tipo3 = false;
                    levelColor=ObtenerLevelColor(level);
                    //primero recorremos el propio nodo para obtener la key
                    foreach (var linea in n.Results)
                    {
                        string todos = "";
                        string[] res = linea.ToString().Split(',');
                        todos = AsignaValores(res);
                        string[] res2 = todos.ToString().Split('·');

                        string disn = res2[0];
                        string label = res2[1];
                        string rel = res2[2];
                        string valor = res2[3];

                        string[] prueba = rel.Split('#');
                        if (prueba.Length > 1)
                        {
                            var prueba1 = prueba[1].ToString();
                            if (prueba1 == "label ")
                            {
                                string valorNodo = "";
                                string[] cadena = rel.Split('#');
                                string[] cadena2 = valor.Split('-');
                                string[] cadena3 = cadena2[0].Split('=');
                                if (cadena2.Length > 1)
                                {
                                    valorNodo = cadena3[1] + ":" + cadena2[1];
                                }
                                else
                                {
                                    valorNodo = cadena3[1];
                                }
                                if ((valorNodo.StartsWith(" Interaction context :")) || (valorNodo.StartsWith(" Protocol")) || (valorNodo.StartsWith(" Pathogen context")) || (valorNodo.StartsWith(" Allele")) || (valorNodo.StartsWith(" Gene -")))//falta pathogencontext la rama mas larga 
                                {
                                    tipo2 = true;
                                }
                                if ((valorNodo.StartsWith(" Description :")))
                                {
                                    tipo3 = true;
                                }
                                string key = "{\"key\":\"";
                                string parent = "\" , \"parent\": \"";
                                string color = "\" ,\"color\":\"";

                                string resultado = key + valorNodo + parent + padre + color + levelColor + "\"";
                                json = json + resultado;
                                padre = valorNodo;
                            }
                        }
                    }
                    if (!tipo2 && !tipo3)
                    {
                        int i = 0;
                        int tot = n.Results.Count;
                        // ahora se calcula el has_value si es que existe.
                        foreach (var linea in n.Results)
                        {
                            string todos = "";
                            string[] res = linea.ToString().Split(',');
                            todos = AsignaValores(res);
                            string[] res2 = todos.ToString().Split('·');

                            string disn = res2[0];
                            string label = res2[1];
                            string rel = res2[2];
                            string valor = res2[3];

                            string[] prueba = rel.Split('#');
                            if (prueba.Length > 1)
                            {
                                var prueba1 = prueba[1].ToString();
                                if (prueba1 == "has_value ")
                                {
                                    /*
                                       @author: Gerardo Lagunes García
                                       @version: Octubre 2016 - Enero 2017
                                       @description:
                                           Actualización que permite hacer un cierre siempre y cuando no exista un elemento de cierre  
                                   */
                                    string sCierre = json.Substring((json.Length - 2), 2);
                                    if (String.Compare(sCierre, "},") != 0)
                                    {
                                        string[] cadena = valor.Split('=');
                                        string texto = ",\"texto\": \"";
                                        string resultado = texto + cadena[1] + "\"";
                                        json = json + resultado.Replace('$', '"') + "},";
                                    }
                                }
                                i++;
                                if (i == tot)
                                {
                                    /*
                                       @author: Gerardo Lagunes García
                                       @version: Octubre 2016 - Enero 2017
                                       @description:
                                           Actualización que permite hacer un cierre siempre y cuando no exista un elemento de cierre  
                                   */
                                    string sCierre = json.Substring((json.Length - 2), 2);
                                    if (String.Compare(sCierre, "},") != 0)
                                    {
                                        string fin = "},";
                                        json = json + fin;
                                    }
                                }
                            }
                            else
                            {
                                //string texto = "\" ,\"texto\": \"\""; // @LnDelete
                                //string fin = "\"},"; LA LINEA DE ABAJO DE PUEDE ELIMINAR // @LnDelete

                                //string texto = " ,\"texto\": \"\""; // @LnDelete
                                //string fin = "},"; // @LnDelete
                                //json = json + texto + fin; // @LnDelete
                            }
                        }
                        // ahora se recorren las relaciones de este nodo
                        foreach (var linea in n.Results)
                        {
                            string todos = "";
                            string[] res = linea.ToString().Split(',');
                            todos = AsignaValores(res);
                            string[] res2 = todos.ToString().Split('·');

                            string disn = res2[0];
                            string label = res2[1];
                            string rel = res2[2];
                            string valor = res2[3];

                            string[] prueba = rel.Split('#');
                            if (prueba.Length > 1)
                            {
                                var prueba1 = prueba[1].ToString();
                                if ((prueba1 == "has_participant ") || (prueba1 == "has_unique_identifier ") || (prueba1 == "is_manifested_as ") || (prueba1 == "has_unique_identifier "))//relaciones
                                {
                                    EjecutaSPARQL(valor, padre, level);
                                }
                            }
                        }
                    }
                    else if (tipo2)
                    {
                        int i = 0;
                        int tot = n.Results.Count;
                        Boolean finHasValue = false;
                        // ahora se calcula el has_value si es que existe.
                        foreach (var linea in n.Results)
                        {
                            string todos = "";
                            string[] res = linea.ToString().Split(',');
                            todos = AsignaValores(res);
                            string[] res2 = todos.ToString().Split('·');

                            string disn = res2[0];
                            string label = res2[1];
                            string rel = res2[2];
                            string valor = res2[3];

                            string[] prueba = rel.Split('#');
                            if (prueba.Length > 1)
                            {
                                var prueba1 = prueba[1].ToString();
                                if (prueba1 == "has_value ")
                                {
                                    string[] cadena = valor.Split('=');
                                    string texto = ",\"texto\": \"";
                                    string resultado = texto + cadena[1] + "\"";
                                    json = json + resultado.Replace('$', '"');
                                }
                                i++;
                                if (i == tot)
                                {
                                    string fin = "},";
                                    json = json + fin;
                                }
                            }
                            else
                            {
                                if (!finHasValue)
                                {
                                    string fin = "},";
                                    json = json + fin;
                                    finHasValue = true;
                                }
                            }
                        }
                        // ahora se recorren las relaciones de este nodo
                        foreach (var linea in n.Results)
                        {
                            string todos = "";
                            string[] res = linea.ToString().Split(',');
                            todos = AsignaValores(res);
                            string[] res2 = todos.ToString().Split('·');

                            string disn = res2[0];
                            string label = res2[1];
                            string rel = res2[2];
                            string valor = res2[3];

                            string[] prueba = rel.Split('#');
                            if (prueba.Length > 1)
                            {
                                var prueba1 = prueba[1].ToString();
                                //ESTO DEBERÍA COPIARLO AL PRINCIPIO PARA QUE SE CREE EL JSON
                                if ((prueba1 == "depends_on ") || (prueba1 == "has_quality ") || (prueba1 == "is_output_of ") || (prueba1 == "is_variant_of ") || (prueba1 == "has_unique_identifier "))//relaciones
                                {
                                    EjecutaSPARQL(valor, padre, level);
                                }
                            }
                        }
                    }
                    else
                    {
                        int i = 0;
                        int tot = n.Results.Count;
                        // ahora se calcula el (has_value y is_describe_by) si es que existe.
                        foreach (var linea in n.Results)
                        {
                            string todos = "";
                            string[] res = linea.ToString().Split(',');
                            todos = AsignaValores(res);
                            string[] res2 = todos.ToString().Split('·');

                            string disn = res2[0];
                            string label = res2[1];
                            string rel = res2[2];
                            string valor = res2[3];

                            string[] prueba = rel.Split('#');
                            if (prueba.Length > 1)
                            {
                                var prueba1 = prueba[1].ToString();

                                /*
                                    @author: Gerardo Lagunes García
                                    @version: Octubre 2016 - Enero 2017
                                    @description:
                                        Actualización que permite corregir la aplicación del resultado de ordenar los resultados
                                        de una consulta SPARQL (unos elementos aparecen antes que otros y no se podrian tratar 
                                        con el algoritmo anterior)  
                                */
                                if (prueba1 == "is_described_by ")
                                {
                                    string[] cadena = valor.Split('=');

                                    string texto = ", \"texto\": \""; ;
                                    string resultado = texto + "url: " + cadena[1];
                                    json = json + resultado;
                                    bEntra = true;
                                   
                                }
                                if (prueba1 == "has_value ")
                                {
                                    string[] cadena = valor.Split('=');
                                    if (!bEntra)
                                    {
                                        string texto = ", \"texto\": \""; ;
                                        string resultado = texto + cadena[1] + "\"},";
                                        json = json + resultado.Replace('$', '"');
                                    }
                                    else {
                                        string resultado = "|" + cadena[1] + "\"},";
                                        json = json + resultado.Replace('$', '"');
                                        bEntra = false;
                                    }
                                }
                                
                                
                                i++;
                                if (i == tot)
                                {
                                    /*
                                       @author: Gerardo Lagunes García
                                       @version: Octubre 2016 - Enero 2017
                                       @description:
                                           Actualización que permite hacer un cierre siempre y cuando no exista un elemento de cierre  
                                   */
                                    string sCierre = json.Substring((json.Length - 2), 2);
                                    if (String.Compare(sCierre, "},") != 0)
                                    {
                                        string fin = "\"},";
                                        json = json + fin;
                                    }
                                }
                            }
                        }

                        // ahora se recorren las relaciones de este nodo
                        foreach (var linea in n.Results)
                        {
                            string todos="";
                            string[] res = linea.ToString().Split(',');
                            todos = AsignaValores(res);
                            string[] res2 = todos.ToString().Split('·');

                            string disn = res2[0];
                            string label = res2[1];
                            string rel = res2[2];
                            string valor = res2[3];

                            string[] prueba = rel.Split('#');
                            if (prueba.Length > 1)
                            {
                                var prueba1 = prueba[1].ToString();
                                if ((prueba1 == "depends_on ") || (prueba1 == "has_quality ") || (prueba1 == "is_output "))//relaciones
                                {
                                    EjecutaSPARQL(valor, padre, level);
                                }
                            }
                        }
                    }
                }
            }
            catch (NotImplementedException queryEx)
            {
                //There was an error executing the query so handle it here
                Console.WriteLine(queryEx.Message);
            }
            return n;
        }

        /*
	        @author: Alejandro Rofríguez González, Gerardo Lagunes García
	        @version: Octubre 2016 - Enero 2017
	        @description:
                Asigna a los valores disn, label, rel y valor segun el contenido del arreglo enviado  
                @param:                    
                    string[] res   : arreglo de strings formado a partir de una URI
                @return: valores de disn, label, rel y valor concatenados por "·"
        */
        public string AsignaValores(string[] res)
        {
            int counter = 0;
            string disn = "";
            string label = "";
            string rel = "";
            string valor = "";

            foreach (var linea2 in res)
            {
                if (!linea2.StartsWith("@"))
                {
                    if (counter == 0)
                    {
                        disn = linea2.ToString();
                    }
                    if (counter == 1)
                    {
                        label = linea2.ToString();
                    }
                    if (counter == 2)
                    {
                        rel = linea2.ToString();
                    }
                    if (counter == 3)
                    {
                        valor = linea2.ToString();
                    }
                    counter++;
                }
            }
            return disn + "·" + label + "·" + rel + "·" + valor;
        }

        /*
	        @author: Alejandro Rofríguez González, Gerardo Lagunes García
	        @version: Octubre 2016 - Enero 2017
	        @description:
                Asigna el color de grafo según sea el nivel jerárquico recibido 
                @param:                    
                    int level   : nivel jerarquico en el JSON
                @return: NOmbre del color
        */
        public string ObtenerLevelColor(int level)
        {
            string levelColor = "";

            if (level == 1)
            {
                levelColor = "orange";
            }
            else if (level == 2)
            {
                levelColor = "lightblue";
            }
            else if (level == 3)
            {
                levelColor = "lightgreen";
            }
            else if (level == 4)
            {
                levelColor = "blue";
            }
            else if (level == 5)
            {
                levelColor = "purple";
            }
            else
            {
                levelColor = "yellow";
            }

            return levelColor;
        }

        /*
	        @author: Alejandro Rofríguez González, Gerardo Lagunes García
	        @version: Octubre 2016 - Enero 2017
	        @description:
                Ejecuta una consulta SPARQL y llama a metodoRecursivo
                @param: 
                    string valor: valor del enlace
                    string padre: padre en el JSON
                    int level   : nivel jerárquico en el JSON
                @return: No retorna
        */
        public void EjecutaSPARQL(string valor, string padre, int level)
        {
                //Decclaramos todos los string necesarios para las consultas
                string[] k = valor.Split('=');
                string url1 = k[1];
                string url = url1.TrimStart();
                //Cargamos el prefijo para usarlo en todas las consultas
                string prefijos = "PREFIX RO: <http://www.obofoundry.org/ro/ro.owl#>PREFIX SIO: <http://semanticscience.org/resource/>PREFIX EDAM:  <http://edamontology.org/>PREFIX PHIO: <http://linkeddata.systems/ontologies/SemanticPHIBase#>PREFIX PUBMED:  <http://linkedlifedata.com/resource/pubmed/>PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>PREFIX up:  <http://purl.uniprot.org/core/>PREFIX foaf: <http://xmlns.com/foaf/0.1/>PREFIX skos: <http://www.w3.org/2004/02/skos/core#>";
                string inicio = "SELECT DISTINCT ?disn_1 ?label ?rel ?valor WHERE { ?disn_1 ?rel ?valor . ?disn_1 rdfs:label ?label FILTER(( ?disn_1 = <";
                //string fin = ">))}";
                string fin = ">))";
                string sfin = "} ORDER BY DESC(?rel)";

                //Se establece el endpoint para las consultas
                SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://linkeddata.systems:8890/sparql"));

                string query = prefijos + inicio + url + fin + sCorreccion + sfin;
                SparqlResultSet resultQuery2 = endpoint.QueryWithResultSet(query);
                //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\gerar\Documents\glg.txt", true)) { file.WriteLine("metodoRecursivo QUERY_3: " + query); }
                //recorremos los resultados de esta primera query
                if (resultQuery2.Results != null)
                {
                    metodoRecursivo(resultQuery2, padre, level);
                }
        }
    }
}
