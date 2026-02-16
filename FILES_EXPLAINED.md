# Estructura del Proyecto y Archivos a Subir

Este documento detalla la estructura final del proyecto **User Ratings** y explica qué función cumple cada archivo.

**Archivos que DEBES subir a GitHub:**

## 1. Raíz del Proyecto

*   **`README.md`**: (Modificado)
    *   **Función**: Es la portada de tu proyecto. Explica qué es el plugin, sus características, cómo instalarlo y configurarlo. Está completamente traducido al español.
*   **`.gitignore`**: (Nuevo)
    *   **Función**: Le dice a Git qué archivos *ignorar*. Esto es crucial para no subir archivos basura o temporales (como las carpetas `bin` y `obj`) que se generan automáticamente al compilar.
*   **`manifest.json`**: (Existente)
    *   **Función**: Es el DNI del plugin para Jellyfin. Contiene el nombre, versión, descripción y compatibilidad. Es lo que lee Jellyfin para mostrarlo en el catálogo.

## 2. Código Fuente (`src/`)

### 2.1 Archivos Principales
*   **`Plugin.cs`**: (Modificado)
    *   **Función**: El cerebro del plugin.
    *   **Cambios**: Se ha configurado para inyectar automáticamente el script de valoraciones (`ratings.js`) en Jellyfin y registrar la configuración (`config.js`).
*   **`Jellyfin.Plugin.UserRatings.csproj`**: (Existente)
    *   **Función**: El archivo de proyecto de .NET. Define las dependencias y cómo se debe compilar el código.

### 2.2 Configuración y Frontend (`src/Configuration/`)
*   **`ratings.js`**: (Modificado y Traducido)
    *   **Función**: El script "mágico" que hace que todo funcione en el navegador.
    *   **Cambios**: Sigue el modelo original pero **totalmente en español**. Se encarga de pintar las estrellas, el formulario de reseña y mostrar los comentarios en las páginas de películas.
*   **`config.js`**: (Renombrado desde `ratings.js` original)
    *   **Función**: Maneja la lógica de la página de configuración en el panel de administrador (botones de exportar, importar, borrar todo).
*   **`configPage.html`**: (Modificado y Traducido)
    *   **Función**: La estructura visual de la página de configuración.

### 2.3 API y Datos (`src/Api/`, `src/Data/`, `src/Models/`)
*   **`RatingsController.cs`** (`src/Api/`):
    *   **Función**: Los "camareros" del plugin. Reciben los pedidos del navegador (guardar nota, pedir reseñas, exportar) y se los pasan a la base de datos.
*   **`RatingRepository.cs`** (`src/Data/`):
    *   **Función**: El "bibliotecario". Se encarga de guardar y recuperar los datos del archivo de historial o base de datos.
*   **`UserRating.cs`** y **`RatingExportDto.cs`** (`src/Models/`):
    *   **Función**: Los planos o moldes de los datos. Definen qué forma tiene una "Valoración" (quién, qué, cuánto, nota).

---

**Archivos que NO se suben (y .gitignore ya bloquea):**
*   Carpetas `bin/` y `obj/`: Son resultado de la compilación.
*   Archivos `.zip` generados.
*   Archivos ocultos de sistema (como `.DS_Store`).
