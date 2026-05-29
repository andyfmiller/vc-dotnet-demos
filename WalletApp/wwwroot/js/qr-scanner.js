// QR code scanner helper using html5-qrcode
// Loaded on pages that include _QrScannerPartial.cshtml

const _qrScanners = {};

function toggleQrScanner(scannerDivId, targetInputId, buttonId, autoSubmitFormId) {
    if (_qrScanners[scannerDivId]) {
        stopQrScanner(scannerDivId, buttonId);
    } else {
        startQrScanner(scannerDivId, targetInputId, buttonId, autoSubmitFormId);
    }
}

function startQrScanner(scannerDivId, targetInputId, buttonId, autoSubmitFormId) {
    const div = document.getElementById(scannerDivId);
    const btn = document.getElementById(buttonId);

    div.style.display = 'block';
    if (btn) btn.innerHTML = '<i class="fas fa-times me-1"></i> Cancel';

    const scanner = new Html5Qrcode(scannerDivId);
    _qrScanners[scannerDivId] = scanner;

    scanner.start(
        { facingMode: 'environment' },
        { fps: 10, qrbox: { width: 250, height: 250 } },
        (decodedText) => {
            // Populate the target input
            const input = document.getElementById(targetInputId);
            if (input) {
                input.value = decodedText;
                input.dispatchEvent(new Event('input', { bubbles: true }));
            }
            // Stop the camera, then auto-submit if a form id was provided
            stopQrScanner(scannerDivId, buttonId, () => {
                if (autoSubmitFormId) {
                    const form = document.getElementById(autoSubmitFormId);
                    if (form) form.submit();
                }
            });
        },
        (_errorMessage) => {
            // Per-frame scan error — ignored
        }
    ).catch((err) => {
        console.warn('QR scanner failed to start:', err);
        div.style.display = 'none';
        if (btn) btn.innerHTML = '<i class="fas fa-camera me-1"></i> Start Camera';
        delete _qrScanners[scannerDivId];
    });
}

function stopQrScanner(scannerDivId, buttonId, onStopped) {
    const scanner = _qrScanners[scannerDivId];
    const div = document.getElementById(scannerDivId);
    const btn = document.getElementById(buttonId);

    if (scanner) {
        scanner.stop().then(() => {
            scanner.clear();
            div.style.display = 'none';
            if (btn) btn.innerHTML = '<i class="fas fa-camera me-1"></i> Start Camera';
            delete _qrScanners[scannerDivId];
            if (typeof onStopped === 'function') onStopped();
        }).catch((err) => {
            console.warn('Error stopping QR scanner:', err);
            if (typeof onStopped === 'function') onStopped();
        });
    } else {
        if (typeof onStopped === 'function') onStopped();
    }
}
