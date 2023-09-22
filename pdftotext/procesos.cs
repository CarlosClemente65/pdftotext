using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System.Text;

namespace pdftotext
{
    internal class procesos
    {
        public string filePath = "";
        public string[] filesPDF;
        public int firstpage = 0;
        public int lastpage = 0;
        //La variable parametroOk sirve para controlar que se ha puesto al menos un parametro correcto
        public bool parametroOk = false;
        public string opcion = "-f";

        public procesos()
        {
            filesPDF = new string[0];
        }

        public void leerFicheros(string[] parametros)
        {
            string folderPath = string.Empty;
            if (Program.debug)
            {
                //Ruta fija para las pruebas
                folderPath = @"c:\borrar\pdftotext";
            }

            //Si el argumento es -l almacena el nombre de todos los PDF de la carpeta
            switch (parametros[0])
            {
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
                            return;
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


        public void generarFicheroTexto()
        {
            foreach (string archivo in filesPDF)
            {
                filePath = archivo;
                //Se asigna al fichero de salida el mismo nombre con extension .txt
                string outputFilePath = System.IO.Path.ChangeExtension(filePath, "txt");
                using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(filePath)))
                {
                    SimpleTextExtractionStrategy extractionStrategy = new SimpleTextExtractionStrategy();
                    //Almacena el contenido del PDF
                    StringBuilder extractedText = new StringBuilder();
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

                    //Graba las paginas del PDF en el fichero de texto
                    for (int i = firstpage; i <= lastpage; i++)
                    {
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
                }
            }
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
    }
}
