using System.Text;

namespace pdftotext
{
    public class buscar
    {
        public string csv(string textoCompleto)
        {
            //Extraer el CSV
            string textoCsv = "Verificación "; //Localiza esta cadena en el texto del PDF
            int indexCsv = textoCompleto.IndexOf(textoCsv); //Asigna la posicion del texto a buscar si lo encuentra
            bool foundCsv = false;//Permite controlar si se ha localizado el CSV
            string Csv = string.Empty; //Inicializa la variable que almacenara el CSV

            while (indexCsv >= 0 && !foundCsv) //Se ejecuta mientras no se encuentre el CSV 
            {
                int inicioCsv = indexCsv + textoCsv.Length;
                int largoCsv = textoCompleto.IndexOf("\n", inicioCsv) - inicioCsv;
                if (largoCsv <= 0)
                {
                    largoCsv = textoCompleto.Length - inicioCsv;
                }
                Csv = textoCompleto.Substring(inicioCsv, 16).Trim();
                if (Csv.Length == 16)
                {
                    foundCsv = true;
                }

                indexCsv = textoCompleto.IndexOf(textoCsv, indexCsv + 1);
            }
            if (!foundCsv)
            {
                Csv = string.Empty;
            }
            
            return Csv;
        }

        public string expediente(string textoCompleto, string csvEncontrado)
        {
            // Extraer el expediente
            string Expediente = string.Empty;
            string textoExpediente = csvEncontrado;
            int indexExpediente = textoCompleto.IndexOf(textoExpediente);
            bool foundExpediente = false;

            while (indexExpediente >= 0 && !foundExpediente)
            {
                int inicioExpediente = indexExpediente - textoExpediente.Length - 1;
                int largoExpediente = textoCompleto.IndexOf("\n", inicioExpediente) - inicioExpediente;
                if (largoExpediente <= 0)
                {
                    largoExpediente = textoCompleto.Length - inicioExpediente;
                }
                Expediente = textoCompleto.Substring(inicioExpediente, 16).Trim();
                if (Expediente.Length > 0)
                {
                    foundExpediente = true;
                }
                indexExpediente = textoCompleto.IndexOf(textoExpediente, indexExpediente + 1);
            }
            if (!foundExpediente)
            {
                Expediente = string.Empty;
            }

            return Expediente;
        }

        public string justificante(string textoCompleto)
        {
            //Extraer el numero de justificante
            string justificante = string.Empty;
            string textoJustificante = "justificante: ";
            int indexJustificante = textoCompleto.IndexOf(textoJustificante);
            bool foundJustificante = false;

            while (indexJustificante >= 0 && !foundJustificante)
            {
                int inicioJustificante = indexJustificante + textoJustificante.Length;
                int largoJustificante = textoCompleto.IndexOf("\n", inicioJustificante) - inicioJustificante;
                if (largoJustificante <= 0)
                {
                    largoJustificante = textoCompleto.Length - inicioJustificante;
                }
                justificante = textoCompleto.Substring(inicioJustificante, largoJustificante).Trim();
                if (justificante.Length > 0)
                {
                    foundJustificante = true;
                }
                indexJustificante = textoCompleto.IndexOf(textoJustificante, indexJustificante + 1);
            }

            if (!foundJustificante)
            {
                justificante = string.Empty;
            }

            return justificante;
        }

        public string Nif(string textoCompleto, string Csv)
        {
            //Extraer NIF
            string Nif = string.Empty;
            string textoNif = Csv;
            int indexNif = textoCompleto.IndexOf(textoNif);
            bool foundNif = false;

            while (indexNif >= 0 && !foundNif)
            {
                int inicioNif = indexNif + textoNif.Length + 1;
                int largoNif = textoCompleto.IndexOf("\n", inicioNif) - inicioNif;
                if (largoNif <= 0)
                {
                    largoNif = textoCompleto.Length - inicioNif;
                }
                Nif = textoCompleto.Substring(inicioNif, largoNif);
                if (Nif.Length > 0)
                {
                    foundNif = true;
                }
                indexNif = textoCompleto.IndexOf(textoNif, indexNif + 1);
            }
            if (!foundNif)
            {
                Nif = string.Empty;
            }
            return Nif;
        }


        public string nombre(string textoCompleto, string Nif)
        {
            //Extraer Nombre
            string Nombre = string.Empty;
            string textoNombre = Nif;
            int indexNombre = textoCompleto.IndexOf(textoNombre);
            bool foundNombre = false;

            while (indexNombre >= 0 && !foundNombre)
            {
                int inicioNombre = indexNombre + textoNombre.Length + 1;
                int largoNombre = textoCompleto.IndexOf("\n", inicioNombre) - inicioNombre;
                if (largoNombre <= 0)
                {
                    largoNombre = textoCompleto.Length - inicioNombre;
                }
                Nombre = textoCompleto.Substring(inicioNombre, largoNombre);
                if (Nombre.Length > 0)
                {
                    foundNombre = true;
                }
                indexNombre = textoCompleto.IndexOf(textoNombre, indexNombre + 1);
            }
            if (!foundNombre)
            {
                Nombre = string.Empty;
            }
            return Nombre;
        }

        public string ejercicio(string Expediente)
        {
            //Extraer ejercicio
            string ejercicio = $"Ejercicio: {Expediente.Substring(0, 4)}";
            return ejercicio;
        }

        public string modelo(string Expediente)
        {
            //Extraer modelo
            string modelo = $"Modelo: {Expediente.Substring(4, 3)}";
            return modelo;

            //Nota: esta parte esta comentada porque inicialmente buscaba el modelo de este modo, pero se puede obtener del propio expediente; dejo el codigo por si en las pruebas no funciona. Segun veo el codigo, si se utiliza habria que pasar como parametro el textoCompleto

            ////string modelo = "";
            ////string textoModelo= "justificante:";
            ////int indexModelo= textoCompleto.IndexOf(textoModelo);
            ////bool foundModelo = false;
            ////while (indexModelo>= 0 && !foundModelo)
            ////{
            ////    int inicioModelo = indexModelo+ textoModelo.Length;
            ////    int largoModelo = textoCompleto.IndexOf("\n", inicioModelo) - inicioModelo;
            ////    if (largoModelo <= 0)
            ////    {
            ////        largoModelo= textoCompleto.Length - inicioModelo;
            ////    }
            ////    modelo= textoCompleto.Substring(inicioModelo, largoModelo).Trim();
            ////    if (modelo.Length > 0)
            ////    {
            ////        foundModelo= true;
            ////        writer.WriteLine($"Modelo: {modelo}");
            ////    }
            ////    indexModelo= textoCompleto.IndexOf(textoModelo, indexModelo + 1);

        }

        public string periodo()
        {
            //Metodo para extraer el periodo del modelo (falta por desarrollar

            //Extraer periodo
            string periodo = string.Empty;
            return periodo;
        }
    }

}
