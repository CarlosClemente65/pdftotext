using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

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
            using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(ficheroPDF)))
            {
                lastpage = pdfDoc.GetNumberOfPages();

                //Extrae el texto de las paginas del PDF y lo almacena en la variable extractedText
                for (int i = firstpage; i <= lastpage; i++)
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
            string texto = string.Empty;
            texto += ($"NIF: {buscar.Nif} \r\n");
            if (buscar.NifConyuge != "")
            {
                texto += ($"NIF conyuge: {buscar.NifConyuge} \r\n");
            }
            texto += ($"Modelo: {buscar.Modelo} \r\n");
            texto += ($"Ejercicio: {buscar.Ejercicio} \r\n");
            texto += ($"Periodo: {buscar.Periodo} \r\n");
            if (buscar.Modelo == "100")
            {
                if (buscar.TributacionConjunta)
                {
                    texto += ("Tributacion: conjunta \r\n");
                }
                else
                {
                    texto += ("Tributacion: individual \r\n");
                }
            }
            texto += ($"Justificante: {buscar.Justificante} \r\n");
            texto += ($"CSV: {buscar.Csv} \r\n");
            texto += ($"Expediente: {buscar.Expediente} \r\n");
            if (!string.IsNullOrEmpty(buscar.fecha036))
            {
                texto += ($"Fecha presentacion 036/037: {buscar.fecha036} \r\n");
            }
            if (buscar.complementaria)
            {
                texto += ($"Complementaria: SI \r\n");
            }

            //Graba el fichero de datos con el texto creado
            grabaFichero(ficheroDatos, texto);
        }

        public void extraeDatosLaboral()
        {
            //Instanciacion de la clase busqueda para usar los metodos
            busquedaLaboral buscar = new busquedaLaboral(Program.textoCompleto, Program.paginasPDF);

            //Hacemos la llamada al metodo buscarDatos para que se almacenen los datos del PDF
            buscar.buscarDatos();

            //Almacena el texto a grabar en el fichero
            string texto = string.Empty;

            switch (buscar.Modelo)
            {
                case "CER":
                    texto += "Modelo: 993";
                    texto += "\r\nTipo: 03";
                    texto += $"\r\nCIF: {buscar.Nif}";
                    texto += $"\r\nEjercicio: {buscar.Ejercicio}";
                    texto += $"\r\nPeriodo: {buscar.Periodo}";
                    break;

                case "RLC":
                case "RNT":
                    if (buscar.Modelo == "RLC")
                    {
                        texto += "Modelo: 997";
                    }
                    else
                    {
                        texto += "Modelo: 996";
                    }

                    switch (buscar.TipoModelo)
                    {
                        case "L00":
                            texto += "\r\nTipo: 00";
                            break;

                        case "L13":
                            texto += "\r\nTipo: 02";
                            break;

                        case "L03":
                            texto += "\r\nTipo: 05";
                            break;

                        case "L90":
                            texto += "\r\nTipo: 90";
                            break;
                    }

                    texto += $"\r\nCIF: {buscar.Nif}";
                    texto += $"\r\nEjercicio: {buscar.Ejercicio}";
                    texto += $"\r\nPeriodo: {buscar.Periodo}";
                    break;

                case "AFIA":
                    texto += "Modelo: 991";
                    texto += "\r\nTipo: 00";
                    texto += $"\r\nCodigo cuenta cotizacion: {buscar.CCC}";
                    texto += $"\r\nNIF trabajador: {buscar.DniTrabajador}";
                    texto += $"\r\nFecha efecto: {buscar.FechaEfecto}";
                    break;

                case "AFIB":
                    texto += "Modelo: 991";
                    texto += "\r\nTipo: 01";
                    texto += $"\r\nCodigo cuenta cotizacion: {buscar.CCC}";
                    texto += $"\r\nNIF trabajador: {buscar.DniTrabajador}";
                    texto += $"\r\nFecha efecto: {buscar.FechaEfecto}";
                    break;

                //Esta pendiente de desarrollo por falta de documento de ejemplo (faltaria el patron de busqueda)
                //case "AFIV": 
                //    texto += "Modelo: 991";
                //    texto += "\r\nTipo: 02";
                //    texto += $"\r\nCodigo cuenta cotizacion: {buscar.CCC}";
                //    texto += $"\r\nNIF trabajador: {buscar.DniTrabajador}";
                //    texto += $"\r\nFecha efecto: {buscar.FechaEfecto}";
                //    break;

                case "AFIC":
                    texto += "Modelo: 991";
                    texto += "\r\nTipo: 03";
                    texto += $"\r\nCodigo cuenta cotizacion: {buscar.CCC}";
                    texto += $"\r\nNIF trabajador: {buscar.DniTrabajador}";
                    texto += $"\r\nFecha efecto: {buscar.FechaEfecto}";
                    break;

                case "IDC":
                    texto += "Modelo: 991";
                    texto += $"\r\nTipo: {buscar.TipoIDC}";
                    texto += $"\r\nCodigo cuenta cotizacion: {buscar.CCC}";
                    texto += $"\r\nNIF trabajador: {buscar.DniTrabajador} ";
                    texto += $"\r\nFecha efecto: {buscar.PeriodoIDC}";
                    break;

                case "ITA":
                    texto += "Modelo: 991";
                    texto += "\r\nTipo: 07";
                    texto += $"\r\nCIF: {buscar.Nif}";
                    texto += $"\r\nEjercicio: {buscar.Ejercicio}";
                    texto += $"\r\nPeriodo: {buscar.Periodo}";
                    break;


                case "HUE":
                    texto += "Modelo: 992";
                    texto += "\r\nTipo: 00";
                    texto += $"\r\nCodigo cuenta cotizacion: {buscar.CCC}";
                    texto += $"\r\nNIF trabajador: {buscar.DniTrabajador}";
                    texto += $"\r\nFecha efecto: {buscar.FechaEfecto}";
                    break;
            }

            if (!string.IsNullOrEmpty(buscar.Observaciones1))
            {
                texto += $"\r\nObservaciones1: {buscar.Observaciones1}";
            }

            if (!string.IsNullOrEmpty(buscar.Observaciones2))
            {
                texto += $"\r\nObservaciones2: {buscar.Observaciones2}";
            }

            if (!string.IsNullOrEmpty(buscar.Observaciones3))
            {
                texto += $"\r\nObservaciones3: {buscar.Observaciones3}";
            }

            if (!string.IsNullOrEmpty(buscar.CampoLibre1))
            {
                texto += $"\r\nCampo libre 1: {buscar.CampoLibre1}";
            }

            if (!string.IsNullOrEmpty(buscar.CampoLibre2))
            {
                texto += $"\r\nCampo libre 2: {buscar.CampoLibre2}";
            }

            //Graba el fichero de datos con el texto creado
            grabaFichero(ficheroDatos, texto);

        }

        public string ProcesaPatron(string patronRegex, int pagina, string Modelo = "", string TipoModelo = "", int valor = 0)
        {
            //Metodo para extraer el texto segun el patron de busqueda pasado. Opcionalmente se puede pasar el modelo, tipo de modelo y el indice del texto encontrado a devolver
            Regex regex = new Regex(patronRegex);
            MatchCollection matches = regex.Matches(Program.paginasPDF[pagina - 1].ToString());

            //Si encuentra algo 
            if (matches.Count > 0)
            {
                return matches[valor].Value; //Se pasa por parametro el valor que se quiere devolver porque en algunos casos no es el primero (valor 0), sino que puede ser el segundo (valor 1) o siguientes
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
            if (Directory.Exists(rutaPDF))
            {
                try
                {
                    archivosPDF.AddRange(Directory.GetFiles(rutaPDF, "*.pdf"));
                }

                catch (Exception ex)
                {
                    grabaFichero("error_proceso.txt", "No hay ningun fichero PDF en la carpeta seleccionada\r\n"+ ex);
                    Environment.Exit(0);
                }
            }
        }
    }
}
