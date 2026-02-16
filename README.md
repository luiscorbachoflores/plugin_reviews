# Plugin de Valoraciones de Usuario para Jellyfin

Un plugin para Jellyfin que permite a los usuarios valorar y reseñar contenido, con soporte para historial de valoraciones e importación/exportación de datos.

## Características

-   **Valoraciones de Usuario**: Califica contenido en una escala de 1 a 5.
-   **Reseñas**: Añade comentarios de texto a tus valoraciones.
-   **Historial**: Registra automáticamente todos los eventos de valoración en un archivo CSV persistente (`ratings_history.csv`).
-   **Importar/Exportar**:
    -   Exporta todas las valoraciones a JSON o CSV.
    -   Importa valoraciones desde una copia de seguridad JSON.
    -   Descarga el historial completo.
-   **Interfaz Integrada**: Las reseñas se muestran directamente en la página de detalles de la película o serie (compatible con Jellyfin 10.11.x+).

## Instalación

1.  Añade la URL del repositorio a tu instancia de Jellyfin:
    `https://github.com/luiscorbachoflores/pluginratingtest/raw/main/manifest.json`
2.  Instala **User Ratings** desde el catálogo.
3.  Reinicia Jellyfin.

## Instalación Manual

1.  Descarga el ZIP de la última versión (release).
2.  Descomprime la carpeta `UserRatings` en el directorio `plugins` de tu Jellyfin.
3.  Reinicia Jellyfin.

## Configuración

Ve a **Panel de Control > Complementos > User Ratings** para acceder a las funciones de importación/exportación y descarga de historial.
