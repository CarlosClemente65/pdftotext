using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System.Text;

namespace pdftotext
{
    internal class procesosPDF
    {

        //Se definen las variables a nivel de clase para que esten accesibles

        //Variables para la gestion de ficheros
        public string ficheroPDF;
        public string ficheroDatos;
        public string ficheroTexto;

        //Variables para la gestion de paginas del PDF 
        public int firstpage = 0;
        public int lastpage = 0;

        //Variables para la extraccion de los datos del PDF
        StringBuilder extractedText = new StringBuilder();
        List<string> paginasPDF = new List<string>();
        string textoCompleto = string.Empty;

        public bool gestionParametros(string[] parametros)
        {
            ficheroPDF = parametros[0];
            ficheroDatos = parametros[1];
            ficheroTexto = parametros[2];

            if (!File.Exists(ficheroPDF))
            {
                File.WriteAllText("error_proceso.txt", "El fichero PDF no existe");
                return false;
            }

            return true;
        }

        public void extraeTextoPDF()
        {
            using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(ficheroPDF)))
            {
                //Si no se pasa la primera pagina se asigna la 1
                if (firstpage == 0)
                {
                    firstpage = 1;
                }
                //Si no se pasa la ultima pagina, se asigna el total de paginas del fichero
                if (lastpage == 0)
                {
                    lastpage = pdfDoc.GetNumberOfPages();
                }

                //Extrae el texto de las paginas del PDF y lo almacena en la variable extractedText
                for (int i = firstpage; i <= lastpage; i++)
                {
                    ITextExtractionStrategy extractionStrategy = new SimpleTextExtractionStrategy();
                    string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), extractionStrategy);
                    paginasPDF.Add(pageText);
                    //Si se activa la variable debug, el fichero de salida se le añade el numero de linea para facilitar las busquedas.
                    if (Program.debug == true)
                    {
                        int lineNumber = 1;
                        //Extrae el texto de la pagina y le añade el numero de linea

                        foreach (string line in pageText.Split('\n'))
                        {
                            extractedText.AppendLine($"Linea {lineNumber++}: {line.Trim()}"); // Agrega el número de línea al principio de cada línea
                        }
                    }
                    else
                    {
                        //Extrae el texto completo de la pagina
                        extractedText.Append($"\nPagina {i}\n");
                        extractedText.Append(pageText);
                    }
                }
                File.WriteAllText(ficheroTexto, extractedText.ToString());

                //Pone a cero las paginas por si se procesan varios ficheros
                firstpage = 0;
                lastpage = 0;
            }
        }

        public void extraeDatosModelo()
        {
            //Crea el StreamWriter para grabar en el fichero a traves de un using para liberar recursos cuando acabe.
            using (StreamWriter writer = new StreamWriter(ficheroDatos))
            {
                textoCompleto = extractedText.ToString().Trim();
                busqueda buscar = new busqueda(textoCompleto, paginasPDF);

                //Hacemos la llamada al metodo buscarDatos para que se almacenen los datos del PDF
                buscar.buscarDatos();

                //Grabamos los datos extraidos del PDF en el fichero
                writer.WriteLine($"NIF: {buscar.Nif}");
                if (buscar.NifConyuge != "")
                {
                    writer.WriteLine($"NIF conyuge: {buscar.NifConyuge}");
                }
                writer.WriteLine($"Modelo: {buscar.Modelo}");
                writer.WriteLine($"Ejercicio: {buscar.Ejercicio}");
                writer.WriteLine($"Periodo: {buscar.Periodo}");
                if (buscar.Modelo == "100")
                {
                    if (buscar.TributacionConjunta)
                    {
                        writer.WriteLine("Tributacion: conjunta");
                    }
                    else
                    {
                        writer.WriteLine("Tributacion: individual");
                    }
                }
                writer.WriteLine($"Justificante: {buscar.Justificante}");
                writer.WriteLine($"CSV: {buscar.Csv}");
                writer.WriteLine($"Expediente: {buscar.Expediente}");
            }
        }
    }
}
