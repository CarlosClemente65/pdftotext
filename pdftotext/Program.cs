using System.Reflection;

[assembly: AssemblyTitle("Convierte un PDF en texto y extrae los datos de los modelos de Hacienda")]
[assembly: AssemblyProduct("dsepdfatexto")]
[assembly: AssemblyDescription("Convierte un PDF en texto y extrae los datos de los modelos de Hacienda")]
//[assembly: AssemblyCompany("Diagram Software Europa S.L.")]
[assembly: AssemblyCopyright("© 2023 - Diagram Software Europa S.L.")]
[assembly: AssemblyVersion("1.1.1.0")]
[assembly: AssemblyFileVersion("1.1.1.0")]


namespace pdftotext
{
    /*
     Aplicacion para convertir a texto un PDF y buscar los datos de los modelos de Hacienda para archivarlos en el GAD.
    Desarrollada por Carlos Clemente - 12/2023
    
    Uso:
        dsepdfatext fichero.pdf -m datosModelo.txt -t textoPDF.txt
        Nota: el segundo parametro es el fichero con los campos localizados y el tercero es el texto completo del PDF. En un futuro podran implementarse mas
    */

    class Program
    {
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

            //Si los parametros pasados son correctos realiza los procesos que se hayan pasado como parametros
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
