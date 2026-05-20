window.geekTopQrScanner = (() => {
    let stream = null;
    let timer = null;
    let detector = null;
    let dotNetRef = null;
    let canvas = null;
    let context = null;

    async function start(videoId, dotNetObjectReference) {
        const hasNativeDetector = "BarcodeDetector" in window;
        const hasJsQrFallback = typeof window.jsQR === "function";

        if (!hasNativeDetector && !hasJsQrFallback) {
            throw new Error("Leitor de QR Code indisponível neste navegador. Use a digitação manual.");
        }

        await stop();

        dotNetRef = dotNetObjectReference;

        if (hasNativeDetector) {
            const supportedFormats = await window.BarcodeDetector.getSupportedFormats();
            if (supportedFormats.includes("qr_code")) {
                detector = new window.BarcodeDetector({ formats: ["qr_code"] });
            }
        }

        if (!detector && hasJsQrFallback) {
            canvas = document.createElement("canvas");
            context = canvas.getContext("2d", { willReadFrequently: true });
        }

        if (!detector && !context) {
            throw new Error("Este navegador não reconhece QR Code pela câmera. Use a digitação manual.");
        }

        stream = await navigator.mediaDevices.getUserMedia({
            video: {
                facingMode: { ideal: "environment" }
            },
            audio: false
        });

        const video = document.getElementById(videoId);
        if (!video) {
            await stop();
            throw new Error("Área de câmera não encontrada.");
        }

        video.srcObject = stream;
        await video.play();

        timer = window.setInterval(async () => {
            if (!detector || !video || video.readyState < 2) {
                if (!context || !video || video.readyState < 2) {
                    return;
                }
            }

            try {
                let rawValue = "";

                if (detector) {
                    const codes = await detector.detect(video);
                    rawValue = codes.find(item => item.rawValue)?.rawValue ?? "";
                } else {
                    canvas.width = video.videoWidth;
                    canvas.height = video.videoHeight;
                    context.drawImage(video, 0, 0, canvas.width, canvas.height);

                    const imageData = context.getImageData(0, 0, canvas.width, canvas.height);
                    rawValue = window.jsQR(imageData.data, imageData.width, imageData.height)?.data ?? "";
                }

                if (rawValue) {
                    await dotNetRef.invokeMethodAsync("OnQrCodeScanned", rawValue);
                    await stop();
                }
            } catch {
                // Some frames can fail while the camera is focusing. Keep scanning.
            }
        }, 350);
    }

    async function stop() {
        if (timer) {
            window.clearInterval(timer);
            timer = null;
        }

        if (stream) {
            for (const track of stream.getTracks()) {
                track.stop();
            }
            stream = null;
        }

        detector = null;
        dotNetRef = null;
        canvas = null;
        context = null;
    }

    return { start, stop };
})();
