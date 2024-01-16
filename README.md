<h1 align="center"> dsepdfatext </h1>
<br>

<h2> Extracción de texto de un PDF y obtención de variables de modelos de Hacienda y de documentos laborales para su tratamiento </h2>
<br>
<h4> @ Carlos Clemente (Diagram Software Europa S.L.) - 12/2023 </h4>

<h3>Descripción</h3>
Permite extraer los datos de un PDF a un fichero de texto, y buscar las variables de los modelos de Hacienda o de documentos laborales para su tratamiento
<br><br>

### Control versiones

* v1.0.0 Primera versión que extrae el texto completo del PDF
* v1.1.0 Incorporada la opción para la extracción de datos de modelos
* v1.2.0 Incorporada la opción para la extracción de datos de documentos laborales
* v1.2.1 Incorporada la opción para el proceso masivo de documentos de una carpeta y poder mostrar la ayuda
* v1.2.2 Modificada la salida de los resultados de los documentos laborales para unificar etiquetas. Revisión modelo 111
* v1.2.3 Añadido método quitaRaros
<br><br>


### Uso:
```
* dsepdfatexto.exe -h
* dsepdfatexto.exe fichero.pdf -t textoPDF.txt -m datosModelo.txt -l datosLaboral.txt 
* dsepdfatexto carpeta [-rm | -rl]
	
 Parametros
    -h                 : Esta ayuda
    fichero.pdf        : Nombre del fichero PDF a procesar (único fichero)
    -t textoPDF.txt    : Nombre del fichero en el que grabar el texto completo del PDF
    -m datosModelo.txt : Nombre del fichero en el que grabar los campos localizados del modelo
    -l datosLaboral.txt: Nombre del fichero en el que grabar los campos localizados del modelo laboral
    carpeta            : Carpeta donde están todos los ficheros PDF a procesar de forma masiva
    -rm                : Parámetro que indica que se procesen los ficheros de la carpeta como modelos
    -rl                : Parámetro que indica que se procesen los ficheros de la carpeta como documentos laborales

```
<br>

### Notas:
* Si se pasa como primer parámetro una carpeta, se procesan los ficheros como modelos (parámetro -rm) o como documentos de laboral (parámetro -rl)
* Si se pasa como primer parámetro -h se muestra la ayuda del programa. Tambien se muestra la ayuda si se ejecuta desde windows
* Si se pasa el fichero.pdf al menos uno de los 3 parametros siguientes son obligatorios (el orden de los parametros es indiferente)
* El parámetro -t va seguido del nombre del fichero con el texto completo del PDF
* El parámetro -m va seguido del nombre del fichero con los campos localizados del modelo
* El parámetro -l va seguido del nombre del fichero con los campos localizados del modelo laboral

