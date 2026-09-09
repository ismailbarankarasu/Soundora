(() => {
    const tokenForm = document.getElementById("playbackTokenForm");
    const panel = document.getElementById("musicPlaybackPanel");
    const title = document.getElementById("playingMusicTitle");
    const message = document.getElementById("musicPlaybackMessage");
    const host = document.getElementById("musicPlayerHost");

    const buttons = document.querySelectorAll(".js-play-music");

    if (!tokenForm || !panel || !title ||
        !message || !host || buttons.length === 0) {
        return;
    }

    const player = document.createElement("audio");
    player.controls = true;
    player.preload = "none";
    player.style.width = "100%";
    player.hidden = true;

    host.appendChild(player);

    let objectUrl = null;

    function resetPlayer() {
        player.pause();
        player.removeAttribute("src");
        player.load();
        player.hidden = true;

        if (objectUrl) {
            URL.revokeObjectURL(objectUrl);
            objectUrl = null;
        }
    }

    function showMessage(text, type) {
        message.className = `alert alert-${type}`;
        message.textContent = text;
    }

    async function readError(response, fallback) {
        const body = await response.json().catch(() => null);
        return body?.message || fallback;
    }

    player.addEventListener("error", () => {
        showMessage(
            "Ses dosyası tarayıcıda oynatılamadı.",
            "danger"
        );
    });

    buttons.forEach(button => {
        button.addEventListener("click", async () => {
            buttons.forEach(item => item.disabled = true);

            resetPlayer();

            panel.hidden = false;
            title.textContent = button.dataset.title;
            showMessage("Ses hazırlanıyor...", "info");

            panel.scrollIntoView({
                behavior: "smooth",
                block: "center"
            });

            try {
                const tokenResponse = await fetch(tokenForm.action, {
                    method: "POST",
                    body: new FormData(tokenForm),
                    credentials: "same-origin",
                    cache: "no-store"
                });

                if (tokenResponse.redirected ||
                    tokenResponse.status === 401) {
                    throw new Error(
                        "Oturumunuz sona ermiş olabilir. Tekrar giriş yapınız."
                    );
                }

                if (!tokenResponse.ok) {
                    throw new Error(
                        "Oynatma başlatılamadı. Sayfayı yenileyip tekrar deneyiniz."
                    );
                }

                const token = await tokenResponse.json();

                if (!token.accessToken) {
                    throw new Error("Oynatma yetkisi alınamadı.");
                }

                const audioResponse = await fetch(
                    button.dataset.playbackUrl,
                    {
                        headers: {
                            Authorization: `Bearer ${token.accessToken}`
                        },
                        credentials: "same-origin",
                        cache: "no-store"
                    }
                );

                if (audioResponse.status === 401) {
                    throw new Error(await readError(
                        audioResponse,
                        "Oynatma yetkisi doğrulanamadı. Dinle butonuna tekrar basınız."
                    ));
                }

                if (!audioResponse.ok) {
                    throw new Error(await readError(
                        audioResponse,
                        "Ses yüklenemedi."
                    ));
                }

                const audioBlob = await audioResponse.blob();

                if (audioBlob.size === 0) {
                    throw new Error("Ses dosyası boş.");
                }

                objectUrl = URL.createObjectURL(audioBlob);

                player.src = objectUrl;
                player.hidden = false;

                try {
                    await player.play();

                    showMessage("Oynatılıyor.", "success");
                } catch (error) {
                    if (error.name === "NotAllowedError") {
                        showMessage(
                            "Ses hazır. Oynatıcıdaki başlat düğmesine basınız.",
                            "info"
                        );
                    } else {
                        throw new Error(
                            "Ses dosyası tarayıcıda oynatılamadı."
                        );
                    }
                }
            } catch (error) {
                resetPlayer();

                showMessage(
                    error.message || "Oynatma sırasında hata oluştu.",
                    "danger"
                );
            } finally {
                buttons.forEach(item => item.disabled = false);
            }
        });
    });

    window.addEventListener("pagehide", resetPlayer);
})();