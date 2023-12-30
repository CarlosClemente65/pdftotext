<h1 align="center"> dsepdfatext </h1>
<br>

<h2> Extraccion de texto de un PDF y obtencion de variables de modelos de Hacienda y de documentos laborales para su tratamiento </h2>
<br>
<h4> @ Carlos Clemente (Diagram Software Europa S.L.) - 12/2023 </h4>

<h3>Descripcion</h3>
Permite extraer los datos de un PDF a un fichero de texto, y buscar las variables de los modelos de Hacienda o de documentos laborales para su tratamiento
<br><br>

### Control versiones

* v1.0.0 Primera version que extrae el texto completo del PDF
* v1.1.0 Incorporada la opcion para la extraccion de datos de modelos
* v1.2.0 Incorporada la opcion para la extraccion de datos de documentos laborales
* v1.2.1 Incorporada la opcion para el proceso masivo de documentos de una carpeta y poder mostrar la ayuda
<br><br>


### Uso:
```
* dsepdfatexto.exe -h
* dsepdfatexto.exe fichero.pdf -t textoPDF.txt -m datosModelo.txt -l datosLaboral.txt 
* dsepdfatexto carpeta [-rm | -rl]
	
 Parametros
    -h                 : Esta ayuda
    fichero.pdf        : Nombre del fichero PDF a procesar (unico fichero)
    -t textoPDF.txt    : Nombre del fichero en el que grabar el texto completo del PDF
    -m datosModelo.txt : Nombre del fichero en el que grabar los campos localizados del modelo
    -l datosLaboral.txt: Nombre del fichero en el que grabar los campos localizados del modelo laboral
    carpeta            : Carpeta donde estan todos los ficheros PDF a procesar de forma masiva
    -rm                : Parametro que indica que se procesen los ficheros de la carpeta como modelos
    -rl                : Parametro que indica que se procesen los ficheros de la carpeta como documentos laborales

```
<br>

### Notas:
* Si se pasa como primer parametro una carpeta, se procesan los ficheros como modelos (parametro -rm) o como documentos de laboral (parametro -rl)
* Si se pasa como primer parametro -h se muestra la ayuda del programa. Tambien se muestra la ayuda si se ejecuta desde windows
* Si se pasa el fichero.pdf al menos uno de los 3 parametros siguientes son obligatorios (el orden de los parametros es indiferente)
* El parametro -t va seguido del nombre del fichero con el texto completo del PDF
* El parametro -m va seguido del nombre del fichero con los campos localizados del modelo
* El parametro -l va seguido del nombre del fichero con los campos localizados del modelo laboral

