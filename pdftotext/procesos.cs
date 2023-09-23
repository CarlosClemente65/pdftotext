using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace pdftotext
{
    internal class procesos
    {
        public string filePath = string.Empty;
        public string[] filesPDF;
        public int firstpage = 0;
        public int lastpage = 0;
        //La variable parametroOk sirve para controlar que se ha puesto al menos un parametro correcto
        public bool parametroOk = false;
        public string opcion = "-f";
        public string outputFilePath = "";
        StringBuilder extractedText = new StringBuilder();

        public procesos()
        {
            filesPDF = new string[0];
        }
        public void MensajeParametros(string mensaje)
        {
            Console.Clear();
            //Console.WriteLine();
            Console.WriteLine(mensaje);
            Console.WriteLine("\nUso:\tpdftotext fichero.pdf [-f | -m] [-l] [-p pagina o intervalo de paginas separadas por un guion]");
            Console.WriteLine("\t-f   Genera texto completo del PDF que se pase como argumento");
            Console.WriteLine("\t-m   Genera un fichero con los datos principales del modelo de Hacienda");
            Console.WriteLine("\t-l   Si se utiliza, debe pasarse como primer parametro, y se procesaran todos los ficheros .PDF de la carpeta de ejecucion de la aplicacion");
            Console.WriteLine("\t     Las opciones -f y -m son incompatibles entre si");
            Console.WriteLine("\t-p   Especifica la pagina o intervalo de paginas a extraer:");
            Console.WriteLine("\t     3 = pagina 3, 2-4 = paginas 2 a 4");
            Console.WriteLine("");
            Console.WriteLine("Pulse una tecla para continuar...");
            Console.ReadKey();
        }

        public void leerFicheros(string[] parametros)
        {
            string folderPath = string.Empty;
            if (Program.debug)
            {
                //Ruta fija para las pruebas
                folderPath = @"c:\borrar\pdftotext";
            }
            else
            {
                //Quitar esta linea cuando acabe el debug y poner la otra
                folderPath = @"c:\borrar\pdftotext";
                //folderPath = Directory.GetCurrentDirectory();
            }

            switch (parametros[0])
            {
                //Si el argumento es -l almacena el nombre de todos los PDF de la carpeta
                case "-l":
                    {
                        parametroOk = true;
                        opcion = "-l";
                        if (Program.debug)
                        {
                            filesPDF = Directory.GetFiles(folderPath, "*.pdf");
                        }
                        else
                        {
                            filesPDF = Directory.GetFiles(".", "*.pdf");
                        }

                        //Mensaje si no hay ningun fichero PDF
                        if (filesPDF.Length == 0)
                        {
                            Console.WriteLine("No hay ningun fichero PDF para procesar");
                            return;
                        }
                        break;
                    }

                default:
                    {
                        //Almacena el nombre del fichero a procesar (primer parametro)
                        filePath = parametros[0];
                        filePath = Path.Combine(folderPath, filePath);
                        if (!filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            filePath += ".pdf";
                        }
                        //Chequea que el fichero pasado existe
                        if (!File.Exists(filePath))
                        {
                            Console.Clear();
                            Console.WriteLine($"El fichero {filePath}.pdf no existe.\nPulse una tecla para salir");
                            Console.ReadKey();
                            Environment.Exit(1);
                        }
                        //Chequeo que el fichero pasado es un PDF
                        if (string.Equals(Path.GetExtension(filePath) != ".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.Clear();
                            Console.WriteLine("El archivo debe ser de tipo PDF.\nPulse una tecla para salir");
                            Console.ReadKey();
                            return;
                        }

                        //Como el argumento no es procesar todos los ficheros, se almacena el fichero pasado como parametro en la variable que procesa el array de ficheros

                        filesPDF = new string[1];
                        filesPDF[0] = filePath;

                        break;
                    }
            }
        }

        public void gestionArgumentos(string[] parametros)
        {
            //Las variables fOption y mOption sirven para chequear que no se pasan los dos parametros
            bool fOption = false;
            bool mOption = false;

            //Proceso de los parametros pasados
            for (int i = 1; i < parametros.Length; i++)
            {
                string parametro = parametros[i].ToLower();
                switch (parametro)
                {
                    case "-f":
                        if (mOption)
                        {
                            MensajeParametros("Las opciones -f y -m son incompatibles entre si");
                        }
                        opcion = "-f";
                        fOption = true;
                        parametroOk = true;
                        break;

                    case "-m":
                        if (fOption)
                        {
                            MensajeParametros("Las opciones -f y -m son incompatibles entre si");
                        }
                        opcion = "-m";
                        mOption = true;
                        parametroOk = true;
                        break;

                    case "-p":
                        //Control de que se pasan las paginas inicio y fin separadas con un guion
                        int pageIndex;
                        if (parametros.Length > Array.IndexOf(parametros, parametro) + 1 && int.TryParse(parametros[Array.IndexOf(parametros, parametro) + 1], out pageIndex))
                        {
                            if (pageIndex < 0)
                            {
                                MensajeParametros("No se puede indicar un numero de pagina negativo");
                                return;
                            }
                            firstpage = lastpage = pageIndex;
                            parametroOk = true;
                        }
                        else if (parametros.Length > Array.IndexOf(parametros, parametro) + 1)
                        {
                            string[] rangePages = parametros[Array.IndexOf(parametros, parametro) + 1].Split('-');
                            if (rangePages.Length == 1)
                            {
                                if (!int.TryParse(rangePages[0], out lastpage) || lastpage < 1)
                                {
                                    MensajeParametros($"El argumento {parametro} debe estar seguido de un numero entero positivo");
                                    return;
                                }
                                firstpage = lastpage;
                            }
                            else if (rangePages.Length == 2)
                            {
                                if (!int.TryParse(rangePages[0], out firstpage) || firstpage < 1)
                                {
                                    MensajeParametros($"El argumento {parametro} debe estar seguido de dos números enteros positivos separados por un guión (-)");
                                    return;
                                }
                                if (!int.TryParse(rangePages[1], out lastpage) || lastpage < 1)
                                {
                                    MensajeParametros($"El argumento {parametro} debe estar seguido de dos números enteros positivos separados por un guión (-)");
                                    return;
                                }
                            }
                            else
                            {
                                MensajeParametros($"El argumento {parametro} debe estar seguido de dos números enteros positivos separados por un guión (-) o de un número entero positivo.");
                                return;
                            }
                            parametroOk = true;
                        }
                        else
                        {
                            MensajeParametros($"El argumento {parametro} debe estar seguido de dos números enteros positivos separados por un guión (-) o de un número entero positivo.");
                            return;
                        }
                        break;
                }
            }
            //Chequea si los parametros pasados son correctos.
            if (!parametroOk)
            {
                MensajeParametros("Algún parametro es incorrecto");
                return;
            }
        }

        public void grabarFichero()
        {
            //Hace un bucle para procesar todos los ficheros que se hayan leido
            foreach (string archivo in filesPDF)
            {
                filePath = archivo;
                //Se asigna al fichero de salida el mismo nombre con extension .txt
                outputFilePath = System.IO.Path.ChangeExtension(filePath, "txt");

                using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(filePath)))
                {
                    SimpleTextExtractionStrategy extractionStrategy = new SimpleTextExtractionStrategy();
                    //Almacena el contenido del PDF

                    //Controla si los numeros de pagina indicados estan en el intervalo de paginas del fichero
                    if (firstpage > pdfDoc.GetNumberOfPages() || lastpage > pdfDoc.GetNumberOfPages())
                    {
                        Console.Clear();
                        Console.WriteLine($"El intervalo de paginas {firstpage}-{lastpage} indicado no existe en el fichero {filePath}");
                        Console.WriteLine("\nPulse una tecla para continuar...");
                        Console.ReadLine();
                        return;
                    }
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
                        //Si se activa la variable debug, el fichero de salida se le añade el numero de linea para facilitar las busquedas.
                        if (Program.debug == true)
                        {
                            int lineNumber = 1;
                            //Extrae el texto de la pagina y le añade el numero de linea
                            string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), extractionStrategy);
                            foreach (string line in pageText.Split('\n'))
                            {
                                extractedText.AppendLine($"Linea {lineNumber++}: {line.Trim()}"); // Agrega el número de línea al principio de cada línea
                            }
                        }
                        else
                        {
                            //Extrae el texto completo de la pagina
                            extractedText.Append(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), extractionStrategy));
                        }
                    }

                    //Pone a cero las paginas por si se procesan varios ficheros
                    firstpage = 0;
                    lastpage = 0;

                    //Graba el fichero completo si el parametro es -f o -l
                    if (opcion == "-f" || opcion == "-l")
                    {
                        File.WriteAllText(outputFilePath, extractedText.ToString());
                    }

                    else if (opcion == "-m")
                    {
                        //Extraer datos del modelo
                        extraeDatosModelo();
                    }
                }
            }
        }

        public void extraeDatosModelo()
        {
            buscar busca = new buscar();

            //Si se pasa como parametro '-m' se generan todos las busquedas de los datos del modelo
            using (StreamWriter writer = new StreamWriter(outputFilePath)) //Crea el StreamWriter para grabar en el fichero a traves de un using para liberar recursos cuando acabe.
            {
                string textoCompleto = extractedText.ToString().Trim();

                //Obtener el CSV del PDF
                string csvPDF = busca.csv(textoCompleto);
                string textoCSV = string.Empty;
                if (csvPDF.Length == 16)
                {
                    textoCSV = $"CSV: {csvPDF}";
                }
                else
                {
                    textoCSV = "CSV no encontrado";
                }
                writer.WriteLine(textoCSV);

                //Obtener expediente del PDF
                string expedientePDF = busca.expediente(textoCompleto, csvPDF);
                string textoExpediente = string.Empty;
                if (expedientePDF.Length > 0)
                {
                    textoExpediente = $"Expediente: {expedientePDF}";
                }
                else
                {
                    textoExpediente = "Expediente no encontrado";
                }
                writer.WriteLine(textoExpediente);

                //Obtener numero de justificante
                string justificantePDF = busca.justificante(textoCompleto);
                string textoJustificante = string.Empty;
                if (justificantePDF.Length > 0)
                {
                    textoJustificante = $"Justificante: {justificantePDF}";
                }
                else
                {
                    textoJustificante = "Justificante no encontrado";
                }
                writer.WriteLine(textoJustificante);

                //Obtener NIF
                string nifPDF = busca.Nif(textoCompleto, csvPDF);
                string textoNif = string.Empty;
                if (nifPDF.Length > 0)
                {
                    textoNif = $"NIF: {nifPDF}";
                }
                else
                {
                    textoNif = "NIF no encontrado";
                }
                writer.WriteLine(textoNif);

                //Obtener Nombre
                string nombrePDF = busca.nombre(textoCompleto, nifPDF);
                string textoNombre = string.Empty;
                if (nombrePDF.Length > 0)
                {
                    textoNombre = $"Nombre: {nombrePDF}";
                }
                else
                {
                    textoNombre = "Nombre no encontrado";
                }
                writer.WriteLine(textoNombre);

                //Obtener ejercicio
                string ejercicioPDF = busca.ejercicio(expedientePDF);
                string textoEjercicio = string.Empty;
                if (ejercicioPDF.Length > 0)
                {
                    textoEjercicio = $"Ejercicio: {ejercicioPDF}";
                }
                else
                {
                    textoEjercicio = "Ejercicio no encontrado";
                }
                writer.WriteLine(textoEjercicio);


                //Obtener modelo
                string modeloPDF = busca.modelo(expedientePDF);
                string textoModelo = string.Empty;
                if (modeloPDF.Length > 0)
                {
                    textoModelo = $"Modelo: {modeloPDF}";
                }
                else
                {
                    textoModelo = "Modelo no encontrado";
                }

                writer.WriteLine(textoModelo);

                //Obtener periodo
                string periodoPDF = busca.periodo();
                writer.WriteLine(periodoPDF);
            }

        }

    }
}
