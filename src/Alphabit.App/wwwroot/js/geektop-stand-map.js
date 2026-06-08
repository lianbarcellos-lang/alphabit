window.geekTopStandMap = (() => {
    const disposers = new Map();

    function getDropPoint(elementId, clientX, clientY) {
        const element = document.getElementById(elementId);
        if (!element) {
            return { x: 0, y: 0 };
        }

        const rect = element.getBoundingClientRect();
        const x = ((clientX - rect.left) / rect.width) * 100;
        const y = ((clientY - rect.top) / rect.height) * 100;

        return {
            x: Math.max(0, Math.min(100, x)),
            y: Math.max(0, Math.min(100, y))
        };
    }

    function enablePinDragging(elementId, dotNetRef) {
        const element = document.getElementById(elementId);
        if (!element) {
            return;
        }

        dispose(elementId);

        let activePin = null;
        let activeStandId = 0;
        let activePoint = null;

        const movePin = (event) => {
            if (!activePin) {
                return;
            }

            activePoint = getDropPoint(elementId, event.clientX, event.clientY);
            activePin.style.left = `${activePoint.x}%`;
            activePin.style.top = `${activePoint.y}%`;
        };

        const stopDrag = async () => {
            if (!activePin) {
                return;
            }

            const pin = activePin;
            const standId = activeStandId;
            const point = activePoint;

            activePin = null;
            activeStandId = 0;
            activePoint = null;
            pin.classList.remove("admin-stand-pin--dragging");

            if (standId > 0 && point) {
                await dotNetRef.invokeMethodAsync("OnStandMapDropped", standId, point.x, point.y);
            }
        };

        const startDrag = (event) => {
            const pin = event.target.closest(".admin-stand-pin");
            if (!pin || !element.contains(pin)) {
                return;
            }

            event.preventDefault();
            activePin = pin;
            activeStandId = Number(pin.dataset.standId || 0);
            activePoint = getDropPoint(elementId, event.clientX, event.clientY);
            pin.classList.add("admin-stand-pin--dragging");
            pin.setPointerCapture?.(event.pointerId);
            movePin(event);
        };

        element.addEventListener("pointerdown", startDrag);
        element.addEventListener("pointermove", movePin);
        element.addEventListener("pointerup", stopDrag);
        element.addEventListener("pointercancel", stopDrag);
        element.addEventListener("pointerleave", stopDrag);

        disposers.set(elementId, () => {
            element.removeEventListener("pointerdown", startDrag);
            element.removeEventListener("pointermove", movePin);
            element.removeEventListener("pointerup", stopDrag);
            element.removeEventListener("pointercancel", stopDrag);
            element.removeEventListener("pointerleave", stopDrag);
        });
    }

    function dispose(elementId) {
        const disposer = disposers.get(elementId);
        if (disposer) {
            disposer();
            disposers.delete(elementId);
        }
    }

    return {
        getDropPoint,
        enablePinDragging,
        dispose
    };
})();
