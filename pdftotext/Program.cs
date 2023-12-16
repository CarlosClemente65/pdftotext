using System.Reflection;

[assembly: AssemblyTitle("Extrae en formato texto los datos de los modelos de Hacienda en PDF")]
[assembly: AssemblyProduct("dsedatosmodelos")]
[assembly: AssemblyDescription("Extrae en formato texto los datos de los modelos de Hacienda en PDF")]
[assembly: AssemblyCompany("Diagram Software Europa S.L.")]
[assembly: AssemblyCopyright("© 2023 - Diagram Software Europa S.L.")]
[assembly: AssemblyVersion("1.1.1.0")]
[assembly: AssemblyFileVersion("1.1.1.0")]


namespace pdftotext
{
    /*
     Aplicacion para la busqueda de campos de los modelos en PDF de Hacienda para archivarlos en el GAD.
    Desarrollada por Carlos Clemente - 12/2023
    
    Uso:
        dse_datosModelos fichero.pdf salidaDatos.txt salidaTexto.txt
        Nota: el segundo parametro es el fichero con los campos localizados y el tercero y siguientes es la forma de extraccion (modelos, texto completo u otras formas que se puedan implementar en el futuro)
        
    */

    class Program
    {
        //La variable debug permite añadir el numero de linea al texto extraido
        public static bool continuar = false;

        static void Main(string[] args)
        {
            //Instanciacion de la clase procesosPDF para acceder a los metodos definidos en ella
            procesosPDF proceso = new procesosPDF();

            //Se deben pasar al menos 3 parametros: fichero PDF, tipo de proceso y fichero de salida
            if (args.Length > 2)
            {
                //Proceso de los parametros que se pasan
                continuar = proceso.gestionParametros(args);
            }

            //Si los parametros pasados son correctos
            if (continuar)
            {
                //Extrae el texto del PDF
                proceso.extraeTextoPDF();

                //Extrae datos del modelo
                if (proceso.procesaModelo)
                {
                    proceso.extraeDatosModelo();
                }

                //Graba el fichero con el texto completo
                if (proceso.extraeTexto)
                {
                    File.WriteAllText(proceso.ficheroTexto, proceso.textoCompleto);
                }
            }
        }
    }
}
