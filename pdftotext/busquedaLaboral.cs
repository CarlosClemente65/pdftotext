using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdftotext
{
    public class busquedaLaboral
    {
        //Instanciacion de la clase procesosPDF para usar sus metodos en esta
        procesosPDF procesosPDF = new procesosPDF();

        public string Nif { get; private set; }
        public string Ejercicio { get; private set; }
        public string Periodo { get; private set; }
        public string Modelo { get; private set; } 

        //Tipos de documento
        string RLC = string.Empty;
        string RNT = string.Empty;
        string ITA = string.Empty;
        string Certificado = string.Empty;
        



        #region patrones de busqueda
        //Definicion de patrones de busqueda de datos
        string patronNif = @"(?=.*[A-Z])\b\d[A-Z0-9]\d{7}[A-Z0-9]\b";
        string patronPeriodo = @"Fecha: \d{2}[/|.]\d{2}[/|.]\d{4}";
        string patronRLC = "";
        string patronRNT = "";
        string patronITA = "";
        string patronCertificado = "CERTIFICADO DE ESTAR AL CORRIENTE";



        #endregion

        public busquedaLaboral(string textoCompleto, List<string> paginasPDF)
        {
            //Construcctor de la clase que inicializa las variables
            Program.textoCompleto = textoCompleto;
            Program.paginasPDF = paginasPDF;
            this.Ejercicio = string.Empty;
            this.Periodo = string.Empty;
            this.Nif = string.Empty;
            this.Modelo = string.Empty;



        }

        public void buscarDatos()
        {
            BuscarTipoDocumento();
            if (!string.IsNullOrEmpty(Certificado))
            {
                BuscarPeriodo();
                BuscarNif();
            }
        }


        #region Metodos de busqueda
        private void BuscarTipoDocumento()
        {
            try
            {
                RLC = procesosPDF.ProcesaPatron(patronRLC,1);
                if (!string.IsNullOrEmpty(RLC))
                {
                    Modelo = "RLC";
                }
                RNT = procesosPDF.ProcesaPatron(patronRNT,1);
                if (!string.IsNullOrEmpty(RNT))
                {
                    Modelo = "RNT";
                }
                ITA = procesosPDF.ProcesaPatron(patronITA,1);
                if (!string.IsNullOrEmpty(ITA))
                {
                    Modelo = "ITA";
                }
                Certificado = procesosPDF.ProcesaPatron(patronCertificado,1);
                if (!string.IsNullOrEmpty(Certificado))
                {
                    Modelo = "CER";
                }
            }
            catch 
            {
                //Si no se encuentran los datos, se graba un fichero con el error.
                procesosPDF.grabaFichero("error_proceso.txt", "Documento no reconocido");
            }

        }

        private void BuscarPeriodo()
        {
            Periodo = procesosPDF.ProcesaPatron(patronPeriodo, 1);
            if (Periodo.Length > 0)
            {
                Ejercicio = Periodo.Substring(Periodo.Length - 4);
                Periodo = Periodo.Substring(10, 2);
            }
        }

        private void BuscarNif()
        {
            Nif = procesosPDF.ProcesaPatron(patronNif, 1);
            if(Nif.Length > 0)
            {
                Nif = Nif.Substring(1, 9);
            }
        }

        #endregion
    }
}
