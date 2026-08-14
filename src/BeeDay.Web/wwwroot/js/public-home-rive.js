const instances = new WeakMap();
let runtimePromise;

function loadRuntime() {
    if (globalThis.rive) {
        return Promise.resolve(globalThis.rive);
    }

    runtimePromise ??= new Promise((resolve, reject) => {
        const script = document.createElement("script");
        script.src = "/js/vendor/rive/rive.js";
        script.async = true;
        script.onload = () => resolve(globalThis.rive);
        script.onerror = () => reject(new Error("The Rive runtime could not be loaded."));
        document.head.append(script);
    });

    return runtimePromise;
}

export async function mount(canvas, source) {
    if (!canvas || instances.has(canvas)) {
        return;
    }

    const host = canvas.closest("[data-rive-host]");

    try {
        const rive = await loadRuntime();
        if (!rive) {
            throw new Error("The Rive runtime did not initialize.");
        }

        rive.RuntimeLoader.setWasmUrl("/js/vendor/rive/rive.wasm");
        rive.RuntimeLoader.setWasmFallbackUrl("/js/vendor/rive/rive_fallback.wasm");

        const motion = window.matchMedia("(prefers-reduced-motion: reduce)");
        let animation;
        const resize = () => animation?.resizeDrawingSurfaceToCanvas();
        const onMotionChange = event => {
            event.matches ? animation?.pause() : animation?.play();
            host?.setAttribute("data-rive-motion", event.matches ? "paused" : "playing");
        };

        animation = new rive.Rive({
            src: source,
            canvas,
            autoplay: !motion.matches,
            layout: new rive.Layout({ fit: rive.Fit.Contain, alignment: rive.Alignment.Center }),
            onLoad: () => {
                resize();
                host?.setAttribute("data-rive-state", "ready");
                host?.setAttribute("data-rive-motion", motion.matches ? "paused" : "playing");
            },
            onLoadError: () => host?.setAttribute("data-rive-state", "error")
        });

        const resizeObserver = new ResizeObserver(resize);
        resizeObserver.observe(canvas);
        motion.addEventListener("change", onMotionChange);
        instances.set(canvas, { animation, motion, onMotionChange, resizeObserver });
    } catch (error) {
        host?.setAttribute("data-rive-state", "error");
        console.warn("BeeDay public Home Rive fallback activated.", error);
    }
}

export function dispose(canvas) {
    const entry = instances.get(canvas);
    if (!entry) {
        return;
    }

    entry.resizeObserver.disconnect();
    entry.motion.removeEventListener("change", entry.onMotionChange);
    entry.animation.cleanup();
    instances.delete(canvas);
}
