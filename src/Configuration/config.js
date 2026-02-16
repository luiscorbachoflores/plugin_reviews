define(['loading', 'dialog', 'mainTabs', 'emby-button'], function (loading, dialog, mainTabs) {

    return function (view, params) {

        view.addEventListener('viewshow', function () {

            // ── EXPORT JSON ─────────────────────────────────────────
            document.getElementById('btnExportRatings').addEventListener('click', function () {
                const apiKey = ApiClient.accessToken();
                const url = ApiClient.getUrl('api/UserRatings/Export') + '?api_key=' + apiKey;
                const a = document.createElement('a');
                a.href = url;
                a.download = 'userratings-export.json';
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
            });

            // ── EXPORT CSV ──────────────────────────────────────────
            document.getElementById('btnExportRatingsCsv').addEventListener('click', function () {
                const apiKey = ApiClient.accessToken();
                const url = ApiClient.getUrl('api/UserRatings/ExportCsv') + '?api_key=' + apiKey;
                const a = document.createElement('a');
                a.href = url;
                a.download = 'userratings-export.csv';
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
            });

            // ── EXPORT HISTORY ──────────────────────────────────────
            document.getElementById('btnExportHistory').addEventListener('click', function () {
                const apiKey = ApiClient.accessToken();
                const url = ApiClient.getUrl('api/UserRatings/ExportHistory') + '?api_key=' + apiKey;
                const a = document.createElement('a');
                a.href = url;
                a.download = 'ratings_history.csv';
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
            });

            // ── IMPORT ──────────────────────────────────────────────
            document.getElementById('btnImportRatings').addEventListener('click', async function () {
                const fileInput = document.getElementById('importFileInput');
                const status = document.getElementById('importStatus');

                if (fileInput.files.length === 0) {
                    dialog.alert("Por favor, selecciona un archivo JSON primero.");
                    return;
                }

                const file = fileInput.files[0];
                const reader = new FileReader();

                reader.onload = async function (e) {
                    try {
                        const json = JSON.parse(e.target.result);
                        loading.show();

                        const apiKey = ApiClient.accessToken();
                        // Import endpoint expects a List<RatingExportDto>
                        await ApiClient.ajax({
                            type: "POST",
                            url: ApiClient.getUrl('api/UserRatings/Import') + '?api_key=' + apiKey,
                            data: JSON.stringify(json),
                            contentType: 'application/json'
                        });

                        loading.hide();
                        status.innerText = "¡Éxito! Importación completada.";
                        status.style.display = 'block';
                        status.style.color = 'green';

                        setTimeout(() => status.style.display = 'none', 5000);

                    } catch (err) {
                        loading.hide();
                        console.error(err);
                        status.innerText = "Error: " + err.message;
                        status.style.display = 'block';
                        status.style.color = 'red';
                    }
                };

                reader.readAsText(file);
            });
        });
    };
});
