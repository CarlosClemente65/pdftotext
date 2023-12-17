using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System.Text;
using Microsoft.SqlServer.Server;

namespace pdftotext
{
    internal class procesosPDF
    {
        //Se definen las variables a nivel de clase para que esten accesibles

        //Variables para la gestion de ficheros
        public string ficheroPDF; //Fichero PDF a procesar
        public string ficheroDatos; //Fichero de salida con los datos del modelo
        public string ficheroTexto; //Fichero de salida con el texto completo

        //Variables para la gestion de parametros
        public bool procesaModelo = false; //Si se pasa el parametro -m se procesan los datos del modelo
        public bool extraeTexto = false; //Si se pasa el parametro -t se graba el texto completo en un fichero
        //Añadir nuevas variables para otros usos en el futuro

        //Variables para la gestion de paginas del PDF 
        public int firstpage = 1;
        public int lastpage = 1;

        //Variables para la extraccion de los datos del PDF
        StringBuilder extractedText = new StringBuilder();
        public List<string> paginasPDF = new List<string>();
        public string textoCompleto = string.Empty;

        public bool gestionParametros(string[] parametros)
        {
            int totalParametros = parametros.Length;
            ficheroPDF = parametros[0];
            //Comprueba si existe el fichero PDF
            if (!File.Exists(ficheroPDF))
            {
                File.WriteAllText("error_proceso.txt", "El fichero PDF no existe");
                return false;
            }

            //Procesado del resto de parametros
            int controlParametros = 0;
            for (int i = 1; i < totalParametros; i++)
            {
                switch (parametros[i])
                {
                    case "-m":
                        //Procesado de modelos PDF. El nombre del fichero de salida debe venir a continuacion del parametro
                        if (totalParametros > i + 1)
                        {
                            //El siguiente parametro debe ser el fichero de salida
                            if (parametros[i + 1].Length > 2)
                            {
                                ficheroDatos = parametros[i + 1];
                                procesaModelo = true;
                                controlParametros++;
                            }
                        }
                        break;

                    case "-t":
                        //Extraccion de texto completo del PDF. El nombre del fichero de salida debe venir a continuacion del parametro.
                        if (totalParametros > i + 1)
                        {
                            //El siguiente parametro debe ser el fichero de salida
                            if (parametros[i + 1].Length > 2)
                            {
                                ficheroTexto = parametros[i + 1];
                                extraeTexto = true;
                                controlParametros++;
                            }
                        }
                        break;
                }
            }

            //Control si los parametros pasados son correctos
            if (controlParametros > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void extraeTextoPDF()
        {
            using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(ficheroPDF)))
            {
                lastpage = pdfDoc.GetNumberOfPages();

                //Extrae el texto de las paginas del PDF y lo almacena en la variable extractedText
                for (int i = firstpage; i <= lastpage; i++)
                {
                    ITextExtractionStrategy extractionStrategy = new SimpleTextExtractionStrategy();
                    string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), extractionStrategy);

                    //Se añade el texto de la pagina a la variable paginasPDF que es una lista con las paginas por separado
                    paginasPDF.Add(pageText);

                    //Se añade el texto de la pagina a la variable extractedText que es un string con todo el texto del PDF
                    extractedText.Append(pageText);
                }

                //Se almacena el texto extraido para poder procesarlo
                textoCompleto = extractedText.ToString().Trim();
            }
        }

        public void extraeDatosModelo()
        {
            //Instanciacion de la clase busqueda para usar los metodos
            busquedaModelo buscar = new busquedaModelo(textoCompleto, paginasPDF);

            //Hacemos la llamada al metodo buscarDatos para que se almacenen los datos del PDF
            buscar.buscarDatos();

            //Almacena el texto a grabar en el fichero
            string texto = string.Empty;
            texto += ($"NIF: {buscar.Nif} \n");
            if (buscar.NifConyuge != "")
            {
                texto += ($"NIF conyuge: {buscar.NifConyuge} \n");
            }
            texto += ($"Modelo: {buscar.Modelo} \n");
            texto += ($"Ejercicio: {buscar.Ejercicio} \n");
            texto += ($"Periodo: {buscar.Periodo} \n");
            if (buscar.Modelo == "100")
            {
                if (buscar.TributacionConjunta)
                {
                    texto += ("Tributacion: conjunta \n");
                }
                else
                {
                    texto += ("Tributacion: individual \n");
                }
            }
            texto += ($"Justificante: {buscar.Justificante} \n");
            texto += ($"CSV: {buscar.Csv} \n");
            texto += ($"Expediente: {buscar.Expediente} \n");
            if (!string.IsNullOrEmpty(buscar.fecha036))
            {
                texto += ($"Fecha presentacion 036/037: {buscar.fecha036} \n");
            }
            if (buscar.complementaria)
            {
                texto += ($"Complementaria: SI \n");
            }

            //Graba el fichero de datos con el texto creado
            File.WriteAllText(ficheroDatos, texto);
        }

        public void grabaFichero(string texto, string pathFichero)
        {
            File.WriteAllText(pathFichero, texto);
        }
    }
}
