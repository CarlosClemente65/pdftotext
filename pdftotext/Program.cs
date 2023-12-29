using System.Reflection;



[assembly: AssemblyTitle("Convierte un PDF en texto y extrae los datos de los modelos de Hacienda")]
[assembly: AssemblyProduct("dsepdfatexto")]
[assembly: AssemblyDescription("Convierte un PDF en texto y extrae los datos de los modelos de Hacienda")]
//[assembly: AssemblyCompany("Diagram Software Europa S.L.")]
[assembly: AssemblyCopyright("© 2023 - Diagram Software Europa S.L.")]
[assembly: AssemblyVersion("1.2.1.0")]
[assembly: AssemblyFileVersion("1.2.1.0")]


namespace pdftotext
{
    /*
    Aplicacion para convertir a texto un PDF y buscar los datos de los modelos de Hacienda para archivarlos en el GAD.
    Con la version 1.2 se incluye la extraccion de datos de los modelos laborales
    Desarrollada por Carlos Clemente - 12/2023
    
    Uso:
        dsepdfatexto fichero.pdf -m datosModelo.txt -t textoPDF.txt -l datosLaboral.txt
        Nota: el parametro -m es el fichero con los campos localizados del modelo, el parametro -t es el texto completo del PDF, y el parametro -l es el fichero con los campos localizados del modelo laboral
    */

    class Program
    {
        //Variable para chequear si los parametros pasados son correctos
        public static bool continuar = false;

        //Definicion de variables para almacenar el texto del PDF (se definen aqui para que esten accesibles desde todas las clases)
        public static List<string> paginasPDF = new List<string>();
        public static string textoCompleto = string.Empty;

        //Instanciacion de la clase procesosPDF para acceder a los metodos definidos en ella
        public static procesosPDF proceso = new procesosPDF();

        static void Main(string[] args)
        {
            //Se deben pasar al menos 3 parametros: fichero PDF, tipo de proceso y fichero de salida
            if (args.Length > 2)
            {
                //Proceso de los parametros que se pasan
                continuar = gestionParametros(args);
            }

            //Si los parametros pasados son correctos realiza los procesos que se hayan pasado como parametros
            if (continuar)
            {
                //Extrae el texto del PDF
                proceso.extraeTextoPDF();

                //Graba el fichero con el texto completo
                if (proceso.extraeTexto)
                {
                    proceso.grabaFichero(proceso.ficheroTexto, textoCompleto);
                }

                //Extrae datos del modelo
                if (proceso.procesaModelo)
                {
                    proceso.extraeDatosModelo();
                }

                //Extrae datos de los documentos de laboral
                if (proceso.procesaLaboral)
                {
                    proceso.extraeDatosLaboral();
                }

            }
            else
            {
                if (Environment.UserInteractive)
                {
                    // Obtener la información del ensamblado actual
                    Assembly assembly = Assembly.GetExecutingAssembly();

                    // Obtener el atributo del copyright del ensamblado
                    AssemblyCopyrightAttribute copyrightAttribute =
                        (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute));

                    //Obtener el atributo del nombre del ensamblado
                    AssemblyProductAttribute nombreProducto = (AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));

                    // Obtener el valor de la propiedad Copyright y nombre del producto
                    string copyrightValue = copyrightAttribute?.Copyright;
                    string nombreValue = nombreProducto?.Product;



                    // Utilizar el valor en una cadena de texto
                    string mensaje = $"{nombreValue} - {copyrightValue}";


                    Console.BackgroundColor = ConsoleColor.DarkRed; 
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Title = mensaje;
                    Console.WriteLine("\n\tEsta aplicacion debe ejecutarse por linea de comandos.");
                    Console.WriteLine("\n\t\tPulse una tecla para salir");
                    Console.BufferHeight = 5;
                    Console.SetWindowPosition(0, 0);
                    Console.SetWindowSize(65,5);
                    Console.ReadKey();
                    Console.ResetColor();
                }
            }

            bool gestionParametros(string[] parametros)
            {
                int totalParametros = parametros.Length;
                proceso.ficheroPDF = parametros[0];

                //Comprueba si existe el fichero PDF
                if (!File.Exists(proceso.ficheroPDF))
                {
                    proceso.grabaFichero("error_proceso.txt", "El fichero PDF no existe");
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
                                    proceso.ficheroDatos = parametros[i + 1];
                                    proceso.procesaModelo = true;
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
                                    proceso.ficheroTexto = parametros[i + 1];
                                    proceso.extraeTexto = true;
                                    controlParametros++;
                                }
                            }
                            break;

                        case "-l":
                            //Procesado de documentos laborales en PDF. El nombre del fichero de salida debe venir a continuacion del parametro
                            if (totalParametros > i + 1)
                            {
                                //El siguiente parametro debe ser el fichero de salida
                                if (parametros[i + 1].Length > 2)
                                {
                                    proceso.ficheroDatos = parametros[i + 1];
                                    proceso.procesaLaboral = true;
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

        }
        
    }
}
