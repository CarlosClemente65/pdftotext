using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdftotext
{
    internal class procesos
    {
        
        public void procesoFicheros(string[] argumentos)
        {
            //Si el argumento es -l almacena el nombre de todos los PDF de la carpeta
            switch (argumentos[0])
            {
                case "-l":
                    {
                       Program.parametroOk = true;
                        Program.opcion = "-l";
                        if (Program.debug)
                        {
                            //Ruta fija para las pruebas
                            string folderpath = @"c:\borrar\pdftotext";
                            Program.filesPDF = Directory.GetFiles(folderpath, "*.pdf");
                        }
                        else
                        {
                            Program.filesPDF = Directory.GetFiles(".", "*.pdf");
                        }

                        //Mensaje si no hay ningun fichero PDF
                        if (Program.filesPDF.Length == 0)
                        {
                            Console.WriteLine("No hay ningun fichero PDF para procesar");
                            return;
                        }
                        break;
                    }

                default:
                    {
                        //Almacena el nombre del fichero a procesar (primer parametro)
                        Program.filePath = argumentos[0];
                        if (!Program.filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            Program.filePath += ".pdf";
                        }
                        //Chequea que el fichero pasado existe
                        if (!File.Exists(Program.filePath))
                        {
                            Console.Clear();
                            Console.WriteLine($"El fichero {Program.filePath}.pdf no existe");
                            Console.WriteLine("Pulse una tecla para salir");
                            Console.ReadKey();
                            return;
                        }
                        //Chequeo que el fichero pasado es un PDF
                        if (Path.GetExtension(Program.filePath) != ".pdf")
                        {
                            Console.Clear();
                            Console.WriteLine("El archivo debe ser de tipo PDF");
                            Console.WriteLine("Pulse una tecla para salir");
                            Console.ReadKey();
                            return;
                        }
                        break;
                    }

            }
        }
    }
}
