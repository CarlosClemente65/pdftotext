using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdftotext
{
    public class busquedaContrato
    {
        ////Definicion de variables necesarias para procesar datos

        //Instanciacion de la clase procesosPDF para usar sus metodos en esta
        procesosPDF procesosPDF = new procesosPDF();

        //Definicion de variables a nivel de clase con sus metodos get y set
        public string RefContrato { get; set; }

        //Definicion de patrones de busqueda de datos

        //El numero de contrato siempre es la cadena "Ref.contrato: " seguida de un numero de 6 digitos con ceros por la izquierda
        private string patronContrato = @"Ref\.contrato:\s*(\d{6})";

        // Constructor de la clase
        public busquedaContrato(string textoCompleto, List<string> paginasPDF)
        {
            //Constructor de la clase que inicializa las variables
            Program.textoCompleto = textoCompleto;
            this.RefContrato = string.Empty;
        }

        public string buscarDatos()
        {
            string resultado = string.Empty;
            try
            {
                RefContrato = procesosPDF.ProcesaPatron(patronContrato, 1,"","",0,true);
                if(RefContrato == "")
                {
                    if(Program.textoCompleto == "")
                    {
                        resultado= "Error: Documento PDF no valido (revise si es una imagen)";
                    }
                    else
                    {
                        resultado = "Error: numero de contrato no encontrado (revise el formato del contrato)";
                    }
                }
                else
                {
                    resultado = $"Contrato: {RefContrato}";
                }
            }
            catch
            {
                resultado = "Error: Se ha producido un error al procesar el fichero";
            }
            return resultado;

        }
    }
}
