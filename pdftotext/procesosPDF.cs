using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System.Text;
using System.Text.RegularExpressions;

namespace pdftotext
{
    internal class procesosPDF
    {
        //Se definen las variables a nivel de clase para que esten accesibles

        //Variables para la gestion de ficheros
        public string ficheroPDF; //Fichero PDF a procesar
        public string ficheroDatos; //Fichero de salida con los datos del modelo
        public string ficheroTexto; //Fichero de salida con el texto completo
        public List<string> archivosPDF = new List<string>(); //Lista con todos los ficheros a procesar

        //Variables para la gestion de parametros
        public bool grabaTexto = false; //Si se pasa el parametro -t se graba el texto completo en un fichero
        public string tipoProceso = string.Empty;
        //Añadir nuevas variables para otros usos en el futuro

        //Variables para la gestion de paginas del PDF 
        public int firstpage = 1;
        public int lastpage = 1;

        //Variables para la extraccion de los datos del PDF
        StringBuilder extractedText = new StringBuilder();


        public void extraeTextoPDF()
        {
            extractedText.Clear();
            using(PdfDocument pdfDoc = new PdfDocument(new PdfReader(ficheroPDF)))
            {
                lastpage = pdfDoc.GetNumberOfPages();

                //Extrae el texto de las paginas del PDF y lo almacena en la variable extractedText
                for(int i = firstpage; i <= lastpage; i++)
                {
                    ITextExtractionStrategy extractionStrategy = new SimpleTextExtractionStrategy();
                    string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), extractionStrategy);

                    //Se añade el texto de la pagina a la variable paginasPDF que es una lista con las paginas por separado
                    Program.paginasPDF.Add(pageText);

                    //Se añade el texto de la pagina a la variable extractedText que es un string con todo el texto del PDF
                    extractedText.Append(pageText);
                }

                //Se almacena el texto extraido para poder procesarlo
                Program.textoCompleto = extractedText.ToString().Trim();
            }
        }

        public void extraeDatosModelo()
        {
            //Instanciacion de la clase busqueda para usar los metodos
            busquedaModelo buscar = new busquedaModelo(Program.textoCompleto, Program.paginasPDF);

            //Hacemos la llamada al metodo buscarDatos para que se almacenen los datos del PDF
            buscar.buscarDatos();

            //Almacena el texto a grabar en el fichero
            StringBuilder texto = new StringBuilder();
            texto.AppendLine($"NIF: {buscar.Nif}");
            if(buscar.NifConyuge != "")
            {
                texto.AppendLine($"NIF conyuge: {buscar.NifConyuge}");
            }
            texto.AppendLine($"Modelo: {buscar.Modelo}");
            texto.AppendLine($"Ejercicio: {buscar.Ejercicio}");
            texto.AppendLine($"Periodo: {buscar.Periodo}");
            if(buscar.Modelo == "100")
            {
                if(buscar.TributacionConjunta)
                {
                    texto.AppendLine("Tributacion: conjunta");
                }
                else
                {
                    texto.AppendLine("Tributacion: individual");
                }
            }
            texto.AppendLine($"Justificante: {buscar.Justificante}");
            texto.AppendLine($"CSV: {buscar.Csv}");
            texto.AppendLine($"Expediente: {buscar.Expediente}");
            if(!string.IsNullOrEmpty(buscar.fecha036))
            {
                texto.AppendLine($"Fecha presentacion 036/037: {buscar.fecha036}");
            }
            if(buscar.complementaria)
            {
                texto.AppendLine($"Complementaria: SI");
            }

            //Graba el fichero de datos con el texto creado
            grabaFichero(ficheroDatos, texto.ToString());
        }

        public void extraeDatosLaboral()
        {
            //Instanciacion de la clase busqueda para usar los metodos
            busquedaLaboral buscar = new busquedaLaboral();

            //Hacemos la llamada al metodo buscarDatos para que se almacenen los datos del PDF
            buscar.buscarDatos();

            //Almacena el texto a grabar en el fichero
            StringBuilder texto = new StringBuilder();

            texto.AppendLine($"Modelo: {busquedaLaboral.ModeloNum}");
            texto.AppendLine($"Tipo modelo: {busquedaLaboral.TipoModelo}");
            texto.AppendLine($"Codigo cuenta cotizacion: {busquedaLaboral.CCC}");
            texto.AppendLine($"CIF empresa: {busquedaLaboral.Nif}");
            texto.AppendLine($"NIF trabajador: {busquedaLaboral.DniTrabajador}");
            texto.AppendLine($"Ejercicio: {busquedaLaboral.Ejercicio}");
            texto.AppendLine($"Periodo: {busquedaLaboral.Periodo}");
            texto.AppendLine($"Fecha efecto: {busquedaLaboral.FechaEfecto}");
            texto.AppendLine($"Observaciones1: {busquedaLaboral.Observaciones1}");
            texto.AppendLine($"Observaciones2: {busquedaLaboral.Observaciones2}");
            texto.AppendLine($"Observaciones3: {busquedaLaboral.Observaciones3}");
            texto.AppendLine($"Campo libre 1: {busquedaLaboral.CampoLibre1}");
            texto.AppendLine($"Campo libre 2: {busquedaLaboral.CampoLibre2}");

            //Graba el fichero de datos con el texto creado
            grabaFichero(ficheroDatos, texto.ToString());

        }

        public void extraeDatosContrato()
        {
            //Instanciacion de la clase busqueda para usar los metodos
            busquedaContrato buscar = new busquedaContrato(Program.textoCompleto, Program.paginasPDF);

            //Hacemos la llamada al metodo buscarDatos para que se almacenen los datos del PDF y se slmacena el texto a grabar en el fichero
            StringBuilder texto = new StringBuilder(buscar.buscarDatos());

            //Graba el fichero de datos con el texto creado
            grabaFichero(ficheroDatos, texto.ToString());

        }

        public string ProcesaPatron(string patronRegex, int pagina, string Modelo = "", string TipoModelo = "", int indice = 0, bool esContrato = false)
        {
            //Metodo para extraer el texto segun el patron de busqueda pasado. Opcionalmente se puede pasar el modelo, tipo de modelo y el indice del texto encontrado a devolver. 

            //Se pasa por parametro el valor (indice) que se quiere devolver porque en algunos casos no es el primero (valor 0), sino que puede ser el segundo (valor 1) o siguientes

            // El parametro 'esContrato' se ha añadido porque en el patron se hace un grupo de captura y solo se quiere devolver el grupo1 que es donde esta el nº contrato

            Regex regex = new Regex(patronRegex);
            MatchCollection matches = regex.Matches(Program.paginasPDF[pagina - 1].ToString());

            // Numero de ocurrecias del texto encontradas
            int ocurrenciasTexto = matches.Count;

            // Si se pasa un indice superior al de ocurrencias, se devuelve la ultima encontrada
            if(indice > ocurrenciasTexto - 1 && ocurrenciasTexto > 0) //Se resta 1 porque el indice empieza por cero
            {
                indice = ocurrenciasTexto - 1;
            }

            //Si encuentra algo 
            if(ocurrenciasTexto > 0)
            {
                if(esContrato && matches[indice].Groups.Count > 0)
                {
                    // Cuando se busca el texto en un contrato solo se devuelve la captura del grupo1 (se busca un texto pero solo se necesita el numero)
                    return matches[indice].Groups[1].Value;
                }
                else
                {
                    // En otro caso se devuelve el valor segun el indice pasado
                    return matches[indice].Value;
                }
            }
            else
            {
                return "";
            }
        }

        public void grabaFichero(string pathFichero, string texto)
        {
            File.WriteAllText(pathFichero, texto);
        }

        public void ProcesoMasivo(string rutaPDF)
        {
            if(Directory.Exists(rutaPDF))
            {
                try
                {
                    archivosPDF.AddRange(Directory.GetFiles(rutaPDF, "*.pdf"));
                }

                catch(Exception ex)
                {
                    grabaFichero("error_proceso.txt", "No hay ningun fichero PDF en la carpeta seleccionada\r\n" + ex);
                    Environment.Exit(0);
                }
            }
        }
    }
}
