# Plugins de Jellyfin — luiscorbachoflores

Dos plugins independientes para Jellyfin (.NET 9 / `Jellyfin.Controller` 10.11.x), cada uno inyecta su propio script en `index.html` mediante un middleware ASP.NET Core en tiempo de respuesta — **sin modificar ningún fichero de jellyfin-web**.

## Instalación (repositorio de plugins)

En Jellyfin, ve a **Panel de control → Complementos → Repositorios** y añade:

```
https://github.com/luiscorbachoflores/jellyfin_plugins/raw/main/manifest.json
```

Instala **Reviews** y/o **JellyAsk** desde el catálogo de complementos y reinicia Jellyfin.

## Reviews

Añade un bloque de reseñas de usuarios a la página de detalle de cada película o serie: valoración por estrellas en pasos de 0,5, comentario de texto y opción de publicar como usuario identificado de Jellyfin o de forma anónima.

- API REST propia (`GET/POST /Reviews/{itemId}`), almacenamiento SQLite.
- El modo "usuario" verifica el token de sesión en el servidor.

Código en [`src/Reviews`](src/Reviews).

## JellyAsk

Añade una entrada **"Pedir película"** al menú de navegación (justo encima de Ajustes). Abre un formulario con un único campo de texto libre ("Incluye todos los detalles posibles para que podamos encontrar la película"); al enviarlo, registra la petición en el **Activity Log nativo de Jellyfin** (Panel de control → Actividad), visible para cualquier plugin de notificaciones (p. ej. un notificador de Telegram) que escuche esos eventos.

- Requiere sesión Jellyfin válida (no admite modo anónimo).
- API REST propia (`POST /JellyAsk/Request`).

Código en [`src/JellyAsk`](src/JellyAsk).

## Desarrollo

Cada plugin es un proyecto .NET independiente:

```
cd src/Reviews   # o src/JellyAsk
dotnet build -c Release -o build
```

El resultado en `build/` incluye el DLL del plugin y sus dependencias. Para desplegar manualmente, copia el contenido a `config/plugins/<Nombre>_<version>/` de tu instancia de Jellyfin.

## Compatibilidad

Probado contra Jellyfin **10.11.11**. El anclaje visual usa clases del cliente web oficial (`.itemDetailPage`, `.overview-controls`, `.mainDrawer-scrollContainer`, `.btnSettings`); si una versión futura de Jellyfin cambia esa estructura, solo haría falta actualizar el JS correspondiente (`src/Reviews/wwwroot/reviews.js` o `src/JellyAsk/wwwroot/jellyask.js`), no el resto del plugin.
